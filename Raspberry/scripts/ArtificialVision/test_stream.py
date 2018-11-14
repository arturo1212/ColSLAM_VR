import picamera
import picamera.array
import numpy as np
import cv2

def white_balance(camera):
        camera.resolution = (640, 480)
        camera.awb_mode = 'off'
        # Start off with ridiculously low gains
        rg, bg = (0.5, 0.5)
        camera.awb_gains = (rg, bg)
        with picamera.array.PiRGBArray(camera, size=(128, 72)) as output:
            # Allow 30 attempts to fix AWB
            for i in range(30):
                # Capture a tiny resized image in RGB format, and extract the
                # average R, G, and B values
                camera.capture(output, format='rgb', resize=(128, 72), use_video_port=True)
                r, g, b = (np.mean(output.array[..., i]) for i in range(3))
                print('R:%5.2f, B:%5.2f = (%5.2f, %5.2f, %5.2f)' % (
                    rg, bg, r, g, b))
                # Adjust R and B relative to G, but only if they're significantly
                # different (delta +/- 2)
                if abs(r - g) > 2:
                    if r > g:
                        rg -= 0.1
                    else:
                        rg += 0.1
                if abs(b - g) > 1:
                    if b > g:
                        bg -= 0.1
                    else:
                       bg += 0.1
                camera.awb_gains = (rg, bg)
                output.seek(0)
                output.truncate()
            return (rg, bg)

def color_filter(frame, lower, upper):
    mask = cv2.inRange(frame, lower , upper);
    return mask
    
    
    
img_counter = 0
with picamera.PiCamera() as camera:
    camera.awb_gains = white_balance(camera)
    rawCapture = picamera.array.PiRGBArray(camera, size=(640, 480))
    for frame in camera.capture_continuous(rawCapture, format="bgr", use_video_port=True):
        # Imagenes
        image = frame.array
        image_hsv = hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)
        
        # Obtener imagen del centro
        w, h = np.size(image, 1), np.size(image, 0) 
        ancho = 50
        crop_img = image_hsv[0:h, w/2-ancho:w/2+ancho]
        
        # Filtrar por color
        sensitivity = 30
        lower = np.array([60 - sensitivity, 100, 60])
        upper = np.array([60 + sensitivity, 255, 255])
        green_filter = color_filter(crop_img, lower, upper)
        
        # Filtrar contornos por tamano
        im2, conts, h = cv2.findContours(green_filter,cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_NONE)
        for c in conts:
            if cv2.contourArea(c) > 300:
                cv2.drawContours(crop_img, [c], -1, 0, -1)
                print("ENCONTRADO", cv2.contourArea(c))
              
        # Mostrar imagenes
        cv2.imshow("Frame", image)
        cv2.imshow("Centro", crop_img)
        cv2.imshow("Filtro verde", green_filter)

        
        k = cv2.waitKey(1)
        if k%256 == 27:
            # ESC pressed
            print("Escape hit, closing...")
            break
        elif k%256 == 32:
            # SPACE pressed
            img_name = "opencv_frame_{}.png".format(img_counter)
            cv2.imwrite(img_name, image)
            print("{} written!".format(img_name))
            img_counter += 1
        # clear the stream in preparation for the next frame
        rawCapture.truncate(0)


