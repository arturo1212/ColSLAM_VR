from __future__ import print_function
from bridge_publisher import BridgeClient
from camera_calibration import *
from marker_homography import getDRotation, getCameraMatrix, getHomography
import datetime
import cv2
import numpy as np
import picamera
import os
import base64
import roslibpy

def color_detection(frame, lower, upper, min_size):
    found = False
    result = []
    mask = cv2.inRange(frame, lower , upper);
    im2, conts, h = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    for c in conts:
        if cv2.contourArea(c) > min_size:
            cv2.drawContours(frame, [c], -1, 0, -1)
            result.append(c)
            found = True
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
    def __init__(self, ip, port, close_distance = 40, sensitivity = 30, MIN_MATCH_COUNT = 25, is_claw = 0):
        self.ros = roslibpy.Ros(host=ip, port=port)
        self.ros.on_ready(self.run)
        self.ros.connect()
        self.marker_found  = False
        self.obj_found  = False         # (NUEVO)
        self.topic_objective = None     # (NUEVO)
        self.topic_homography = None
        self.topic_reset  = None
        self.topic_stream = None
        self.topic_near = None
        self.close_distance = close_distance
        self.distance = 999
        self.sensitivity = sensitivity
        self.MIN_MATCH_COUNT = MIN_MATCH_COUNT
        self.is_claw = is_claw
        self.ros.run_forever()

    def resetme(self, data):
        print("RESETEADO")
        self.marker_found = False   

    def getDistance(self, data):
        splited = data["data"].split(",")
        self.distance = int(splited[1])  

    def create_topics(self):
        self.topic_homography = roslibpy.Topic(self.ros, "homography", "std_msgs/String")
        self.topic_objective = roslibpy.Topic(self.ros, "objective", "std_msgs/String") # (NUEVO)
        self.topic_stream = roslibpy.Topic(self.ros, "stream", "std_msgs/String")
        self.topic_reset = roslibpy.Topic(self.ros, "reset", "std_msgs/String")
        self.topic_near = roslibpy.Topic(self.ros, "arduino", "std_msgs/String")
        self.topic_near.subscribe(self.getDistance)
        self.topic_reset.subscribe(self.resetme)
        self.topic_homography.advertise()
        self.topic_objective.advertise()    # (NUEVO)
        self.topic_stream.advertise()

    def run(self):
        self.create_topics()                        # Configuracion de ROS
        K = getCameraMatrix("chessboard.png")       # Obtener matriz de calibracion
        reference = cv2.imread('reference1.png',0)  # Cargar imagen de referencia para homografia
        center_width = 50
        resolution = (640,480)

        # Valores del rango de color (VERDE)
        lower_green = np.array([60 - self.sensitivity, 100, 60])
        upper_green = np.array([60 + self.sensitivity, 255, 255])

        #(NUEVO) AGREGADAS VARIABLES PARA ROJO
        lower_red = np.array ([160,100,100])
        upper_red = np.array ([179,255,255])
        #MIN_MATCH_COUNT = 25
        MIN_CONTOUR_SIZE = 300

        with picamera.PiCamera() as camera:     
            camera = best_camera_config(camera)
            rawCapture = picamera.array.PiRGBArray(camera, size=resolution)

            # Ciclo de lecturas de frames
            for frame in camera.capture_continuous(rawCapture, format="bgr", use_video_port=True):
                # Imagenes y espacios de colores
                #print("CONN: " + str(self.ros.is_connected))
                image = frame.array                                         # Imagen sin procesar.
                resized = cv2.resize(image,(320,240))
                retval, buff = cv2.imencode('.jpg', resized)
                jpg_as_text = base64.b64encode(buff)
                stream_data = {"data" : jpg_as_text}
                self.topic_stream.publish(stream_data)

                # Procesar las imagenes
                image_hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)    # Espacio HSV
                crop_img = get_center_segment(image_hsv, center_width)      # Obtener imagen del centro.

                #(NUEVO) Cambio de nombre a las variables lower y upper
                green_img, cnts, color_found = color_detection(crop_img, lower_green, upper_green, MIN_CONTOUR_SIZE) # Filtro por color
                
                # Verificacion de colores y marcas
                if(color_found and not self.marker_found and self.is_claw==0):                   # Si encontramos color y no hemos visto marca.
                    self.topic_homography.publish({"data" : "hold"})         # Notificar inicio de procesamiento al servidor.
                    print("Distance: "+str(self.distance))

                    if(self.close_distance >= self.distance):
                        M, count = getHomography(image, reference, self.MIN_MATCH_COUNT, True)       # Obtener matriz de homografia
                        if(M is None):                                      # Si no hay suficientes atributos coincidentes
                            print("MISS: ", str(count)) 
                        else:                                               # Si hay suficientes atributos coincidentes
                            ys = getDRotation(K, M)
                            self.topic_homography.publish({"data":"found," + str(ys[0]) + "," + str(ys[1])}) # Publicar vector y cambiar booleano
                            self.marker_found = True
                            print("FOUND: ", str(ys), str(count))
                
                # (NUEVO) Deteccion de color rojo
                red_img, cnts, obj_found = color_detection(crop_img, lower_red, upper_red, MIN_CONTOUR_SIZE) # Filtro por color
                if(obj_found):
                    self.topic_objective.publish({"data":"found"}) # Publicar vector y cambiar booleano
                    self.obj_found = True
                    print("Objetivo encontrado")
                

                # Mostrar imagenes
                #cv2.imshow("Frame", image)
                #cv2.imshow("Centro", crop_img)
                #cv2.imshow("Filtro verde", green_img)

                k = cv2.waitKey(1)
                if k%256 == 27:
                    break
                rawCapture.truncate(0) # Limpiar stream


vm = VisionMonitor("rosnodes", 9090, int(os.environ['CLOSEDIS']), int(os.environ['CSENSITIVITY']), int(os.environ['MINMATCH']), int(os.environ['ISCLAW']))