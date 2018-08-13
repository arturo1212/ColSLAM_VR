#include <Servo.h>
#define window 7

Servo myservo;
// Variables del MPU6050

// Odometry variables
unsigned long start;

int getDistance()
{
  int8_t iter=0;
  float IRprom=0, LIDARprom=0; // Promedio de lecturas de distancia para el IR y el LIDAR dentro de la ventana window
  // unsigned long pulseWidth = pulseIn(5, HIGH);    // Lectura del LIDAR.
  //int val = 6787 / (analogRead(A0) - 3) - 4;

  while(iter < window ){
    int aux = analogRead(A0);
    if (aux == 7 ){
      aux++;
    }
    //IRprom += 6787 / (aux - 3) - 4;
    IRprom += 13 * pow(aux * 0.0048828125, -1) * 2;
    iter++;
  }
  IRprom = IRprom/window;
    return IRprom;
}

void setup() {
  Serial.begin(57600); // Start serial communications
  myservo.attach(9);
  myservo.write(0);
  delay(1000);
}

void loop() {
  char copy[15];
  //Serial.println(String(getDistance()));
  for(int pos = 0; pos <= 180; pos += 1)
  {
    myservo.write(pos);
    Serial.println( String(getDistance()) + "," + String(pos));
  }
  for(int pos = 180; pos>=0; pos-=1)
  {
    myservo.write(pos);
    //delay(10);
    Serial.println(String(getDistance()) + "," + String(pos));
  }
}
