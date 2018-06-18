#include <Servo.h>            //Servo library

//PID angle control for Parallax 360 
Servo servo_test;        //initialize a servo object for the connected servo  

#define DEMORA    5000  // 5000 mseg
unsigned long start;
int pinFeedback   = A0;
                
float target_angle= 220.0;
float tHigh       = 0;
float tLow        = 0;
int tCycle        = 0;
int valA = 85;
float dc          = 0;
float angle       = 0; //Measured angle from feedback
float dcMin       = 2.9; //From Parallax spec sheet
float dcMax       = 97.1; //From Parallax spec sheet
float Kp          = 0.7; //Proportional Gain, higher values for faster response, higher values contribute to overshoot.
float Ki          = 0.2; //Integral Gain, higher values to converge faster to zero error, higher values produce oscillations. Higher values are more unstable near a target_angle = 0.
float iLimit      = 5; //Arbitrary Anti-wind-up
float Kd          = 1; //Derivative Gain, higher values dampen oscillations around target_angle. Higher values produce more holding state jitter. May need filter for error noise.
float prev_error  = 0;
float prev_pError = 0;
float error       = 0;
float pError      = 0;
float iError      = 0;
 
void setup() { 
  servo_test.attach(3); // attach the signal pin of servo to pin3 of arduino
  pinMode(A0, INPUT);
  Serial.begin(9600);
} 
  
void loop() { 
    if (Serial.available() > 0) {
        valA=Serial.parseInt();
    }
    servo_test.write(valA);
    mido_angulo();
    Serial.println(angle);

}

void mido_angulo() {
  while(1) {  //From Parallax spec sheet
    tHigh  = pulseIn(pinFeedback, HIGH);
    tLow   = pulseIn(pinFeedback, LOW);
    tCycle = tHigh + tLow;

    if ( tCycle > 1000 && tCycle < 1200) {
        break; //valid tCycle;
    }
    else{
        return; //Invalid tCycle;
    }
  }
  
  dc = (100 * tHigh) / tCycle; //From Parallax spec sheet, you are trying to determine the percentage of the HIGH in the pulse
  angle = ((dc - dcMin) * 360) / (dcMax - dcMin + 1); //From Parallax spec sheet

  //Keep measured angles within bounds
  if (angle < 0.0) {
      return;
      angle = 0.0;
  }
  else 
  if (angle > 359.0) {
      return;
      angle = 359.0;
  }
}
