import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)

GPIO.setup(17, GPIO.OUT)
GPIO.setup(27, GPIO.OUT)

r = GPIO.PWM(17, 50)
l = GPIO.PWM(27, 50)

r.start(1)
l.start(1)
try:
    while True:
        pass
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