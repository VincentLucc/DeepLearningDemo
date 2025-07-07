# model_runner.py
from PIL import Image
from io import BytesIO
import torchvision.transforms as transforms
import torch

from anomalib.deploy import TorchInferencer
from anomalib.data.utils.tiler import Tiler
from torch import Tensor
import time
from torchvision.transforms.functional import to_pil_image
from PIL import Image
import os
os.environ["TRUST_REMOTE_CODE"] = "1"

class ModelRunner:
    # Static profiles
    profile_config = {
        0: {
            "description": "PatchCore_512",
            "model_name": "PatchCore",
            "model_path": r"E:\Backup\Companies\PackSmart\Projects\DeepLearning\Models\512\model.pt",
            "patch_size": 512,
            "overlap_size": 64,
            "number_of_chunks": 8, #4
        },
        1: {
            "description": "PatchCore_672",
            "model": "PatchCore",
            "model_path": r"E:\Backup\Companies\PackSmart\Projects\DeepLearning\Models\672\model.pt",
            "patch_size": 672,
            "overlap_size": 64,
            "number_of_chunks": 4, #2
        }
        # Add more profiles here
    }

    def __init__(self, timeout: int = None, profileindex: int = 0,
                 warm_up_img_height: int = 3200, warm_up_img_width: int = 1440):
        if profileindex not in self.profile_config:
            raise ValueError(f"Unknown profile index: {profileindex}")
        profile = self.profile_config[profileindex]

        self.model = TorchInferencer(path=profile["model_path"])
        self.patch_size = profile["patch_size"]
        self.stride = self.patch_size - profile["overlap_size"]
        self.number_of_chunks = profile['number_of_chunks']

        self.time = timeout
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

    def infer_image(self, input_data, width=1440, height=3200, mode='L', return_type='bytes'): # raw_bytes: bytes
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

        if return_type == 'bytes':
            return_bytes = (final_entire.squeeze() * 255).to(torch.uint8).numpy().tobytes()

            end = time.time()
            print("   + inner cal: " + str(end-start))

            return bytearray(return_bytes)
        
        elif return_type == 'tensor':
            gray_tensor = final_entire[:, 0:1, :, :]

            end = time.time()
            print("   + inner cal: " + str(end-start))

            return gray_tensor
    
    def pre_process_get_chunks(self, image: torch.Tensor) -> tuple[Tensor, ...]:
        tiles = self.tiler.tile(image)
        chunks = torch.chunk(tiles, chunks=self.number_of_chunks, dim=0)
        
        return chunks
        