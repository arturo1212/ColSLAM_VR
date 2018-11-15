import roslibpy
ros = roslibpy.Ros("rosnodes", port=9090)
ros.on_ready(lambda: print('Is ROS connected?', ros.is_connected))
