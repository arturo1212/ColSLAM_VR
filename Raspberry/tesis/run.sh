#roscore & 
roslaunch rosbridge_server rosbridge_websocket.launch & 
sleep 15
python /home/arduino_reader.py
