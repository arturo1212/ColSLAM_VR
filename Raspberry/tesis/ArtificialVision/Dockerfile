FROM raspi-opencv

RUN sudo apt-get update && sudo apt-get upgrade -y
RUN pip install --upgrade pip && pip install websocket-client roslibpy

ADD *.py ./ 
