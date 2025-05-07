using HalconDotNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;
using DevExpress.XtraExport.Helpers;
using DevExpress.Charts.Native;

namespace Deep_Learning_Demo
{

    public class csViewPortLayout
    {
        /// <summary>
        /// Link to window forms nothing to do with actual image
        /// </summary>
        public float ExtRow { get; set; }
        public float ExtCol { get; set; }
        public float ExtWidth { get; set; }
        public float ExtHeight { get; set; }

        /// <summary>
        /// View mapping to the actual image
        /// </summary>
        public int PartRow1 { get; set; }
        public int PartRow2 { get; set; }
        public int PartCol1 { get; set; }
        public int PartCol2 { get; set; }

        /// <summary>
        /// Mouse position linked to the window pixels
        /// </summary>
        public int MouseX { get; set; }
        public int MouseY { get; set; }
        public int MouseDelta { get; set; }

        /// <summary>
        /// Mouse position inside the halcon window
        /// </summary>
        public int MouseX_Halcon { get; set; }
        public int MouseY_Halcon { get; set; }

        /// <summary>
        /// Ext defined win display window size irrelavent to the actaul image
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetExtendInfo(HTuple row, HTuple col, HTuple width, HTuple height)
        {
            ExtRow = row;
            ExtCol = col;
            ExtWidth = width;
            ExtHeight = height;
        }

        /// <summary>
        /// Part info connect the actual image to the view port
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetPartInfo(HTuple row1, HTuple col1, HTuple row2, HTuple col2)
        {
            PartRow1 = (int)row1.D;
            PartCol1 = (int)col1.D;
            PartRow2 = (int)row2.D;
            PartCol2 = (int)col2.D;
        }

        public void SetMouseWindowPosition(MouseEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;
            MouseDelta = e.Delta;
        }

        public void SetMouseHalconPosition(HMouseEventArgs e)
        {
            MouseX_Halcon = (int)e.X;
            MouseY_Halcon = (int)e.Y;
        }
    }

    public class csHalconColors
    {
        public const string Red = "red";
        public const string Green = "green";
        public const string Blue = "blue";
        public const string Yellow = "yellow";

        /// <summary>
        /// Transparent color used for region display
        /// </summary>
        public const string RedTrans_80 = "#ff000080";
        public const string RedTrans_40 = "#ff000040";
        public const string GreenTrans_80 = "#00ff0080";
        public const string BlueTrans_80 = "#0000ff80";



