import os
import glob
import sys
import math
import numpy as np

def calculate_stats(computed, measured):
    
    sqr_diffs = []
    diffs = []
    for c,m in zip(computed, measured):
        diff = abs(m - c)
        sqrdiff = (c-m)**2
        sqrdiff = sum(sqrdiff)/sqrdiff.size
        diff = sum(diff)/diff.size
        sqr_diffs.append(sqrdiff)
        diffs.append(diff)
    return diffs, sqr_diffs

def magnitude(tuple_vector):
    return math.sqrt(tuple_vector[0]**2 + tuple_vector[1]**2)

def tuple_diff(t1,t2):
    return (t1[0]-t2[0],t1[1]-t2[1])

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
            
            matrix.append(np.array(current_result))
        
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
                
            matrix.append(np.array(current_result))
        
        ir_diffs, ir_sqr_diffs = calculate_stats(ir_computed_matrix, ir_measured_matrix)
        lidar_diffs, lidar_sqr_diffs = calculate_stats(lidar_computed_matrix, lidar_measured_matrix)

        #Diferencia media
        ir_mean_diff = np.mean(ir_diffs)
        lidar_mean_diff = np.mean(lidar_diffs)
        #Diferencia cuadrada media
        ir_mean_sqr_diff =  np.mean(ir_sqr_diffs)
        lidar_mean_sqr_diff = np.mean(lidar_sqr_diffs)
        #Desviacion estandar de la diferencia media
        ir_std_diff = np.std(ir_diffs, ddof=1)
        lidar_std_diff = np.std(lidar_diffs, ddof=1)
        #Desviacion estandar de la diferencia cuadrada media
        ir_std_sqr_diff = np.std(ir_sqr_diffs, ddof=1)
        lidar_std_sqr_diff = np.std(lidar_sqr_diffs,ddof=1)

        print("IR----------")
        print("Diferencia cuadrada media: " + str(ir_mean_sqr_diff))
        print("Std: " + str(ir_std_sqr_diff))
        print("Diferencia media: " + str(ir_mean_diff))
        print("Std: " + str(ir_std_diff))
        print("Choques por corrida: " + str(ir_collisions/len(ir_sqr_diffs)))
        print("Porcentaje de verde: " + str(ir_green_detections/len(ir_sqr_diffs)))
        print("Porcentaje de marcador: " + str(ir_marker_detections/len(ir_sqr_diffs)))
        print("FALTAN FALSOS")
        print("Errores por corrida:")

        print(ir_sqr_diffs)
        print(ir_diffs)

        print("LIDAR----------")
        print("Diferencia cuadrada media: " + str(lidar_mean_sqr_diff))
        print("Std: " + str(lidar_std_sqr_diff))
        print("Diferencia media: " + str(lidar_mean_diff))
        print("Std: " + str(lidar_std_diff))
        print("Choques por corrida: " + str(lidar_collisions/len(lidar_sqr_diffs)))
        print("Porcentaje de verde: " + str(lidar_green_detections/len(lidar_sqr_diffs)))
        print("Porcentaje de marcador: " + str(lidar_marker_detections/len(lidar_sqr_diffs)))
        print("FALTAN FALSOS")
        print("Errores por corrida:")

        print(lidar_sqr_diffs)
        print(lidar_diffs)
        
    elif sys.argv[1] == "Merge":
        full_merge_fodler = os.path.join(cwd,merge_folder)
        data_file = os.path.join(full_merge_fodler, "data.txt")

        #Leemos data medida y computda
        f = open(data_file,'r')
        lines = f.readlines()
        errors = []
        for i in range (4,len(lines),3):
            init_pose = lines[i].strip().split(',')
            end_pose = lines[i+1].strip().split(',')
            measured_dist_between = int(lines[i+2])

            init_pose = (float(init_pose[0]),float(init_pose[1]))
            end_pose = (float(end_pose[0]),float(end_pose[1]))
            
            displacement_vector = tuple_diff(end_pose, init_pose)
            calculated_dist_between = magnitude(displacement_vector)*100

            error = measured_dist_between - calculated_dist_between
            errors.append(abs(error))

        print("MERGE")
        print("Error promedio: " + str(np.mean(np.array(errors))))
        print("Errores:")
        print(errors)

