import sys
import select
import tty
import termios
import pigpio

pi = pigpio.pi()
# Servo Parallax PWM: 1280–1720
SERVOr = 17  # Modificar PINES PARA GPIO
SERVOl = 27

def isData():
    return select.select([sys.stdin], [], [], 0.25) == ([sys.stdin], [], [])

old_settings = termios.tcgetattr(sys.stdin)
try:
    tty.setcbreak(sys.stdin.fileno())

    i = 0
    while 1:
        #print(i)
        i += 1
        if isData():
            c = sys.stdin.read(1)
            if c == 'p':         # x1b is ESC
                pi.set_servo_pulsewidth(SERVOr, 0)
                pi.set_servo_pulsewidth(SERVOl, 0)
                break
            if(c == "w"):
                pi.set_servo_pulsewidth(SERVOr, 1280)
                pi.set_servo_pulsewidth(SERVOl, 1720)
            elif(c == "s"):
                pi.set_servo_pulsewidth(SERVOr, 1720)
                pi.set_servo_pulsewidth(SERVOl, 1280)
            elif(c == "a"):
                pi.set_servo_pulsewidth(SERVOr, 1720)
                pi.set_servo_pulsewidth(SERVOl, 1720)
            elif(c == "d"):
                pi.set_servo_pulsewidth(SERVOr, 1280)
                pi.set_servo_pulsewidth(SERVOl, 1280)
            print("KEY", c)
            continue
        pi.set_servo_pulsewidth(SERVOr, 0)
        pi.set_servo_pulsewidth(SERVOl, 0) 
finally:
    termios.tcsetattr(sys.stdin, termios.TCSADRAIN, old_settings)
