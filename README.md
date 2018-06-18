# ColSLAM_VR

## Unity
- Utilizar script "NaiveMapping" que incluye comunicacion con ROS y mapeo (SIN ODOMETRIA). Se agrega al gameObject que representa al vehiculo.

- Mover parametro de permanencia del bloque.

## Arduino
- Cargar el script scan_test.ino

## Raspberry
- Existen 2 Dockerfiles: ros_base (que se encarga de instalar los paquetes necesarios) y ros_tesis (que copia archivo e inicializa procesos).

- Ejecutar los siguientes comandos en orden:

1.- roscore &

2.- roslaunch rosbridge_server rosbridge_websocket.launch &

3.- python /home/arduino_reader.py

Esto inicializa los nodos de ROS y abre los websockets para la comunicacion con Unity.

# Referencias
