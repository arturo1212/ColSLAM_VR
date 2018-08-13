import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)

GPIO.setup(17, GPIO.OUT)
GPIO.setup(27, GPIO.OUT)

l = GPIO.PWM(17, 50)
r = GPIO.PWM(27, 50)

r.start(6.2)
l.start(8)
try:
    while True:
        dc = float(input())
        l.ChangeDutyCycle(dc)
        """
        p.ChangeDutyCycle(7.5)  # turn towards 90 degree
        time.sleep(1) # sleep 1 second
        p.ChangeDutyCycle(2.5)  # turn towards 0 degree
        time.sleep(1) # sleep 1 second
        p.ChangeDutyCycle(12.5) # turn towards 180 degree
        time.sleep(1) # sleep 1 second"""
except KeyboardInterrupt:
    r.stop()
    l.stop()
    GPIO.cleanup()
