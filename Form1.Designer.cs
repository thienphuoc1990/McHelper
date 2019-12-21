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
            this.ButtonOpenAuto = new System.Windows.Forms.Button();
            this.buttonThemNhanVat = new System.Windows.Forms.Button();
            this.dataGridViewCharacters = new System.Windows.Forms.DataGridView();
            this.buttonXoaNhanVat = new System.Windows.Forms.Button();
            this.buttonSuaNhanVat = new System.Windows.Forms.Button();
            this.buttonOpenGame = new System.Windows.Forms.Button();
            this.labelAuthorVersion = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCharacters)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonOpenAuto
            // 
            this.ButtonOpenAuto.Location = new System.Drawing.Point(372, 295);
            this.ButtonOpenAuto.Name = "ButtonOpenAuto";
            this.ButtonOpenAuto.Size = new System.Drawing.Size(116, 23);
            this.ButtonOpenAuto.TabIndex = 0;
            this.ButtonOpenAuto.Text = "Mở Auto";
            this.ButtonOpenAuto.UseVisualStyleBackColor = true;
            this.ButtonOpenAuto.Click += new System.EventHandler(this.buttonOpenAuto_Click);
            // 
            // buttonThemNhanVat
            // 
            this.buttonThemNhanVat.Location = new System.Drawing.Point(12, 295);
            this.buttonThemNhanVat.Name = "buttonThemNhanVat";
            this.buttonThemNhanVat.Size = new System.Drawing.Size(114, 23);
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
            this.dataGridViewCharacters.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewCharacters.MultiSelect = false;
            this.dataGridViewCharacters.Name = "dataGridViewCharacters";
            this.dataGridViewCharacters.RowHeadersWidth = 70;
            this.dataGridViewCharacters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewCharacters.Size = new System.Drawing.Size(564, 277);
            this.dataGridViewCharacters.TabIndex = 6;
            // 
            // buttonXoaNhanVat
            // 
            this.buttonXoaNhanVat.Location = new System.Drawing.Point(132, 295);
            this.buttonXoaNhanVat.Name = "buttonXoaNhanVat";
            this.buttonXoaNhanVat.Size = new System.Drawing.Size(114, 23);
            this.buttonXoaNhanVat.TabIndex = 7;
            this.buttonXoaNhanVat.Text = "Xóa nhân vật";
            this.buttonXoaNhanVat.UseVisualStyleBackColor = true;
            this.buttonXoaNhanVat.Click += new System.EventHandler(this.buttonXoaNhanVat_Click);
            // 
            // buttonSuaNhanVat
            // 
            this.buttonSuaNhanVat.Location = new System.Drawing.Point(252, 295);
            this.buttonSuaNhanVat.Name = "buttonSuaNhanVat";
            this.buttonSuaNhanVat.Size = new System.Drawing.Size(114, 23);
            this.buttonSuaNhanVat.TabIndex = 8;
            this.buttonSuaNhanVat.Text = "Sửa nhân vật";
            this.buttonSuaNhanVat.UseVisualStyleBackColor = true;
            this.buttonSuaNhanVat.Click += new System.EventHandler(this.buttonSuaNhanVat_Click);
            // 
            // buttonOpenGame
            // 
            this.buttonOpenGame.Location = new System.Drawing.Point(494, 295);
            this.buttonOpenGame.Name = "buttonOpenGame";
            this.buttonOpenGame.Size = new System.Drawing.Size(82, 23);
            this.buttonOpenGame.TabIndex = 9;
            this.buttonOpenGame.Text = "Mở Game";
            this.buttonOpenGame.UseVisualStyleBackColor = true;
            this.buttonOpenGame.Click += new System.EventHandler(this.buttonOpenGame_Click);
            // 
            // labelAuthorVersion
            // 
            this.labelAuthorVersion.AutoSize = true;
            this.labelAuthorVersion.Location = new System.Drawing.Point(12, 327);
            this.labelAuthorVersion.Name = "labelAuthorVersion";
            this.labelAuthorVersion.Size = new System.Drawing.Size(35, 13);
            this.labelAuthorVersion.TabIndex = 10;
            this.labelAuthorVersion.Text = "label1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 349);
            this.Controls.Add(this.labelAuthorVersion);
            this.Controls.Add(this.buttonOpenGame);
            this.Controls.Add(this.buttonSuaNhanVat);
            this.Controls.Add(this.buttonXoaNhanVat);
            this.Controls.Add(this.dataGridViewCharacters);
            this.Controls.Add(this.buttonThemNhanVat);
            this.Controls.Add(this.ButtonOpenAuto);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoVPT - Tử La Lan";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCharacters)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonOpenAuto;
        private System.Windows.Forms.Button buttonThemNhanVat;
        private System.Windows.Forms.DataGridView dataGridViewCharacters;
        private System.Windows.Forms.Button buttonXoaNhanVat;
        private System.Windows.Forms.Button buttonSuaNhanVat;
        private System.Windows.Forms.Button buttonOpenGame;
        private System.Windows.Forms.Label labelAuthorVersion;
    }
}

