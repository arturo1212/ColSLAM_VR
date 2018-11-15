#!/usr/bin/env python
import rospy
from std_msgs.msg import String
    
def listener():
    rospy.init_node('reset', anonymous=True)
    rospy.spin()

if __name__ == '__main__':
    listener()
