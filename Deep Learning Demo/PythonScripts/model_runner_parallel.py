# model_runner.py
from PIL import Image
import torchvision.transforms as transforms
import torch

from anomalib.deploy import TorchInferencer
from anomalib.data.utils.tiler import Tiler
from torch import Tensor
import time
from torchvision.transforms.functional import to_pil_image
from PIL import Image

import sys
from multiprocessing import shared_memory
import numpy as np
import os
os.environ["TRUST_REMOTE_CODE"] = "1"

class ModelRunner:
    # Static profiles
    profile_config = {
        0: {
            "description": "PatchCore_512",
            "model_name": "PatchCore",
            "model_path": r"C:\Users\klin\OneDrive - Pack-Smart Inc\Desktop\Windows_Training\training_test\results\Patchcore\custom\v17\weights\torch\model.pt",
            "patch_size": 512,
            "overlap_size": 64,
            "number_of_chunks": 8, #4
        },
        1: {
            "description": "PatchCore_672",
            "model": "PatchCore",
            "model_path": r"C:\Users\klin\OneDrive - Pack-Smart Inc\Desktop\Windows_Training\training_test\results\Patchcore\custom\v21\weights\torch\model.pt",
            "patch_size": 672,
            "overlap_size": 64,
            "number_of_chunks": 4, #2
        }
        # Add more profiles here
    }

    def __init__(self, profileindex: int = 0, warm_up_img_height: int = 3200, warm_up_img_width: int = 1440):
        if profileindex not in self.profile_config:
            raise ValueError(f"Unknown profile index: {profileindex}")
        profile = self.profile_config[profileindex]

        self.model = TorchInferencer(path=profile["model_path"])
        self.patch_size = profile["patch_size"]
        self.stride = self.patch_size - profile["overlap_size"]
        self.number_of_chunks = profile['number_of_chunks']

        self.image_height = warm_up_img_height
        self.image_width = warm_up_img_width

        self.to_tensor = transforms.ToTensor()
        self.tiler = Tiler(tile_size=self.patch_size, stride=self.stride, mode="interpolation")

        self._warm_up()
    
    def _warm_up(self):
        dummy = torch.randn(1, 3, self.image_height, self.image_width)
        # tiles = self.tiler.tile(dummy)
        # chunks = torch.chunk(tiles, chunks=self.number_of_chunks, dim=0)
        chunks = self.pre_process_get_chunks(dummy)
        for i, chunk in enumerate(chunks):
            self.model.predict(image=chunk)

    def infer_image(self, input_data, width=1440, height=3200, mode='L', model_index=0) -> bytes:
        start = time.time()
        
        if isinstance(input_data, bytes):
            """Infer directly from raw image bytes (fast, no disk I/O)."""
            # image = Image.open(BytesIO(input_data)).convert("RGB")
            image = Image.frombytes(mode=mode, size=(width, height), data=input_data).convert("RGB")
        elif isinstance(input_data, Image.Image):
            image = input_data
        else:
            raise TypeError("Unsupported input type. Must be bytes or PIL.Image.Image.")

        input_tensor = self.to_tensor(image)
        chunks = self.pre_process_get_chunks(input_tensor)
        
        chunks_anomaly_results = []
        for i, chunk in enumerate(chunks):
            predictions = self.model.predict(image=chunk)

            chunks_anomaly_results.append(predictions.anomaly_map.cpu())

        # RGB to Gray
        final_outputs = torch.cat(chunks_anomaly_results, dim=0).unsqueeze(1)
        final_entire = self.tiler.untile(final_outputs)
        return_bytes = (final_entire.squeeze() * 255).to(torch.uint8).numpy().tobytes()

        end = time.time()
        print(f"    + {model_index} Inference time: {end - start:.2f} seconds")

        return return_bytes
    
    def pre_process_get_chunks(self, image: torch.Tensor) -> tuple[Tensor, ...]:
        tiles = self.tiler.tile(image)
        chunks = torch.chunk(tiles, chunks=self.number_of_chunks, dim=0)
        
        return chunks
        
class SharedMemoryRunner:
    def __init__(self, input_shm_name, output_shm_name, width, height, model_index):
        self.width = width
        self.height = height
        self.model_index = model_index

        self.runner = ModelRunner(profileindex=model_index,
                                  warm_up_img_height=height,
                                  warm_up_img_width=width)

        self.input_shm = shared_memory.SharedMemory(name=input_shm_name)
        self.output_shm = shared_memory.SharedMemory(name=output_shm_name)

        self.input_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=self.input_shm.buf)
        self.output_buffer = np.ndarray((height, width), dtype=np.uint8, buffer=self.output_shm.buf)

    def run(self, timeout=3):
        print(f"{self.model_index}: Started, waiting for input... (timeout: {timeout}s)")

        timeout_started = False
        last_checked = None

        while True:
            # Check flag byte for input ready
            if self.input_buffer[0, 0] == 255:
                if not timeout_started:
                    last_checked = time.time()
                    timeout_started = True

                self.input_buffer[0, 0] = 0  # Clear input flag
                raw_bytes = self.input_buffer.tobytes()

                print(f"{self.model_index}: Running inference...")
                result = self.runner.infer_image(raw_bytes, self.width, self.height, model_index=self.model_index)

                np_result = np.frombuffer(result, dtype=np.uint8).reshape(self.height, self.width)
                self.output_buffer[:] = np_result
                self.output_buffer[0, 0] = 255  # Signal result ready

                print(f"{self.model_index}: Inference done.")

                # Reset timeout flag after successful processing
                timeout_started = False
                last_checked = None

            elif timeout_started:
                if time.time() - last_checked > timeout:
                    print(f"{self.model_index}: Timeout reached after {timeout} seconds. Exiting.")
                    break

            # If no input and no timeout started, just wait without timeout checking
            time.sleep(0.05)

if __name__ == "__main__":
    # Expected args:
    # input_shm_name output_shm_name width height model_index model_name [timeout]
    if len(sys.argv) < 7:
        print("Usage: python shared_model_runner.py <input_shm> <output_shm> <width> <height> <model_index> <model_name> [timeout]")
        sys.exit(1)

    input_shm_name = sys.argv[1]
    output_shm_name = sys.argv[2]
    width = int(sys.argv[3])
    height = int(sys.argv[4])
    model_index = int(sys.argv[5])
    timeout = int(sys.argv[6]) if len(sys.argv) > 7 else 30  # Default 30 seconds

    runner = SharedMemoryRunner(input_shm_name, output_shm_name, width, height, model_index)
    runner.run(timeout=timeout)