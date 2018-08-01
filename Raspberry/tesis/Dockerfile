FROM ros:tesis_base

ADD arduino_reader.py /home/.
ADD run.sh /.
CMD ["/bin/bash", "-c", "chmod 777 run.sh && ./run.sh"]
#CMD ["/bin/bash", "-c", "roscore & roslaunch rosbridge_server rosbridge_websocket.launch & sleep 10 && python /home/arduino_reader.py"] 
#CMD roslaunch rosbridge_server rosbridge_websocket.launch 
#CMD python /home/arduino_reader.py

