using DevExpress.Utils.About;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using DevExpress.XtraEditors;
using Newtonsoft.Json;
 
namespace Deep_Learning_Demo
{
    /// <summary>
    /// Customized extension methods
    /// </summary>
    public static class csHalconExtension
    {

        /// <summary>
        /// Convert htuple list value to Lit<Object>
        /// Which can be easily saved in the xml file
        /// </summary>
        /// <returns></returns>
        public static List<object> ToListObjects(this HTuple hTupleList)
        {
            List<object> list = new List<object>();

            if (hTupleList == null) return null;

            if (hTupleList.Type == HTupleType.MIXED)
            {
                foreach (var item in hTupleList.ToOArr()) list.Add(item);
            }


            return list;
        }

 

        /// <summary>
        /// Get the image data without format
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] HobjectToRawByte(this HObject image)
        {
            try
            {
                HOperatorSet.CountChannels(image, out HTuple channels);
                if (channels.L == 1)
                {
                    HOperatorSet.GetImagePointer1(image, out HTuple pointer, out HTuple type, out HTuple width, out HTuple height);
                    long lSize = width.L * height.L;
                    byte[] array = new byte[width.L * height.L];
                    //Get old image address
                    unsafe
                    {
                        byte* imagePointer = ((byte*)pointer.L);
                        fixed (byte* arrayPointer = array)
                        {
                            Buffer.MemoryCopy(imagePointer, arrayPointer, lSize, lSize);
                        }
                    }

                    return array;

                }
                else if (channels.L == 3)
                {
                    //Get data pointer
                    HOperatorSet.GetImagePointer3(image, out HTuple pointerRed, out HTuple pointerGreen, out HTuple pointerBlue, out HTuple type, out HTuple width, out HTuple height);
                    long lSizeSingle = width.L * height.L;
                    long lSizeTotal = lSizeSingle * 3;
                    byte[] dataArray = new byte[lSizeTotal];

                    unsafe
                    {
                        //Get old image address
                        byte* r = ((byte*)pointerRed.L);
                        byte* g = ((byte*)pointerGreen.L);
                        byte* b = ((byte*)pointerBlue.L);


                        fixed (byte* arrayPointer = dataArray)
                        {
                            byte* pStartRed = arrayPointer;
                            byte* pStartGreen = arrayPointer + width.L * height.L;
                            byte* pStartBlue = arrayPointer + width.L * height.L * 2;
                            Parallel.Invoke(
                             () =>
                             { //R
                                 Buffer.MemoryCopy(r, pStartRed, lSizeSingle, lSizeSingle);
                             },
                             () =>
                             {//g
                                 Buffer.MemoryCopy(g, pStartGreen, lSizeSingle, lSizeSingle);
                             },
                             () =>
                             { //b
                                 Buffer.MemoryCopy(b, pStartBlue, lSizeSingle, lSizeSingle);
                             }
                         );
                        }
                    }

                    return dataArray;
                }
                else
                {
                    "Undefined Image type".TraceRecord();
                    return null;
                }

            }
            catch (Exception ex)
            {
                ex.TraceException("HobjectToRawByte");
                return null;
            }
        }


