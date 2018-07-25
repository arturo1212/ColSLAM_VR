FROM ros:tesis_base

ADD arduino_reader.py /home/.

CMD roscore &
CMD roslaunch rosbridge_server rosbridge_websocket.launch &
CMD python /home/arduino_reader.py