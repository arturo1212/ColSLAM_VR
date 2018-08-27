import serial
import rospy
from std_msgs.msg import String
import sys
import time

ser = serial.Serial('/dev/ttyACM0', 57600)
pub = rospy.Publisher('arduino', String, queue_size=1)
rospy.init_node('arduino_talker', anonymous=True)
rate = rospy.Rate(60) # 60hz
gyro_values = []
start = time.time()
while not rospy.is_shutdown():
        result = str(ser.readline().strip())    # read up to one hundred bytes
        splitted = result.split(',')
        gyro_reading = float(splitted[0])
        gyro_values.append(gyro_reading)
        end = time.time()
        if end - start >= 120: # En secs
                print("#DIFF: ",gyro_values[0],gyro_reading)
                gyro_values.clear()
                start = time.time()
        pub.publish(result)
        rate.sleep()