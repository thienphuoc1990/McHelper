namespace AutoVPT
{
    partial class TestForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTestPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownTestX = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTestY = new System.Windows.Forms.NumericUpDown();
            this.buttonTestCapturePosition = new System.Windows.Forms.Button();
            this.textBoxTestID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonClickRightOnImage = new System.Windows.Forms.Button();
            this.buttonClickRightOnPoint = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path:";
            // 
            // textBoxTestPath
            // 
            this.textBoxTestPath.Location = new System.Drawing.Point(59, 39);
            this.textBoxTestPath.Name = "textBoxTestPath";
            this.textBoxTestPath.Size = new System.Drawing.Size(163, 20);
            this.textBoxTestPath.TabIndex = 1;
            this.textBoxTestPath.Text = "resources/in_map/char.png";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "X:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(153, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Y:";
            // 
            // numericUpDownTestX
            // 
            this.numericUpDownTestX.Location = new System.Drawing.Point(79, 96);
            this.numericUpDownTestX.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownTestX.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDownTestX.Name = "numericUpDownTestX";
            this.numericUpDownTestX.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownTestX.TabIndex = 6;
            // 
            // numericUpDownTestY
            // 
            this.numericUpDownTestY.Location = new System.Drawing.Point(225, 98);
            this.numericUpDownTestY.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownTestY.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDownTestY.Name = "numericUpDownTestY";
            this.numericUpDownTestY.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownTestY.TabIndex = 7;
            // 
            // buttonTestCapturePosition
            // 
            this.buttonTestCapturePosition.Location = new System.Drawing.Point(16, 174);
            this.buttonTestCapturePosition.Name = "buttonTestCapturePosition";
            this.buttonTestCapturePosition.Size = new System.Drawing.Size(110, 23);
            this.buttonTestCapturePosition.TabIndex = 9;
            this.buttonTestCapturePosition.Text = "Capture Position";
            this.buttonTestCapturePosition.UseVisualStyleBackColor = true;
            this.buttonTestCapturePosition.Click += new System.EventHandler(this.buttonTestCapturePosition_Click);
            // 
            // textBoxTestID
            // 
            this.textBoxTestID.Location = new System.Drawing.Point(59, 9);
            this.textBoxTestID.Name = "textBoxTestID";
            this.textBoxTestID.Size = new System.Drawing.Size(163, 20);
            this.textBoxTestID.TabIndex = 11;
            this.textBoxTestID.Text = "cb";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "ID:";
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Location = new System.Drawing.Point(225, 122);
            this.numericUpDownHeight.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownHeight.Name = "numericUpDownHeight";
            this.numericUpDownHeight.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownHeight.TabIndex = 15;
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Location = new System.Drawing.Point(79, 122);
            this.numericUpDownWidth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownWidth.Name = "numericUpDownWidth";
            this.numericUpDownWidth.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownWidth.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(153, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Height:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 124);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Width:";
            // 
            // buttonClickRightOnImage
            // 
            this.buttonClickRightOnImage.Location = new System.Drawing.Point(16, 203);
            this.buttonClickRightOnImage.Name = "buttonClickRightOnImage";
            this.buttonClickRightOnImage.Size = new System.Drawing.Size(110, 23);
            this.buttonClickRightOnImage.TabIndex = 16;
            this.buttonClickRightOnImage.Text = "Click right on image";
            this.buttonClickRightOnImage.UseVisualStyleBackColor = true;
            this.buttonClickRightOnImage.Click += new System.EventHandler(this.buttonClickRightOnImage_Click);
            // 
            // buttonClickRightOnPoint
            // 
            this.buttonClickRightOnPoint.Location = new System.Drawing.Point(16, 232);
            this.buttonClickRightOnPoint.Name = "buttonClickRightOnPoint";
            this.buttonClickRightOnPoint.Size = new System.Drawing.Size(110, 23);
            this.buttonClickRightOnPoint.TabIndex = 17;
            this.buttonClickRightOnPoint.Text = "Click right on point";
            this.buttonClickRightOnPoint.UseVisualStyleBackColor = true;
            this.buttonClickRightOnPoint.Click += new System.EventHandler(this.buttonClickRightOnPoint_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 494);
            this.Controls.Add(this.buttonClickRightOnPoint);
            this.Controls.Add(this.buttonClickRightOnImage);
            this.Controls.Add(this.numericUpDownHeight);
            this.Controls.Add(this.numericUpDownWidth);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxTestID);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.buttonTestCapturePosition);
            this.Controls.Add(this.numericUpDownTestY);
            this.Controls.Add(this.numericUpDownTestX);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxTestPath);
            this.Controls.Add(this.label1);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.Load += new System.EventHandler(this.TestForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTestY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxTestPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownTestX;
        private System.Windows.Forms.NumericUpDown numericUpDownTestY;
        private System.Windows.Forms.Button buttonTestCapturePosition;
        private System.Windows.Forms.TextBox textBoxTestID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonClickRightOnImage;
        private System.Windows.Forms.Button buttonClickRightOnPoint;
    }
}