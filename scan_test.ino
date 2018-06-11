#include <Servo.h>
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
#include <ros.h>
#include <std_msgs/String.h>
#define window 7

#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    #include "Wire.h"
#endif
Servo myservo;
// Variables del MPU6050
MPU6050 mpu;

// MPU control/status vars
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer

// orientation/motion vars
Quaternion q;           // [w, x, y, z]         quaternion container
VectorFloat gravity;    // [x, y, z]            gravity vector
float ypr[3];           // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector

volatile bool mpuInterrupt = false;     // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
    mpuInterrupt = true;
}

void Get_yaw(){
    if (!dmpReady) return;
    mpuInterrupt = false;
    mpuIntStatus = mpu.getIntStatus();
    fifoCount = mpu.getFIFOCount();
    if ((mpuIntStatus & 0x10) || fifoCount == 1024) {
        mpu.resetFIFO();

    } else if (mpuIntStatus & 0x02) {
        while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();
        mpu.getFIFOBytes(fifoBuffer, packetSize);
        fifoCount -= packetSize;
        mpu.dmpGetQuaternion(&q, fifoBuffer);
        mpu.dmpGetGravity(&gravity, &q);
        mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
    }
}

int getDistance()
{
  int8_t iter=0;
  float IRprom=0, LIDARprom=0; // Promedio de lecturas de distancia para el IR y el LIDAR dentro de la ventana window
  // unsigned long pulseWidth = pulseIn(5, HIGH);    // Lectura del LIDAR.
  // int val = 6787 / (analogRead(A0) - 3)) - 4;

  while(iter < window ){
    int aux = analogRead(A0);
    if (aux ==3){
      aux++;
    }
    IRprom += 6787 / (aux - 3) - 4;
    LIDARprom += pulseIn(5, HIGH)/10;
    iter++;
  }
  LIDARprom = LIDARprom/window;
  IRprom = IRprom/window;
  if(IRprom > 35 ){
    LIDARprom = LIDARprom - 15;
    if (LIDARprom > 85 ){
        return LIDARprom + 5;
      }else {
        return LIDARprom;
      }
   } else {
    return IRprom;
   }
}

void setup() {
  Serial.begin(57600); // Start serial communications
  myservo.attach(9);
  pinMode(6, OUTPUT); // Set pin 2 as trigger pin
  digitalWrite(6, LOW); // Set trigger LOW for continuous read
  pinMode(5, INPUT); // Set pin 3 as monitor pin
  #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
        Wire.begin();
        TWBR = 24; // 400kHz I2C clock (200kHz if CPU is 8MHz)
    #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
        Fastwire::setup(400, true);
    #endif
    mpu.initialize();
    devStatus = mpu.dmpInitialize();
    if (devStatus == 0) {
        mpu.setDMPEnabled(true);
        attachInterrupt(digitalPinToInterrupt(2), dmpDataReady, RISING);
        mpuIntStatus = mpu.getIntStatus();
        dmpReady = true;
        packetSize = mpu.dmpGetFIFOPacketSize();
    }
  myservo.write(0);
  delay(1000);
}

void loop() {
  char copy[15];
  
  for(int pos = 0; pos <= 180; pos += 1)
  {
    myservo.write(pos);
    //delay(10);
    Get_yaw();
    Serial.println(String(ypr[0]*180/M_PI,4)+ "," + String(getDistance()) + "," + String(180-pos));
  }
  for(int pos = 180; pos>=0; pos-=1)
  {
    myservo.write(pos);
    //delay(10);
    Get_yaw();
    Serial.println(String(ypr[0]*180/M_PI,4)+ "," + String(getDistance()) + "," + String(180-pos));
  }
}
