import sys
import select
import tty
import termios
import pigpio

pi = pigpio.pi()
# Servo Parallax PWM: 1280–1720
SERVOr = 17  # Modificar PINES PARA GPIO
SERVOl = 27
pi.set_PWM_frequency(SERVOl, 50)
pi.set_PWM_frequency(SERVOr, 50)
print(pi.get_PWM_frequency(SERVOr))

def forward(sr, sl):
    pi.set_servo_pulsewidth(sl, 1720)
    pi.set_servo_pulsewidth(sr, 1280)

def backward(sr, sl):
    pi.set_servo_pulsewidth(sr, 1720)
    pi.set_servo_pulsewidth(sl, 1280)

def stop(sr, sl):
    pi.set_servo_pulsewidth(sr, 0)
    pi.set_servo_pulsewidth(sl, 0)

def turn_left(sr, sl):
    pi.set_servo_pulsewidth(SERVOr, 1280)
    pi.set_servo_pulsewidth(SERVOl, 1280)

def turn_right(sr, sl):
    pi.set_servo_pulsewidth(SERVOr, 1720)
    pi.set_servo_pulsewidth(SERVOl, 1720)
    
def isData():
    return select.select([sys.stdin], [], [], 0.05) == ([sys.stdin], [], [])

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
                stop(SERVOr, SERVOl)
                break
            if(c == "w"):
                forward(SERVOl, SERVOr)
            elif(c == "a"):
                turn_left(SERVOr, SERVOl)
            elif(c == "s"):
                backward(SERVOl, SERVOr)
            elif(c == "d"):
                turn_right(SERVOr, SERVOl)
            print("KEY", c)
            continue
        stop(SERVOl, SERVOr)
finally:
    termios.tcsetattr(sys.stdin, termios.TCSADRAIN, old_settings)
