
namespace Deep_Learning_Demo
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.OpenImageBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.RequestBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.WorkModeBarEditItem = new DevExpress.XtraBars.BarEditItem();
            this.workModeLookUpEdit = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            this.ResetViewBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.MainMenuBar = new DevExpress.XtraBars.Bar();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.ImageSizeBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.ProcessTimeBarButtonItem = new DevExpress.XtraBars.BarButtonItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.imageCollection32 = new DevExpress.Utils.ImageCollection(this.components);
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.hWindowControl1 = new HalconDotNet.HWindowControl();
            this.propertyGridControl1 = new DevExpress.XtraVerticalGrid.PropertyGridControl();
            this.Root = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.FakeResponseBarToggleSwitchItem = new DevExpress.XtraBars.BarToggleSwitchItem();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.workModeLookUpEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection32)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.propertyGridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1,
            this.MainMenuBar,
            this.bar3});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.HtmlImages = this.imageCollection32;
            this.barManager1.Images = this.imageCollection32;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.OpenImageBarButtonItem,
            this.RequestBarButtonItem,
            this.ImageSizeBarButtonItem,
            this.ProcessTimeBarButtonItem,
            this.ResetViewBarButtonItem,
            this.WorkModeBarEditItem,
            this.FakeResponseBarToggleSwitchItem});
            this.barManager1.LargeImages = this.imageCollection32;
            this.barManager1.MainMenu = this.MainMenuBar;
            this.barManager1.MaxItemId = 7;
            this.barManager1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.workModeLookUpEdit});
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 1;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.OpenImageBarButtonItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.RequestBarButtonItem, true),
            new DevExpress.XtraBars.LinkPersistInfo(this.WorkModeBarEditItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.FakeResponseBarToggleSwitchItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.ResetViewBarButtonItem, true)});
            this.bar1.OptionsBar.AllowQuickCustomization = false;
            this.bar1.OptionsBar.DrawBorder = false;
            this.bar1.OptionsBar.DrawDragBorder = false;
            this.bar1.Text = "Tools";
            // 
            // OpenImageBarButtonItem
            // 
            this.OpenImageBarButtonItem.Caption = "Open Image";
            this.OpenImageBarButtonItem.Id = 0;
            this.OpenImageBarButtonItem.ImageOptions.Image = global::Deep_Learning_Demo.Properties.Resources.loadfrom_32x32;
            this.OpenImageBarButtonItem.ImageOptions.LargeImage = global::Deep_Learning_Demo.Properties.Resources.open2_32x32;
            this.OpenImageBarButtonItem.Name = "OpenImageBarButtonItem";
            this.OpenImageBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.OpenImageBarButtonItem_ItemClick);
            // 
            // RequestBarButtonItem
            // 
            this.RequestBarButtonItem.Caption = "Request Deep Result";
            this.RequestBarButtonItem.Id = 1;
            this.RequestBarButtonItem.ImageOptions.Image = global::Deep_Learning_Demo.Properties.Resources.convert_32x32;
            this.RequestBarButtonItem.ImageOptions.LargeImage = global::Deep_Learning_Demo.Properties.Resources.convert_32x32;
            this.RequestBarButtonItem.Name = "RequestBarButtonItem";
            this.RequestBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.RequestBarButtonItem_ItemClick);
            // 
            // WorkModeBarEditItem
            // 
            this.WorkModeBarEditItem.Caption = "barEditItem1";
            this.WorkModeBarEditItem.Edit = this.workModeLookUpEdit;
            this.WorkModeBarEditItem.Id = 5;
            this.WorkModeBarEditItem.Name = "WorkModeBarEditItem";
            this.WorkModeBarEditItem.Size = new System.Drawing.Size(100, 0);
            this.WorkModeBarEditItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.WorkModeBarEditItem_ItemClick);
            // 
            // workModeLookUpEdit
            // 
            this.workModeLookUpEdit.AutoHeight = false;
            this.workModeLookUpEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.workModeLookUpEdit.Name = "workModeLookUpEdit";
            // 
            // ResetViewBarButtonItem
            // 
            this.ResetViewBarButtonItem.Caption = "Reset Zoom";
            this.ResetViewBarButtonItem.Id = 4;
            this.ResetViewBarButtonItem.ImageOptions.Image = global::Deep_Learning_Demo.Properties.Resources.zoom100percent_32x32;
            this.ResetViewBarButtonItem.ImageOptions.ImageIndex = 3;
            this.ResetViewBarButtonItem.ImageOptions.LargeImage = global::Deep_Learning_Demo.Properties.Resources.zoom100percent_32x32;
            this.ResetViewBarButtonItem.ImageOptions.LargeImageIndex = 3;
            this.ResetViewBarButtonItem.Name = "ResetViewBarButtonItem";
            this.ResetViewBarButtonItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.ResetViewBarButtonItem_ItemClick);
            // 
            // MainMenuBar
            // 
            this.MainMenuBar.BarName = "Main menu";
            this.MainMenuBar.DockCol = 0;
            this.MainMenuBar.DockRow = 0;
            this.MainMenuBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.MainMenuBar.OptionsBar.MultiLine = true;
            this.MainMenuBar.OptionsBar.UseWholeRow = true;
            this.MainMenuBar.Text = "Main menu";
            this.MainMenuBar.Visible = false;
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockRow = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.bar3.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.ImageSizeBarButtonItem),
            new DevExpress.XtraBars.LinkPersistInfo(this.ProcessTimeBarButtonItem)});
            this.bar3.OptionsBar.AllowQuickCustomization = false;
            this.bar3.OptionsBar.DrawDragBorder = false;
            this.bar3.OptionsBar.UseWholeRow = true;
            this.bar3.Text = "Status bar";
            // 
            // ImageSizeBarButtonItem
            // 
            this.ImageSizeBarButtonItem.Caption = "Image Size";
            this.ImageSizeBarButtonItem.Id = 2;
            this.ImageSizeBarButtonItem.Name = "ImageSizeBarButtonItem";
            // 
            // ProcessTimeBarButtonItem
            // 
            this.ProcessTimeBarButtonItem.Caption = "Last Request: N/A";
            this.ProcessTimeBarButtonItem.Id = 3;
            this.ProcessTimeBarButtonItem.Name = "ProcessTimeBarButtonItem";
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(898, 75);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 541);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(898, 29);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 75);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 466);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(898, 75);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 466);
            // 
            // imageCollection32
            // 
            this.imageCollection32.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection32.ImageStream")));
            this.imageCollection32.Images.SetKeyName(0, "apply_32x32.png");
            this.imageCollection32.Images.SetKeyName(1, "open2_32x32.png");
            this.imageCollection32.Images.SetKeyName(2, "convert_32x32.png");
            this.imageCollection32.Images.SetKeyName(3, "zoom100percent_32x32.png");
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.hWindowControl1);
            this.layoutControl1.Controls.Add(this.propertyGridControl1);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 75);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.Root;
            this.layoutControl1.Size = new System.Drawing.Size(898, 466);
            this.layoutControl1.TabIndex = 4;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // hWindowControl1
            // 
            this.hWindowControl1.BackColor = System.Drawing.Color.Black;
            this.hWindowControl1.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl1.Location = new System.Drawing.Point(290, 44);
            this.hWindowControl1.Name = "hWindowControl1";
            this.hWindowControl1.Size = new System.Drawing.Size(584, 398);
            this.hWindowControl1.TabIndex = 5;
            this.hWindowControl1.WindowSize = new System.Drawing.Size(584, 398);
            // 
            // propertyGridControl1
            // 
            this.propertyGridControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.propertyGridControl1.Location = new System.Drawing.Point(24, 44);
            this.propertyGridControl1.MenuManager = this.barManager1;
            this.propertyGridControl1.Name = "propertyGridControl1";
            this.propertyGridControl1.OptionsView.AllowReadOnlyRowAppearance = DevExpress.Utils.DefaultBoolean.True;
            this.propertyGridControl1.Size = new System.Drawing.Size(238, 398);
            this.propertyGridControl1.TabIndex = 4;
            // 
            // Root
            // 
            this.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.Root.GroupBordersVisible = false;
            this.Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlGroup2,
            this.layoutControlGroup1});
            this.Root.Name = "Root";
            this.Root.Size = new System.Drawing.Size(898, 466);
            this.Root.TextVisible = false;
            // 
            // layoutControlGroup2
            // 
            this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem2});
            this.layoutControlGroup2.Location = new System.Drawing.Point(266, 0);
            this.layoutControlGroup2.Name = "layoutControlGroup2";
            this.layoutControlGroup2.Size = new System.Drawing.Size(612, 446);
            this.layoutControlGroup2.Text = "Window";
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.hWindowControl1;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(588, 402);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(266, 446);
            this.layoutControlGroup1.Text = "Parameters";
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.propertyGridControl1;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(242, 402);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FakeResponseBarToggleSwitchItem
            // 
            this.FakeResponseBarToggleSwitchItem.Caption = "Fake Response";
            this.FakeResponseBarToggleSwitchItem.Id = 6;
            this.FakeResponseBarToggleSwitchItem.Name = "FakeResponseBarToggleSwitchItem";
            this.FakeResponseBarToggleSwitchItem.CheckedChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.FakeResponseBarToggleSwitchItem_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 570);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "MainForm";
            this.Text = "Deep Learning Client Demo";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.workModeLookUpEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection32)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.propertyGridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Root)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.Bar MainMenuBar;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem OpenImageBarButtonItem;
        private DevExpress.Utils.ImageCollection imageCollection32;
        private DevExpress.XtraBars.BarButtonItem RequestBarButtonItem;
        private DevExpress.XtraBars.BarButtonItem ImageSizeBarButtonItem;
        private DevExpress.XtraBars.BarButtonItem ProcessTimeBarButtonItem;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraVerticalGrid.PropertyGridControl propertyGridControl1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private HalconDotNet.HWindowControl hWindowControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private System.Windows.Forms.Timer timer1;
        private DevExpress.XtraBars.BarButtonItem ResetViewBarButtonItem;
        private DevExpress.XtraBars.BarEditItem WorkModeBarEditItem;
        private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit workModeLookUpEdit;
        private DevExpress.XtraBars.BarToggleSwitchItem FakeResponseBarToggleSwitchItem;
    }
}

