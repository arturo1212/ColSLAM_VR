#roscore &
make
make install
pigpiod 
roslaunch rosbridge_server rosbridge_websocket.launch & 
sleep 15
python /home/arduino_reader.py &
python /home/unity_listener.py 
