using DevExpress.Utils.Design;
using DevExpress.XtraReports.UI;
using HalconDotNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace OpenCV_Halcon_Demo
{
    public class csHalconWindow
    {
        /// <summary>
        /// Main window
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public HTuple hv_WindowHandle;

        /// <summary>
        /// Current display objects
        /// </summary>
        public HalconView View;

 
        /// <summary>
        /// Parent window if exist
        /// </summary>
        public HWindowControl windowControl;

        private bool ControlInvalid => windowControl == null ||
                                windowControl.Disposing ||
                                windowControl.IsDisposed ||
                                !windowControl.Visible ||
                                DisposeRequest;


        //Point when mouse move
        public HalconPoint MouseMovePosition;
        public int MouseGrayValue;

        //Point when mouse key down
        public HalconPoint MouseDownPosition;

        /// <summary>
        /// Value type
        /// </summary>
        public System.Drawing.Size LastWindowSize;

        /// <summary>
        /// Last image shown
        /// </summary>
        public System.Drawing.Size LastImageSize;

        public DateTime LastMouseUpTime;


        public csViewPort DisplayPort;

        /// <summary>
        /// Indicate whether set the view port of the image is required
        /// </summary>
        public bool IsImageViewPortInit { get; set; }


        /// <summary>
        /// Store current drawing items
        /// </summary>
        public csDrawItem DrawData { get; set; }

        private csViewPortLayout ViewLayout { get; set; }

 
        /// <summary>
        /// Check display view time consumption
        /// </summary>
        public Stopwatch watchView = new Stopwatch();

        /// <summary>
        /// Current tool selection
        /// </summary>
        public string FocusedToolID;

        /// <summary>
        /// indicate user selection
        /// </summary>
        public csDisplayHObject FocusedDispObject;

        public event EventHandler ManualMouseDoubleClick;
        public event EventHandler ManualMouseClick;
 
        /// <summary>
        /// Mark the window to be disposed
        /// </summary>
        public bool DisposeRequest;

        public csHalconWindow()
        {
            MouseDownPosition = new HalconPoint();
            MouseMovePosition = new HalconPoint();
        }

        /// <summary>
        /// Use halcon window, time-consuming
        /// </summary>
        /// <param name="hWindow"></param>

        public bool LinkWindow(HWindowControl hWindow)
        {
            //Clean up
            if (windowControl != null) ClearEvents();

            //Init variables
            windowControl = hWindow;
            hv_WindowHandle = hWindow.HalconWindow;
            View = new HalconView();
            ViewLayout = new csViewPortLayout();
            DrawData = new csDrawItem();

            ClearLastSize();

            try
            {
                //Set draw
                HOperatorSet.SetColor(hv_WindowHandle, csHalconColors.Red);
                HOperatorSet.SetDraw(hv_WindowHandle, "margin");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("csHalconWindow.LinkWindow:\r\n" + ex.Message);
                return false;
            }

            //Reload the events
            windowControl.SizeChanged += WindowControl_SizeChanged;
            windowControl.MouseMove += WindowControl_MouseMove;
            windowControl.MouseWheel += WindowControl_MouseWheel;
            windowControl.MouseDown += WindowControl_MouseDown;
            windowControl.MouseUp += WindowControl_MouseUp;


            //Finish up
            Clear();
            return true;
        }


        private void WindowControl_MouseUp(object sender, MouseEventArgs e)
        {
            //Trace.WriteLine("WindowControl_MouseUp");

            //Click event doesn't trigger, added manual double click
            int iDoubleClickGap = 250;
            if ((csDateTimeHelper.CurrentTime - LastMouseUpTime) > TimeSpan.FromMilliseconds(iDoubleClickGap))
            {//Mouse is moving
                LastMouseUpTime = csDateTimeHelper.CurrentTime;
                //Clicked once
                ManualMouseClick?.Invoke(this, e);
            }
            else
            {//Clicked twice or above
                ManualMouseDoubleClick?.Invoke(this, e);
            }


        }


   

        private void WindowControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (ControlInvalid) return;

            //Ignore action when drawing
            if (DrawData.IsDrawing) return;

            var position = GetMouseImagePosition(e);
            if (position == null) return;

            //Remember current mouse position as scale point
            MouseDownPosition = position;
        }



        private void WindowControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ControlInvalid) return;

            // Mouse wheel event might grab an error X reading, use previouly save mouse position for mouse wheel event
            var position = GetMouseImagePosition();
            if (position == null) return;

            if (e.Delta > 0)
            {
                ZoomOut(position);
            }
            else if (e.Delta < 0)
            {
                ZoomIn(position);
            }
        }

        public HObject GetCurrentViewImage()
        {
            if (View == null) return null;
            return View.GetViewImage();
        }

        public void Clear()
        {
            HOperatorSet.ClearWindow(hv_WindowHandle);
            View.ClearAll();
        }

        private void ClearEvents()
        {
            windowControl.SizeChanged -= WindowControl_SizeChanged;
            windowControl.MouseMove -= WindowControl_MouseMove;
            windowControl.MouseWheel -= WindowControl_MouseWheel;
            windowControl.MouseDown -= WindowControl_MouseDown;
            windowControl.MouseUp -= WindowControl_MouseUp;
        }

        public void Dispose()
        {
            ClearEvents();
            //Init memory
            HOperatorSet.ClearWindow(hv_WindowHandle);
            View.ClearAll();

            windowControl.Dispose(); //will auto dispose
        }

        private void WindowControl_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("WindowControl_Click");
        }

        /// <summary>
        /// Init size memory will cause a re-fit when image shown
        /// </summary>
        public void ClearLastSize()
        {
            LastImageSize = new System.Drawing.Size(0, 0);
            LastWindowSize = new System.Drawing.Size(0, 0);
        }




        /// <summary>
        /// Zoom 30% by default, magenify the image
        /// </summary>
        /// <param name="zoomRatio"></param>
        public void ZoomIn(HalconPoint zoomPosition)
        {
            //Set Image Scale
            ZoomImageViewPort(zoomPosition, 1.2f);

            DisplayView();
        }


        /// <summary>
        /// Zoom 50% by default, shrink the image
        /// </summary>
        public void ZoomOut(HalconPoint zoomPosition)
        {
            //Set Image Scale
            ZoomImageViewPort(zoomPosition, 0.8f);

            DisplayView();
        }

        public void ZoomOrigin()
        {
            //ZoomImageViewPort(null, null);
            IsImageViewPortInit = false;//Force reset the display area
            DisplayView();
        }

        

 
   

        /// <summary>
        /// Close and get current draw location
        /// </summary>
        /// <param name="drawObject"></param>
        public void CloseDrawRegion()
        {
            try
            {
                if (DrawData.DrawHandle != null)
                {
                    if (DrawData.DrawShape == _shapeType.Rectangle2 && DrawData.Rectangle2 != null)
                    {
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "row", out HTuple row);
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "column", out HTuple column);
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "phi", out HTuple phi);
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "length1", out HTuple length1);
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "length2", out HTuple length2);

                        //Show draw result
                        Trace.WriteLine($"Draw:R({row.D.ToString("f0")}),C({column.D.ToString("f0")})," +
                            $"Deg({phi.TupleDeg().D.ToString("f0")}),L1({length1.D.ToString("f0")}),L2({length2.D.ToString("f0")})");

                        //Load values
                        DrawData.Rectangle2.Init(row, column, phi, length1, length2);

                        //Check if alignment exist
                        if (DrawData.PositionOffsetMatrix != null)
                        {
                            HOperatorSet.HomMat2dInvert(DrawData.PositionOffsetMatrix, out HTuple matrixInvert);
                            DrawData.Rectangle2.MapRegion(matrixInvert);
                        }

                        //Finish assinment clear reference
                        DrawData.Rectangle2 = null;
                    }
                    else if (DrawData.DrawShape == _shapeType.Line && DrawData.Line != null)
                    {
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "row1", out HTuple row1);
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "column1", out HTuple col1);
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "row2", out HTuple row2);
                        HOperatorSet.GetDrawingObjectParams(DrawData.DrawHandle, "column2", out HTuple col2);

                        //Load values
                        DrawData.Line.Init(row1, col1, row2, col2);
                        DrawData.Line.GenLinePosition(); //generate line position info

                        //Check if alignment exist
                        if (DrawData.PositionOffsetMatrix != null)
                        {
                            HOperatorSet.HomMat2dInvert(DrawData.PositionOffsetMatrix, out HTuple matrixInvert);
                            DrawData.Line.MapRegion(matrixInvert);
                        }

                        //Finish assinment clear reference
                        DrawData.Line = null;
                    }

                    HOperatorSet.DetachDrawingObjectFromWindow(hv_WindowHandle, DrawData.DrawHandle);
                    DrawData.PositionOffsetMatrix = null;
                    DrawData.DrawHandle.Dispose();
                    DrawData.DrawHandle = null;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("csHalconWindow.CloseDrawRegion\r\n" + ex.Message);
            }

            DrawData.IsDrawing = false;
        }


        private void WindowControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (ControlInvalid) return;

            try
            {
                //Get image gray value
                if (!GetMouseImagePositionAndGray(e)) return;

                //Disable image move when drawing
                if (DrawData.IsDrawing) return;


                //Check shift required
                if (e.Button == MouseButtons.Left)
                {
                    if (MouseMovePosition.Row != MouseDownPosition.Row || MouseMovePosition.Column != MouseDownPosition.Column)
                    {//Shift the image
                        //Get offset
                        var rowOffset = MouseDownPosition.Row - MouseMovePosition.Row;
                        var columnOffset = MouseDownPosition.Column - MouseMovePosition.Column;
                        HOperatorSet.GetPart(hv_WindowHandle, out HTuple row1, out HTuple column1, out HTuple row2, out HTuple column2);

                        HOperatorSet.SetPart(hv_WindowHandle, row1 + rowOffset, column1 + columnOffset, row2 + rowOffset, column2 + columnOffset);
                        Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} WindowControl_MouseMove.SetPart");
                        DisplayView();
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine($"csHalconWindow.WindowControl_MouseMove.Exception:\r\n{exception.GetMessageDetail()}");
            }

        }


        public bool TryDrawRectangle2(out Rectange2Data rectange2)
        {
            rectange2 = new Rectange2Data();

            try
            {
                //Draw method
                HOperatorSet.DrawRectangle2(hv_WindowHandle, out HTuple row, out HTuple column, out HTuple phi, out HTuple length1, out HTuple length2);
                rectange2.Init(row, column, phi, length1, length2);

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("csHalconView.TryDrawRectangle2:\r\n" + ex.Message);
                return false;
            }
        }



        public bool TryDrawLine(out csLineData Line)
        {
            Line = new csLineData();
            try
            {
                //Draw line
                windowControl.HalconWindow.DrawLine(out double row1, out double column1, out double row2, out double column2);
                Line.Init(row1, column1, row2, column2);

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("csHalconView.TryDrawLine:\r\n" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Mouse wheel event might grab an error X reading, use previouly save mouse position for mouse wheel event
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private HalconPoint GetMouseImagePosition(MouseEventArgs e = null)
        {
            var mousePosition = new HalconPoint();

            try
            {
                //Get current window size
                if (ControlInvalid)
                {
                    Trace.WriteLine("csHalconWindow.GetMouseImagePosition:Window disposed");
                    return null;
                }

                //Notice row ,col value is not correct, ignore
                HOperatorSet.GetWindowExtents(hv_WindowHandle, out HTuple row, out HTuple col, out HTuple width, out HTuple height);

                //Read from mouse move record instead, since the mouse X position might change only in the mouse wheel event
                int mouseX = ViewLayout.MouseX;
                int mouseY = ViewLayout.MouseY;
                if (e != null)
                {
                    mouseX = e.X;
                    mouseY = e.Y;
                    ViewLayout.SetMouseWindowPosition(e);
                }


                //Check mouse X in the halcon window or not (Edge is window, not the image itself)
                double gapX = (windowControl.Size.Width - width.D) / 2.0;
                if (mouseX <= gapX || mouseX >= (gapX + width.D)) return null;

                //Check mouse Y in the halcon window or not (Edge is window, not the image itself)
                double gapY = (windowControl.Size.Height - height.D) / 2.0;
                if (mouseY <= gapY || mouseY >= (gapY + height.D)) return null;


                HOperatorSet.GetMposition(hv_WindowHandle, out HTuple iRow, out HTuple iColumn, out HTuple iButton);
                mousePosition.Row = (int)iRow.D;
                mousePosition.Column = (int)iColumn.D;

                return mousePosition;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("csHalconWindow.GetMouseImagePosition:\r\n" + ex.Message);
                return null;
            }
        }

        private bool GetMouseImagePositionAndGray(MouseEventArgs e)
        {
            //Init variables
            try
            {
                //Get current window size
                if (ControlInvalid)
                {
                    Trace.WriteLine("csHalconWindow.GetMouseImagePositionGray:Window disposed");
                    return false;
                }

                //Get current window size
                //Notice row ,col value is not correct, ignore
                HOperatorSet.GetWindowExtents(hv_WindowHandle, out HTuple row, out HTuple col, out HTuple width, out HTuple height);

                ViewLayout.SetExtendInfo(row, col, width, height);
                ViewLayout.SetMouseWindowPosition(e);


                //Check image valid
                if (!View.IsViewImageValid)
                {
                    ResetMouseMovePosition();
                    return false;
                }

                //Directly get the mouse position
                HOperatorSet.GetMposition(hv_WindowHandle, out HTuple hv_Row, out HTuple hv_Column, out HTuple hv_Button);

                //Check mouse inside the image
                if (hv_Row.D < 0 ||
                    hv_Row.D >= LastImageSize.Height ||
                    hv_Column.D < 0 ||
                    hv_Column.D >= LastImageSize.Width)
                {
                    ResetMouseMovePosition();
                    return false;
                }

                //Always set the mouse position
                MouseMovePosition.Row = hv_Row;
                MouseMovePosition.Column = hv_Column;
                //Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} csHalconWindow.GetMouseImagePositionGray.Position: {MouseMovePosition.Row}, {MouseMovePosition.Column}.");

                //Get the mouse gray value
                var dispImage = View.GetLastDisplayImageItem();
                if (dispImage == null) dispImage = View.ViewImage;
                HOperatorSet.GetGrayval(dispImage, hv_Row, hv_Column, out HTuple hv_Grayval);

                //Save the gray value
                MouseGrayValue = hv_Grayval;
                //Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} csHalconWindow.GetMouseImagePositionGray.Gray: {MouseGrayValue}.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("csHalconWindow.GetMouseImagePositionGray:\r\n" + ex.Message);
                ResetMouseMovePosition();
                return false;
            }

            //Pass all steps
            return true;
        }

        private void ResetMouseMovePosition()
        {
            MouseMovePosition.Reset();
            MouseGrayValue = -1;
        }

        private void WindowControl_SizeChanged(object sender, EventArgs e)
        {
            if (ControlInvalid) return;
            Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} WindowControl_SizeChanged:{windowControl.Size}");

            try
            {
                if (!View.IsViewImageValid) return;

                //Only reset the image view port when size change is large
                if (this.LastWindowSize.Width == 0 ||
                    this.LastWindowSize.Height == 0)
                {//Size always reset 
                    IsImageViewPortInit = false;
                }
                else
                {
                    float minimumChangeRatio = 0.1f;
                    var currentSize = this.windowControl.Size;
                    var widthDiff = Math.Abs((float)currentSize.Width / (float)this.LastWindowSize.Width - 1);
                    var heightDiff = Math.Abs((float)currentSize.Height / (float)this.LastWindowSize.Height - 1);
                    string sSizeInfo = $"{LastWindowSize},{currentSize},[{widthDiff},{heightDiff}]";

                    if (widthDiff > minimumChangeRatio || heightDiff > minimumChangeRatio)
                    {
                        //Only change the view port when the window size changes is large
                        //This avoids false resetting the viewport when window control has overlapping items
                        IsImageViewPortInit = false;
                        Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} WindowControl_SizeChanged.UpdateViewPort: {sSizeInfo}");
                    }
                    else
                    {
                        Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} WindowControl_SizeChanged.IgnoreViewPort: {sSizeInfo}");
                    }

                }
                DisplayView();

            }
            catch (Exception exception)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} csHalconWindow.WindowControl_SizeChanged:\r\n" + exception.Message);
            }
            finally
            {
                this.LastWindowSize = this.windowControl.Size;
            }
        }



        public void DisplayImage(HObject image)
        {
            var newView = new HalconView(image);
            this.View = newView;
            DisplayView();
        }

        /// <summary>
        /// Internal usage only, for image without view
        /// </summary>
        /// <param name="Image"></param>
        private void DisplayBaseImage(HObject Image)
        {
            //Check image
            if (Image == null || !Image.IsInitialized())
            {
                HOperatorSet.SetSystem("flush_graphic", "false");//Minimum screen blink
                HOperatorSet.ClearWindow(hv_WindowHandle);
                return;
            }

            HOperatorSet.SetSystem("flush_graphic", "false");//Minimum screen blink
            HOperatorSet.ClearWindow(hv_WindowHandle);



            //Set image view port
            if (!IsImageViewPortInit || !IsImageSizeEqual(Image))
            {
                InitImageViewPort(Image);
                IsImageViewPortInit = true;
            }

            HOperatorSet.DispObj(Image, hv_WindowHandle);
            HOperatorSet.SetSystem("flush_graphic", "true");
        }



        private bool IsImageSizeEqual(HObject hImage)
        {
            HOperatorSet.GetImageSize(hImage, out HTuple width, out HTuple height);
            int iWidth = (int)width.D;
            int iHeight = (int)height.D;

            if (LastImageSize.Width == iWidth &&
                LastImageSize.Height == iHeight)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// Keep the image in ratio
        /// </summary>
        public void InitImageViewPort(HObject Image)
        {
            try
            {
                if (!Image.IsValid())
                {
                    Trace.WriteLine("csHalconWindow.InitImageViewPort.InvalidImage");
                    return;
                }

                //Get current window size
                if (ControlInvalid)
                {
                    Trace.WriteLine("GetMouseImagePosition:Window disposed");
                    return;
                }

                //Get image and window size ratio
                HOperatorSet.GetImageSize(Image, out HTuple hv_WidthImage, out HTuple hv_HeightImage);
                HOperatorSet.GetWindowExtents(hv_WindowHandle, out HTuple row, out HTuple column, out HTuple hv_WidthWindow, out HTuple hv_HeightWindow);
                double dRatioImage = hv_WidthImage.D / hv_HeightImage.D;
                double dRatioWindow = hv_WidthWindow.D / hv_HeightWindow.D;
                csViewPort displayPort = new csViewPort();

                //Save the image size
                LastImageSize = new System.Drawing.Size((int)hv_WidthImage.D, (int)hv_HeightImage.D);

                //Set the view port based on the window and image ratio
                if (dRatioImage == dRatioWindow)
                {//Same ratio, directly display the whole image
                    displayPort.Row1 = 0;
                    displayPort.Col1 = 0;
                    displayPort.Row1 = 0;
                    displayPort.Row1 = 0;
                    HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_HeightImage, hv_WidthImage);
                }
                else if (dRatioWindow > dRatioImage)
                {//Image fill top and bottom, but left and right is empty

                    //Set top and bottom
                    double dRow1 = 0;
                    double dRow2 = hv_HeightImage.D - 1;

                    //Get the image display width inside the window(< Window Width)
                    double dImageWidthInWindow = (hv_HeightWindow.D * hv_WidthImage.D) / hv_HeightImage.D;

                    //Get full window width inside the image(> Image Width)
                    double dWindowWidthInImage = (hv_WidthImage.D * hv_WidthWindow.D) / dImageWidthInWindow;

                    //Calculate the left and right start points
                    double dCol1 = hv_WidthImage.D / 2 - dWindowWidthInImage / 2;
                    double dCol2 = hv_WidthImage.D / 2 + dWindowWidthInImage / 2;
                    HOperatorSet.SetPart(hv_WindowHandle, dRow1, dCol1, dRow2, dCol2);
                }
                else if (dRatioWindow < dRatioImage)
                {//Image fill left and right, but top and bottom is empty
                 //Set left and right
                    double dCol1 = 0;
                    double dCol2 = hv_WidthImage.D - 1;

                    //Get the image display height inside the window(< Window Height)
                    double dImageHeightInWindow = (hv_WidthWindow.D * hv_HeightImage.D) / hv_WidthImage.D;

                    //Get full window width inside the image(> Image Height)
                    double dWindowHeightInImage = (hv_HeightImage.D * hv_HeightWindow.D) / dImageHeightInWindow;

                    //Calculate the left and right start points
                    double dRow1 = hv_HeightImage.D / 2 - dWindowHeightInImage / 2;
                    double dRow2 = hv_HeightImage.D / 2 + dWindowHeightInImage / 2;
                    HOperatorSet.SetPart(hv_WindowHandle, dRow1, dCol1, dRow2, dCol2);
                }

                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} InitImageViewPort.SetPart");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} InitImageViewPort.Exception:\r\n{e.GetMessageDetail()}");
            }
        }

        /// <summary>
        /// Must remove all int calculation replace with float or double
        /// </summary>
        /// <param name="zoomPosition"></param>
        /// <param name="isZoomIn">Zoom in/Zoom out/Reset</param>
        public void ZoomImageViewPort(HalconPoint zoomPosition, float fZoomFactor)
        {
            if (!View.IsViewImageValid) return;

            try
            {
                //Init variables
                if (fZoomFactor < 0f || fZoomFactor > 10f)
                {
                    Trace.WriteLine($"ZoomImageViewPort: Input out of range.{fZoomFactor}");
                    return;
                }


                //Get current view port location
                HOperatorSet.GetPart(hv_WindowHandle, out HTuple hRow1, out HTuple hColumn1, out HTuple hRow2, out HTuple hColumn2);
                //Trace.WriteLine($"ZoomImageViewPort.ImagePart: [Row1:{hRow1.D},Col1:{hColumn1.D},Row2:{hRow2.D},Col2:{hColumn2.D}]");

                //View height
                float preHeight = (float)(hRow2.D - hRow1.D);
                float preWidth = (float)(hColumn2.D - hColumn1.D);

                //Check the zoom position
                //Already loaded in the "WindowControl_MouseWheel" event
                if (zoomPosition == null)
                {//Use center position to zoom
                    zoomPosition = new HalconPoint()
                    {
                        Row = (float)(hRow1.D + preHeight / 2),
                        Column = (float)(hColumn1.D + preWidth / 2)
                    };
                }

                //Caculate window location and size
                //Compare with fit whole window,zoomed image gap between image and window edge changed by a zoom ratio 
                float fRowGapLeft = zoomPosition.Row - (float)hRow1.D; //Gap of mouse point to left view port edge
                float fColumnGapTop = zoomPosition.Column - (float)hColumn1.D;//Gap of mouse point to top view port edge

                //New window start point
                float newRow1 = zoomPosition.Row - (fRowGapLeft / fZoomFactor); //New view left position, mouse point minus new left gap
                float newColumn1 = zoomPosition.Column - (fColumnGapTop / fZoomFactor);//New view top position, mouse point minus new top gap

                //Define end point:
                //End point = Scaled start + scaled size)
                float newRow2 = newRow1 + preHeight / fZoomFactor;
                float newColumn2 = newColumn1 + preWidth / fZoomFactor;

                //Protection
                //Halcon support maximum 32K*32K pixel, exceeded will cause exception
                if (preHeight * fZoomFactor > 32000 || preWidth * fZoomFactor > 32000)
                {//Set back to normal, avoid stuck in minimum size
                    Trace.WriteLine("ZoomImageViewPort: Limit Reached.");
                    var displayPort = new csViewPort(hRow1.D * 0.7, hColumn1.D * 0.7, hRow2.D * 0.7, hColumn2.D * 0.7);
                    SetViewPort(displayPort);
                }
                else
                {//Directly set 
                    var displayPort = new csViewPort(newRow1, newColumn1, newRow2, newColumn2);
                    SetViewPort(displayPort);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ZoomImageViewPort.Exception:\r\n{ex.GetMessageDetail()}");
            }
        }



        private void SetViewPort(csViewPort displayPort)
        {
            HOperatorSet.SetPart(hv_WindowHandle, displayPort.Row1, displayPort.Col1, displayPort.Row2, displayPort.Col2);
            DisplayPort = displayPort;
            Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} csHalconWindow.SetViewPort: [{displayPort.GetDisplay()}]");
        }


        public void DisplayView()
        {
            watchView.Restart();
            try
            {
                //Check clear screen request
                if (View.ViewType == _viewType.ClearScreen)
                {
                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    return;
                }

                lock (View.ViewImageLock)
                {
                    DisplayBaseImage(View.ViewImage);
                }


                DisplayGenericObjects();
                DisplayFocusedItem();

                //Finish up, show changes
                HOperatorSet.SetSystem("flush_graphic", "true");
            }

            catch (Exception ex)
            {
                Trace.WriteLine("csHalconWindow.DisplayView:\r\n" + ex.ToString());
            }

            watchView.Stop();
            //Trace.WriteLine("View Update:" + watchView.ElapsedMilliseconds);
        }

        public void DisplayFocusedItem()
        {
            try
            {

                //Make sure display item is valid
                if (FocusedDispObject == null ||
                    FocusedDispObject.ViewItem == null ||
                    !FocusedDispObject.ViewItem.IsInitialized())
                {
                    return;
                }


                string sColor = string.Empty;
                var itemType = FocusedDispObject.ViewItem.GetHalconType();
                if (itemType == _hObjectType.Region)
                {
                    HOperatorSet.SetLineStyle(hv_WindowHandle, 10);
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 3);
                    HOperatorSet.SetColor(hv_WindowHandle, "white");
                    HOperatorSet.DispObj(FocusedDispObject.ViewItem, hv_WindowHandle);
                }

            }
            catch (Exception e)
            {
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} DisplayFocusedItem.Exception:\r\n{e.GetMessageDetail()}");

            }
            finally
            {
                HOperatorSet.SetLineStyle(hv_WindowHandle, new HTuple());
            }
        }



        private void DisplayGenericObjects()
        {
            try
            {
                HOperatorSet.SetSystem("flush_graphic", "false");//Minimum screen blink
                int iWinTextRow = 0;//Record number of window text alread displayed
                HOperatorSet.SetLineWidth(hv_WindowHandle, 2);
                View.SortDisplayObjects();

                lock (View.lockDisplayItems)
                {
                    foreach (var item in View.DisplayItems)
                    {
                        //Check tool selection
                        if (string.IsNullOrEmpty(this.FocusedToolID))
                        {//When no tool is selected, show all tool in normal color
                            item.IsFocused = true;
                        }
                        else
                        {//When there is a selected tool, only show the selected tool in original color
                            item.IsFocused = this.FocusedToolID == item.ToolID;
                        }
                        int iLineWidth = item.IsFocused ? 3 : 2;
                        HOperatorSet.SetLineWidth(hv_WindowHandle, iLineWidth);

                        //....Removed
                        if (item is csDisplayHObject dispObjectItem)
                        {
                           
                        }
                 
 
                    }

                    //Resume settings
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 2);
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"csHalconWindow.DisplayGenericObjects.Exception\r\n {e.GetMessageDetail()}");
            }
        }

     

 

      
        public bool SaveImage(HObject ho_Image, string sPath)
        {
            //Try to read image
            try
            {
                //Check folder
                string sDirectory = Path.GetDirectoryName(sPath);
                if (!Directory.Exists(sDirectory)) Directory.CreateDirectory(sDirectory);

                HOperatorSet.WriteImage(ho_Image, "tiff", 0, sPath);
            }
            catch (Exception e)
            {
                Trace.WriteLine("csHalconWindow.SaveImage:\r\n" + e.Message);
                return false;
            }

            //Pass all steps
            return true;
        }

        public csDisplayHObject GetMousePointObject()
        {
            if (MouseDownPosition == null) return null;
            List<csDisplayHObject> possibleItems = new List<csDisplayHObject>();
            csDisplayHObject currentObject = null;

            try
            {
                lock (View.lockDisplayItems)
                {
                    //Get all possible items
                    foreach (var displayItem in View.DisplayItems)
                    {
                        currentObject = displayItem as csDisplayHObject;
                        if (currentObject == null) continue;
                        int iCount = currentObject.ViewItem.GetItemCount();
                        if (iCount < 1) continue;

                        //Check item is region
                        var halconType = currentObject.ViewItem.GetHalconType();
                        if (halconType == _hObjectType.Region)
                        {
                            HOperatorSet.TestRegionPoint(currentObject.ViewItem, (double)MouseDownPosition.Row, (double)MouseDownPosition.Column, out HTuple hv_IsIn);
                            if (hv_IsIn.I == 0) continue;
                            possibleItems.Add(currentObject);
                        }
                    }


                    //Check first filter result
                    if (possibleItems.Count == 0) return null;
                    if (possibleItems.Count == 1) return possibleItems[0];

                    //Remove all parent region
                    RemoveParentRegion(possibleItems);
                    if (possibleItems.Count == 1) return possibleItems[0];

                    //Check overlap
                    return GetClosestRegion(possibleItems, MouseDownPosition);

                }
            }
            catch (Exception e)
            {
                string sObjectInfo = "Null";
                if (currentObject != null)
                {
                    int iCount = currentObject.ViewItem.GetItemCount();
                    var type = currentObject.ViewItem.GetHalconType();
                    sObjectInfo = $"Count:{iCount}, Type:{type}";
                }
                Trace.WriteLine($"GetMousePointObject:Exception: ({sObjectInfo})\r\n{e.GetMessageDetail()}");
            }

            //No matches
            return null;
        }

        /// <summary>
        /// Remove a region if it's parent of another region
        /// </summary>
        /// <param name="possibleItems"></param>
        private void RemoveParentRegion(List<csDisplayHObject> possibleItems)
        {
            //For debug usage
            csDisplayHObject parentItem = null;
            csDisplayHObject subItem = null;

            try
            {
                foreach (var item1 in possibleItems)
                {
                    parentItem = item1;

                    foreach (var item2 in possibleItems)
                    {
                        //Ignore same item
                        subItem = item2;
                        if (parentItem == subItem) continue;

                        HOperatorSet.TestSubsetRegion(subItem.ViewItem, parentItem.ViewItem, out HTuple ht_IsSubset);
                        if (ht_IsSubset.I != 1) continue;

                        //Show the removed item
                        HOperatorSet.AreaCenter(parentItem.ViewItem, out HTuple areaParent, out HTuple rowParent, out HTuple colParent);
                        HOperatorSet.AreaCenter(subItem.ViewItem, out HTuple areaSub, out HTuple rowSub, out HTuple colSub);

                        Trace.WriteLine($"Remove Parent Region: " +
                                        $"(Parent: R{rowParent.D.ToString("f1")},C{colParent.D.ToString("f1")},S{areaParent.L}), " +
                                        $"(Subitem: R{rowSub.D.ToString("f1")},C{colSub.D.ToString("f1")},S{areaSub.L})");

                        possibleItems.Remove(parentItem);
                        RemoveParentRegion(possibleItems);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                string sInfo = $"Parent:{parentItem.ViewItem.GetItemCount()}, SubItem:{subItem.ViewItem.GetItemCount()}";
                Trace.WriteLine($"RemoveParentRegion.Exception.{sInfo}\r\n{e.GetMessageDetail()}");
            }
        }

        private csDisplayHObject GetClosestRegion(List<csDisplayHObject> possibleItems, HalconPoint position)
        {
            try
            {
                if (possibleItems == null) return null;
                if (possibleItems.Count == 1) return possibleItems[0];


                var distanceResults = possibleItems.Select(item =>
                {
                    HOperatorSet.AreaCenter(item.ViewItem, out HTuple area, out HTuple row, out HTuple col);
                    HOperatorSet.DistancePp(row, col, (double)position.Row, (double)position.Column, out HTuple distance);
                    return distance.D;
                }).ToList();
                var dMax = distanceResults.Max();
                int iIndex = distanceResults.IndexOf(dMax);
                Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} Region Selection: " +
                                $"(Mouse: R{position.Row.ToString("f0")},C{position.Column.ToString("f0")}), " +
                                $"(Distance: {dMax.ToString("f1")})");
                return possibleItems[iIndex];
            }
            catch (Exception e)
            {
                Trace.WriteLine($"GetClosestRegion:Exception\r\n{e.GetMessageDetail()}");
                return null;
            }

        }
    }
}

