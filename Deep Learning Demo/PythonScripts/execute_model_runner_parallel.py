from run.model_runner import ModelRunner
from PIL import Image
from torchvision.utils import save_image
import time
from multiprocessing import shared_memory
import subprocess
import os
import numpy as np
import signal
import win32file, win32event, win32con

# Static values
CommunicationNameBase="DeltaX-Vision_Anomaly"
# Size can't be changed when created, use a larger size when init
DefaultSize=100 * 1024 * 1024

def save_image(byte_data, width, height, path):
    image = Image.frombytes("L", (width, height), byte_data)
    image.save(path)

def launch_model_process(input_shm, output_shm, profile_index):
    # Step 1: Paths
    current_dir = os.path.dirname(os.path.abspath(__file__))          # project_root/
    parent_dir = os.path.dirname(current_dir)                          # some_root_folder/

    python_venv = os.path.join(parent_dir, "Scripts", "python.exe")   # Adjust if on Linux
    script_path = os.path.join(current_dir, "run", "model_runner_parallel.py")
    
    return subprocess.Popen([
        python_venv, script_path,  # Replace with full path if needed
        input_shm, output_shm,
        str(profile_index)
    ])

def main():
    width, height = 1440, 3200
    timeout_sec = 30

    # Shared memory names and profiles
    shm_profiles = [
        ("model1_input", "model1_output", 0),  # profile_index = 0
        ("model2_input", "model2_output", 1),  # profile_index = 1
    ]

    # Create shared memory blocks
    shms = []
    for input_name, output_name, _ in shm_profiles:
        input_shm = shared_memory.SharedMemory(name=input_name, create=True, size=DefaultSize)
        output_shm = shared_memory.SharedMemory(name=output_name, create=True, size=DefaultSize)
        shms.append((input_shm, output_shm))

    # Launch model subprocesses
    procs = []
    for (input_name, output_name, profile_index) in shm_profiles:
        proc = launch_model_process(input_name, output_name, profile_index)
        procs.append(proc)

    try:
        # Load input image
        image = Image.open(r"C:\Users\klin\OneDrive - Pack-Smart Inc\Desktop\Windows_Training\training_test\datasets\Pack_Smart\abnormal\20250603_172353_639.tiff").convert("L")
        img_bytes = image.tobytes()

        results = []

        # # Send image to models
        # for (input_shm, output_shm), (input_name, output_name, profile_index) in zip(shms, shm_profiles):
        #     input_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=input_shm.buf)
        #     output_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=output_shm.buf)

        #     input_buffer[:] = np.frombuffer(img_bytes, dtype=np.uint8).reshape((height, width))
        #     input_buffer[0, 0] = 255  # Signal input ready

        # # Wait and retrieve results
        # for (input_shm, output_shm), (_, _, profile_index) in zip(shms, shm_profiles):
        #     output_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=output_shm.buf)

        #     print(f"Waiting for model {profile_index} to finish...")
        #     start_time = time.time()
        #     while output_buffer[0, 0] != 255:
        #         if time.time() - start_time > timeout_sec:
        #             raise TimeoutError(f"Model {profile_index} timed out.")
        #         time.sleep(0.05)

        #     output_buffer[0, 0] = 0  # Clear output flag
        #     results.append(output_buffer.tobytes())

        for i in range(10):
            # Send image to models
            for (input_shm, output_shm), (input_name, output_name, profile_index) in zip(shms, shm_profiles):
                input_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=input_shm.buf)
                output_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=output_shm.buf)

                input_buffer[:] = np.frombuffer(img_bytes, dtype=np.uint8).reshape((height, width))
                input_buffer[0, 0] = 255  # Signal input ready

            # Wait and retrieve results
            for (input_shm, output_shm), (_, _, profile_index) in zip(shms, shm_profiles):
                output_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=output_shm.buf)

                print(f"Waiting for model {profile_index} to finish...")
                start_time = time.time()
                while output_buffer[0, 0] != 255:
                    if time.time() - start_time > timeout_sec:
                        raise TimeoutError(f"Model {profile_index} timed out.")
                    time.sleep(0.05)

                output_buffer[0, 0] = 0  # Clear output flag

        # # Save result of both models
        # save_image(results[0], width, height, r"C:\Users\klin\OneDrive - Pack-Smart Inc\Desktop\Windows_training\training_test\results\Patchcore\latest\images\inference_test\inference_testoutput_model1.tiff")
        # save_image(results[1], width, height, r"C:\Users\klin\OneDrive - Pack-Smart Inc\Desktop\Windows_training\training_test\results\Patchcore\latest\images\inference_test\inference_testoutput_model2.tiff")
        # print("Inference complete for both models.")

    finally:
        # Cleanup
        for input_shm, output_shm in shms:
            input_shm.close()
            input_shm.unlink()
            output_shm.close()
            output_shm.unlink()

        for proc in procs:
            proc.send_signal(signal.SIGTERM)
            proc.wait()

        print("Resources and subprocesses cleaned up.")

if __name__ == "__main__":
   main()