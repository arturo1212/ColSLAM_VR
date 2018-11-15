FROM raspi-opencv

ENV DEBIAN_FRONTEND noninteractive
RUN sudo apt-get update && sudo apt-get upgrade -y
RUN sudo apt-get install nginx libraspberrypi-bin -y
RUN pip install --upgrade pip && pip install websocket-client roslibpy

ADD *.py ./
ADD *.png ./ 
