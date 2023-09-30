using System.Drawing;
using System.Windows.Forms;


namespace A_Scout_Viewer
{    

    partial class A_Scout : MetroFramework.Forms.MetroForm //상속 클래스 변경 
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.btPreview = new MetroFramework.Controls.MetroButton();
            this.btStop = new MetroFramework.Controls.MetroButton();
            this.btCapture = new MetroFramework.Controls.MetroButton();
            this.btPlay = new MetroFramework.Controls.MetroButton();
            this.btSave = new MetroFramework.Controls.MetroButton();
            this.TileState = new MetroFramework.Controls.MetroTile();
            this.tbPlay = new MetroFramework.Controls.MetroTrackBar();
            this.cbPlaySpeed = new MetroFramework.Controls.MetroComboBox();
            this.lbFrameRate = new MetroFramework.Drawing.Html.HtmlLabel();
            this.tbFrameRate = new MetroFramework.Controls.MetroTrackBar();
            this.tbISO = new MetroFramework.Controls.MetroTrackBar();
            this.lbISO = new MetroFramework.Drawing.Html.HtmlLabel();
            this.tbExposure = new MetroFramework.Controls.MetroTrackBar();
            this.lbExposure = new MetroFramework.Drawing.Html.HtmlLabel();
            this.btPlayMini = new MetroFramework.Controls.MetroButton();
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tgFocusMode = new MetroFramework.Controls.MetroToggle();
            this.htmlLabel1 = new MetroFramework.Drawing.Html.HtmlLabel();
            this.lbFPS = new MetroFramework.Drawing.Html.HtmlLabel();
            this.btFilePlay = new MetroFramework.Controls.MetroButton();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.metroCBCam3 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCBCam2 = new MetroFramework.Controls.MetroCheckBox();
            this.metroCBCam1 = new MetroFramework.Controls.MetroCheckBox();
            this.lbFPS2 = new MetroFramework.Drawing.Html.HtmlLabel();
            this.lbFPS3 = new MetroFramework.Drawing.Html.HtmlLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btPreview
            // 
            this.btPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPreview.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.btPreview.Location = new System.Drawing.Point(21, 472);
            this.btPreview.Name = "btPreview";
            this.btPreview.Size = new System.Drawing.Size(180, 45);
            this.btPreview.TabIndex = 0;
            this.btPreview.Text = "Live View";
            this.btPreview.UseSelectable = true;
            this.btPreview.Click += new System.EventHandler(this.btPreview_Click);
            // 
            // btStop
            // 
            this.btStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btStop.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.btStop.Location = new System.Drawing.Point(21, 542);
            this.btStop.Name = "btStop";
            this.btStop.Size = new System.Drawing.Size(180, 45);
            this.btStop.TabIndex = 2;
            this.btStop.Text = "Live Stop";
            this.btStop.UseSelectable = true;
            this.btStop.Click += new System.EventHandler(this.btStop_Click);
            // 
            // btCapture
            // 
            this.btCapture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btCapture.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.btCapture.Location = new System.Drawing.Point(21, 612);
            this.btCapture.Name = "btCapture";
            this.btCapture.Size = new System.Drawing.Size(180, 45);
            this.btCapture.TabIndex = 3;
            this.btCapture.Text = "Record 3 Seconds";
            this.btCapture.UseSelectable = true;
            this.btCapture.Click += new System.EventHandler(this.btCapture_Click);
            // 
            // btPlay
            // 
            this.btPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPlay.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.btPlay.Location = new System.Drawing.Point(21, 736);
            this.btPlay.Name = "btPlay";
            this.btPlay.Size = new System.Drawing.Size(180, 45);
            this.btPlay.TabIndex = 4;
            this.btPlay.Text = "Play";
            this.btPlay.UseSelectable = true;
            this.btPlay.Click += new System.EventHandler(this.btPlay_Click);
            // 
            // btSave
            // 
            this.btSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btSave.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.btSave.Location = new System.Drawing.Point(21, 806);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(180, 45);
            this.btSave.TabIndex = 5;
            this.btSave.Text = "Save";
            this.btSave.UseSelectable = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // TileState
            // 
            this.TileState.ActiveControl = null;
            this.TileState.Location = new System.Drawing.Point(21, 128);
            this.TileState.Name = "TileState";
            this.TileState.Size = new System.Drawing.Size(180, 120);
            this.TileState.TabIndex = 0;
            this.TileState.Text = "Camera State";
            this.TileState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.TileState.TileImage = global::A_Scout_Viewer.Properties.Resources.Golf;
            this.TileState.TileImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.TileState.TileTextFontSize = MetroFramework.MetroTileTextSize.Tall;
            this.TileState.UseSelectable = true;
            this.TileState.Click += new System.EventHandler(this.TileState_Click);
            // 
            // tbPlay
            // 
            this.tbPlay.BackColor = System.Drawing.Color.Transparent;
            this.tbPlay.Location = new System.Drawing.Point(309, 959);
            this.tbPlay.Name = "tbPlay";
            this.tbPlay.Size = new System.Drawing.Size(1115, 51);
            this.tbPlay.TabIndex = 6;
            this.tbPlay.Text = "metroTrackBar1";
            this.tbPlay.Scroll += new System.Windows.Forms.ScrollEventHandler(this.tbPlay_Scroll);
            // 
            // cbPlaySpeed
            // 
            this.cbPlaySpeed.FormattingEnabled = true;
            this.cbPlaySpeed.ItemHeight = 23;
            this.cbPlaySpeed.Items.AddRange(new object[] {
            "PlaySpeed  1 x",
            "PlaySpeed  1/2 x",
            "PlaySpeed  1/3 x",
            "PlaySpeed  1/4 x"});
            this.cbPlaySpeed.Location = new System.Drawing.Point(21, 682);
            this.cbPlaySpeed.Name = "cbPlaySpeed";
            this.cbPlaySpeed.Size = new System.Drawing.Size(180, 29);
            this.cbPlaySpeed.TabIndex = 7;
            this.cbPlaySpeed.UseSelectable = true;
            this.cbPlaySpeed.SelectedIndexChanged += new System.EventHandler(this.cbPlaySpeed_SelectedIndexChanged);
            // 
            // lbFrameRate
            // 
            this.lbFrameRate.AutoScroll = true;
            this.lbFrameRate.AutoScrollMinSize = new System.Drawing.Size(198, 53);
            this.lbFrameRate.AutoSize = false;
            this.lbFrameRate.BackColor = System.Drawing.SystemColors.Window;
            this.lbFrameRate.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbFrameRate.Location = new System.Drawing.Point(1449, 128);
            this.lbFrameRate.Name = "lbFrameRate";
            this.lbFrameRate.Size = new System.Drawing.Size(408, 50);
            this.lbFrameRate.TabIndex = 9;
            this.lbFrameRate.Text = "Frame Rate";
            // 
            // tbFrameRate
            // 
            this.tbFrameRate.BackColor = System.Drawing.Color.Transparent;
            this.tbFrameRate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tbFrameRate.Location = new System.Drawing.Point(1450, 199);
            this.tbFrameRate.Name = "tbFrameRate";
            this.tbFrameRate.Size = new System.Drawing.Size(407, 37);
            this.tbFrameRate.TabIndex = 10;
            this.tbFrameRate.Text = "Frame Rate";
            this.tbFrameRate.Scroll += new System.Windows.Forms.ScrollEventHandler(this.tbFrameRate_Scroll);
            // 
            // tbISO
            // 
            this.tbISO.BackColor = System.Drawing.Color.Transparent;
            this.tbISO.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tbISO.Location = new System.Drawing.Point(1451, 353);
            this.tbISO.Name = "tbISO";
            this.tbISO.Size = new System.Drawing.Size(407, 37);
            this.tbISO.TabIndex = 12;
            this.tbISO.Text = "ISO";
            this.tbISO.Scroll += new System.Windows.Forms.ScrollEventHandler(this.tbISO_Scroll);
            // 
            // lbISO
            // 
            this.lbISO.AutoScroll = true;
            this.lbISO.AutoScrollMinSize = new System.Drawing.Size(69, 53);
            this.lbISO.AutoSize = false;
            this.lbISO.BackColor = System.Drawing.SystemColors.Window;
            this.lbISO.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbISO.Location = new System.Drawing.Point(1450, 282);
            this.lbISO.Name = "lbISO";
            this.lbISO.Size = new System.Drawing.Size(408, 50);
            this.lbISO.TabIndex = 11;
            this.lbISO.Text = "ISO";
            // 
            // tbExposure
            // 
            this.tbExposure.BackColor = System.Drawing.Color.Transparent;
            this.tbExposure.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tbExposure.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tbExposure.Location = new System.Drawing.Point(1452, 515);
            this.tbExposure.Name = "tbExposure";
            this.tbExposure.Size = new System.Drawing.Size(407, 37);
            this.tbExposure.TabIndex = 14;
            this.tbExposure.Text = "ISO";
            this.tbExposure.Scroll += new System.Windows.Forms.ScrollEventHandler(this.tbExposure_Scroll);
            // 
            // lbExposure
            // 
            this.lbExposure.AutoScroll = true;
            this.lbExposure.AutoScrollMinSize = new System.Drawing.Size(255, 53);
            this.lbExposure.AutoSize = false;
            this.lbExposure.BackColor = System.Drawing.SystemColors.Window;
            this.lbExposure.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbExposure.Location = new System.Drawing.Point(1451, 444);
            this.lbExposure.Name = "lbExposure";
            this.lbExposure.Size = new System.Drawing.Size(408, 50);
            this.lbExposure.TabIndex = 13;
            this.lbExposure.Text = "Exposure Time";
            // 
            // btPlayMini
            // 
            this.btPlayMini.BackgroundImage = global::A_Scout_Viewer.Properties.Resources.Play;
            this.btPlayMini.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPlayMini.Location = new System.Drawing.Point(225, 959);
            this.btPlayMini.Name = "btPlayMini";
            this.btPlayMini.Size = new System.Drawing.Size(60, 60);
            this.btPlayMini.TabIndex = 8;
            this.btPlayMini.UseSelectable = true;
            this.btPlayMini.Click += new System.EventHandler(this.btPlayMini_Click);
            // 
            // metroPanel1
            // 
            this.metroPanel1.BackgroundImage = global::A_Scout_Viewer.Properties.Resources.AIVIWORKS_CI_1;
            this.metroPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 10;
            this.metroPanel1.Location = new System.Drawing.Point(21, 980);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(180, 40);
            this.metroPanel1.TabIndex = 0;
            this.metroPanel1.TabStop = true;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 10;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(225, 128);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(600, 412);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox_DoubleClick);
            // 
            // tgFocusMode
            // 
            this.tgFocusMode.AutoSize = true;
            this.tgFocusMode.Location = new System.Drawing.Point(1682, 663);
            this.tgFocusMode.Name = "tgFocusMode";
            this.tgFocusMode.Size = new System.Drawing.Size(80, 34);
            this.tgFocusMode.TabIndex = 15;
            this.tgFocusMode.Text = "Off";
            this.tgFocusMode.UseSelectable = true;
            this.tgFocusMode.CheckedChanged += new System.EventHandler(this.tgFocusMode_CheckedChanged);
            // 
            // htmlLabel1
            // 
            this.htmlLabel1.AutoScroll = true;
            this.htmlLabel1.AutoScrollMinSize = new System.Drawing.Size(205, 53);
            this.htmlLabel1.AutoSize = false;
            this.htmlLabel1.BackColor = System.Drawing.SystemColors.Window;
            this.htmlLabel1.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.htmlLabel1.Location = new System.Drawing.Point(1449, 649);
            this.htmlLabel1.Name = "htmlLabel1";
            this.htmlLabel1.Size = new System.Drawing.Size(322, 50);
            this.htmlLabel1.TabIndex = 16;
            this.htmlLabel1.Text = "Focus Mode";
            // 
            // lbFPS
            // 
            this.lbFPS.AutoScroll = true;
            this.lbFPS.AutoScrollMinSize = new System.Drawing.Size(207, 53);
            this.lbFPS.AutoSize = false;
            this.lbFPS.BackColor = System.Drawing.SystemColors.Window;
            this.lbFPS.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbFPS.Location = new System.Drawing.Point(1452, 767);
            this.lbFPS.Name = "lbFPS";
            this.lbFPS.Size = new System.Drawing.Size(408, 50);
            this.lbFPS.TabIndex = 14;
            this.lbFPS.Text = "Cam1 FPS : ";
            // 
            // btFilePlay
            // 
            this.btFilePlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btFilePlay.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.btFilePlay.Location = new System.Drawing.Point(21, 876);
            this.btFilePlay.Name = "btFilePlay";
            this.btFilePlay.Size = new System.Drawing.Size(180, 45);
            this.btFilePlay.TabIndex = 17;
            this.btFilePlay.Text = "File Open";
            this.btFilePlay.UseSelectable = true;
            this.btFilePlay.Click += new System.EventHandler(this.btFilePlay_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(831, 128);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(600, 412);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 19;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.DoubleClick += new System.EventHandler(this.pictureBox_DoubleClick);
            // 
            // pictureBox3
            // 
            this.pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox3.Location = new System.Drawing.Point(225, 546);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(600, 412);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 20;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.DoubleClick += new System.EventHandler(this.pictureBox_DoubleClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.metroCBCam3);
            this.groupBox1.Controls.Add(this.metroCBCam2);
            this.groupBox1.Controls.Add(this.metroCBCam1);
            this.groupBox1.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.Location = new System.Drawing.Point(13, 268);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(196, 183);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Camera List";
            // 
            // metroCBCam3
            // 
            this.metroCBCam3.AutoSize = true;
            this.metroCBCam3.FontSize = MetroFramework.MetroCheckBoxSize.Tall;
            this.metroCBCam3.Location = new System.Drawing.Point(28, 132);
            this.metroCBCam3.Name = "metroCBCam3";
            this.metroCBCam3.Size = new System.Drawing.Size(103, 25);
            this.metroCBCam3.TabIndex = 2;
            this.metroCBCam3.Text = "Camera 3";
            this.metroCBCam3.UseSelectable = true;
            this.metroCBCam3.CheckedChanged += new System.EventHandler(this.metroCBCam3_CheckedChanged);
            // 
            // metroCBCam2
            // 
            this.metroCBCam2.AutoSize = true;
            this.metroCBCam2.FontSize = MetroFramework.MetroCheckBoxSize.Tall;
            this.metroCBCam2.Location = new System.Drawing.Point(28, 88);
            this.metroCBCam2.Name = "metroCBCam2";
            this.metroCBCam2.Size = new System.Drawing.Size(103, 25);
            this.metroCBCam2.TabIndex = 1;
            this.metroCBCam2.Text = "Camera 2";
            this.metroCBCam2.UseSelectable = true;
            this.metroCBCam2.CheckedChanged += new System.EventHandler(this.metroCBCam2_CheckedChanged);
            // 
            // metroCBCam1
            // 
            this.metroCBCam1.AutoSize = true;
            this.metroCBCam1.FontSize = MetroFramework.MetroCheckBoxSize.Tall;
            this.metroCBCam1.Location = new System.Drawing.Point(28, 44);
            this.metroCBCam1.Name = "metroCBCam1";
            this.metroCBCam1.Size = new System.Drawing.Size(103, 25);
            this.metroCBCam1.TabIndex = 0;
            this.metroCBCam1.Text = "Camera 1";
            this.metroCBCam1.UseSelectable = true;
            this.metroCBCam1.CheckedChanged += new System.EventHandler(this.metroCBCam1_CheckedChanged);
            // 
            // lbFPS2
            // 
            this.lbFPS2.AutoScroll = true;
            this.lbFPS2.AutoScrollMinSize = new System.Drawing.Size(207, 53);
            this.lbFPS2.AutoSize = false;
            this.lbFPS2.BackColor = System.Drawing.SystemColors.Window;
            this.lbFPS2.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbFPS2.Location = new System.Drawing.Point(1452, 846);
            this.lbFPS2.Name = "lbFPS2";
            this.lbFPS2.Size = new System.Drawing.Size(408, 50);
            this.lbFPS2.TabIndex = 15;
            this.lbFPS2.Text = "Cam2 FPS : ";
            // 
            // lbFPS3
            // 
            this.lbFPS3.AutoScroll = true;
            this.lbFPS3.AutoScrollMinSize = new System.Drawing.Size(207, 53);
            this.lbFPS3.AutoSize = false;
            this.lbFPS3.BackColor = System.Drawing.SystemColors.Window;
            this.lbFPS3.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbFPS3.Location = new System.Drawing.Point(1452, 925);
            this.lbFPS3.Name = "lbFPS3";
            this.lbFPS3.Size = new System.Drawing.Size(408, 50);
            this.lbFPS3.TabIndex = 16;
            this.lbFPS3.Text = "Cam3 FPS : ";
            // 
            // A_Scout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(19F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1900, 1040);
            this.Controls.Add(this.lbFPS3);
            this.Controls.Add(this.lbFPS2);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btFilePlay);
            this.Controls.Add(this.lbFPS);
            this.Controls.Add(this.tgFocusMode);
            this.Controls.Add(this.tbExposure);
            this.Controls.Add(this.tbISO);
            this.Controls.Add(this.lbExposure);
            this.Controls.Add(this.tbFrameRate);
            this.Controls.Add(this.lbISO);
            this.Controls.Add(this.lbFrameRate);
            this.Controls.Add(this.btPlayMini);
            this.Controls.Add(this.cbPlaySpeed);
            this.Controls.Add(this.tbPlay);
            this.Controls.Add(this.TileState);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.btPlay);
            this.Controls.Add(this.btCapture);
            this.Controls.Add(this.metroPanel1);
            this.Controls.Add(this.btStop);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btPreview);
            this.Controls.Add(this.htmlLabel1);
            this.Font = new System.Drawing.Font("Arial Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "A_Scout";
            this.Padding = new System.Windows.Forms.Padding(38, 100, 38, 33);
            this.Text = "A-Scout Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.A_Scout_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroButton btPreview;
        private System.Windows.Forms.PictureBox pictureBox1;
        private MetroFramework.Controls.MetroButton btStop;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroButton btCapture;
        private MetroFramework.Controls.MetroButton btPlay;
        private MetroFramework.Controls.MetroButton btSave;
        private MetroFramework.Controls.MetroTile TileState;
        private MetroFramework.Controls.MetroTrackBar tbPlay;
        private MetroFramework.Controls.MetroComboBox cbPlaySpeed;
        private MetroFramework.Controls.MetroButton btPlayMini;
        private MetroFramework.Drawing.Html.HtmlLabel lbFrameRate;
        private MetroFramework.Controls.MetroTrackBar tbFrameRate;
        private MetroFramework.Controls.MetroTrackBar tbISO;
        private MetroFramework.Drawing.Html.HtmlLabel lbISO;
        private MetroFramework.Controls.MetroTrackBar tbExposure;
        private MetroFramework.Drawing.Html.HtmlLabel lbExposure;
        private MetroFramework.Controls.MetroToggle tgFocusMode;
        private MetroFramework.Drawing.Html.HtmlLabel htmlLabel1;
        private MetroFramework.Drawing.Html.HtmlLabel lbFPS;
        private MetroFramework.Controls.MetroButton btFilePlay;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private GroupBox groupBox1;
        private MetroFramework.Controls.MetroCheckBox metroCBCam2;
        private MetroFramework.Controls.MetroCheckBox metroCBCam1;
        private MetroFramework.Controls.MetroCheckBox metroCBCam3;
        private MetroFramework.Drawing.Html.HtmlLabel lbFPS2;
        private MetroFramework.Drawing.Html.HtmlLabel lbFPS3;
    }
}