        public static List<string> GetMultipleColorParameters(_HalconColor color, double solidness)
        {
            List<string> parameters = new List<string>();
            foreach (_HalconColor option in Enum.GetValues(typeof(_HalconColor)))
            {
                if (color.HasFlag(option))
                {
                    string sValue = GetSingleColorParameter(option, solidness);
                    parameters.Add(sValue);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Convert color to rgb gray value
        /// </summary>
        public static HTuple GetSingleColorGray(_HalconColor color)
        {
            string sColor = csEnumHelper<_HalconColor>.GetDefaultValue(color).ToString();
            byte colorR= Convert.ToByte(sColor.Substring(0,2),16);
            byte colorG = Convert.ToByte(sColor.Substring(2, 2), 16);
            byte colorB = Convert.ToByte(sColor.Substring(4, 2), 16);
            return new HTuple(new int[]{ (int)colorR , (int)colorG , (int)colorB });
        }

        public static string GetSingleColorParameter(_HalconColor color, double solidness)
        {

            string sColor = csEnumHelper<_HalconColor>.GetDefaultValue(color).ToString();
            //Get Solidness, make sure return similar format
            if (solidness <= 0)
                return $"#{sColor}10";//Return light color
            else if (solidness >= 1)
                return $"#{sColor}ff";//Return solid color

            //Get calculated color
            var ColorValue = (byte)(255 * solidness);
            string sSolid = BitConverter.ToString(new byte[] { ColorValue }).ToLower();
            
            return $"#{sColor}{sSolid}";
        }

    }


    [Flags]
    public enum _HalconColor
    {
        [XmlEnum("1"), DefaultValue("000000"), Description("Black")]
        Black = 1 << 0,
        [XmlEnum("2"), DefaultValue("ffffff"), Description("White")]
        White = 1 << 1,
        [XmlEnum("3"), DefaultValue("ff0000"), Description("Red")]
        Red = 1 << 2,
        [XmlEnum("4"), DefaultValue("00ff00"), Description("Green")]
        Green = 1 << 3,
        [XmlEnum("5"), DefaultValue("0000ff"), Description("Blue")]
        Blue = 1 << 4,
        [XmlEnum("6"), DefaultValue("00ffff"), Description("Cyan")]
        Cyan = 1 << 5,
        [XmlEnum("7"), DefaultValue("ff00ff"), Description("Magenta")]
        Magenta = 1 << 6,
        [XmlEnum("8"), DefaultValue("ffff00"), Description("Yellow")]
        Yellow = 1 << 7,
        [XmlEnum("9"), DefaultValue("696969"), Description("Dim Gray")]
        DimGray = 1 << 8,
        [XmlEnum("10"), DefaultValue("bebebe"), Description("Gray")]
        Gray = 1 << 9,
        [XmlEnum("11"), DefaultValue("d3d3d3"), Description("Light Gray")]
        LightGray = 1 << 10,
        [XmlEnum("12"), DefaultValue("7b68ee"), Description("Medium Slate Blue")]
        MediumSlateBlue = 1 << 11,
        [XmlEnum("13"), DefaultValue("ff7f50"), Description("Coral")]
        Coral = 1 << 12,
        [XmlEnum("14"), DefaultValue("6a5acd"), Description("Slate Blue")]
        SlateBlue = 1 << 13,
        [XmlEnum("15"), DefaultValue("00ff7f"), Description("Spring Green")]
        SpringGreen = 1 << 14,
        [XmlEnum("16"), DefaultValue("ff4500"), Description("Orange Red")]
        OrangeRed = 1 << 15,
        [XmlEnum("17"), DefaultValue("ffa500"), Description("Orange")]
        Orange = 1 << 16,
        [XmlEnum("18"), DefaultValue("556b2f"), Description("Dark Olive Green")]
        DarkOliveGreen = 1 << 17,
        [XmlEnum("19"), DefaultValue("ffc0cb"), Description("Pink")]
        Pink = 1 << 18,
        [XmlEnum("20"), DefaultValue("5f9ea0"), Description("Cadet Blue")]
        CadetBlue = 1 << 19,
    }



    /// <summary>
    /// Rectangel without angle info (Angle=0) 
    /// </summary>
    public class Rectange1Data
    {
        [Browsable(false)]
        public float Row1 { get; set; }
        [Browsable(false)]
        public float Column1 { get; set; }
        [Browsable(false)]
        public float Row2 { get; set; }
        [Browsable(false)]
        public float Column2 { get; set; }

        [DisplayName("Row")]
        public float RowCenter { get; set; }

        [DisplayName("Column")]
        public float ColumnCenter { get; set; }

        [DisplayName("Width")]
        public float Width { get; set; }

        [DisplayName("Height")]
        public float Height { get; set; }

        /// <summary>
        /// Top position of the char, used to show text
        /// </summary>
        [Browsable(false)]
        public HalconPoint charTop { get; set; }
        /// <summary>
        /// Bottom position of the char, used to show text
        /// </summary>
        [Browsable(false)]
        public HalconPoint charBottom { get; set; }

        public Rectange1Data()
        {

        }

        public void Display(HTuple window)
        {
            HOperatorSet.DispRectangle1(window,(double)Row1, (double)Column1, (double)Row2, (double)Column2);
        }

        public void Load(double r1, double c1, double r2, double c2)
        {
            Load((float)r1, (float)c1, (float)r2, (float)c2);
        }

        public void Load(float r1, float c1, float r2, float c2)
        {
            Row1 = r1; Column1 = c1; Row2 = r2; Column2 = c2;
            Width = Math.Abs(Column1 - Column2);
            Height = Math.Abs(Row1 - Row2);
            RowCenter = Math.Max(Row1, Row2) - Height / 2;
            ColumnCenter = Math.Max(Column1, Column2) - Width / 2;
            charTop = new HalconPoint(Row1 - 25, ColumnCenter - 5);
            charBottom = new HalconPoint(Row2 + 5, ColumnCenter - 5);
        }

        public bool MapRectangle1(HTuple mapMatrix)
        {
            try
            {
                //Affine center point
                HOperatorSet.AffineTransPoint2d(mapMatrix, RowCenter, ColumnCenter, out HTuple newRowCenter, out HTuple newColumnCenter);
                RowCenter = (float)newRowCenter.D;
                ColumnCenter = (float)newColumnCenter.D;

                //Affine left point
                HOperatorSet.AffineTransPoint2d(mapMatrix, Row1, Column1, out HTuple newRow1, out HTuple newCol1);
                Row1 = (float)newRow1.D;
                Column1 = (float)newCol1.D;

                //Affine right point
                HOperatorSet.AffineTransPoint2d(mapMatrix, Row2, Column2, out HTuple newRow2, out HTuple newCol2);
                Row2 = (float)newRow2.D;
                Column2 = (float)newCol2.D;

                //Map top point
                HOperatorSet.AffineTransPoint2d(mapMatrix, charTop.Row, charTop.Column, out HTuple newTopRow, out HTuple newTopColumn);
                charTop.Row = (float)newTopRow.D;
                charTop.Column = (float)newTopColumn.D;

                //Map bottom point
                HOperatorSet.AffineTransPoint2d(mapMatrix, charBottom.Row, charBottom.Column, out HTuple newBottomRow, out HTuple newBottomColumn);
                charBottom.Row = (float)newBottomRow.D;
                charBottom.Column = (float)newBottomColumn.D;

                //Complete
                return true;
            }
            catch (Exception ex)
            {
                string sMessage = $"MapRectangle1.Exception:\r\n{ex.Message}";
                Trace.WriteLine(sMessage);
                return false;
            }

        }
    }

    [XmlType("ImageSize")]
    public class csImageSize
    {

        public int Width { get; set; }
        public int Height { get; set; }

        public csImageSize(HTuple width, HTuple height)
        {
            Width = width.GetHalconInt();
            Height = height.GetHalconInt();
        }
    }

    /// <summary>
    /// Rectangle area with orientation
    /// Match the Halcon rect2 type
    /// </summary>
    public class Rectange2Data
    {

        [Browsable(false)]
        public float Row { get; set; }
        [Browsable(false)]
        public float Column { get; set; }

        /// <summary>
        /// Angle in radian
        /// </summary>
        [Browsable(false)]

        public float Phi { get; set; }



        /// <summary>
        /// half width
        /// </summary>
        [Browsable(false)]
        public float Length1 { get; set; }
        /// <summary>
        /// Half height
        /// </summary>
        [Browsable(false)]
        public float Length2 { get; set; }

        /// <summary>
        /// Indicate this rectangle is previouly defined or not
        /// </summary>
        public bool IsInit { get; set; }

        public Rectange2Data()
        {

        }

        public void Init(float fRow, float fColumn, float fPhi, float fLength1, float fLength2)
        {
            Row = fRow;
            Column = fColumn;
            Phi = fPhi;
            Length1 = fLength1;
            Length2 = fLength2;
            IsInit = true;


        }

        public HTuple ToHtuple()
        {

            List<float> parameters = new List<float>() { Row, Column, Phi, Length1, Length2 };
            HTuple ht_Value = new HTuple(parameters.ToArray());
            return ht_Value;
        }

        public void Init(HTuple hRow, HTuple hColumn, HTuple hPhi, HTuple hLength1, HTuple hLength2)
        {
            Row = (float)hRow.D;
            Column = (float)hColumn.D;
            Phi = (float)hPhi.D;
            Length1 = (float)hLength1.D;
            Length2 = (float)hLength2.D;
            IsInit = true;
        }

        /// <summary>
        /// Only works without scale
        /// </summary>
        /// <param name="mappingMatrix"></param>
        /// <returns></returns>
        public bool MapRegion(HTuple mappingMatrix)
        {
            try
            {
                //Don't map the whole rect2 directly since it will lose the angle info
                //Alignment angle +-180， when mapping rectangle the angle range will be +-90

                //Transfor the center point
                HOperatorSet.AffineTransPoint2d(mappingMatrix, Row, Column, out HTuple centerRow, out HTuple centerColumn);


                //Default angle range is +-90, calculate the actual orientation (+-180) by using a reference line
                var originRefPoint = GetDirectionPoint();
                //Shift the reference point
                HOperatorSet.AffineTransPoint2d(mappingMatrix, originRefPoint.Row, originRefPoint.Column, out HTuple newRefRow, out HTuple newRefCOlumn);


                //Get the new angle
                csLineData refLine = new csLineData();
                refLine.Init(centerRow, centerColumn, newRefRow, newRefCOlumn);



                //Read result
                Row = (float)centerRow.D;
                Column = (float)centerColumn.D;
                Phi = refLine.RotationRadian;
                //Keep leng1 and leng2 the same

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Rectange2Data.Exception:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get point from the center of the rectangel2 to the center point of the edge
        /// </summary>
        private HalconPoint GetDirectionPoint()
        {
            try
            {

                //Get the reference point
                //Assume the phi is 0
                var referencePoint = new HalconPoint(Row, Column + Length1);
                HOperatorSet.VectorAngleToRigid(Row, Column, 0, Row, Column, Phi, out HTuple horiMat);

                //Get actual point by mapping
                HOperatorSet.AffineTransPoint2d(horiMat, referencePoint.Row, referencePoint.Column, out HTuple newRow, out HTuple newCol);

                referencePoint.Row = (float)newRow.D;
                referencePoint.Column = (float)newCol.D;

                return referencePoint;


            }
            catch (Exception ex)
            {

                Trace.WriteLine("GetDirectionPoint:\r\n" + ex.Message);
                return null;
            }
        }

        public void CreateSampleRegion(HTuple halconWindow)
        {
            HOperatorSet.GetPart(halconWindow, out HTuple row1, out HTuple column1, out HTuple row2, out HTuple column2);
            float iHeightSegment = (float)(row2.D - row1.D) / 4;
            float iWidthSegment = (float)(column2.D - column1.D) / 4;

            Row = (float)row1.D + 2 * iHeightSegment;
            Column = (float)column1.D + 2 * iWidthSegment;
            Phi = 0;
            Length1 = iWidthSegment;
            Length2 = iHeightSegment;
            IsInit = true;
        }

        public HObject CreateRegion()
        {
            try
            {
                HOperatorSet.GenRectangle2(out HObject Rectangle, Row, Column, Phi, Length1, Length2);
                return Rectangle;
            }
            catch (Exception ex)
            {

                Trace.WriteLine("Rectange2Data.CreateRegion\r\n" + ex.Message);
                return null;
            }

        }

        public bool LoadFromRectRegion(HObject hoRegion)
        {
            try
            {
                if (hoRegion == null ||
                    !hoRegion.IsInitialized())
                {
                    return false;
                }

                HOperatorSet.SmallestRectangle2(hoRegion, out HTuple row, out HTuple col, out HTuple phi, out HTuple length1, out HTuple length2);
                Init(row, col, phi, length1, length2);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"LoadFromRectRegion:{e.GetMessageDetail()}");
                return false;
            }

            //Pass all
            return true;
        }

    }

    /// <summary>
    /// Drawing shape type
    /// </summary>
    public enum _shapeType
    {
        Line,
        Rectangle2
    }

    [XmlType("LineData")]
    public class csLineData
    {
        public float Row1 { get; set; }

        [DisplayName("Col.1")]
        public float Column1 { get; set; }

        public float Row2 { get; set; }
        [DisplayName("Col.2")]
        public float Column2 { get; set; }


        public float RotationRadian { get; set; }
        /// <summary>
        /// Line angle is same when using as horizental degree by default
        /// 90~0~-90 Degree, with continous degrees
        /// </summary>
        [DisplayName("Degree")]
        public float RotationDegree => GetRotationDegree();


        public float CenterRow { get; set; }
        public float CenterColumn { get; set; }

        /// <summary>
        /// Line length
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// Indicate whether this line object been first initialized
        /// </summary>
        [Browsable(false)]
        public bool IsInit { get; set; }


        public csLineData() { }

        public csLineData(HTuple hRow1, HTuple hColumn1, HTuple hRow2, HTuple hColumn2)
        {
            Init(hRow1, hColumn1, hRow2, hColumn2);
        }



        public void Init(HTuple hRow1, HTuple hColumn1, HTuple hRow2, HTuple hColumn2)
        {
            Row1 = (float)hRow1.D;
            Column1 = (float)hColumn1.D;
            Row2 = (float)hRow2.D;
            Column2 = (float)hColumn2.D;
            GenLinePosition();
            IsInit = true;
        }


        public void Init(float fRow1, float fColumn1, float fRow2, float fColumn2)
        {
            Row1 = fRow1;
            Column1 = fColumn1;
            Row2 = fRow2;
            Column2 = fColumn2;
            GenLinePosition();
            IsInit = true;
        }

        /// <summary>
        /// Line angle is same when using as horizental degree by default
        /// 90~0~-90 Degree, with continous degrees
        /// When in vertical angle will jump from -90 to +90 without gap
        /// Causing alignment function not working properly
        /// </summary>
        private float GetVerticalSafeDegree()
        {
            if (RotationDegree < 0)
            {
                var degree = 180 - (-RotationDegree);
                return degree;
            }
            else
            {
                return RotationDegree;
            }
        }

        /// <summary>
        /// Line angle is same when using as horizental degree by default
        /// 90~0~-90 Degree, with continous degrees
        /// When in vertical angle will jump from -90 to +90 without gap
        /// Causing alignment function not working properly
        /// </summary>
        public float GetVerticalSafePhi()
        {
            HTuple safeDegree = GetVerticalSafeDegree();
            return (float)safeDegree.TupleRad().D;
        }


        private float GetRotationDegree()
        {
            HTuple rad = RotationRadian;
            float degree = (float)rad.TupleDeg().D;

            return degree;
        }

        /// <summary>
        /// Generate line position data for future usage
        /// </summary>
        public void GenLinePosition()
        {
            try
            {
                HOperatorSet.LinePosition(Row1, Column1, Row2, Column2, out HTuple centerRow, out HTuple centerColumn, out HTuple length, out HTuple phi);
                CenterRow = (float)centerRow.D;
                CenterColumn = (float)centerColumn.D;
                Length = (float)length.D;

                //Line orientation angle from position is +-90, to match the alignment +-180, transformation is required
                HTuple degreeRead = phi.TupleDeg();
                if ((degreeRead.D > 0 && degreeRead.D <= 90) || degreeRead.D == -90)
                {//Check verticial orientation 
                    if (Row1 - Row2 > 0)
                    {
                        RotationRadian = (float)phi.D;
                    }
                    else
                    {
                        HTuple valueDegree = -(180 - degreeRead.D);
                        RotationRadian = (float)valueDegree.TupleRad().D;
                    }
                }
                else if (degreeRead.D <= 0 && degreeRead.D > -90)
                {//Check horizontal orientation
                    if (Column2 - Column1 > 0)
                    {
                        RotationRadian = (float)phi.D;
                    }
                    else
                    {
                        HTuple valueDegree = 180 - (-degreeRead.D);
                        RotationRadian = (float)valueDegree.TupleRad().D;
                    }
                }
                else
                {
                    RotationRadian = (float)phi.D;
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine("LineData.GenLinePosition:\r\n" + ex.Message);
            }
        }

        public void LoadSampleLine(HTuple halconWindow, bool IsVertical)
        {
            HOperatorSet.GetPart(halconWindow, out HTuple row1, out HTuple column1, out HTuple row2, out HTuple column2);
            float iHeightSegment = (float)(row2.D - row1.D) / 4;
            float iWidthSegment = (float)(column2.D - column1.D) / 4;

            if (IsVertical)
            {//Set default vertical line
                Row1 = (float)row1.D + 3 * iHeightSegment;
                Column1 = (float)column1.D + iWidthSegment;
                Row2 = (float)row1.D + iHeightSegment;
                Column2 = (float)column1.D + iWidthSegment;
            }
            else
            {//Set default horizontal line
                Row1 = (float)row1.D + 3 * iHeightSegment;
                Column1 = (float)column1.D + iWidthSegment;
                Row2 = (float)row1.D + 3 * iHeightSegment;
                Column2 = (float)column1.D + 3 * iWidthSegment;
            }

            IsInit = true;

            GenLinePosition();
        }

        public string GetLineDisplayText()
        {
            if (!IsInit) return "N/A";

            return $"{Row1.ToString("f0")},{Column1.ToString("f0")},{Row2.ToString("f0")},{Column2.ToString("f0")}";

        }

        public bool MapRegion(HTuple mappingMatrix)
        {
            try
            {
                HOperatorSet.AffineTransPixel(mappingMatrix, Row1, Column1, out HTuple newRow1, out HTuple newCol1);
                HOperatorSet.AffineTransPixel(mappingMatrix, Row2, Column2, out HTuple newRow2, out HTuple newCol2);
                Init(Row1, Column1, Row2, Column2);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LineData.Exception:{ex.Message}");
                return false;
            }
        }



        /// <summary>
        /// XLD display object is sub-pixel based
        /// </summary>
        /// <returns></returns>
        public HObject GetLineXld()
        {
            if (!IsInit) return null;

            try
            {
                var row = new HTuple(new double[] { Row1, Row2 });
                var col = new HTuple(new double[] { Column1, Column2 });
                HOperatorSet.GenContourPolygonXld(out HObject contourLine, row, col);
                return contourLine;

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }
    }

    public class csDisplayArrow : csDisplayItemBase
    {
        /// <summary>
        /// Start point
        /// </summary>
        public float Row1 { get; set; }
        public float Column1 { get; set; }

        /// <summary>
        /// End point
        /// </summary>
        public float Row2 { get; set; }
        public float Column2 { get; set; }

        /// <summary>
        /// Arrow size
        /// </summary>
        public float Size { get; set; }

        public csDisplayArrow()
        {

        }

        /// <summary>
        /// Directly get rectange's direction display
        /// </summary>
        /// <param name="rectange2Data"></param>
        public csDisplayArrow(Rectange2Data rectange2Data, string sToolID)
        {
            ToolID = sToolID;

            //Set start point
            Row1 = rectange2Data.Row;
            Column1 = rectange2Data.Column;

            //Calculate second point
            float fArrowLength1 = rectange2Data.Length1 - 20; //Inside the rectangle
            float fArrowLength2 = rectange2Data.Length1 + 50; //Outside the rectangle
            float fRowDis1 = (float)Math.Sin(rectange2Data.Phi) * fArrowLength1;
            float fColDis1 = (float)Math.Cos(rectange2Data.Phi) * fArrowLength1;
            float fRowDis2 = (float)Math.Sin(rectange2Data.Phi) * fArrowLength2;
            float fColDis2 = (float)Math.Cos(rectange2Data.Phi) * fArrowLength2;
            Row1 = rectange2Data.Row - fRowDis1;
            Row2 = rectange2Data.Row - fRowDis2;
            Column1 = rectange2Data.Column + fColDis1;
            Column2 = rectange2Data.Column + fColDis2;

            Size = 4;
        }



    }

    public class PositionData
    {
        public float Row { get; set; }
        public float Column { get; set; }

        [Browsable(false)]
        public float RotationRadian { get; set; }

        [DisplayName("Degree")]
        public float RotationDegree => (float)(new HTuple(RotationRadian).TupleDeg().D);

        public void Load(PositionData position)
        {
            Row = position.Row;
            Column = position.Column;
            RotationRadian = position.RotationRadian;
        }

        public void Load(HalconPoint halconPoint)
        {
            Row = halconPoint.Row;
            Column = halconPoint.Column;
        }

        public void LoadLineCenter(csLineData line)
        {
            Row = line.CenterRow;
            Column = line.CenterColumn;
            RotationRadian = line.RotationRadian;
        }





        public PositionData Copy()
        {
            return new PositionData() { Row = Row, Column = Column, RotationRadian = RotationRadian };
        }
    }

    public class PositionView : PositionData
    {
        public string Name { get; set; }
    }

   


    public enum _imageTextType
    {
        /// <summary>
        /// Show text at defined position
        /// </summary>
        [XmlEnum("0")]
        Default = 0,
        /// <summary>
        /// Adjust the text to allow show text in the center of the defined position
        /// </summary>
        [XmlEnum("1")]
        TextBox_GreenText = 1,
        [XmlEnum("2")]
        TextBox_RedText = 2,
    }

    [XmlType("WindowText")]
    public class csDisplayWindowText : csDisplayItemBase
    {
        public string Text { get; set; }

        /// <summary>
        /// Valid color parameter can be directly used by the halcon
        /// </summary>
        public string HalconColor { get; set; }

        public csDisplayWindowText(string sText, string sHalconColor, string sToolID = null)
        {
            Text = sText;
            HalconColor = sHalconColor;
            ToolID = sToolID;
        }
    }

    public class HalconPoint
    {
        public float Row { get; set; }
        public float Column { get; set; }

        public HalconPoint()
        {
            Reset();
        }

        public HalconPoint(int iRow, int iColumn)
        {
            Row = iRow;
            Column = iColumn;
        }

        public HalconPoint(long lRow, long lColumn)
        {
            Row = (int)lRow;
            Column = (int)lColumn;
        }

        public HalconPoint(float fRow, float fColumn)
        {
            Row = fRow;
            Column = fColumn;
        }

        public HalconPoint(double dRow, double dColumn)
        {
            Row = (float)dRow;
            Column = (float)dColumn;
        }

        public HalconPoint(HTuple fRow, HTuple fColumn)
        {
            Row = (float)fRow.D;
            Column = (float)fColumn.D;
        }

        public void Reset()
        {
            Row = -1;
            Column = -1;
        }

        public bool IsValid()
        {
            return Row != -1 && Column != -1;
        }

        public bool WithInImage(HObject image)
        {
            try
            {
                HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);

                if (Row < 0 || Row > height.L) return false;

                if (Column < 0 || Column > width.L) return false;

                return true;

            }
            catch (Exception e)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} HalconPoint.WithInImage.Exception:\r\n{e.GetMessageDetail()}");
                return false;
            }
        }
    }


    /// <summary>
    /// Record the display port
    /// </summary>
    public class csViewPort
    {
        public float Row1 { get; set; }
        public float Col1 { get; set; }
        public float Row2 { get; set; }
        public float Col2 { get; set; }
        public csViewPort()
        {

        }

        public csViewPort(float _row1, float _col1, float _row2, float _col2)
        {
            Init(_row1, _col1, _row2, _col2);
        }


        public csViewPort(int _row1, int _col1, int _row2, int _col2)
        {
            Init(_row1, _col1, _row2, _col2);
        }

        public csViewPort(double _row1, double _col1, double _row2, double _col2)
        {
            Init((float)_row1, (float)_col1, (float)_row2, (float)_col2);
        }



        private void Init(float _row1, float _col1, float _row2, float _col2)
        {
            Row1 = _row1;
            Col1 = _col1;
            Row2 = _row2;
            Col2 = _col2;
        }

        public string GetDisplay()
        {
            return $"{Row1.ToString("f1")},{Col1.ToString("f1")},{Row2.ToString("f1")},{Col2.ToString("f1")}";
        }
    }

    public class csDrawItem
    {
        /// <summary>
        /// Indicate whether a draw action is in process
        /// </summary>
        public bool IsDrawing { get; set; }

        /// <summary>
        /// Drawing object handle
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public HTuple DrawHandle { get; set; }

        /// <summary>
        /// Position from orgin to current position
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public HTuple PositionOffsetMatrix { get; set; }

        /// <summary>
        /// Currently active draw region type
        /// </summary>
        public _shapeType DrawShape { get; set; }

        /// <summary>
        /// Currently active draw rectangle2
        /// </summary>
        public Rectange2Data Rectangle2 { get; set; }

        /// <summary>
        /// Currently active draw line
        /// </summary>
        public csLineData Line { get; set; }
    }

    public enum _hObjectType
    {
        [XmlEnum("0")]
        Image,
        [XmlEnum("1")]
        Region,
        [XmlEnum("2")]
        Contour,
        [XmlEnum("99")]
        Undefined
    }

}
