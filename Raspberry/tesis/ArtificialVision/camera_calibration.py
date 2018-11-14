import numpy as np
import cv2
import glob
import picamera
import picamera.array

# termination criteria
def getCameraMatrix(fname, DEBUG=False):
    criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)
    # prepare object points, like (0,0,0), (1,0,0), (2,0,0) ....,(6,5,0)
    objp = np.zeros((7*7,3), np.float32)
    objp[:,:2] = np.mgrid[0:7,0:7].T.reshape(-1,2)
    objpoints = [] # 3d point in real world space
    imgpoints = [] # 2d points in image plane.
    img = cv2.imread(fname)
    #img = cv2.resize(img, (int(width), int(height)), interpolation = cv2.INTER_CUBIC)
    gray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)
    ret, corners = cv2.findChessboardCorners(gray, (7,7),None) # Hallar esquinas del chessboard
    
    # If found, add object points, image points (after refining them)
    if ret == True:
        objpoints.append(objp)
        corners2 = cv2.cornerSubPix(gray,corners,(11,11),(-1,-1),criteria)
        imgpoints.append(corners2)

        ret, mtx, dist, rvecs, tvecs = cv2.calibrateCamera(objpoints, imgpoints, gray.shape[::-1],None,None)
        if(DEBUG):
            # Draw and display the corners
            img = cv2.drawChessboardCorners(img, (7,7), corners2,ret)
            print(mtx)
            cv2.imshow('img',img)
            cv2.waitKey(5000)
            cv2.destroyAllWindows()
        return mtx
    else:
        print("Calibration error", corners.size())

def best_camera_config(camera):
        camera.resolution = (640, 480)
        camera.awb_mode = 'off'
        # Valores iniciales
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
        camera.awb_gains = (rg, bg)
        return camera 
