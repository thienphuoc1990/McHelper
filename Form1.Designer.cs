namespace AutoVPT
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
            this.buttonThemNhanVat = new System.Windows.Forms.Button();
            this.dataGridViewCharacters = new System.Windows.Forms.DataGridView();
            this.buttonXoaNhanVat = new System.Windows.Forms.Button();
            this.buttonSuaNhanVat = new System.Windows.Forms.Button();
            this.buttonOpenGame = new System.Windows.Forms.Button();
            this.labelAuthorVersion = new System.Windows.Forms.Label();
            this.buttonRunAuto = new System.Windows.Forms.Button();
            this.buttonStopAuto = new System.Windows.Forms.Button();
            this.buttonConfigAuto = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.buttonOpenTestForm = new System.Windows.Forms.Button();
            this.buttonRunEvent = new System.Windows.Forms.Button();
            this.tabPageStatus = new System.Windows.Forms.TabPage();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCharacters)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.tabPageStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonThemNhanVat
            // 
            this.buttonThemNhanVat.Location = new System.Drawing.Point(6, 295);
            this.buttonThemNhanVat.Name = "buttonThemNhanVat";
            this.buttonThemNhanVat.Size = new System.Drawing.Size(101, 23);
            this.buttonThemNhanVat.TabIndex = 5;
            this.buttonThemNhanVat.Text = "Thêm nhân vật";
            this.buttonThemNhanVat.UseVisualStyleBackColor = true;
            this.buttonThemNhanVat.Click += new System.EventHandler(this.buttonThemNhanVat_Click);
            // 
            // dataGridViewCharacters
            // 
            this.dataGridViewCharacters.AllowUserToAddRows = false;
            this.dataGridViewCharacters.AllowUserToDeleteRows = false;
            this.dataGridViewCharacters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.dataGridViewCharacters.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridViewCharacters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCharacters.GridColor = System.Drawing.SystemColors.Window;
            this.dataGridViewCharacters.Location = new System.Drawing.Point(6, 6);
            this.dataGridViewCharacters.MultiSelect = false;
            this.dataGridViewCharacters.Name = "dataGridViewCharacters";
            this.dataGridViewCharacters.RowHeadersWidth = 70;
            this.dataGridViewCharacters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewCharacters.Size = new System.Drawing.Size(735, 283);
            this.dataGridViewCharacters.TabIndex = 6;
            // 
            // buttonXoaNhanVat
            // 
            this.buttonXoaNhanVat.Location = new System.Drawing.Point(113, 295);
            this.buttonXoaNhanVat.Name = "buttonXoaNhanVat";
            this.buttonXoaNhanVat.Size = new System.Drawing.Size(98, 23);
            this.buttonXoaNhanVat.TabIndex = 7;
            this.buttonXoaNhanVat.Text = "Xóa nhân vật";
            this.buttonXoaNhanVat.UseVisualStyleBackColor = true;
            this.buttonXoaNhanVat.Click += new System.EventHandler(this.buttonXoaNhanVat_Click);
            // 
            // buttonSuaNhanVat
            // 
            this.buttonSuaNhanVat.Location = new System.Drawing.Point(217, 295);
            this.buttonSuaNhanVat.Name = "buttonSuaNhanVat";
            this.buttonSuaNhanVat.Size = new System.Drawing.Size(87, 23);
            this.buttonSuaNhanVat.TabIndex = 8;
            this.buttonSuaNhanVat.Text = "Sửa nhân vật";
            this.buttonSuaNhanVat.UseVisualStyleBackColor = true;
            this.buttonSuaNhanVat.Click += new System.EventHandler(this.buttonSuaNhanVat_Click);
            // 
            // buttonOpenGame
            // 
            this.buttonOpenGame.Location = new System.Drawing.Point(310, 295);
            this.buttonOpenGame.Name = "buttonOpenGame";
            this.buttonOpenGame.Size = new System.Drawing.Size(65, 23);
            this.buttonOpenGame.TabIndex = 9;
            this.buttonOpenGame.Text = "Mở Game";
            this.buttonOpenGame.UseVisualStyleBackColor = true;
            this.buttonOpenGame.Click += new System.EventHandler(this.buttonOpenGame_Click);
            // 
            // labelAuthorVersion
            // 
            this.labelAuthorVersion.AutoSize = true;
            this.labelAuthorVersion.Location = new System.Drawing.Point(13, 366);
            this.labelAuthorVersion.Name = "labelAuthorVersion";
            this.labelAuthorVersion.Size = new System.Drawing.Size(35, 13);
            this.labelAuthorVersion.TabIndex = 10;
            this.labelAuthorVersion.Text = "label1";
            // 
            // buttonRunAuto
            // 
            this.buttonRunAuto.Location = new System.Drawing.Point(469, 295);
            this.buttonRunAuto.Name = "buttonRunAuto";
            this.buttonRunAuto.Size = new System.Drawing.Size(71, 23);
            this.buttonRunAuto.TabIndex = 11;
            this.buttonRunAuto.Text = "Chạy Auto";
            this.buttonRunAuto.UseVisualStyleBackColor = true;
            this.buttonRunAuto.Click += new System.EventHandler(this.buttonRunAuto_Click);
            // 
            // buttonStopAuto
            // 
            this.buttonStopAuto.Location = new System.Drawing.Point(546, 295);
            this.buttonStopAuto.Name = "buttonStopAuto";
            this.buttonStopAuto.Size = new System.Drawing.Size(77, 23);
            this.buttonStopAuto.TabIndex = 12;
            this.buttonStopAuto.Text = "Ngừng Auto";
            this.buttonStopAuto.UseVisualStyleBackColor = true;
            this.buttonStopAuto.Click += new System.EventHandler(this.buttonStopAuto_Click);
            // 
            // buttonConfigAuto
            // 
            this.buttonConfigAuto.Location = new System.Drawing.Point(381, 295);
            this.buttonConfigAuto.Name = "buttonConfigAuto";
            this.buttonConfigAuto.Size = new System.Drawing.Size(82, 23);
            this.buttonConfigAuto.TabIndex = 13;
            this.buttonConfigAuto.Text = "Cài đặt Auto";
            this.buttonConfigAuto.UseVisualStyleBackColor = true;
            this.buttonConfigAuto.Click += new System.EventHandler(this.buttonConfigAuto_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageMain);
            this.tabControl1.Controls.Add(this.tabPageStatus);
            this.tabControl1.Location = new System.Drawing.Point(12, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(754, 350);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.buttonOpenTestForm);
            this.tabPageMain.Controls.Add(this.buttonRunEvent);
            this.tabPageMain.Controls.Add(this.dataGridViewCharacters);
            this.tabPageMain.Controls.Add(this.buttonStopAuto);
            this.tabPageMain.Controls.Add(this.buttonConfigAuto);
            this.tabPageMain.Controls.Add(this.buttonRunAuto);
            this.tabPageMain.Controls.Add(this.buttonThemNhanVat);
            this.tabPageMain.Controls.Add(this.buttonXoaNhanVat);
            this.tabPageMain.Controls.Add(this.buttonSuaNhanVat);
            this.tabPageMain.Controls.Add(this.buttonOpenGame);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(746, 324);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Bảng chính";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // buttonOpenTestForm
            // 
            this.buttonOpenTestForm.Location = new System.Drawing.Point(712, 295);
            this.buttonOpenTestForm.Name = "buttonOpenTestForm";
            this.buttonOpenTestForm.Size = new System.Drawing.Size(28, 23);
            this.buttonOpenTestForm.TabIndex = 15;
            this.buttonOpenTestForm.UseVisualStyleBackColor = true;
            this.buttonOpenTestForm.Click += new System.EventHandler(this.buttonOpenTestForm_Click);
            // 
            // buttonRunEvent
            // 
            this.buttonRunEvent.Location = new System.Drawing.Point(629, 295);
            this.buttonRunEvent.Name = "buttonRunEvent";
            this.buttonRunEvent.Size = new System.Drawing.Size(77, 23);
            this.buttonRunEvent.TabIndex = 14;
            this.buttonRunEvent.Text = "Chạy Event";
            this.buttonRunEvent.UseVisualStyleBackColor = true;
            this.buttonRunEvent.Click += new System.EventHandler(this.buttonRunEvent_Click);
            // 
            // tabPageStatus
            // 
            this.tabPageStatus.Controls.Add(this.textBoxStatus);
            this.tabPageStatus.Location = new System.Drawing.Point(4, 22);
            this.tabPageStatus.Name = "tabPageStatus";
            this.tabPageStatus.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStatus.Size = new System.Drawing.Size(746, 324);
            this.tabPageStatus.TabIndex = 1;
            this.tabPageStatus.Text = "Trạng thái";
            this.tabPageStatus.UseVisualStyleBackColor = true;
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(3, 3);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(815, 318);
            this.textBoxStatus.TabIndex = 65;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 385);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.labelAuthorVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoVPT - Tử La Lan";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCharacters)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.tabPageStatus.ResumeLayout(false);
            this.tabPageStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonThemNhanVat;
        private System.Windows.Forms.Button buttonXoaNhanVat;
        private System.Windows.Forms.Button buttonSuaNhanVat;
        private System.Windows.Forms.Button buttonOpenGame;
        private System.Windows.Forms.Label labelAuthorVersion;
        private System.Windows.Forms.Button buttonRunAuto;
        private System.Windows.Forms.Button buttonStopAuto;
        private System.Windows.Forms.Button buttonConfigAuto;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TabPage tabPageStatus;
        public System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonOpenTestForm;
        public System.Windows.Forms.DataGridView dataGridViewCharacters;
        private System.Windows.Forms.Button buttonRunEvent;
    }
}

