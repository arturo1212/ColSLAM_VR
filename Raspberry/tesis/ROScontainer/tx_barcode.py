from tx_base import run_tx

node_name = "barcode_talker"
publisher_name = "barcode"
HOST      = "localhost"
PORT      = 5010
BUFF_SIZE = 64

run_tx(publisher_name, node_name, HOST, PORT, BUFF_SIZE)
