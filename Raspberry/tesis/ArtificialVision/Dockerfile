FROM tdicola/pi_python_opencv_contrib

RUN apt-get update
RUN apt-get -y upgrade
RUN apt-get install python-picamera python3-picamera nginx nano vim 
RUN pip install websocket-client roslibpy


ADD *.py ./
ADD *.png ./ 
