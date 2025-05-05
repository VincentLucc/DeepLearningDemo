using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
 
namespace OpenCV_Halcon_Demo
{
    public partial class csHalconHelper
    {
        /// <summary>
        /// 3.1415926 Radian = 180 degree
        /// </summary>
        public static float DegreePerRadian = 57.2957795f;

        /// <summary>
        /// 45 degree in rad
        /// </summary>
        public static double HalconDegree45 = 0.7853981634;

        public static List<string> Commands = new List<string>()
        {
            "find_shape_model",
            "find_scaled_shape_model",
            "find_aniso_shape_model",
            "affine_trans_contour_xld",
            "affine_trans_region"
        };

        /// <summary>
        /// Byte image to Hobject
        /// </summary>
        /// <param name="bImage"></param>
        /// <returns></returns>
        public static HObject BinaryImage2HalconImage(byte[] bImage)
        {
            HObject ho_Image = null;

            try
            {
                if (bImage == null) return null;

                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream.Write(bImage, 0, bImage.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    ho_Image = (HObject)formatter.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("csHalconWindow.LoadBinaryImage:\r\n" + ex.Message);
                return null;
            }

            //Pass all steps
            return ho_Image;
        }

        public static byte[] HalconImage2ByteArray(HObject ho_Image)
        {
            if (ho_Image == null) return null;

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, ho_Image);
                return stream.ToArray();
            }
        }

        

        public static Bitmap Hobject2BitmapSize(HObject sourceImage, float maxWidth = 600f, float maxHeight = 400f)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                if (sourceImage == null || !sourceImage.IsInitialized()) return null;

                HOperatorSet.GetImageSize(sourceImage, out HTuple width, out HTuple height);

                double fZoom = 1;

                //Check width
                if (width > maxWidth) fZoom = maxWidth / width;

                //Check height
                if (height > maxHeight)
                {
                    var heightZoom = maxHeight / height;
                    if (heightZoom < fZoom) fZoom = heightZoom;
                }

                //Zoom the image
                HObject imageProcess = sourceImage;
                if (fZoom != 1)
                {
                    HOperatorSet.ZoomImageFactor(sourceImage, out HObject imageZoomed, fZoom, fZoom, "constant");
                    imageProcess = imageZoomed;
                }

