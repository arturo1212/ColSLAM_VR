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

  /*// GIA
  float accbias[] = {-10.13, 14.22, 37.23};
  float gyrobias[] = {-0.11, -0.55, 0.43};
  float magbias[] = {141.97, 335.15, -229.02};
  float magscale[] = {1.00, 1.07, 0.94};
  */
  
  // David 2
  float accbias[] = {-105.59, 17.94, 29.85};
  float gyrobias[] = {-0.07, -0.57, 0.40};
  float magbias[] = {191.66, 283.17, -194.58};
  float magscale[] = {0.98, 0.99, 1.04};

  /*// Casa David
  float accbias[] = {-134.83, -29.91, 33.02};
  float gyrobias[] = {-0.42, 2.45, -0.50};
  float magbias[] = {124.75, 95.20, -311.45};
  float magscale[] = {1.09, 1.09, 0.86};
  */
  
  /* GIA mesa
  float accbias[] = {-61.71, 4.82, 16.11};
  float gyrobias[] = {-4.59, -0.35, 2.70};
  float magbias[] = {-40.68, 308.03, -92.67};
  float magscale[] = {0.99, 1.05, 0.96}; 
    */
  

  for(int i=0;i<3;i++){
    mpu.setAccBias(i,accbias[i]*0.001);
    mpu.setGyroBias(i,gyrobias[i]);
    mpu.setMagBias(i,magbias[i]);
    mpu.setMagScale(i,magscale[i]);
  }
  
  /*
    *< calibration parameters > Valores en el GIA (suelto en la mesa)
    accel bias [g]: 
    -61.71, 4.82, 16.11
    gyro bias [deg/s]: 
    -4.59, -0.35, 2.70
    mag bias [mG]: 
    -40.68, 308.03, -92.67
    mag scale []: 
    0.99, 1.05, 0.96
    
    Valores en casa de David (montado en robot)
    < calibration parameters >
    accel bias [g]:
    -105.59, 17.94, 29.85
    gyro bias [deg/s]:
    -0.07, -0.57, 0.40
    mag bias [mG]:
    191.66, 283.17, -194.58
    mag scale []:
    0.98, 0.99, 1.04

    < calibration parameters > Calibracion casa de David inalambrica
    accel bias [g]:
    -148.13, 34.48, 26.06
    gyro bias [deg/s]:
    -0.10, -0.50, 0.34
    mag bias [mG]:
    243.13, 354.86, -260.02
    mag scale []:
    1.07, 1.08, 0.87

  < calibration parameters > ULTIMOS BUENOS
    accel bias [g]: 
    -115.05, 59.14, -7.39
    gyro bias [deg/s]: 
    0.58, 1.07, 0.35
    mag bias [mG]: 
    35.38, 211.88, -372.39
    mag scale []: 
    1.00, 1.17, 0.87
    
    < calibration parameters > Calibracion en plano
    accel bias [g]: 
    -113.59, 53.41, -7.26
    gyro bias [deg/s]: 
    0.51, 1.03, 0.31
    mag bias [mG]: 
    12.38, 194.08, -233.39
    mag scale []: 
    0.81, 0.86, 1.65

    < calibration parameters > Calibracion IR
    accel bias [g]:
    -10.13, 14.22, 37.23
    gyro bias [deg/s]:
    -0.11, -0.55, 0.43
    mag bias [mG]:
    141.97, 335.15, -229.02
    mag scale []:
    1.00, 1.07, 0.94

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
