#include <Servo.h>

Servo claw_servo, rotation_servo;  // create servo object to control a servo
String action;
int angle_open = 175, angle_close = 140;

void setup() {
  Serial.begin(57600); // opens serial port, sets data rate to 57600 bps
  claw_servo.attach(9);
  claw_servo.attach(10);
  claw_servo.write(angle_close);
  delay(15); 
}

void loop() {
  while(Serial.available()) {
    action = Serial.readString();// read the incoming data as string
    
    int action_number = 0;
    if(action_number == 1){
      claw_servo.write(angle_open);
      delay(15); 
    }
    else if(action_number == 0){
      claw_servo.write(angle_close);
      delay(15); 
    }
    else if(action_number <= 180 && action_number >= 2){
      rotation_servo.write(action_number);
    }
    Serial.println(action);
  }
  
}
