wget https://github.com/joan2937/pigpio/archive/master.zip
unzip master.zip
cd pigpio-master
make
make install
sudo pigpiod
python move_robot_manual.py