        public static HObject MonoBytesToHObject(this byte[] byteData, int iWdith, int iHeight)
        {
            try
            {
                unsafe
                {
                    fixed (byte* bytePointer = byteData)
                    {
                        HOperatorSet.GenImage1(out HObject imageNew, "byte", iWdith, iHeight, (IntPtr)bytePointer);
                        return imageNew;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.TraceException("MonoBytesToHObject");
                return null;
            }
        }


        public static (bool IsSuccess, string Message) SaveImage(this HObject image, string sPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sPath)) return (false, "The target file path is empty.");

                //Get directory
                string sDir = Path.GetDirectoryName(sPath);
                if (string.IsNullOrWhiteSpace(sDir)) return (false, $"Invalid target folder: {sPath}");
                if (!Directory.Exists(sDir))
                {
                    Directory.CreateDirectory(sDir);
                }

                HOperatorSet.WriteImage(image, "tiff", 0, sPath);

                return (true, sPath);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return (false, e.Message);
            }
        }

        public static int GetHalconInt(this HTuple oValue)
        {
            if (oValue == null) return -1;
            switch (oValue.Type)
            {
                case HTupleType.EMPTY:
                    return -1;
                case HTupleType.INTEGER:
                    return oValue.I;
                case HTupleType.LONG:
                    return (int)oValue.L;
                case HTupleType.DOUBLE:
                    return (int)oValue.D;
                case HTupleType.STRING:
                    if (int.TryParse(oValue.S, out int iValue)) return iValue;
                    else return -1;
                case HTupleType.HANDLE:
                case HTupleType.MIXED:
                default:
                    return -1;
            }
        }



        /// <summary>
        /// The image directly generated using compose3 won't be treated as proper RGB image in certain cases, use this method instead
        /// </summary>
        /// <param name="imageGray"></param>
        /// <returns></returns>
        public static HObject GrayImageToRGBImage(this HObject imageGray)
        {
            try
            {
                HOperatorSet.TransFromRgb(imageGray, imageGray, imageGray, out HObject ImageResult1, out HObject imageResult2, out HObject imageResult3, "hsv");
                HOperatorSet.TransToRgb(ImageResult1, imageResult2, imageResult3, out HObject imageRead, out HObject imageGreen, out HObject imageBlue, "hsv");
                ImageResult1.Dispose();
                imageResult2.Dispose();
                imageResult3.Dispose();
                HOperatorSet.Compose3(imageRead, imageGreen, imageBlue, out HObject multiChannelImage);
                imageRead.Dispose();
                imageGreen.Dispose();
                imageBlue.Dispose();
                return multiChannelImage;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} csHalconCommonExt.GrayImageToRGBImage:{e.GetMessageDetail()}");
                return null;
            }
        }

        public static (bool? IsSuccess, string Message) SaveImageAsAction(this HObject image, string sDefaultFolder = null, string sFilter = null)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                //Set default folder
                if (!string.IsNullOrWhiteSpace(sDefaultFolder))
                {
                    saveFileDialog.InitialDirectory = sDefaultFolder;
                }

                //Set filter
                if (string.IsNullOrWhiteSpace(sFilter))
                {
                    saveFileDialog.Filter = "Tiff File|*.tiff|tif File|*.tif|JPG File|*.jpg|JPEG File|*.jpeg|BMP File|*.bmp|All File|*.*";
                }
                else
                {
                    saveFileDialog.Filter = sFilter;
                }



                if (saveFileDialog.ShowDialog() != DialogResult.OK) return (null, null);
                return image.SaveImage(saveFileDialog.FileName);
            }
        }

        public static HTuple GetRowTuple(this List<HalconPoint> halconPoints)
        {
            HTuple rowData = new HTuple(halconPoints.Select(a => Convert.ToDouble(a.Row)).ToArray());
            return rowData;
        }

        public static HTuple GetColumnTuple(this List<HalconPoint> halconPoints)
        {
            HTuple colData = new HTuple(halconPoints.Select(a => Convert.ToDouble(a.Column)).ToArray());
            return colData;
        }

        public static void ClearDictionary(this HTuple ht_Dict)
        {
            if (ht_Dict == null) return;
            HOperatorSet.GetDictParam(ht_Dict, "keys", new HTuple(), out HTuple ht_Keys);
            HOperatorSet.RemoveDictKey(ht_Dict, ht_Keys);
        }

        public static void ClearHObject(this HObject ho_Object)
        {
            if (ho_Object == null) return;
            ho_Object.Dispose();
            ho_Object = null;
        }



        public static bool IsValid(this HObject ho_Self)
        {
            if (ho_Self == null ||
                !ho_Self.IsInitialized())
            {
                return false;
            }

            int iCount = ho_Self.CountObj();
            if (iCount == 0) return false;

            return true;
        }

        public static (int[] Rows, int[] Columns) GetImagePoints(this HObject image)
        {
            if (!image.IsValid()) return (null, null);

            HOperatorSet.GetImageSize(image, out HTuple imageWdith, out HTuple imageHeigt);

            long lTotal = imageWdith.L * imageHeigt.L;
            var rows = new int[lTotal];
            var cols = new int[lTotal];
            for (int i = 0; i < imageHeigt.L; i++)
            {
                for (int j = 0; j < imageWdith.L; j++)
                {
                    long lPosition = i * imageWdith.L + j;
                    rows[lPosition] = i;
                    cols[lPosition] = j;
                }
            }

            return (rows, cols);
        }

        public static int ChannelCount(this HObject ho_Self)
        {
            try
            {
                HOperatorSet.CountChannels(ho_Self, out HTuple channels);
                return channels.GetIntValue();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ChannelCount:{e.GetMessageDetail()}");
                return -1;
            }
        }

        /// <summary>
        /// List objects to htuple value
        /// </summary>
        /// <param name="listObjects"></param>
        /// <returns></returns>
        public static HTuple ToHtuple(this List<object> listObjects)
        {
            HTuple tupleValue = new HTuple();

            foreach (var item in listObjects)
            {
                var valueType = item.GetType();
                if (item is int)
                {
                    tupleValue.Append((int)item);
                }
                else if (item is Int64)
                {
                    tupleValue.Append((Int64)item);
                }
                else if (item is float)
                {
                    tupleValue.Append((float)item);
                }
                else if (item is double)
                {
                    tupleValue.Append((double)item);
                }
                else if (item is string)
                {
                    tupleValue.Append((string)item);
                }
                else
                {
                    string sType = item == null ? "" : item.ToString();
                    Trace.WriteLine($"ToHtuple:Missing Match.({sType})");
                }

            }

            return tupleValue;
        }

        public static string GetStringValue(this HTuple hv_Value)
        {
            string sValue = string.Empty;
            if (hv_Value.Type == HTupleType.STRING)
            {
                sValue = hv_Value.S;
            }
            else if (hv_Value.Type == HTupleType.LONG)
            {
                sValue = hv_Value.L.ToString();
            }
            else if (hv_Value.Type == HTupleType.DOUBLE)
            {
                sValue = hv_Value.D.ToString();
            }

            return sValue;
        }

        public static string GetStringValue(this HTupleElements hv_Value)
        {
            return GetStringValue(hv_Value);
        }




        /// <summary>
        /// Get average gray value of each column
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static List<double> GetColumnGrayHistogram(this HObject image, int iHeightLimit = 500)
        {
            List<double> colGrayList = new List<double>();

            try
            {
                if (image == null || !image.IsInitialized()) return null;

                HOperatorSet.GetImageSize(image, out HTuple hWidth, out HTuple hHeight);
                int iWidth = hWidth.I;
                int iHeight = hHeight.I;

                //Prepare the pixel count
                for (int i = 0; i < iWidth; i++)
                {

                    int[] rows = null;
                    int[] cols = null;

                    //When the size is smaller than limit, use the image size
                    if (iHeight <= iHeightLimit)
                    {
                        rows = new int[iHeight];
                        cols = new int[iHeight];

                        //Set position
                        for (int j = 0; j < iHeight; j++)
                        {
                            cols[j] = i;//Same column
                            rows[j] = j;//Position of each row        
                        }
                    }
                    //When the size is bigger than limit, only use the center area
                    else
                    {
                        rows = new int[iHeightLimit];
                        cols = new int[iHeightLimit];

                        //Get the start position
                        int iStart = (iHeight - iHeightLimit) / 2;

                        //Set position
                        for (int j = 0; j < iHeightLimit; j++)
                        {
                            cols[j] = i;//Same column
                            rows[j] = j + iStart;//Position of each row        
                        }
                    }


                    HTuple rowData = new HTuple(rows);
                    HTuple colData = new HTuple(cols);

                    HOperatorSet.GetGrayval(image, rowData, colData, out HTuple grayVal);

                    int iCOunt = grayVal.TupleLength();
                    double dAVG = grayVal.LArr.Average();

                    colGrayList.Add(dAVG);
                }

                return colGrayList;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} GetColumnGrayAVG:\r\n{ex.Message}");
                return null;
            }
        }

        public static bool? IsRegion(this HObject hObject)
        {
            //Check null
            if (hObject == null) return null;

            //Must have at least one item
            int iCount = hObject.CountObj();
            if (iCount == 0) return null;

            //Get type
            string sType = hObject.GetObjClass();
            var halconType = GetHalconType(sType);
            return halconType == _hObjectType.Region;
        }

        public static bool? IsContour(this HObject hObject)
        {
            //Check null
            if (hObject == null) return null;

            //Must have at least one item
            int iCount = hObject.CountObj();
            if (iCount == 0) return null;

            //Get type
            string sType = hObject.GetObjClass();
            var halconType = GetHalconType(sType);
            return halconType == _hObjectType.Contour;
        }


        /// <summary>
        /// Get item count without exception
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        public static int GetItemCount(this HObject hObject)
        {
            try
            {
                if (hObject == null || !hObject.IsInitialized()) return 0;
                int iCount = hObject.CountObj();
                return iCount;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"csHalconCommon.GetItemCount.Exception\r\n{e.GetMessageDetail()}");
                return -1;
            }
        }

        public static _hObjectType GetHalconType(this HObject hObject)
        {
            if (hObject == null) return _hObjectType.Undefined;
            int iCount = hObject.CountObj();
            //Empty object doesn't have class type
            if (iCount == 0) return _hObjectType.Undefined;

            string sType = hObject.GetObjClass();
            var halconType = GetHalconType(sType);
            return halconType;
        }

        public static _hObjectType GetHalconType(string sType)
        {
            if (sType == "image") return _hObjectType.Image;
            else if (sType == "region") return _hObjectType.Region;
            else if (sType == "contour" || sType == "xld_cont") return _hObjectType.Contour;
            else return _hObjectType.Undefined;
        }

        public static HTuple ToHalconAngleRad(this float fValue)
        {
            HTuple tValue = new HTuple(fValue);
            return tValue.TupleRad();
        }


        public static HTuple ToHalconAngleDegree(this float fValue)
        {
            HTuple tValue = new HTuple(fValue);
            return tValue.TupleDeg();
        }

        public static float ToAngleDegree(this float fValue)
        {
            return fValue * csHalconHelper.DegreePerRadian;
        }

        /// <summary>
        /// Modify any degree angles to the angle within +-90 range
        /// </summary>
        /// <param name="degreeList"></param>
        /// <returns></returns>
        public static HTuple ToSigned180AngleDegree(this HTuple degreeList)
        {
            List<double> doubleList = new List<double>();
            foreach (var d in degreeList.DArr)
            {
                var degree = d.ToSigned180AngleDegree();
                doubleList.Add(degree);
            }

            return new HTuple(doubleList.ToArray());
        }

        /// <summary>
        /// Modify any degree angle to angle within +-90 range
        /// </summary>
        /// <param name="fValue"></param>
        /// <returns></returns>
        public static float ToSigned180AngleDegree(this float fValue)
        {
            if (fValue > 90)
            {
                return ToSigned180AngleDegree(fValue - 180);
            }
            else if (fValue < -90)
            {
                return ToSigned180AngleDegree(fValue + 180);
            }
            else
            {
                return fValue;
            }
        }



        /// <summary>
        /// Modify any degree angle to angle within +-90 range
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        public static double ToSigned180AngleDegree(this double dValue)
        {
            if (dValue > 90)
            {
                return ToSigned180AngleDegree(dValue - 180);
            }
            else if (dValue < -90)
            {
                return ToSigned180AngleDegree(dValue + 180);
            }
            else
            {
                return dValue;
            }
        }

        /// <summary>
        /// Modify any degree angle to angle within +-180 range
        /// This operation will remove line arrow degree, keeps only line orientation
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        public static double ToSigned360AngleDegree(this double dValue)
        {
            if (dValue > 180)
            {
                return ToSigned360AngleDegree(dValue - 360);
            }
            else if (dValue < -180)
            {
                return ToSigned360AngleDegree(dValue + 360);
            }
            else
            {
                return dValue;
            }
        }

        /// <summary>
        /// From degree angle to radius angle
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        public static double ToRadianAngle(this double dValue)
        {
            if (dValue == 0) return 0;
            return dValue / csHalconHelper.DegreePerRadian;
        }

        /// <summary>
        /// From radius angle to degree angle
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        public static double ToDegreeAngle(this double dValue)
        {
            return dValue * csHalconHelper.DegreePerRadian;
        }

        /// <summary>
        /// Modify any degree angle to angle within +-180 range
        /// This operation will remove line arrow degree, keeps only line orientation
        /// </summary>
        /// <param name="fValue"></param>
        /// <returns></returns>
        public static float ToSigned360AngleDegree(this float fValue)
        {
            if (fValue > 180)
            {
                return ToSigned180AngleDegree(fValue - 360);
            }
            else if (fValue < -180)
            {
                return ToSigned180AngleDegree(fValue + 360);
            }
            else
            {
                return fValue;
            }
        }


        public static HTuple ToHalconTuple(this bool bValue)
        {
            var ht_Value = new HTuple(bValue);
            return ht_Value;
        }




    }


}

