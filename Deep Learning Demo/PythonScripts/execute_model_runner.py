from run.model_runner import ModelRunner
from PIL import Image
from torchvision.utils import save_image
import time

def main():
    runner = ModelRunner(profileindex=1)
    
    image = Image.open(r"C:\Users\klin\OneDrive - Pack-Smart Inc\Desktop\Windows_Training\training_test\datasets\Pack_Smart\abnormal\20250603_172353_639.tiff").convert("RGB")
    
    sec_list = []

    for i in range(15):
        start = time.time()
        tensor_result = runner.infer_image(image, return_type='tensor')
        end = time.time()
        sec_list.append(end-start)
        print("   + outer cal: " + str(end-start))

    average = sum(sec_list) / len(sec_list)
    print(average)  
    
    save_image(tensor_result, r"C:\Users\klin\OneDrive - Pack-Smart Inc\Desktop\Windows_Training\training_test\results\Patchcore\latest\images\inference_test\x.tiff")

if __name__ == "__main__":
   main()