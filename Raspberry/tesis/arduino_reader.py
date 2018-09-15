import serial
import rospy
from std_msgs.msg import String
import sys
import time

ser = serial.Serial('/dev/ttyACM0', 57600)
pub = rospy.Publisher('arduino', String, queue_size=1)
rospy.init_node('arduino_talker', anonymous=True)
rate = rospy.Rate(60) # 60hz
start = time.time()
while not rospy.is_shutdown():
        result = str(ser.readline().strip())    # read up to one hundred bytes
        #splitted = result.decode('utf-8').split(',')
        #sys.stderr.write(splitted[0]+"\n")
        end = time.time()
        pub.publish(result)
        rate.sleep()
