import socket
import time
import json

# Define the server address and port
server_address = ('127.0.0.1', 1511)

# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

try:
    print("Starting the server...")
    rigid_body_id = 1
    pos_x, pos_y, pos_z = 0.0, 0.0, 0.0
    qx, qy, qz, qw = 0.0, 0.0, 0.0, 1.0

    while True:
        # Update the dummy camera data
        pos_x += 0.01
        pos_y += 0.01
        pos_z += 0.01
        qx += 0.001
        qy += 0.001
        qz += 0.001
        qw += 0.001

        # Create a dummy camera data packet
        camera_data = {
            "RigidBodyID": rigid_body_id,
            "Position": {"x": pos_x, "y": pos_y, "z": pos_z},
            "Rotation": {"qx": qx, "qy": qy, "qz": qz, "qw": qw}
        }
        data = json.dumps(camera_data)
        print(f"Sending data: {data}")
        
        # Send the data packet to the server
        sent = sock.sendto(data.encode(), server_address)
        print(f"Sent {sent} bytes to {server_address}")
        
        # Wait for a short period before sending the next packet
        time.sleep(0.1)  # Reduced sleep interval for faster data change rate

finally:
    sock.close()
