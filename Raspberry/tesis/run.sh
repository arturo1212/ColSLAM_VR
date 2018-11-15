#roscore &
pigpiod 
roslaunch rosbridge_server rosbridge_websocket.launch & 
sleep 15
python /home/arduino_reader.py &
python /home/node_homography.py &
python /home/node_reset.py &
python /home/node_stream.py &
python /home/unity_listener.py 
