import numpy as np
import pytesseract
from PIL import Image
# import the necessary packages
from picamera.array import PiRGBArray
from picamera import PiCamera
import time
import cv2
# Problemas: Ejecutable de la raspberry
# Problemas: Archivo de lenguajes (agregar en la carpeta de tessdata_dir_config)
# Debug: Comentarios y argumento adicional de config

pytesseract.pytesseract.tesseract_cmd = r'/usr/bin/tesseract'

def recognition(image):
    lastresult = ''
    img = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    kernel = np.ones((1, 1), np.uint8)
    img = cv2.dilate(img, kernel, iterations=1)
    img = cv2.erode(img, kernel, iterations=1)
    #tessdata_dir_config = '--tessdata-dir "/home/rvq/github/tesseract/tessdata/" --psm 10  --oem 2 '
    arr = Image.fromarray(img)
    result = pytesseract.image_to_string(arr) #config = tessdata_dir_config
    print(result)

# Inicializacion
camera = PiCamera()
camera.resolution = (640, 480)
camera.framerate = 32
rawCapture = PiRGBArray(camera, size=(640, 480))
time.sleep(0.1)	# Dejar que la camara inicialice todo
 
# capture frames from the camera
for frame in camera.capture_continuous(rawCapture, format="bgr", use_video_port=True):
	image = frame.array
	recognition(image)
	cv2.imshow("Frame", image)	# show the frame
	key = cv2.waitKey(1) & 0xFF
	rawCapture.truncate(0) 		# clear the stream in preparation for the next frame
	if key == ord("q"): 		# if the `q` key was pressed, break from the loop
		break
