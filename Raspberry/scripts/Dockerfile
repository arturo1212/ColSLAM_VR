FROM ros:tesis_base

RUN apt-get update && apt-get -y install zip unzip wget
ADD move_robot_manual.py .
ADD install_pigpio.sh .
CMD ["/bin/bash", "-c", "chmod 777 install_pigpio.sh && ./install_pigpio.sh"]
CMD ["/usr/bin/svscan", "/etc/svscan/"]