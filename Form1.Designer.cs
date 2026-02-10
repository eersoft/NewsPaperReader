namespace NewsPaperReader
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.MainViewer = new PdfiumViewer.PdfViewer();
            this.btnTest = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.lstPage = new System.Windows.Forms.ListBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.fPanel = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel_head = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button14 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button18 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.panel_Top = new System.Windows.Forms.Panel();
            this.toolBar_Top = new System.Windows.Forms.ToolStrip();
            this.tbZoomIn = new System.Windows.Forms.ToolStripButton();
            this.tbZoomOut = new System.Windows.Forms.ToolStripButton();
            this.tbToWidth = new System.Windows.Forms.ToolStripButton();
            this.tbToHeight = new System.Windows.Forms.ToolStripButton();
            this.tbLeft90 = new System.Windows.Forms.ToolStripButton();
            this.tbRight90 = new System.Windows.Forms.ToolStripButton();
            this.tbFullScreen = new System.Windows.Forms.ToolStripButton();
            this.label1 = new System.Windows.Forms.Label();
            this.panelCenter = new System.Windows.Forms.Panel();
            this.labInfo = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.linkEersoft = new System.Windows.Forms.LinkLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.fPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel_head.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel_Top.SuspendLayout();
            this.toolBar_Top.SuspendLayout();
            this.panelCenter.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainViewer
            // 
            this.MainViewer.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MainViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainViewer.Location = new System.Drawing.Point(0, 0);
            this.MainViewer.Margin = new System.Windows.Forms.Padding(0);
            this.MainViewer.Name = "MainViewer";
            this.MainViewer.ShowToolbar = false;
            this.MainViewer.Size = new System.Drawing.Size(1164, 670);
            this.MainViewer.TabIndex = 0;
            this.MainViewer.ZoomMode = PdfiumViewer.PdfViewerZoomMode.FitWidth;
            // 
            // btnTest
            // 
            this.btnTest.AutoSize = true;
            this.btnTest.Image = ((System.Drawing.Image)(resources.GetObject("btnTest.Image")));
            this.btnTest.Location = new System.Drawing.Point(3, 3);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(86, 38);
            this.btnTest.TabIndex = 0;
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // button6
            // 
            this.button6.AutoSize = true;
            this.button6.Image = ((System.Drawing.Image)(resources.GetObject("button6.Image")));
            this.button6.Location = new System.Drawing.Point(87, 179);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(112, 38);
            this.button6.TabIndex = 5;
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.AutoSize = true;
            this.button7.Image = ((System.Drawing.Image)(resources.GetObject("button7.Image")));
            this.button7.Location = new System.Drawing.Point(3, 135);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(105, 38);
            this.button7.TabIndex = 6;
            this.button7.Text = " ";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.AutoSize = true;
            this.button8.Image = ((System.Drawing.Image)(resources.GetObject("button8.Image")));
            this.button8.Location = new System.Drawing.Point(3, 47);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(156, 38);
            this.button8.TabIndex = 7;
            this.button8.Text = " ";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // lstPage
            // 
            this.lstPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstPage.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lstPage.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lstPage.FormattingEnabled = true;
            this.lstPage.HorizontalScrollbar = true;
            this.lstPage.IntegralHeight = false;
            this.lstPage.ItemHeight = 16;
            this.lstPage.Location = new System.Drawing.Point(0, 0);
            this.lstPage.Name = "lstPage";
            this.lstPage.Size = new System.Drawing.Size(263, 405);
            this.lstPage.TabIndex = 10;
            this.lstPage.SelectedIndexChanged += new System.EventHandler(this.lstPage_SelectedIndexChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // fPanel
            // 
            this.fPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.fPanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.fPanel.Controls.Add(this.splitContainer1);
            this.fPanel.Location = new System.Drawing.Point(0, 0);
            this.fPanel.Name = "fPanel";
            this.fPanel.Size = new System.Drawing.Size(263, 670);
            this.fPanel.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel_head);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lstPage);
            this.splitContainer1.Size = new System.Drawing.Size(263, 670);
            this.splitContainer1.SplitterDistance = 261;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel_head
            // 
            this.panel_head.AutoScroll = true;
            this.panel_head.Controls.Add(this.flowLayoutPanel1);
            this.panel_head.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_head.Location = new System.Drawing.Point(0, 0);
            this.panel_head.Name = "panel_head";
            this.panel_head.Size = new System.Drawing.Size(263, 261);
            this.panel_head.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.btnTest);
            this.flowLayoutPanel1.Controls.Add(this.button14);
            this.flowLayoutPanel1.Controls.Add(this.button8);
            this.flowLayoutPanel1.Controls.Add(this.button11);
            this.flowLayoutPanel1.Controls.Add(this.button5);
            this.flowLayoutPanel1.Controls.Add(this.button7);
            this.flowLayoutPanel1.Controls.Add(this.button15);
            this.flowLayoutPanel1.Controls.Add(this.button4);
            this.flowLayoutPanel1.Controls.Add(this.button6);
            this.flowLayoutPanel1.Controls.Add(this.button13);
            this.flowLayoutPanel1.Controls.Add(this.button18);
            this.flowLayoutPanel1.Controls.Add(this.button16);
            this.flowLayoutPanel1.Controls.Add(this.button17);
            this.flowLayoutPanel1.Controls.Add(this.button12);
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.button19);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(263, 261);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // button14
            // 
            this.button14.AutoSize = true;
            this.button14.Image = ((System.Drawing.Image)(resources.GetObject("button14.Image")));
            this.button14.Location = new System.Drawing.Point(95, 3);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(103, 38);
            this.button14.TabIndex = 19;
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button11
            // 
            this.button11.AutoSize = true;
            this.button11.Image = ((System.Drawing.Image)(resources.GetObject("button11.Image")));
            this.button11.Location = new System.Drawing.Point(3, 91);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(101, 38);
            this.button11.TabIndex = 16;
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button5
            // 
            this.button5.AutoSize = true;
            this.button5.Image = ((System.Drawing.Image)(resources.GetObject("button5.Image")));
            this.button5.Location = new System.Drawing.Point(110, 91);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(91, 38);
            this.button5.TabIndex = 15;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button15
            // 
            this.button15.AutoSize = true;
            this.button15.Image = ((System.Drawing.Image)(resources.GetObject("button15.Image")));
            this.button15.Location = new System.Drawing.Point(114, 135);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(88, 38);
            this.button15.TabIndex = 20;
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // button4
            // 
            this.button4.AutoSize = true;
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.Location = new System.Drawing.Point(3, 179);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(78, 38);
            this.button4.TabIndex = 14;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button13
            // 
            this.button13.AutoSize = true;
            this.button13.Image = ((System.Drawing.Image)(resources.GetObject("button13.Image")));
            this.button13.Location = new System.Drawing.Point(3, 223);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(116, 38);
            this.button13.TabIndex = 18;
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button18
            // 
            this.button18.AutoSize = true;
            this.button18.Image = ((System.Drawing.Image)(resources.GetObject("button18.Image")));
            this.button18.Location = new System.Drawing.Point(125, 223);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(100, 38);
            this.button18.TabIndex = 23;
            this.button18.UseVisualStyleBackColor = true;
            this.button18.Click += new System.EventHandler(this.button18_Click);
            // 
            // button16
            // 
            this.button16.AutoSize = true;
            this.button16.Image = ((System.Drawing.Image)(resources.GetObject("button16.Image")));
            this.button16.Location = new System.Drawing.Point(3, 267);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(109, 38);
            this.button16.TabIndex = 21;
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // button17
            // 
            this.button17.AutoSize = true;
            this.button17.Image = ((System.Drawing.Image)(resources.GetObject("button17.Image")));
            this.button17.Location = new System.Drawing.Point(118, 267);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(121, 38);
            this.button17.TabIndex = 22;
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // button12
            // 
            this.button12.AutoSize = true;
            this.button12.Image = ((System.Drawing.Image)(resources.GetObject("button12.Image")));
            this.button12.Location = new System.Drawing.Point(3, 311);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(145, 38);
            this.button12.TabIndex = 17;
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button19
            // 
            this.button19.AutoSize = true;
            this.button19.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button19.Location = new System.Drawing.Point(3, 355);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(179, 30);
            this.button19.TabIndex = 24;
            this.button19.Text = "没有我想看的报纸";
            this.button19.UseVisualStyleBackColor = true;
            this.button19.Click += new System.EventHandler(this.button19_Click);
            // 
            // panel_Top
            // 
            this.panel_Top.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_Top.AutoSize = true;
            this.panel_Top.Controls.Add(this.toolBar_Top);
            this.panel_Top.Location = new System.Drawing.Point(786, 0);
            this.panel_Top.Margin = new System.Windows.Forms.Padding(0);
            this.panel_Top.Name = "panel_Top";
            this.panel_Top.Size = new System.Drawing.Size(378, 56);
            this.panel_Top.TabIndex = 3;
            // 
            // toolBar_Top
            // 
            this.toolBar_Top.Dock = System.Windows.Forms.DockStyle.None;
            this.toolBar_Top.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbZoomIn,
            this.tbZoomOut,
            this.tbToWidth,
            this.tbToHeight,
            this.tbLeft90,
            this.tbRight90,
            this.tbFullScreen});
            this.toolBar_Top.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolBar_Top.Location = new System.Drawing.Point(0, 0);
            this.toolBar_Top.Name = "toolBar_Top";
            this.toolBar_Top.Size = new System.Drawing.Size(373, 56);
            this.toolBar_Top.TabIndex = 1;
            this.toolBar_Top.Text = "顶部工具栏";
            // 
            // tbZoomIn
            // 
            this.tbZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("tbZoomIn.Image")));
            this.tbZoomIn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbZoomIn.Name = "tbZoomIn";
            this.tbZoomIn.Size = new System.Drawing.Size(36, 53);
            this.tbZoomIn.Text = "放大";
            this.tbZoomIn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tbZoomIn.ToolTipText = "放大";
            this.tbZoomIn.Click += new System.EventHandler(this.tbZoomIn_Click);
            // 
            // tbZoomOut
            // 
            this.tbZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("tbZoomOut.Image")));
            this.tbZoomOut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbZoomOut.Name = "tbZoomOut";
            this.tbZoomOut.Size = new System.Drawing.Size(36, 53);
            this.tbZoomOut.Text = "缩小";
            this.tbZoomOut.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tbZoomOut.Click += new System.EventHandler(this.tbZoomOut_Click);
            // 
            // tbToWidth
            // 
            this.tbToWidth.Image = ((System.Drawing.Image)(resources.GetObject("tbToWidth.Image")));
            this.tbToWidth.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbToWidth.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbToWidth.Name = "tbToWidth";
            this.tbToWidth.Size = new System.Drawing.Size(60, 53);
            this.tbToWidth.Text = "适合宽度";
            this.tbToWidth.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tbToWidth.Click += new System.EventHandler(this.tbToWidth_Click);
            // 
            // tbToHeight
            // 
            this.tbToHeight.Image = ((System.Drawing.Image)(resources.GetObject("tbToHeight.Image")));
            this.tbToHeight.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbToHeight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbToHeight.Name = "tbToHeight";
            this.tbToHeight.Size = new System.Drawing.Size(60, 53);
            this.tbToHeight.Text = "适应高度";
            this.tbToHeight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tbToHeight.Click += new System.EventHandler(this.tbToHeight_Click);
            // 
            // tbLeft90
            // 
            this.tbLeft90.Image = ((System.Drawing.Image)(resources.GetObject("tbLeft90.Image")));
            this.tbLeft90.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbLeft90.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbLeft90.Name = "tbLeft90";
            this.tbLeft90.Size = new System.Drawing.Size(72, 53);
            this.tbLeft90.Text = "逆时针旋转";
            this.tbLeft90.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tbLeft90.Click += new System.EventHandler(this.tbLeft90_Click);
            // 
            // tbRight90
            // 
            this.tbRight90.Image = ((System.Drawing.Image)(resources.GetObject("tbRight90.Image")));
            this.tbRight90.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbRight90.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbRight90.Name = "tbRight90";
            this.tbRight90.Size = new System.Drawing.Size(72, 53);
            this.tbRight90.Text = "顺时针旋转";
            this.tbRight90.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tbRight90.Click += new System.EventHandler(this.tbRight90_Click);
            // 
            // tbFullScreen
            // 
            this.tbFullScreen.Image = ((System.Drawing.Image)(resources.GetObject("tbFullScreen.Image")));
            this.tbFullScreen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tbFullScreen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbFullScreen.Name = "tbFullScreen";
            this.tbFullScreen.Size = new System.Drawing.Size(36, 53);
            this.tbFullScreen.Text = "全屏";
            this.tbFullScreen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tbFullScreen.Click += new System.EventHandler(this.tbFullScreen_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("华文中宋", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(-1, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(430, 42);
            this.label1.TabIndex = 4;
            this.label1.Text = "在线报纸阅读器";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelCenter
            // 
            this.panelCenter.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.panelCenter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCenter.Controls.Add(this.labInfo);
            this.panelCenter.Controls.Add(this.label3);
            this.panelCenter.Controls.Add(this.label2);
            this.panelCenter.Controls.Add(this.linkEersoft);
            this.panelCenter.Controls.Add(this.label1);
            this.panelCenter.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelCenter.Location = new System.Drawing.Point(452, 216);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Size = new System.Drawing.Size(430, 222);
            this.panelCenter.TabIndex = 5;
            // 
            // labInfo
            // 
            this.labInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labInfo.Location = new System.Drawing.Point(0, 208);
            this.labInfo.Name = "labInfo";
            this.labInfo.Size = new System.Drawing.Size(428, 12);
            this.labInfo.TabIndex = 8;
            this.labInfo.Text = "准备就绪...";
            this.labInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("华文宋体", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(61, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(325, 66);
            this.label3.TabIndex = 7;
            this.label3.Text = "使用说明 ：\r\n鼠标指向屏幕左侧选择报纸和版面\r\n鼠标指向屏幕右上方显示工具栏";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.SystemColors.Info;
            this.label2.Location = new System.Drawing.Point(-1, 167);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(430, 23);
            this.label2.TabIndex = 6;
            this.label2.Text = "所有报纸版权归原网站或出版社所有";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkEersoft
            // 
            this.linkEersoft.LinkColor = System.Drawing.SystemColors.Info;
            this.linkEersoft.Location = new System.Drawing.Point(3, 0);
            this.linkEersoft.Name = "linkEersoft";
            this.linkEersoft.Size = new System.Drawing.Size(50, 18);
            this.linkEersoft.TabIndex = 5;
            this.linkEersoft.TabStop = true;
            this.linkEersoft.Text = "EERSOFT";
            this.linkEersoft.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkEersoft_LinkClicked);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(154, 311);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 25;
            this.button1.Text = "工厂模式测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1164, 670);
            this.Controls.Add(this.panelCenter);
            this.Controls.Add(this.panel_Top);
            this.Controls.Add(this.fPanel);
            this.Controls.Add(this.MainViewer);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Name = "Form1";
            this.Text = "Eersoft在线报纸阅读器";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.fPanel.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel_head.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel_Top.ResumeLayout(false);
            this.panel_Top.PerformLayout();
            this.toolBar_Top.ResumeLayout(false);
            this.toolBar_Top.PerformLayout();
            this.panelCenter.ResumeLayout(false);
            this.panelCenter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PdfiumViewer.PdfViewer MainViewer;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.ListBox lstPage;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel fPanel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel_head;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Panel panel_Top;
        private System.Windows.Forms.ToolStrip toolBar_Top;
        private System.Windows.Forms.ToolStripButton tbLeft90;
        private System.Windows.Forms.ToolStripButton tbRight90;
        private System.Windows.Forms.ToolStripButton tbFullScreen;
        private System.Windows.Forms.ToolStripButton tbZoomIn;
        private System.Windows.Forms.ToolStripButton tbZoomOut;
        private System.Windows.Forms.ToolStripButton tbToWidth;
        private System.Windows.Forms.ToolStripButton tbToHeight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.LinkLabel linkEersoft;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labInfo;
        private System.Windows.Forms.Button button1;
    }
}

