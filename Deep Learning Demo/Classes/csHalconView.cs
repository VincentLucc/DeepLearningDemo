using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.XtraBars.Docking2010.Views;
using HalconDotNet;
using Newtonsoft.Json;

namespace OpenCV_Halcon_Demo
{
    [Serializable]
    public class HalconView
    {
        public _viewType ViewType;

        public DateTime CreateTime = csDateTimeHelper.CurrentTime;

        public HObject ViewImage;
        public object ViewImageLock = new object();
        public bool IsViewImageValid => CheckViewImageValid();

        /// <summary>
        /// Halcon objects for display
        /// </summary>
        public List<csDisplayItemBase> DisplayItems { get; set; }
        public object lockDisplayItems = new object();

        public HalconView()
        {
            InitObjects();
        }

        public HalconView(HObject viewImage)
        {
            InitObjects();
            SetViewImage(viewImage);
        }

        private void InitObjects()
        {
            ViewType = _viewType.General;
            DisplayItems = new List<csDisplayItemBase>();
        }

        public List<csDisplayWindowText> GetWindowsTexts()
        {
            List<csDisplayWindowText> windowTexts = new List<csDisplayWindowText>();
            lock (lockDisplayItems)
            {
                foreach (var dispItem in DisplayItems)
                {
                    if (dispItem is csDisplayWindowText windowText)
                    {
                        windowTexts.Add(windowText);
                    }
                }
            }

            return windowTexts;
        }

