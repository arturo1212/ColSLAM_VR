#!/usr/bin/env python
import rospy
from std_msgs.msg import String
import sys
import select
import tty
import termios
import pigpio

pi = pigpio.pi()
SERVOr = 17
SERVOl = 27

pi.set_PWM_frequency(SERVOl, 50)
pi.set_PWM_frequency(SERVOr, 50)
def callback(data):
    msg = data.data
    rospy.loginfo(rospy.get_caller_id() + "I heard %s", msg)
    info  = msg.split(",")
    pwm_r = info[0]    
    pwm_l = info[1]
    pi.set_servo_pulsewidth(SERVOr, int(pwm_r))
    pi.set_servo_pulsewidth(SERVOl, int(pwm_l))
    
def listener():
    rospy.init_node('muevemeste')
    rospy.Subscriber("movement", String, callback)
    rospy.spin()

if __name__ == '__main__':
    listener()
