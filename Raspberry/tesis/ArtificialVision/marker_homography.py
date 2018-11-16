import numpy as np
import cv2
from camera_calibration import getCameraMatrix
import math

def takeSecond(elem):
    return elem[1]

def RMatrixToEuler(R):
    """ Transforma la matriz de rotacion en angulos de Euler """
    sy = math.sqrt(R[0][0] * R[0][0] +  R[1][0] * R[1][0])
    singular = sy < 1e-6
    if  not singular :
        x = math.atan2(R[2,1] , R[2,2])
        y = math.atan2(-R[2,0], sy)
        z = math.atan2(R[1,0], R[0,0])
    else :
        x = math.atan2(-R[1,2], R[1,1])
        y = math.atan2(-R[2,0], sy)
        z = 0
    return [x, y, z]

def getHomography(frame, reference, MIN_MATCH_COUNT, DEBUG=False):
    FLANN_INDEX_KDTREE = 0
    sift = cv2.xfeatures2d.SIFT_create()
    kp1, des1 = sift.detectAndCompute(reference,None)
    kp2, des2 = sift.detectAndCompute(frame,None)
    index_params = dict(algorithm = FLANN_INDEX_KDTREE, trees = 5)
    search_params = dict(checks = 50)
    flann = cv2.FlannBasedMatcher(index_params, search_params)
    matches = flann.knnMatch(des1,des2,k=2)

    good = []
    for m,n in matches:
        if m.distance < 0.7*n.distance:
            good.append(m)

    if len(good)> MIN_MATCH_COUNT:
        src_pts = np.float32([ kp1[m.queryIdx].pt for m in good ]).reshape(-1,1,2)
        dst_pts = np.float32([ kp2[m.trainIdx].pt for m in good ]).reshape(-1,1,2)
        M, mask = cv2.findHomography(src_pts, dst_pts, cv2.RANSAC, 5.0)
    else:
        print("No hay suficientes coincidencias - %d/%d" % (len(good), MIN_MATCH_COUNT))
        return None, len(good)

    if(DEBUG):
        matchesMask = mask.ravel().tolist()
        h,w = reference.shape
        pts = np.float32([ [0,0],[0,h-1],[w-1,h-1],[w-1,0] ]).reshape(-1,1,2)
        dst = cv2.perspectiveTransform(pts,M)
        frame = cv2.polylines(frame,[np.int32(dst)],True,255,3, cv2.LINE_AA)
        draw_params = dict(matchColor = (0,255,0), # draw matches in green color
                       singlePointColor = None,
                       matchesMask = matchesMask, # draw only inliers
                       flags = 2)
        l = list(np.int32(dst))
        lista = []
        for i in l:
            point = (i[0][0],i[0][1])
            lista.append(point)

        lista.sort(key=takeSecond)
        img3 = cv2.drawMatches(reference,kp1,frame,kp2,good,None,**draw_params)
        # Vector normal
        v1 = np.append(dst[0][0], [0]) - np.append(dst[1][0], [0])
        v2 = np.append(dst[1][0], [0]) - np.append(dst[2][0], [0])
        #cv2.imwrite("hola.jpg", img3)
    return M, len(good)

def getDRotation(K, M):
    num, Rs, Ts, Ns  = cv2.decomposeHomographyMat(M, K)
    R1, R2 = Rs[0], Rs[2]
    lista = [RMatrixToEuler(R1)[1], RMatrixToEuler(R2)[1]]
    return lista

def test():
    frame = cv2.imread('opencv_frame_2.png',0)      # trainImage    
    reference = cv2.imread('opencv_frame_1.png',0)  # queryImage
    K = getCameraMatrix("chessboard.png")
    M = getHomography(frame, reference, True)      
    ys = getDRotation(K, M)
    print(ys)

#test()

