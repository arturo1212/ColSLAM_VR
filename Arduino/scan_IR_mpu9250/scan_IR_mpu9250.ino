#include <Servo.h>
#include "MPU9250.h"
#define window 7
Servo myservo;
// Variables del MPU6050
MPU9250 mpu;

// Odometry variables
unsigned long start;
int pinFeedback_left  = A1;
int pinFeedback_right = A2;
int tCycle;
int valA    = 85;
float odom1, odom2;
float tHigh;
float tLow;
float dc;
float angle;    //Measured angle from feedback
float dcMin = 2.9;  //From Parallax spec sheet
float dcMax = 97.1; //From Parallax spec sheet

float mido_angulo(int pin) {
  while(1) {  //From Parallax spec sheet
    tHigh  = pulseIn(pin, HIGH);
    tLow   = pulseIn(pin, LOW);
    tCycle = tHigh + tLow;

    if ( tCycle > 1000 && tCycle < 1200) {
      break; //valid tCycle;
    }
  }
  dc = (100 * tHigh) / tCycle; //From Parallax spec sheet, you are trying to determine the percentage of the HIGH in the pulse
  angle = ((dc - dcMin) * 360) / (dcMax - dcMin + 1); //From Parallax spec sheet
/*if (angle < 0.0) {
    angle = 0.0;
  }
  else 
  if (angle > 359.0) {
    angle = 359.0;
  }*/
  return angle;
}

int getDistance()
{
  int8_t iter=0;
  float IRprom=0, LIDARprom=0; // Promedio de lecturas de distancia para el IR y el LIDAR dentro de la ventana window
  // unsigned long pulseWidth = pulseIn(5, HIGH);    // Lectura del LIDAR.
  //int val = 6787 / (analogRead(A0) - 3) - 4;

  while(iter < window ){
    int aux = analogRead(A0);
    /*if (aux == 7 ){
      aux++;
    }*/
    IRprom += 13 * pow(aux * 0.0048828125, -1) * 2;
    iter++;
  }
  IRprom = IRprom/window;
  //Serial.println("IR: " + String(IRprom));
  if(IRprom > 30 ){
    IRprom+=5;  
  }
  return IRprom;
}

float inline mapto360(float rotation){
  return rotation < 0 ? 360 + rotation : rotation;
}

float Remap(float value, float from1, float to1, float from2, float to2)
    {
        if (value == 0) return (int)value;
        return ((value - from1) / (to1 - from1) * (to2 - from2) + from2);
    }

void setup() {
  Serial.begin(115200); // Start serial communications
  myservo.attach(9);
  pinMode(pinFeedback_left, INPUT);
  pinMode(pinFeedback_right, INPUT);

  Wire.begin();
  myservo.write(20);
  delay(2000);
  mpu.setup();
  delay(5000);

  // GIA
  float accbias[] = {-114.38, 19.04, 30.94};
  float gyrobias[] = {-0.08, -0.47, 0.36};
  float magbias[] = {188.11, 293.93, -198.02};
  float magscale[] = {1.04, 1.05, 0.92};

  for(int i=0;i<3;i++){
    mpu.setAccBias(i,accbias[i]*0.001);
    mpu.setGyroBias(i,gyrobias[i]);
    mpu.setMagBias(i,magbias[i]);
    mpu.setMagScale(i,magscale[i]);
  }
  
  /* ULTIMOS BUENOS
  < calibration parameters >
  accel bias [g]:
  -114.38, 19.04, 30.94
  gyro bias [deg/s]:
  -0.08, -0.47, 0.36
  mag bias [mG]:
  188.11, 293.93, -198.02
  mag scale []:
  1.04, 1.05, 0.92
  */

  //mpu.calibrateAccelGyro();
  //mpu.calibrateMag();

  //mpu.printCalibration();

}

void loop() {
  //char copy[15];
  //Serial.println(String(getDistance()));
  
  for(int pos = 20; pos <= 180; pos += 1)
  { 
    mpu.update();
    //Serial.println("Entro");
    odom1 = mido_angulo(pinFeedback_left);
    odom2 = mido_angulo(pinFeedback_right);
    //Serial.println("Angulos");
    myservo.write(pos);
    Serial.println(String(mpu.getYaw())+ "," + String(getDistance()) + "," + String(Remap(pos,20,180,0,180))+","+String(odom1)+","+String(odom2));
    delay(65);
  }
  //Serial.println("Result: "+String(getDistance()));
  for(int pos = 180; pos>=20; pos-=1)
  {
    mpu.update();
    odom1 = mido_angulo(pinFeedback_left);
    odom2 = mido_angulo(pinFeedback_right);
    myservo.write(pos);
    //delay(10);
    Serial.println(String(mpu.getYaw())+ "," + String(getDistance()) + "," + String(Remap(pos,20,180,0,180))+","+String(odom1)+","+String(odom2));
    delay(65);
  }
}