                var result = Hobject2BitmapV2(imageProcess);
                HOperatorSet.GetImageSize(imageProcess, out HTuple hWidth, out HTuple hHeight);
                stopwatch.Stop();
                Trace.WriteLine($"Hobject2BitmapSize({width.I}x{height.I},{hWidth.D}x{hHeight.D}):{stopwatch.ElapsedMilliseconds}ms");
                return result;

            }
            catch (Exception ex)
            {

                Trace.WriteLine($"Hobject2BitmapSize({maxWidth},{maxHeight}):\r\n{ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Convert halcon image to bitmap
        /// Solve the memory access issue
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <returns></returns>
        public static Bitmap Hobject2BitmapV2(HObject sourceImage)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                if (sourceImage == null || !sourceImage.IsInitialized()) return null;

                //Check image type
                HOperatorSet.CountChannels(sourceImage, out HTuple hv_Channels);

                //Get Image size
                HOperatorSet.GetImageSize(sourceImage, out HTuple width0, out HTuple height0);


                //Mono Image
                HObject imageRGB;
                if (hv_Channels.L == 1)
                {
                    //Convert gray scale image to color image
                    //bmp image are 3 channel image
                    HOperatorSet.Compose3(sourceImage, sourceImage, sourceImage, out imageRGB);
                }
                //Color image
                else if (hv_Channels.L >= 3)
                {
                    //Force to 3 channels image
                    HOperatorSet.Decompose3(sourceImage, out HObject image1, out HObject image2, out HObject image3);
                    HOperatorSet.Compose3(image1, image2, image3, out imageRGB);
                }
                else return null;

                if (imageRGB == null) return null;
                //Get data pointer
                HOperatorSet.GetImagePointer3(imageRGB, out HTuple pointerRed, out HTuple pointerGreen,
                    out HTuple pointerBlue, out HTuple type, out HTuple width, out HTuple height);

                //create new image
                Bitmap bmpImage = new Bitmap(width.I, height.I, PixelFormat.Format32bppArgb);

                BitmapData bitmapData = bmpImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);
                unsafe
                {
                    //Get new image address
                    byte* bNew = (byte*)bitmapData.Scan0;

                    //Get old image address
                    byte* r = ((byte*)pointerRed.L);
                    byte* g = ((byte*)pointerGreen.L);
                    byte* b = ((byte*)pointerBlue.L);



                    int iPixels = width * height;
                    for (int i = 0; i < iPixels; i++)
                    {
                        bNew[i * 4] = (b)[i]; //B
                        bNew[i * 4 + 1] = (g)[i]; //G
                        bNew[i * 4 + 2] = (r)[i]; //R
                        bNew[i * 4 + 3] = 255; //Transparency   
                    }
                }

                bmpImage.UnlockBits(bitmapData);
                stopwatch.Stop();
                //Trace.WriteLine($"Hobject2BitmapV2({bmpImage.Width}x{bmpImage.Height}):{stopwatch.ElapsedMilliseconds}ms");
                return bmpImage;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Hobject2Bitmap:\r\n" + ex.Message);
                return null;
            }

        }

        /// <summary>
        /// Tranform point to a new point based on mapping matrix
        /// </summary>
        /// <param name="point"></param>
        /// <param name="mappingMatrix"></param>
        /// <returns></returns>
        public static HalconPoint MapPoint(HalconPoint point, HTuple mappingMatrix)
        {
            try
            {
                HOperatorSet.AffineTransPoint2d(mappingMatrix, point.Row, point.Column, out HTuple rowOut,
                    out HTuple colOut);
                var newPoint = new HalconPoint(rowOut, colOut);
                return newPoint;
            }
            catch (Exception)
            {

                return null;
            }
        }

        /// <summary>
        /// Manual method 50~100 times faster
        /// Use c# loop instead of halcon internal method
        /// </summary>
        /// <param name="ho_SourceImage"></param>
        /// <param name="ho_ProcessedImages"></param>
        /// <param name="hv_DivisionNum"></param>
        public static List<HImage> DivideImageSharp(HImage ho_SourceImage, int iDivisionNum)
        {
            //Init variables
            List<HImage> ho_ProcessedImages = new List<HImage>();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool showDebugInfo = false;

            try
            {
                //Get all points
                var imagePointer = ho_SourceImage.GetImagePointer1(out string sType, out int iWidth, out int iHeight);

                //Get byte array 
                int iSize = iWidth * iHeight;

                //Copy all data to local
                byte[] sourceBuffer = new byte[iSize];
                Marshal.Copy(imagePointer, sourceBuffer, 0, iSize);

                //Get height of images         
                int iHeightAvg = iHeight / iDivisionNum;
                int iExtraLines = iHeight % iDivisionNum;
                List<int> heightList = new List<int>();
                List<byte[]> imageBufferList = new List<byte[]>();
                for (int i = 0; i < iDivisionNum; i++)
                {

                    int iSubImageHeight = 0;
                    int iPixels = 0;

                    //Get height of each images
                    if (i >= iExtraLines)
                    {
                        iSubImageHeight = iHeightAvg;
                        heightList.Add(iSubImageHeight);
                    }
                    else
                    {
                        iSubImageHeight = iHeightAvg + 1;
                        heightList.Add(iSubImageHeight);
                    }

                    //Prepare buttfer list
                    iPixels = iSubImageHeight * iWidth;
                    imageBufferList.Add(new byte[iPixels]);
                }

                watch.Stop();
                if (showDebugInfo) Trace.WriteLine($"Prepare duration:{watch.ElapsedMilliseconds}ms");
                watch.Restart();

                //Copy data to buffer line by line
                int iImageIndex = 0;
                int iPositionSource = 0;
                int iPositionTarget = 0;
                int iLineCount = 0;
                for (int i = 0; i < iHeight; i++)
                {
                    var BufferCurrent = imageBufferList[iImageIndex];

                    iPositionTarget = iLineCount * iWidth;
                    iPositionSource = i * iWidth;
                    Array.Copy(sourceBuffer, iPositionSource, BufferCurrent, iPositionTarget, iWidth);

                    iImageIndex += 1;
                    if (iImageIndex >= iDivisionNum)
                    {
                        iImageIndex = 0;
                        iLineCount += 1;
                    }
                }

                watch.Stop();
                if (showDebugInfo) Trace.WriteLine($"Copy to buffer duration:{watch.ElapsedMilliseconds}ms");
                watch.Restart();


                //Add result
                for (int i = 0; i < iDivisionNum; i++)
                {
                    unsafe
                    {
                        fixed (byte* pBuffer = imageBufferList[i])
                        {
                            IntPtr ptr = (IntPtr)pBuffer;
                            HImage fImage = new HImage("byte", iWidth, heightList[i], ptr);
                            ho_ProcessedImages.Add(fImage);

                        }
                    }
                }

                watch.Stop();
                if (showDebugInfo) Trace.WriteLine($"Create image duration:{watch.ElapsedMilliseconds}ms");
                watch.Restart();

            }
            catch (Exception ex)
            {
                //Always show exception
                Trace.WriteLine($"DivideImageSharp:\r\n{ex.Message}");
                return null;
            }

            return ho_ProcessedImages;
        }



        /// <summary>
        /// Get the angle from (0,0) to point (x,y)
        /// Get degree from 0~ +180 and 0~ -180
        /// </summary>
        /// <returns></returns>
        public static double GetPointAngle(HalconPoint point)
        {
            try
            {
                //Line orientation result is -90 to 90
                HOperatorSet.LineOrientation(0, 0, point.Row, point.Column, out HTuple phi);

                //Convert angle result from -90 ~ +90 to -180 ~ +180
                //Get angle in different quadrant
                if ((point.Row > 0 && point.Column > 0) ||
                    (point.Row < 0 && point.Column > 0))
                {
                    return phi.TupleDeg();
                }
                else if (point.Row > 0 && point.Column < 0)
                {
                    return phi.TupleDeg() - 180;
                }
                else if (point.Row < 0 && point.Column < 0)
                {
                    //180-(-phi.TupleDeg())
                    return 180 + phi.TupleDeg();
                }
                else return 0;

            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the center of two angles
        /// Result is -180 to +180
        /// </summary>
        /// <param name="angle1"></param>
        /// <param name="angle2"></param>
        /// <returns></returns>
        public static double GetAngleCenter(double angle1, double angle2)
        {
            if (angle1 == 0 && angle2 == 0) return 0;

            //Both angle in range 0 ~ +180
            if (angle1 > 0 && angle2 > 0)
            {
                var offset = Math.Abs(angle1 - angle2);
                return Math.Min(angle1, angle2) + offset / 2;
            }
            //Both angle in range 0 ~ -180
            else if (angle1 < 0 && angle2 < 0)
            {
                var offset = Math.Abs(angle1 - angle2);
                return Math.Max(angle1, angle2) - offset / 2;
            }
            //One angle is 0 ~ 180 and another is 0 ~ -180
            else if ((angle1 > 0 && angle2 < 0) ||
                     (angle1 < 0 && angle2 > 0))
            {
                var absAngle1 = Math.Abs(angle1);
                var absAngle2 = Math.Abs(angle2);
                var offset = absAngle1 + absAngle2;
                var positiveAngle = Math.Max(angle1, angle2);

                //Angle difference is smaller than 180, center angle is on right side
                if (offset < 180)
                {
                    return positiveAngle - offset / 2;
                }
                //Angle difference is bigger than 180, center angle is on left side
                else if (offset > 180)
                {
                    //Get angle on another side
                    var oppositeAngle1 = 180 - absAngle1;
                    var oppositeAngle2 = 180 - absAngle2;
                    var oppositeRangle = oppositeAngle1 + oppositeAngle2;
                    var resultAngle = positiveAngle + oppositeRangle / 2;

                    //Convert result angle to with in 0~180 to 0~-180
                    if (resultAngle > 180)
                    {
                        //-(180- (actual - 180))
                        return resultAngle - 360;
                    }
                    //Actual<180
                    else return resultAngle;
                }
                else return 0;
            }
            else return 0;
        }

        public static HalconPoint GetLinesIntersection(csLineData line1, csLineData line2)
        {
            try
            {
                if (line1 == null ||
                    line2 == null ||
                    !line1.IsInit ||
                    !line2.IsInit)
                {
                    Trace.WriteLine($"csHalconCommon.GetLinesIntersection: Invalid source line.");
                    return null;
                }

                //Get intersection
                HOperatorSet.IntersectionLines(line1.Row1, line1.Column1, line1.Row2, line1.Column2,
                                               line2.Row1, line2.Column1, line2.Row2, line2.Column2,
                                               out HTuple RowInter, out HTuple ColumnInter, out HTuple IsOverlapping);

                if (RowInter.Length == 0)
                {
                    Trace.WriteLine($"csHalconCommon.GetLinesIntersection: No intersection found.");
                    return null;
                }

                //Save position
                HalconPoint pIntersection = new HalconPoint(RowInter.D, ColumnInter.D);
                return pIntersection;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"csHalconCommon.GetLinesIntersection.Exception:{ex.GetMessageDetail()}");
                return null;
            }

        }

    

        public static Rectange2Data CreateImageSampleRegion(HObject image)
        {
            HOperatorSet.GetImageSize(image, out HTuple hWidth, out HTuple hHeight);


            var drawObject = new Rectange2Data()
            {
                Row = (float)(hHeight.D) / 2f,
                Column = (float)(hWidth.D) / 2f,
                Phi = 0,
                Length1 = (float)(hWidth.D) / 4f,
                Length2 = (float)(hHeight.D) / 4f,
                IsInit = true
            };

            return drawObject;
        }


        public static List<string> ToValue()
        {
            var list = new List<string>();
            return list;
        }






        public static List<Rectange1Data> GetRectangle1(HObject rectangle1Region)
        {
            List<Rectange1Data> rectange1s = new List<Rectange1Data>();
            HOperatorSet.RegionFeatures(rectangle1Region, "row1", out HTuple row1);
            HOperatorSet.RegionFeatures(rectangle1Region, "row2", out HTuple row2);
            HOperatorSet.RegionFeatures(rectangle1Region, "column1", out HTuple column1);
            HOperatorSet.RegionFeatures(rectangle1Region, "column2", out HTuple column2);

            for (int i = 0; i < row1.DArr.Length; i++)
            {
                var border = new Rectange1Data();
                border.Load(row1.DArr[i], column1.DArr[i], row2.DArr[i], column2.DArr[i]);
                rectange1s.Add(border);
            }
            return rectange1s;
        }


        public static List<string> GetAccDevices()
        {
            var deviceList = new List<string>();

            try
            {
                HOperatorSet.QueryAvailableComputeDevices(out HTuple deviceIdentifier);
                if (deviceIdentifier == null || deviceIdentifier.Length == 0) return deviceList;

                for (int i = 0; i < deviceIdentifier.Length; i++)
                {
                    HOperatorSet.GetComputeDeviceInfo(deviceIdentifier[i], "name", out HTuple deviceName);
                    deviceList.Add(deviceName.S);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("TryGetAccDevices:\r\n" + ex.Message);
            }

            return deviceList;
        }


        /// <summary>
        /// Notice:
        /// This call should in a clean thread
        /// If other thread is using halcon, this command will cause exception.
        /// Make sure to close all cameras before project start.
        /// </summary>
        /// <param name="iThreadNumber"></param>
        public static void SetThreadNumberLimit(int iThreadNumber)
        {
            try
            {

                //Get current setting before apply settings again
                var value = HSystem.GetSystem("thread_num");

                if (iThreadNumber < 1)
                {
                    //Skip setting if value is the same
                    if (value.Type == HTupleType.STRING && value.S == "default") return;

                    HOperatorSet.SetSystem("thread_num", "default");
                }
                else
                {
                    if (iThreadNumber > Environment.ProcessorCount)
                    {
                        iThreadNumber = Environment.ProcessorCount - 1;
                        if (iThreadNumber < 1) iThreadNumber = 1;
                    }

                    //Skip setting if value is the same
                    if ((value.Type == HTupleType.INTEGER || value.Type == HTupleType.LONG)
                        && value.I == iThreadNumber) return;

                    HOperatorSet.SetSystem("thread_num", iThreadNumber);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SetThreadNumberLimit:({iThreadNumber},{Environment.ProcessorCount})\r\n" +
                                ex.Message);
            }
        }

        public static HObject ReadImageFile(string sFileName, out string sMessage)
        {
            sMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(sFileName))
                {
                    sMessage = $"The file path is empty.";
                    return null;
                }



                if (!File.Exists(sFileName))
                {
                    sMessage = $"Image file doesn't exist.\r\n{sFileName}";
                    return null;
                }

                HOperatorSet.ReadImage(out HObject image, sFileName);
                return image;
            }
            catch (Exception e)
            {
                sMessage = e.GetMessageDetail();
                Trace.WriteLine($"csHalconCommon.GetImage:{sFileName}\r\n{sMessage}");
                return null;
            }
        }

        public static HObject ReadImageFile(string sFilePath)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(sFilePath)) return null;
                HOperatorSet.ReadImage(out HObject image, sFilePath);
                return image;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ReadImageFile:{e.GetMessageDetail()}");
                return null;
            }
        }

    }
}
