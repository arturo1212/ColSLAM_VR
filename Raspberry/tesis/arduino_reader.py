import serial
import rospy
from std_msgs.msg import String
import sys
import time
ser = serial.Serial('/dev/ttyACM0', 115200)

def callback(data)
	msg = data.data
    rospy.loginfo(rospy.get_caller_id() + "I claw %s", msg)
    action = int(msg)    
    ser.write(msg+"\n")

pub = rospy.Publisher('arduino', String, queue_size=1)
rospy.init_node('arduino_talker', anonymous=True)
rate = rospy.Rate(60) # 60hz
start = time.time()
rospy.init_node('mirameste')
rospy.Subscriber("claw", String, callback)
while not rospy.is_shutdown():
        result = str(ser.readline().strip())    # read up to one hundred bytes
        #splitted = result.decode('utf-8').split(',')
        #sys.stderr.write(splitted[0]+"\n")
        #end = time.time()
        pub.publish(result)
        rate.sleep()