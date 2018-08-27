import serial
import sys
import time
import matplotlib.pyplot as plt

ser = serial.Serial('/dev/ttyACM0', 57600)
gyro_values = []
start = time.time()
while True:
        result = str(ser.readline().strip())    # read up to one hundred bytes
        splitted = result.decode('utf-8').split(',')
        gyro_reading = float(splitted[0])
        gyro_values.append(gyro_reading)
        end = time.time()
        if end - start >= 1800: # En secs
                print("#DIFF: ",gyro_values[0], gyro_reading)
                plt.figure(figsize=(10, 5))
                plt.plot(gyro_values,'b')
                plt.title("Valores del gyro")
                plt.savefig("gyro.png")
                gyro_values.clear()
                start = time.time()
                