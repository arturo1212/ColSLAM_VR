#include <Servo.h>
#include "MPU9250.h"

#define window 7
Servo myservo;
// Variables del MPU6050
MPU9250 mpu;

// Odometry variables

int getDistance()
{
  int8_t iter=0;
  float IRprom=0, LIDARprom=0; // Promedio de lecturas de distancia para el IR y el LIDAR dentro de la ventana window
  unsigned long pulseWidth = pulseIn(5, HIGH);    // Lectura del LIDAR.
  int val = 6787 / (analogRead(A0) - 3) - 4;

  while(iter < window ){
    int aux = analogRead(A0);
    if (aux == 7 ){
      aux++;
    }
    IRprom += 13 * pow(aux * 0.0048828125, -1) * 2;
    LIDARprom += pulseIn(5, HIGH)/10;
    iter++;
  }
  LIDARprom = LIDARprom/window;
  
  IRprom = IRprom/window;
  Serial.println("IR: " + String(IRprom));
  if(IRprom > 35 ){
    LIDARprom = LIDARprom - 15;
    if (LIDARprom > 85 ){
      Serial.println("LIDAR: " + String(LIDARprom+5));
        return LIDARprom + 5;
      }else {
        Serial.println("LIDAR: " + String(LIDARprom));
        return LIDARprom;
      }
   } 
   else {
    return IRprom;
   }
}

float Remap(float value, float from1, float to1, float from2, float to2)
    {
        if (value == 0) return (int)value;
        return ((value - from1) / (to1 - from1) * (to2 - from2) + from2);
    }

void setup() {
  Serial.begin(115200); // Start serial communications
  
  pinMode(6, OUTPUT); // Set pin 6 as trigger pin LIDAR
  digitalWrite(6, LOW); // Set trigger LOW for continuous read LIDAR
  pinMode(5, INPUT); // Set pin 5 as monitor pin LIDAR
  
}

void loop() {
    getDistance();
  //char copy[15];
  //Serial.println(String(getDistance()));
}
