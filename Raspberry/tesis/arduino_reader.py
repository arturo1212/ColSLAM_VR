import serial
import rospy
from std_msgs.msg import String
import sys

ser = serial.Serial('/dev/ttyACM0', 57600)
pub = rospy.Publisher('arduino', String, queue_size=1)
rospy.init_node('arduino_talker', anonymous=True)
rate = rospy.Rate(60) # 60hz
while not rospy.is_shutdown():
        try:
                result = str(ser.readline().strip())    # read up to one hundred bytes
                pub.publish(result)
                rate.sleep()
        except:
                print("Arduino disconnected")