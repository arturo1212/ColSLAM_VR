FROM colslam:base_ros

RUN apt-get update && apt-get -y install python3-serial python-serial
ADD PIGPIO/ /.
RUN make \
    && make install
ADD *.py /home/
ADD run.sh .
CMD ["source", "/opt/ros/kinetic/setup.bash"]
CMD ["/bin/bash", "-c", "chmod 777 run.sh && ./run.sh"]
#CMD ["/bin/bash", "-c", "roscore & roslaunch rosbridge_server rosbridge_websocket.launch & sleep 10 && python /home/arduino_reader.py"]
 
#CMD roslaunch rosbridge_server rosbridge_websocket.launch 
#CMD python /home/arduino_reader.py

