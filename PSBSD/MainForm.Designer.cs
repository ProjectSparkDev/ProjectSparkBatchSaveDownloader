namespace PSBSD
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            DownloadBtn = new Button();
            BrowseBtn = new Button();
            pictureBox1 = new PictureBox();
            LogList = new ListBox();
            label1 = new Label();
            button1 = new Button();
            progressBar = new ProgressBar();
            pictureBox2 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // DownloadBtn
            // 
            DownloadBtn.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            DownloadBtn.Location = new Point(12, 372);
            DownloadBtn.Name = "DownloadBtn";
            DownloadBtn.Size = new Size(142, 58);
            DownloadBtn.TabIndex = 1;
            DownloadBtn.Text = "Authenticate and Download";
            DownloadBtn.UseVisualStyleBackColor = true;
            DownloadBtn.Click += DownloadBtn_Click;
            // 
            // BrowseBtn
            // 
            BrowseBtn.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BrowseBtn.Location = new Point(160, 378);
            BrowseBtn.Name = "BrowseBtn";
            BrowseBtn.Size = new Size(142, 47);
            BrowseBtn.TabIndex = 2;
            BrowseBtn.Text = "Set output folder";
            BrowseBtn.UseVisualStyleBackColor = true;
            BrowseBtn.Click += BrowseBtn_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(255, 80);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // LogList
            // 
            LogList.FormattingEnabled = true;
            LogList.ItemHeight = 15;
            LogList.Location = new Point(0, 92);
            LogList.Name = "LogList";
            LogList.Size = new Size(484, 274);
            LogList.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(273, 37);
            label1.Name = "label1";
            label1.Size = new Size(200, 32);
            label1.TabIndex = 5;
            label1.Text = "Save Downloader";
            // 
            // button1
            // 
            button1.Location = new Point(389, 372);
            button1.Name = "button1";
            button1.Size = new Size(84, 25);
            button1.TabIndex = 6;
            button1.Text = "Copy output";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(0, 436);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(484, 23);
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 7;
            // 
            // pictureBox2
            // 
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(308, 365);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(75, 73);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 8;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 461);
            Controls.Add(progressBar);
            Controls.Add(button1);
            Controls.Add(LogList);
            Controls.Add(BrowseBtn);
            Controls.Add(DownloadBtn);
            Controls.Add(pictureBox1);
            Controls.Add(label1);
            Controls.Add(pictureBox2);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(500, 500);
            MinimumSize = new Size(500, 500);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Project spark save downloader";
            Shown += MainForm_Shown;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button DownloadBtn;
        private Button BrowseBtn;
        private PictureBox pictureBox1;
        private ListBox LogList;
        private Label label1;
        private Button button1;
        private ProgressBar progressBar;
        private PictureBox pictureBox2;
    }
}
