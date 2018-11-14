import json
import websocket

class BridgeClient:
    def __init__(self, ip, port):
        self.ip = ip
        self.port = port
        self.ws = None
    
    def connect(self):
        try:
            WEBSOCKET_URL = "ws://"+self.ip+":"+str(self.port)
            self.ws = websocket.create_connection(WEBSOCKET_URL)
        except Exception as e:
            print("Error: ", e)
    
    def advertise(self, topic):
        if(self.ws is None):
            print("Error: No se ha creado una conexion")
            return -1
        advertise_msg = {
          "op": "advertise",
          "topic": topic,
          "type": "std_msgs/String"
        }
        websocket_send_wrapper(ws,advertise_msg)

    def publish(topic, string_data):
        if(self.ws is None):
            print("Error: No se ha creado una conexion")
            return -1
        my_message = {"data" : string_data}
        pub_msg = {
          "op": "publish",
          "topic": "/" + topic,
          "msg": my_message
        }
        websocket_send_wrapper(ws,pub_msg)