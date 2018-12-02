import os
import glob
import sys
import numpy as np

if __name__ == "__main__":
    cwd = os.getcwd()
    slam_folder = "SLAM"
    merge_folder = "Merge"

    if sys.argv[1] == "SLAM":
        full_slam_fodler = os.path.join(cwd,slam_folder)
        computed = os.path.join(full_slam_fodler, "Computed")
        measured = os.path.join(full_slam_fodler, "Measured")

        ir_computed_matrix = []
        lidar_computed_matrix = []

        #Extraemos los valores computados
        for filename in os.listdir(computed):
            f = open(os.path.join(computed, filename),'r')
            current_result = []
            matrix = []
            if "LIDAR" in filename:
                matrix = lidar_computed_matrix
            elif ("IR" in filename):
                matrix = ir_computed_matrix

            for line in f:
                line_data = line.split('/')
                # dato en cm
                #print(line_data, os.path.join(computed, filename))
                current_result.append(float(line_data[1]))
            
            matrix.append(current_result)
        
        ir_measured_matrix = []
        lidar_measured_matrix = []
        ir_collisions = lidar_collisions = 0
        ir_green_detections = lidar_green_detections = ir_falsegreen_detections = lidar_falsegreen_detections = 0
        ir_marker_detections = lidar_marker_detections = ir_falsemarker_detections = lidar_falsemarker_detections = 0

        #Extraemos los valores medidos
        for filename in os.listdir(measured):
            f = open( os.path.join(measured, filename),'r')
            current_result = []
            green_detections = 0
            marker_detections = 0
            faslegreen_detections = 0
            faslemarker_detections = 0
            collisions = 0

            for line in f:
                line_data = line.split(' ')
                #Vemos si hubo deteccion de marca o verde
                end = False
                if "-V" in line:
                    end = True
                    faslegreen_detections += 1
                elif "V" in line:
                    end = True
                    green_detections += 1
                if "-M" in line:
                    end = True
                    faslemarker_detections += 1
                elif "M" in line:
                    end = True
                    marker_detections += 1
                if end:
                    continue
                # dato en cm
                dato = float(line_data[0])
                #Verificams si es colision
                if dato < 0:
                    collisions +=1
                current_result.append(abs(dato))
            
            matrix = []
            if "LIDAR" in filename:
                matrix = lidar_measured_matrix
                lidar_collisions += collisions
                lidar_falsegreen_detections += faslegreen_detections
                lidar_green_detections += green_detections
                lidar_falsemarker_detections += faslemarker_detections
                lidar_marker_detections += marker_detections
            elif ("IR" in filename):
                matrix = ir_measured_matrix
                ir_collisions += collisions
                ir_falsegreen_detections += faslegreen_detections
                ir_green_detections += green_detections
                ir_falsemarker_detections += faslemarker_detections
                ir_marker_detections += marker_detections
                
            matrix.append(current_result)

        print("IR Measured")
        print(ir_measured_matrix)
        print("LIDAR Measured")
        print(lidar_measured_matrix)

