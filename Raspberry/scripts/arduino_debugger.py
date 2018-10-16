import serial
import sys
import time

ser = serial.Serial('/dev/ttyACM0', 115200)
start = time.time()
while True:
        result = str(ser.readline().strip())    # read up to one hundred bytes
        print(result)
        #splitted = result.decode('utf-8').split(',')
        #sys.stderr.write(splitted[0]+"\n")
        #end = time.time()
