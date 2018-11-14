from bridge_publisher import BridgeClient
from camera_calibration import *
from marker_homography import getDRotation, getCameraMatrix, getHomography
import datetime
import cv2
import numpy as np
import picamera
import os

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

img_counter = 0
dir_name_found = "FOUND" + datetime.datetime.now().strftime("%Y_%m_%d_%H_%M_%S")
dir_name_miss = "MISS"  + datetime.datetime.now().strftime("%Y_%m_%d_%H_%M_%S")

#if not os.path.exists(dir_name):
#    os.makedirs(dir_name)

with picamera.PiCamera() as camera:
    camera = best_camera_config(camera)
    rawCapture = picamera.array.PiRGBArray(camera, size=(640, 480))
    for frame in camera.capture_continuous(rawCapture, format="bgr", use_video_port=True):
        # Imagenes y espacios de colores
        image = frame.array                                         # Imagen sin procesar.
        image_hsv = hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)    # Espacio HSV
        crop_img = get_center_segment(image_hsv, 50)                # Obtener imagen del centro.
        
        # Filtrar por color
        sensitivity = 30
        lower = np.array([60 - sensitivity, 100, 60])
        upper = np.array([60 + sensitivity, 255, 255])
        green_img, cnts, color_found = color_detection(crop_img, lower, upper, 300)
        
        if(color_found): #and not marker_found):
            K = getCameraMatrix("chessboard.png")
            M = getHomography(frame, reference, True)  
            if(M is None):
                #take_snapshot(images, names, img_counter, dirname_miss)
                #img_counter += 1
                continue 
            ys = getDRotation(K, M)
            # Publicar DRot
            # Captura de pantalla
            #take_snapshot(images, names, img_counter, dirname_found)
            img_counter += 1

        # Mostrar imagenes
        cv2.imshow("Frame", image)
        cv2.imshow("Centro", crop_img)
        cv2.imshow("Filtro verde", green_img)
        
        k = cv2.waitKey(1)
        if k%256 == 27:
            print("Escape hit, closing...")
            break

        elif k%256 == 32:
            # SPACE pressed
            images, names = [image, crop_img, green_img], ["original", "center", "color"]
            take_snapshot(images, names, img_counter, dirname_miss)
            img_counter += 1
        # clear the stream in preparation for the next frame
        rawCapture.truncate(0)

