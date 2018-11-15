#!/usr/bin/env python
import rospy
from std_msgs.msg import String
def callback():
    pass    
    
def listener():
    rospy.init_node('streamN', anonymous=True)
    rospy.Subscriber("stream", String, callback)
    rospy.spin()

if __name__ == '__main__':
    listener()
