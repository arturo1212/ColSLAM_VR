import roslibpy
ros = roslibpy.Ros("rosnodes", port=9090)
def meme():
	print('Is ROS connected?', ros.is_connected)
ros.on_ready(meme)
ros.run_forever()