from tx_base import run_tx

node_name = "color_talker"
publisher_name = "color"
HOST      = "localhost"
PORT      = 5015
BUFF_SIZE = 64

run_tx(publisher_name, node_name, HOST, PORT, BUFF_SIZE)
