#include "MPU9250.h"

MPU9250 mpu;

void setup()
{
    Serial.begin(115200);

    Wire.begin();

    delay(2000);
    mpu.setup();
    delay(5000);
    float accbias[] = {-61.71, 4.82, 16.11};
    float gyrobias[] = {-4.59, -0.35, 2.70};
    float magbias[] = {-40.68, 308.03, -92.67};
    float magscale[] = {0.99, 1.05, 0.96};

    for(int i=0;i<3;i++){
      mpu.setAccBias(i,accbias[i]*0.001);
      mpu.setGyroBias(i,gyrobias[i]);
      mpu.setMagBias(i,magbias[i]);
      mpu.setMagScale(i,magscale[i]);
    }
    /*
     *< calibration parameters >
      accel bias [g]: 
      -61.71, 4.82, 16.11
      gyro bias [deg/s]: 
      -4.59, -0.35, 2.70
      mag bias [mG]: 
      -40.68, 308.03, -92.67
      mag scale []: 
      0.99, 1.05, 0.96
     */

    //calibrate anytime you want to
    //mpu.calibrateAccelGyro();
    //mpu.calibrateMag();

    mpu.printCalibration();
}

void loop()
{
    static uint32_t prev_ms = millis();
    if ((millis() - prev_ms) > 16)
    {
        mpu.update();
        //mpu.print();

        //Serial.print("roll  (x-forward (north)) : ");
        //Serial.println(mpu.getRoll());
        //Serial.print("pitch (y-right (east))    : ");
        //Serial.println(mpu.getPitch());
        //Serial.print("yaw   (z-down (down))     : ");
        Serial.println(mpu.getYaw());
        
        prev_ms = millis();
    }
}

