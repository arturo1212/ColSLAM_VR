FROM ros:tesis_base

ADD arduino_reader.py /home/.
ADD run.sh /home/.

RUN ["chmod", "+x", "/home/run.sh"]

CMD roscore &
CMD roslaunch rosbridge_server rosbridge_websocket.launch &
CMD python /home/arduino_reader.py