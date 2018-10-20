#include "MPU9250.h"

MPU9250 mpu;

void setup()
{
    Serial.begin(115200);

    Wire.begin();

    delay(2000);
    mpu.setup();

    delay(5000);

    // calibrate anytime you want to
    mpu.calibrateAccelGyro();
    mpu.calibrateMag();

    mpu.printCalibration();
}

void loop()
{
   static uint32_t prev_ms = millis();
    if ((millis() - prev_ms) > 16)
    {
        mpu.update();
        Serial.println("Merwebo");
        Serial.println(mpu.getRoll());
        Serial.println(mpu.getPitch());
        Serial.println(mpu.getYaw());

        prev_ms = millis();
    }
}
