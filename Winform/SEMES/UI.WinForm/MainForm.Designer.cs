namespace UI.WinForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnMaximize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelSideMenu = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnCalendar = new System.Windows.Forms.Button();
            this.btnHistory = new System.Windows.Forms.Button();
            this.btnPacients = new System.Windows.Forms.Button();
            this.btnUsers = new System.Windows.Forms.Button();
            this.panelMenuHeader = new System.Windows.Forms.Panel();
            this.lblLastName = new System.Windows.Forms.Label();
            this.linkProfile = new System.Windows.Forms.LinkLabel();
            this.pictureBoxPhoto = new System.Windows.Forms.PictureBox();
            this.btnSideMenu = new System.Windows.Forms.PictureBox();
            this.lblPosition = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.panelDesktopHeader = new System.Windows.Forms.Panel();
            this.lblCaption = new System.Windows.Forms.Label();
            this.btnChildFormClose = new System.Windows.Forms.Button();
            this.panelDesktop = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.PanelClientArea.SuspendLayout();
            this.panelTitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panelSideMenu.SuspendLayout();
            this.panelMenuHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPhoto)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSideMenu)).BeginInit();
            this.panelDesktopHeader.SuspendLayout();
            this.panelDesktop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelClientArea
            // 
            this.PanelClientArea.Controls.Add(this.panelDesktop);
            this.PanelClientArea.Controls.Add(this.panelDesktopHeader);
            this.PanelClientArea.Controls.Add(this.panelSideMenu);
            this.PanelClientArea.Controls.Add(this.panelTitleBar);
            this.PanelClientArea.Location = new System.Drawing.Point(2, 2);
            this.PanelClientArea.Margin = new System.Windows.Forms.Padding(6);
            this.PanelClientArea.Size = new System.Drawing.Size(2596, 1104);
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.RoyalBlue;
            this.panelTitleBar.Controls.Add(this.label1);
            this.panelTitleBar.Controls.Add(this.pictureBox2);
            this.panelTitleBar.Controls.Add(this.btnMinimize);
            this.panelTitleBar.Controls.Add(this.btnMaximize);
            this.panelTitleBar.Controls.Add(this.btnClose);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelTitleBar.Margin = new System.Windows.Forms.Padding(6);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Size = new System.Drawing.Size(2596, 74);
            this.panelTitleBar.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(76, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(259, 31);
            this.label1.TabIndex = 15;
            this.label1.Text = "PCB 불량 검사 프로그램";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(17, 15);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(6);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(54, 46);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 6;
            this.pictureBox2.TabStop = false;
            // 
            // btnMinimize
            // 
            this.btnMinimize.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.MediumSeaGreen;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Image = global::UI.WinForm.Properties.Resources.btnMinimize;
            this.btnMinimize.Location = new System.Drawing.Point(2368, 0);
            this.btnMinimize.Margin = new System.Windows.Forms.Padding(6);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(76, 74);
            this.btnMinimize.TabIndex = 2;
            this.btnMinimize.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // btnMaximize
            // 
            this.btnMaximize.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnMaximize.FlatAppearance.BorderSize = 0;
            this.btnMaximize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.MediumSlateBlue;
            this.btnMaximize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaximize.Image = global::UI.WinForm.Properties.Resources.btnMaximize;
            this.btnMaximize.Location = new System.Drawing.Point(2444, 0);
            this.btnMaximize.Margin = new System.Windows.Forms.Padding(6);
            this.btnMaximize.Name = "btnMaximize";
            this.btnMaximize.Size = new System.Drawing.Size(76, 74);
            this.btnMaximize.TabIndex = 1;
            this.btnMaximize.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnMaximize.UseVisualStyleBackColor = true;
            this.btnMaximize.Click += new System.EventHandler(this.btnMaximize_Click);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.IndianRed;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Image = global::UI.WinForm.Properties.Resources.btnClose;
            this.btnClose.Location = new System.Drawing.Point(2520, 0);
            this.btnClose.Margin = new System.Windows.Forms.Padding(6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(76, 74);
            this.btnClose.TabIndex = 0;
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // panelSideMenu
            // 
            this.panelSideMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(59)))), ((int)(((byte)(104)))));
            this.panelSideMenu.Controls.Add(this.btnLogout);
            this.panelSideMenu.Controls.Add(this.btnReports);
            this.panelSideMenu.Controls.Add(this.btnCalendar);
            this.panelSideMenu.Controls.Add(this.btnHistory);
            this.panelSideMenu.Controls.Add(this.btnPacients);
            this.panelSideMenu.Controls.Add(this.btnUsers);
            this.panelSideMenu.Controls.Add(this.panelMenuHeader);
            this.panelSideMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSideMenu.Location = new System.Drawing.Point(0, 74);
            this.panelSideMenu.Margin = new System.Windows.Forms.Padding(6);
            this.panelSideMenu.Name = "panelSideMenu";
            this.panelSideMenu.Size = new System.Drawing.Size(498, 1030);
            this.panelSideMenu.TabIndex = 1;
            // 
            // btnLogout
            // 
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("MS Reference Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogout.ForeColor = System.Drawing.Color.DarkGray;
            this.btnLogout.Image = ((System.Drawing.Image)(resources.GetObject("btnLogout.Image")));
            this.btnLogout.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogout.Location = new System.Drawing.Point(0, 683);
            this.btnLogout.Margin = new System.Windows.Forms.Padding(4);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.btnLogout.Size = new System.Drawing.Size(498, 83);
            this.btnLogout.TabIndex = 25;
            this.btnLogout.Text = "  로그아웃";
            this.btnLogout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // btnReports
            // 
            this.btnReports.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReports.FlatAppearance.BorderSize = 0;
            this.btnReports.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReports.Font = new System.Drawing.Font("MS Reference Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReports.ForeColor = System.Drawing.Color.DarkGray;
            this.btnReports.Image = ((System.Drawing.Image)(resources.GetObject("btnReports.Image")));
            this.btnReports.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReports.Location = new System.Drawing.Point(0, 600);
            this.btnReports.Margin = new System.Windows.Forms.Padding(4);
            this.btnReports.Name = "btnReports";
            this.btnReports.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.btnReports.Size = new System.Drawing.Size(498, 83);
            this.btnReports.TabIndex = 24;
            this.btnReports.Text = "  분석 결과";
            this.btnReports.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReports.UseVisualStyleBackColor = true;
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click);
            // 
            // btnCalendar
            // 
            this.btnCalendar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCalendar.FlatAppearance.BorderSize = 0;
            this.btnCalendar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalendar.Font = new System.Drawing.Font("MS Reference Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalendar.ForeColor = System.Drawing.Color.DarkGray;
            this.btnCalendar.Image = ((System.Drawing.Image)(resources.GetObject("btnCalendar.Image")));
            this.btnCalendar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCalendar.Location = new System.Drawing.Point(0, 517);
            this.btnCalendar.Margin = new System.Windows.Forms.Padding(4);
            this.btnCalendar.Name = "btnCalendar";
            this.btnCalendar.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.btnCalendar.Size = new System.Drawing.Size(498, 83);
            this.btnCalendar.TabIndex = 23;
            this.btnCalendar.Text = "  사내 일정";
            this.btnCalendar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCalendar.UseVisualStyleBackColor = true;
            this.btnCalendar.Click += new System.EventHandler(this.btnCalendar_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHistory.FlatAppearance.BorderSize = 0;
            this.btnHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHistory.Font = new System.Drawing.Font("MS Reference Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHistory.ForeColor = System.Drawing.Color.DarkGray;
            this.btnHistory.Image = ((System.Drawing.Image)(resources.GetObject("btnHistory.Image")));
            this.btnHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHistory.Location = new System.Drawing.Point(0, 434);
            this.btnHistory.Margin = new System.Windows.Forms.Padding(4);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.btnHistory.Size = new System.Drawing.Size(498, 83);
            this.btnHistory.TabIndex = 22;
            this.btnHistory.Text = "  검사 기록";
            this.btnHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // btnPacients
            // 
            this.btnPacients.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPacients.FlatAppearance.BorderSize = 0;
            this.btnPacients.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPacients.Font = new System.Drawing.Font("MS Reference Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPacients.ForeColor = System.Drawing.Color.DarkGray;
            this.btnPacients.Image = ((System.Drawing.Image)(resources.GetObject("btnPacients.Image")));
            this.btnPacients.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPacients.Location = new System.Drawing.Point(0, 351);
            this.btnPacients.Margin = new System.Windows.Forms.Padding(4);
            this.btnPacients.Name = "btnPacients";
            this.btnPacients.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.btnPacients.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnPacients.Size = new System.Drawing.Size(498, 83);
            this.btnPacients.TabIndex = 21;
            this.btnPacients.Text = "  메신저";
            this.btnPacients.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPacients.UseVisualStyleBackColor = true;
            this.btnPacients.Click += new System.EventHandler(this.btnPacients_Click);
            // 
            // btnUsers
            // 
            this.btnUsers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUsers.FlatAppearance.BorderSize = 0;
            this.btnUsers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsers.Font = new System.Drawing.Font("MS Reference Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUsers.ForeColor = System.Drawing.Color.DarkGray;
            this.btnUsers.Image = ((System.Drawing.Image)(resources.GetObject("btnUsers.Image")));
            this.btnUsers.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUsers.Location = new System.Drawing.Point(0, 268);
            this.btnUsers.Margin = new System.Windows.Forms.Padding(4);
            this.btnUsers.Name = "btnUsers";
            this.btnUsers.Padding = new System.Windows.Forms.Padding(22, 0, 0, 0);
            this.btnUsers.Size = new System.Drawing.Size(498, 83);
            this.btnUsers.TabIndex = 20;
            this.btnUsers.Text = "  사원 정보";
            this.btnUsers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUsers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUsers.UseVisualStyleBackColor = true;
            this.btnUsers.Click += new System.EventHandler(this.btnUsers_Click);
            // 
            // panelMenuHeader
            // 
            this.panelMenuHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(52)))), ((int)(((byte)(93)))));
            this.panelMenuHeader.Controls.Add(this.lblLastName);
            this.panelMenuHeader.Controls.Add(this.linkProfile);
            this.panelMenuHeader.Controls.Add(this.pictureBoxPhoto);
            this.panelMenuHeader.Controls.Add(this.btnSideMenu);
            this.panelMenuHeader.Controls.Add(this.lblPosition);
            this.panelMenuHeader.Controls.Add(this.lblName);
            this.panelMenuHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMenuHeader.Location = new System.Drawing.Point(0, 0);
            this.panelMenuHeader.Margin = new System.Windows.Forms.Padding(6);
            this.panelMenuHeader.Name = "panelMenuHeader";
            this.panelMenuHeader.Size = new System.Drawing.Size(498, 212);
            this.panelMenuHeader.TabIndex = 0;
            // 
            // lblLastName
            // 
            this.lblLastName.AutoSize = true;
            this.lblLastName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLastName.ForeColor = System.Drawing.Color.DarkGray;
            this.lblLastName.Location = new System.Drawing.Point(156, 66);
            this.lblLastName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblLastName.Name = "lblLastName";
            this.lblLastName.Size = new System.Drawing.Size(55, 30);
            this.lblLastName.TabIndex = 17;
            this.lblLastName.Text = "이름";
            // 
            // linkProfile
            // 
            this.linkProfile.ActiveLinkColor = System.Drawing.SystemColors.Highlight;
            this.linkProfile.AutoSize = true;
            this.linkProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkProfile.LinkColor = System.Drawing.Color.RoyalBlue;
            this.linkProfile.Location = new System.Drawing.Point(13, 144);
            this.linkProfile.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.linkProfile.Name = "linkProfile";
            this.linkProfile.Size = new System.Drawing.Size(126, 31);
            this.linkProfile.TabIndex = 16;
            this.linkProfile.TabStop = true;
            this.linkProfile.Text = "나의 프로필";
            this.linkProfile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkProfile_LinkClicked);
            // 
            // pictureBoxPhoto
            // 
            this.pictureBoxPhoto.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxPhoto.Image = global::UI.WinForm.Properties.Resources.defaultImageProfileUser;
            this.pictureBoxPhoto.Location = new System.Drawing.Point(13, 20);
            this.pictureBoxPhoto.Margin = new System.Windows.Forms.Padding(6);
            this.pictureBoxPhoto.Name = "pictureBoxPhoto";
            this.pictureBoxPhoto.Size = new System.Drawing.Size(139, 118);
            this.pictureBoxPhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPhoto.TabIndex = 13;
            this.pictureBoxPhoto.TabStop = false;
            this.pictureBoxPhoto.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxPhoto_Paint);
            // 
            // btnSideMenu
            // 
            this.btnSideMenu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSideMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSideMenu.Image = ((System.Drawing.Image)(resources.GetObject("btnSideMenu.Image")));
            this.btnSideMenu.Location = new System.Drawing.Point(410, 9);
            this.btnSideMenu.Margin = new System.Windows.Forms.Padding(6);
            this.btnSideMenu.Name = "btnSideMenu";
            this.btnSideMenu.Size = new System.Drawing.Size(32, 32);
            this.btnSideMenu.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnSideMenu.TabIndex = 12;
            this.btnSideMenu.TabStop = false;
            this.btnSideMenu.Click += new System.EventHandler(this.btnSideMenu_Click);
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPosition.ForeColor = System.Drawing.Color.DarkGray;
            this.lblPosition.Location = new System.Drawing.Point(156, 96);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(55, 30);
            this.lblPosition.TabIndex = 15;
            this.lblPosition.Text = "직급";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblName.Location = new System.Drawing.Point(156, 35);
            this.lblName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 32);
            this.lblName.TabIndex = 14;
            this.lblName.Text = "성";
            // 
            // panelDesktopHeader
            // 
            this.panelDesktopHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelDesktopHeader.Controls.Add(this.lblCaption);
            this.panelDesktopHeader.Controls.Add(this.btnChildFormClose);
            this.panelDesktopHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelDesktopHeader.Location = new System.Drawing.Point(498, 74);
            this.panelDesktopHeader.Margin = new System.Windows.Forms.Padding(6);
            this.panelDesktopHeader.Name = "panelDesktopHeader";
            this.panelDesktopHeader.Size = new System.Drawing.Size(2098, 55);
            this.panelDesktopHeader.TabIndex = 2;
            // 
            // lblCaption
            // 
            this.lblCaption.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.DimGray;
            this.lblCaption.Location = new System.Drawing.Point(65, 0);
            this.lblCaption.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(919, 55);
            this.lblCaption.TabIndex = 18;
            this.lblCaption.Text = "Child Form Title";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnChildFormClose
            // 
            this.btnChildFormClose.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnChildFormClose.FlatAppearance.BorderSize = 0;
            this.btnChildFormClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.IndianRed;
            this.btnChildFormClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChildFormClose.Image = global::UI.WinForm.Properties.Resources.CloseDark;
            this.btnChildFormClose.Location = new System.Drawing.Point(0, 0);
            this.btnChildFormClose.Margin = new System.Windows.Forms.Padding(6);
            this.btnChildFormClose.Name = "btnChildFormClose";
            this.btnChildFormClose.Size = new System.Drawing.Size(65, 55);
            this.btnChildFormClose.TabIndex = 19;
            this.btnChildFormClose.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnChildFormClose.UseVisualStyleBackColor = true;
            this.btnChildFormClose.Click += new System.EventHandler(this.btnChildFormClose_Click);
            // 
            // panelDesktop
            // 
            this.panelDesktop.Controls.Add(this.pictureBox3);
            this.panelDesktop.Controls.Add(this.label3);
            this.panelDesktop.Controls.Add(this.label2);
            this.panelDesktop.Controls.Add(this.pictureBox1);
            this.panelDesktop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDesktop.Location = new System.Drawing.Point(498, 129);
            this.panelDesktop.Margin = new System.Windows.Forms.Padding(6);
            this.panelDesktop.Name = "panelDesktop";
            this.panelDesktop.Size = new System.Drawing.Size(2098, 975);
            this.panelDesktop.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.DimGray;
            this.label3.Location = new System.Drawing.Point(896, 793);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(237, 36);
            this.label3.TabIndex = 16;
            this.label3.Text = "세메스 아산 사업장";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.DimGray;
            this.label2.Location = new System.Drawing.Point(812, 694);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(424, 52);
            this.label2.TabIndex = 15;
            this.label2.Text = "PCB 불량 검사 프로그램";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(546, 107);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(931, 211);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(546, 417);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(6);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(931, 211);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 17;
            this.pictureBox3.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2600, 1108);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(403, 125);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "메인 프로그램";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.PanelClientArea.ResumeLayout(false);
            this.panelTitleBar.ResumeLayout(false);
            this.panelTitleBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panelSideMenu.ResumeLayout(false);
            this.panelMenuHeader.ResumeLayout(false);
            this.panelMenuHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPhoto)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSideMenu)).EndInit();
            this.panelDesktopHeader.ResumeLayout(false);
            this.panelDesktop.ResumeLayout(false);
            this.panelDesktop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSideMenu;
        private System.Windows.Forms.Panel panelMenuHeader;
        private System.Windows.Forms.LinkLabel linkProfile;
        internal System.Windows.Forms.PictureBox pictureBoxPhoto;
        internal System.Windows.Forms.PictureBox btnSideMenu;
        internal System.Windows.Forms.Label lblPosition;
        internal System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Panel panelTitleBar;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnMaximize;
        private System.Windows.Forms.Button btnClose;
        internal System.Windows.Forms.Label lblLastName;
        internal System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Panel panelDesktop;
        private System.Windows.Forms.Panel panelDesktopHeader;
        private System.Windows.Forms.Button btnChildFormClose;
        internal System.Windows.Forms.Label lblCaption;
        internal System.Windows.Forms.Button btnReports;
        internal System.Windows.Forms.Button btnCalendar;
        internal System.Windows.Forms.Button btnHistory;
        internal System.Windows.Forms.Button btnPacients;
        internal System.Windows.Forms.Button btnUsers;
        internal System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.PictureBox pictureBox1;
        internal System.Windows.Forms.PictureBox pictureBox3;
    }
}