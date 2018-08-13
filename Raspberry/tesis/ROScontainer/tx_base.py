import rospy
from std_msgs.msg import String
import sys
import socket

def run_tx(n_name, p_name, host, port, buf_size):
    # Configuracion de ROS
    pub = rospy.Publisher(p_name, String, queue_size=1)
    rospy.init_node(n_name, anonymous=True)
    rate = rospy.Rate(60) # 60hz

    # Configuracion del Socket
    serversocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    serversocket.bind((host, port))
    serversocket.listen(5) # become a server socket, maximum 5 connections

    while not rospy.is_shutdown():
        connection, address = serversocket.accept()
        msg = connection.recv(buf_size) # Recibir serial del codigo de barras
        if len(msg) > 0:
            result = str(msg.strip())
            pub.publish(result)
            rate.sleep()
            print buf
    serversocket.close()
