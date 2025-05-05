using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace OpenCV_Halcon_Demo.Classes
{
    internal class csOpenCVWindow
    {
        private PictureBox DisplayBox;

        public Mat CurrentImage;

        public csOpenCVWindow(PictureBox _pixBox)
        {
            DisplayBox = _pixBox;
            //Allow to show all image area
            DisplayBox.SizeMode = PictureBoxSizeMode.Zoom;
            
        }


        public Mat OpenImage(string sFileName, out string sMessage)
        {
            sMessage = string.Empty;
            try
            {
                var image = Cv2.ImRead(sFileName);

                if (image == null) return null;
                CurrentImage?.Dispose();
                CurrentImage = image;

                return CurrentImage;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"csOpenCVWindow.OpenImage.Exception:{e.GetMessageDetail()}");
                return null;
            }
        }

        public void DisplayImage(Mat image)
        {
            //Convert to color space
            Cv2.CvtColor(image, image, ColorConversionCodes.BGR2RGB);

            //Convert to bitmap
            var bitmap = image.ToBitmap();
 
            //Update picture box size
            if (DisplayBox.Width != bitmap.Width) DisplayBox.Width = bitmap.Width;
            if (DisplayBox.Height != bitmap.Height) DisplayBox.Height = bitmap.Height;

            //Show image
            if (DisplayBox.Image != null)
            {
                DisplayBox.Image.Dispose();
                DisplayBox.Image = null;
            }

            DisplayBox.Image = bitmap;
        }
    }
}
