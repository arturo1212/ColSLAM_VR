from bridge_publisher import BridgeClient
from camera_calibration import *
from marker_homography import getDRotation, getCameraMatrix, getHomography
import datetime
import cv2
import numpy as np
import picamera
import os
import base64

def color_detection(frame, lower, upper, min_size):
    found = False
    result = []
    mask = cv2.inRange(frame, lower , upper);
    im2, conts, h = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    for c in conts:
        if cv2.contourArea(c) > min_size:
            cv2.drawContours(crop_img, [c], -1, 0, -1)
            result.append(c)
            found = True
            print("ENCONTRADO", cv2.contourArea(c)) 
    return mask, result, found
    
def get_center_segment(image, half_width):
    w, h = np.size(image, 1), np.size(image, 0) 
    crop_img = image[0:h, w/2-half_width:w/2+half_width]   
    return crop_img 

def take_snapshot(images, names, img_counter, dirname):
    for i in range(len(images)):
        img_name = dirname + "/" + names[i] + "_{}.png".format(img_counter)
        cv2.imwrite(img_name, images[i])

class VisionMonitor:
    def __init__(self, ip, port):
        self.ros = roslibpy.Ros(host='rosnodes', port=9090)
        self.color_found  = False
        self.topic_homography = None
        self.topic_reset  = None
        self.topic_stream = None
    
    def resetme(self, data):
        self.color_found = True    

    def create_topics(self):
        topic_homography = roslibpy.Topic(self.ros, "homography", "std_msgs/String")
        topic_stream = roslibpy.Topic(self.ros, "stream", "std_msgs/String")
        topic_reset = roslibpy.Topic(self.ros, "reset", "std_msgs/String")
        topic_homography.advertise()
        topic_stream.advertise()

    def run(self):
        self.create_topics()                        # Configuracion de ROS
        K = getCameraMatrix("chessboard.png")       # Obtener matriz de calibracion
        reference = cv2.imread('reference1.png',0)  # Cargar imagen de referencia para homografia
        center_width = 50
        resolution = (640,480)

        # Valores del rango de color (VERDE)
        sensitivity = 30
        lower = np.array([60 - sensitivity, 100, 60])
        upper = np.array([60 + sensitivity, 255, 255])
        MIN_MATCH_COUNT = 50
        MIN_CONTOUR_SIZE = 300

        with picamera.PiCamera() as camera:     
            camera = best_camera_config(camera)
            rawCapture = picamera.array.PiRGBArray(camera, size=resolution)

            # Ciclo de lecturas de frames
            for frame in camera.capture_continuous(rawCapture, format="bgr", use_video_port=True):
                # Imagenes y espacios de colores
                image = frame.array                                         # Imagen sin procesar.
                retval, buff = cv2.imencode('.jpg', image)
                jpg_as_text = base64.b64encode(buff)
                self.topic_stream.publish(jpg_as_text)
                image_hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)    # Espacio HSV
                crop_img = get_center_segment(image_hsv, center_width)      # Obtener imagen del centro.
                green_img, cnts, color_found = color_detection(crop_img, lower, upper, MIN_CONTOUR_SIZE) # Filtro por color
                
                # Verificacion de colores y marcas
                if(color_found and not marker_found):                   # Si encontramos color y no hemos visto marca.
                    self.topic_homography.publish("hold_nada")          # Notificar inicio de procesamiento al servidor.
                    M, count = getHomography(image, reference, MIN_MATCH_COUNT, True)       # Obtener matriz de homografia
                    if(M is None):                                      # Si no hay suficientes atributos coincidentes
                        print("MISS: ", str(count)) 
                    else:                                               # Si hay suficientes atributos coincidentes
                        ys = getDRotation(K, M)
                        self.topic_homography.publish("found_"+str(ys)) # Publicar vector y cambiar booleano
                        self.color_found = True
                        print("FOUND: ", str(ys), str(count))

                # Mostrar imagenes
                #cv2.imshow("Frame", image)
                #cv2.imshow("Centro", crop_img)
                #cv2.imshow("Filtro verde", green_img)
                k = cv2.waitKey(1)
                if k%256 == 27:
                    break
                rawCapture.truncate(0) # Limpiar stream