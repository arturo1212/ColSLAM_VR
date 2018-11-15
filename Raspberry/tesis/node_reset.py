#!/usr/bin/env python
import rospy
from std_msgs.msg import String
def callback():
    pass    

def listener():
    rospy.init_node('resetN', anonymous=True)
    rospy.Subscriber("reset", String, callback)
    rospy.spin()

if __name__ == '__main__':
    listener()