        public HObject GetLastDisplayImageItem()
        {
            lock (lockDisplayItems)
            {
                //Get from last to front
                for (int i = DisplayItems.Count - 1; i >= 0; i--)
                {
                    var dispItem = DisplayItems[i];
                    if (dispItem is csDisplayHObject dispObject)
                    {
                        if (dispObject.Type == _DisplayObjectType.Image)
                        {
                            return dispObject.ViewItem;
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Making sure image always shown before other objects
        /// </summary>
        public void SortDisplayObjects()
        {
            lock (lockDisplayItems)
            {
                List<csDisplayItemBase> imageItems = new List<csDisplayItemBase>();
                List<csDisplayItemBase> otherItems = new List<csDisplayItemBase>();

                foreach (var displayItem in DisplayItems)
                {
                    var dispObject = displayItem as csDisplayHObject;
                    if (dispObject == null || dispObject.Type != _DisplayObjectType.Image)
                    {
                        otherItems.Add(displayItem);
                    }
                    else
                    {
                        imageItems.Add(displayItem);
                    }
                }

                DisplayItems.Clear();
                DisplayItems.AddRange(imageItems);
                DisplayItems.AddRange(otherItems);
            }
        }



        public void AddImageText(string sText, HTuple hRow, HTuple hColumn, string sToolID = null, _imageTextType textType = _imageTextType.Default)
        {
            if (string.IsNullOrWhiteSpace(sText)) return;

            var newText = new csDisplayImageText(sText, hRow, hColumn, sToolID, textType);

            lock (lockDisplayItems)
            {
                DisplayItems.Add(newText);
            }
        }



        public void AddMappedImageText(string sText, HTuple hRow, HTuple hColumn, HTuple mapMatrix)
        {
            try
            {

                HOperatorSet.AffineTransPoint2d(mapMatrix, hRow, hColumn, out HTuple newRow, out HTuple newColumn);
                AddImageText(sText, newRow, newColumn);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AddMappedImageText.Exception:\r\n" + ex.Message);
            }
        }




        public void SetViewImage(HObject Image)
        {
            ClearAll();


            lock (ViewImageLock)
            {
                try
                {
                    //Avoid source image been changed
                    if (Image != null)
                    {
                        //ViewImage = Image.Clone();
                        HOperatorSet.CopyImage(Image, out ViewImage);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("SetViewImage:\r\n" + ex.Message);
                }
            }
        }

        /// <summary>
        /// Only modify the view image
        /// Don't keep the old image
        /// </summary>
        public void ReplaceViewImage(HObject input)
        {
            lock (ViewImageLock)
            {
                if (ViewImage != null)
                {
                    ViewImage?.Dispose();
                    ViewImage = null;
                }

                ViewImage = input;
            }

        }

        public HObject GetViewImage()
        {
            lock (ViewImageLock)
            {
                try
                {
                    //Avoid source image been changed
                    if (ViewImage == null || !ViewImage.IsInitialized()) return null;
                    return ViewImage.Clone();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("GetViewImage:\r\n" + ex.Message);
                    return null;
                }
            }
        }

        public void ClearViewImage()
        {
            lock (ViewImageLock)
            {
                if (ViewImage != null)
                {
                    ViewImage?.Dispose();
                    ViewImage = null;
                }
            }
        }

        public bool CheckViewImageValid()
        {
            lock (ViewImageLock)
            { return ViewImage != null && ViewImage.IsInitialized(); }
        }

        public void SetDisplayObject(HObject ho_Display, string sToolID, _DisplayObjectType displayType = _DisplayObjectType.General)
        {

            ClearDisplayItems();

            lock (lockDisplayItems)
            {
                if (ho_Display == null) return;
                var displayObject = new csDisplayHObject()
                {
                    ViewItem = ho_Display.Clone(),
                    Type = displayType,
                    ToolID = sToolID
                };

                DisplayItems.Add(displayObject);
            }
        }

        public void AddDisplayRect1(Rectange1Data displayItem, string sToolID)
        {

            var rect1 = new Rectange1Data();
            rect1.Load(displayItem.Row1, displayItem.Column1, displayItem.Row2, displayItem.Column2);

            var dispItem = new csDisplayRect1()
            {
                Rect1 = rect1,
                ToolID = sToolID
            };


            lock (lockDisplayItems)
            {
                DisplayItems.Add(dispItem);
            }
        }

        public void AddDisplayHalconObject(HObject ho_Display, string sToolID, _DisplayObjectType displayType = _DisplayObjectType.General, int iItemOrder = 0)
        {
            lock (lockDisplayItems)
            {
                if (ho_Display == null) return;
                var displayObject = new csDisplayHObject()
                {
                    ViewItem = ho_Display.Clone(),
                    Type = displayType,
                    ToolID = sToolID,
                    ItemOrder = iItemOrder
                };

                DisplayItems.Add(displayObject);
            }
        }

        public void AddMappedDisplayRegion(HObject ho_Display, HTuple mapMatrix, string sToolID, _DisplayObjectType displayType = _DisplayObjectType.General)
        {
            try
            {
                if (ho_Display == null) return;
                HOperatorSet.AffineTransRegion(ho_Display, out HObject ho_Affine, mapMatrix, "nearest_neighbor");
                AddDisplayHalconObject(ho_Affine, sToolID, displayType);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AddMappedDisplayObject.Exception:" + ex.Message);
                return;
            }
        }

        public void ClearDisplayItems()
        {
            lock (lockDisplayItems)
            {
                //Init all
                //Manual dispose
                foreach (var item in DisplayItems)
                {
                    if (item is csDisplayHObject)
                    {
                        var hObject = item as csDisplayHObject;
                        hObject.ViewItem?.Dispose();
                    }
                }
                DisplayItems.Clear();
            }
        }


        /// <summary>
        /// Int,float,double,htuple
        /// </summary>
        /// <param name="oRow"></param>
        /// <param name="oColumn"></param>
        public void AddMark(object oRow, object oColumn, string sToolID = null)
        {
            //Htuple might contains multiple marks
            if (oRow is HTuple && oColumn is HTuple)
            {
                var hRow = (HTuple)oRow;
                var hCol = (HTuple)oColumn;

                if (hRow.Type == HTupleType.DOUBLE)
                {
                    if (hRow.DArr.Length != hCol.DArr.Length) return;

                    lock (lockDisplayItems)
                    {
                        for (int i = 0; i < hRow.DArr.Length; i++)
                        {
                            var pData = new HalconPoint(hRow.DArr[i], hCol.DArr[i]);
                            DisplayItems.Add(new csDisplayMark(pData, sToolID));
                        }
                    }
                }
                else if (hRow.Type == HTupleType.INTEGER)
                {
                    if (hRow.IArr.Length != hCol.IArr.Length) return;

                    lock (lockDisplayItems)
                    {
                        for (int i = 0; i < hRow.IArr.Length; i++)
                        {
                            var pData = new HalconPoint(hRow.IArr[i], hCol.IArr[i]);
                            DisplayItems.Add(new csDisplayMark(pData, sToolID));
                        }
                    }
                }
            }
            else
            {
                var newMark = new csDisplayMark(oRow, oColumn);

                lock (lockDisplayItems)
                {
                    DisplayItems.Add(newMark);
                }
            }
        }




        /// <summary>
        /// Add rectange's direction display
        /// </summary>
        /// <param name="rectange2Data"></param>
        public void AddArrow(Rectange2Data rectange2Data, string sToolID = null)
        {
            var arrowData = new csDisplayArrow(rectange2Data, sToolID);

            lock (lockDisplayItems)
            {
                DisplayItems.Add(arrowData);
            }
        }


        public void AddWindowText(string sText, string sHalconColor = csHalconColors.Red, string sToolID = null)
        {
            var winText = new csDisplayWindowText(sText, sHalconColor, sToolID);
            lock (lockDisplayItems)
            {
                DisplayItems.Add(winText);
            }
        }




        public void ClearAll()
        {
            ClearViewImage();
            ClearAllDisplayItems();
        }

        public void ClearAllDisplayItems()
        {
            ClearDisplayItems();
        }



        public HalconView CloneView()
        {
            var newView = new HalconView();

            lock (ViewImageLock)
            {
                if (ViewImage != null)
                    newView.ViewImage = ViewImage.Clone();
            }

            lock (lockDisplayItems)
            {
                foreach (var item in DisplayItems)
                {
                    if (item is csDisplayHObject displayHObject)
                    {
                        displayHObject.Clone();
                        newView.DisplayItems.Add(displayHObject);
                    }
                    else
                    {//No dispose method, directly copy
                        newView.DisplayItems.Add(item);
                    }
                }
            }

            return newView;

        }


        /// <summary>
        /// The image is only used for saving purpose
        /// The output image is a rgb image
        /// </summary>
        /// <returns></returns>
        public HObject GenerateViewImage(out string sMessage)
        {
            //Create an empty
            HObject displayImage;
            sMessage = string.Empty;

            try
            {
                //Check clear screen request
                if (this.ViewType == _viewType.ClearScreen) return null;

                //Copy the base image
                lock (this.ViewImageLock)
                {
                    if (!this.ViewImage.IsValid())
                    {
                        sMessage = "The view image is empty.";
                        return null;
                    }

                    displayImage = this.ViewImage.Clone();
                }

                //Make sure the image is a rgb image
                if (displayImage.ChannelCount() == 1)
                {
                    var imageRGB = displayImage.GrayImageToRGBImage();
                    displayImage = imageRGB;
                }

                //Paint generic objects
                PaintGenericObjects(displayImage);
                //Paint window info
                return displayImage;
            }

            catch (Exception ex)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} csHalconWindow.GenerateViewImage:\r\n{ex.ToString()}");

                return null;
            }
        }


        /// <summary>
        /// Paint object on the image
        /// </summary>
        /// <param name="displayImage"></param>
        private void PaintGenericObjects(HObject displayImage)
        {
            try
            {
                SortDisplayObjects();

                lock (this.lockDisplayItems)
                {
                    foreach (var item in this.DisplayItems)
                    {
                        if (item is csDisplayHObject dispObjectItem)
                        {
                            PaintGenericHobjectAction(displayImage, dispObjectItem);
                        }
                        else if (item is csDisplayMark markItem)
                        {
                            if (!markItem.MarkPoint.IsValid()) continue;
                            HTuple hv_Green = csHalconColors.GetSingleColorGray(_HalconColor.Green);
                            HOperatorSet.GenCrossContourXld(out HObject cross, markItem.MarkPoint.Row, markItem.MarkPoint.Column, 30, csHalconHelper.HalconDegree45);
                            HOperatorSet.GenRegionContourXld(cross, out HObject regionContour, "margin");
                            HOperatorSet.OverpaintRegion(displayImage, regionContour, hv_Green, "fill");
                            regionContour.Dispose();
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} csHalconWindow.PaintGenericObjects.Exception\r\n {e.GetMessageDetail()}");
            }
        }



        /// <summary>
        /// Directly paint on the origin image
        /// fast process speed
        /// </summary>
        /// <param name="displayImage"></param>
        /// <param name="dispObject"></param>
        private void PaintGenericHobjectAction(HObject displayImage, csDisplayHObject dispObject)
        {
            try
            {
                //Make sure display item is valid
                if (dispObject == null ||
                    dispObject.ViewItem == null ||
                    !dispObject.ViewItem.IsInitialized())
                {
                    return;
                }

                HTuple hv_Green = csHalconColors.GetSingleColorGray(_HalconColor.Green);


                switch (dispObject.ViewItem.GetHalconType())
                {
                    case _hObjectType.Image:
                        HOperatorSet.OverpaintGray(dispObject.ViewItem, displayImage);
                        break;
                    case _hObjectType.Region:
                        HOperatorSet.OverpaintRegion(displayImage, dispObject.ViewItem, hv_Green, "margin");
                        break;
                    case _hObjectType.Contour:
                        HOperatorSet.GenRegionContourXld(dispObject.ViewItem, out HObject regionContour, "margin");
                        HOperatorSet.OverpaintRegion(displayImage, regionContour, hv_Green, "fill");
                        regionContour.Dispose();
                        break;
                    case _hObjectType.Undefined:
                        break;
                }


            }
            catch (Exception e)
            {
                Trace.WriteLine($"csHalconView.PaintGenericHobjectAction.Exception:\r\n" + e.GetMessageDetail());
            }

        }

    }


    public class HalconViewBuffer
    {
        /// <summary>
        /// Used by other thread to request view update, 
        /// No direct view update to avoid UI Jam
        /// If inspection exist, buffer saves
        /// </summary>
        public ConcurrentQueue<HalconView> Buffers { get; set; }
        public int BufferLimit = 5;
        public int BufferCount => Buffers.Count;

        public HalconView LastView;

        public HalconViewBuffer()
        {
            Buffers = new ConcurrentQueue<HalconView>();
        }

        public HalconView AddImageView(HObject ho_Image)
        {
            //Create new view
            HalconView view = new HalconView();
            view.SetViewImage(ho_Image);

            AddView(view);
            return view;
        }

        public HalconView AddEmptyView()
        {
            HalconView view = new HalconView();
            view.ViewType = _viewType.ClearScreen;

            AddView(view);
            return view;
        }

        public void AddView(HalconView newView)
        {
            // UI interaction Display
            while (Buffers.Count > BufferLimit)
            {
                if (Buffers.TryDequeue(out HalconView bufferView))
                {
                    bufferView.ClearAll();
                    bufferView = null;
                }
            }

            //Let main thread to control refresh speed if called from another thread.
            Buffers.Enqueue(newView);
        }

        public HalconView GetNextView()
        {
            if (Buffers.Count > 0)
            {
                if (Buffers.TryDequeue(out HalconView view))
                {
                    //Save the view for future
                    if (LastView != null) LastView.ClearAll();
                    LastView = view.CloneView();
                    return view;
                }
            }

            return null;
        }

        public HalconView GetLastView()
        {
            if (LastView == null)
            {
                HalconView view = new HalconView();
                view.ViewType = _viewType.ClearScreen;
                return view;
            }

            return LastView.CloneView();
        }

        public void ClearBuffer()
        {
            while (Buffers.Count > 0)
            {
                if (Buffers.TryDequeue(out HalconView view))
                {
                    view.ClearAll();
                }
            }
        }
    }

    public class csDisplayItemBase
    {

        /// <summary>
        /// Indicate item belong to which tool
        /// </summary>
        public string ToolID { get; set; }

        /// <summary>
        /// True: show original color
        /// False: show transparent color
        /// </summary>
        public bool IsFocused { get; set; }
    }

    public class csDisplayHObject : csDisplayItemBase
    {
        public HObject ViewItem { get; set; }

        public _DisplayObjectType Type { get; set; }
        /// <summary>
        /// Used to locate the search region type in a tool
        /// </summary>
        public int ItemOrder { get; set; }

        public csDisplayHObject()
        {
            Type = _DisplayObjectType.General;
        }

        public csDisplayHObject Clone()
        {
            csDisplayHObject displayObject = new csDisplayHObject();
            displayObject.Type = this.Type;
            if (ViewItem != null && ViewItem.IsInitialized())
            {
                displayObject.ViewItem = this.ViewItem.Clone();
            }
            return displayObject;
        }

        public int GetItemCount()
        {
            try
            {
                if (ViewItem == null || ViewItem.IsInitialized()) return 0;
                int iCount = ViewItem.CountObj();
                return iCount;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return -1;
            }
        }
    }

    public class csDisplayMark : csDisplayItemBase
    {
        public HalconPoint MarkPoint { get; set; }

        public csDisplayMark()
        {

        }

        public csDisplayMark(HalconPoint point, string sToolID = null)
        {
            MarkPoint = point;
            ToolID = sToolID;
        }

        public csDisplayMark(object Row, object Column, string sToolID = null)
        {
            ToolID = sToolID;

            if ((Row is float && Column is float) || (Row is double && Column is double))
            {
                MarkPoint = new HalconPoint(Convert.ToDouble(Row), Convert.ToDouble(Column));
            }
            else if (Row is int && Column is int)
            {
                MarkPoint = new HalconPoint((int)Row, (int)Column);
            }
            else if (Row is HTuple && Column is HTuple)
            {
                var hRow = (HTuple)Row;
                var hCol = (HTuple)Column;

                if (hRow.Type == HTupleType.DOUBLE)
                {
                    if (hRow.DArr.Length < 1 || hRow.DArr.Length != hCol.DArr.Length) return;
                    //Only add one result
                    MarkPoint = new HalconPoint(hRow.DArr[0], hCol.DArr[0]);
                }
                else if (hRow.Type == HTupleType.INTEGER)
                {
                    if (hRow.IArr.Length < 1 || hRow.IArr.Length != hCol.IArr.Length) return;
                    //Only add one result
                    MarkPoint = new HalconPoint(hRow.DArr[0], hCol.DArr[0]);
                }
                else
                {
                    MarkPoint = new HalconPoint();
                }
            }
            else
            {
                MarkPoint = new HalconPoint();
            }

        }
    }

    public class csDisplayRect1 : csDisplayItemBase
    {
        public Rectange1Data Rect1;

        public csDisplayRect1()
        {

        }
    }

    public class csDisplayRect2 : csDisplayItemBase
    {
        public Rectange2Data Rect2;

        public csDisplayRect2(Rectange2Data _rect2)
        {
            Rect2 = _rect2;
        }
    }

    /// <summary>
    /// Text in coordinates of the image
    /// </summary>
    public class csDisplayImageText : csDisplayItemBase
    {

        public string Text { get; set; }

        [XmlIgnore, JsonIgnore]
        public HTuple Row { get; set; }

        [XmlIgnore, JsonIgnore]
        public HTuple Column { get; set; }

        public _imageTextType TextType { get; set; }


        public csDisplayImageText()
        {
            Text = "";
            TextType = _imageTextType.Default;
        }

        public csDisplayImageText(string sText, HTuple row, HTuple col, string sToolID = null, _imageTextType textType = _imageTextType.Default)
        {
            Text = sText;
            Row = row;
            Column = col;
            this.ToolID = sToolID;
            TextType = textType;
        }

        public string GetTextColor_Halcon()
        {

            string sColor = IsFocused ? csHalconColors.Green : csHalconColors.GreenTrans_80;

            if (TextType == _imageTextType.TextBox_RedText)
            {
                sColor = IsFocused ? csHalconColors.Red : csHalconColors.RedTrans_80;
            }

            return sColor;

        }

    }

    public enum _viewType
    {
        /// <summary>
        /// Show all items
        /// </summary>
        General,
        //Init screen only
        ClearScreen,
    }

    public enum _DisplayObjectType
    {
        /// <summary>
        /// Show in green color if color required
        /// </summary>
        General,
        /// <summary>
        /// Image Items needs to be displayed before other others
        /// </summary>
        Image,
        /// <summary>
        /// Regular region show as border
        /// Show various color if color required
        /// </summary>
        RegionBorder,
        Contour,
        /// <summary>
        /// Regular filled region show in half trans color
        /// Show red and fill the display
        /// </summary>
        RegionTrans,
        /// <summary>
        /// Show in solid red color
        /// </summary>
        RegionSolid,
        /// <summary>
        /// Inspection Region of the tool
        /// </summary>
        RegionSearchBorder,
    }
}
