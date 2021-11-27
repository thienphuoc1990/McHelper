
namespace McHelper
{
    partial class Form1
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
            this.buttonOpenWindow = new System.Windows.Forms.Button();
            this.buttonReloadDatabase = new System.Windows.Forms.Button();
            this.dataGridViewLinks = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLinks)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOpenWindow
            // 
            this.buttonOpenWindow.Location = new System.Drawing.Point(12, 12);
            this.buttonOpenWindow.Name = "buttonOpenWindow";
            this.buttonOpenWindow.Size = new System.Drawing.Size(115, 23);
            this.buttonOpenWindow.TabIndex = 0;
            this.buttonOpenWindow.Text = "Open Window";
            this.buttonOpenWindow.UseVisualStyleBackColor = true;
            this.buttonOpenWindow.Click += new System.EventHandler(this.ButtonOpenWindow_Click);
            // 
            // buttonReloadDatabase
            // 
            this.buttonReloadDatabase.Location = new System.Drawing.Point(673, 12);
            this.buttonReloadDatabase.Name = "buttonReloadDatabase";
            this.buttonReloadDatabase.Size = new System.Drawing.Size(115, 23);
            this.buttonReloadDatabase.TabIndex = 1;
            this.buttonReloadDatabase.Text = "Reload Database";
            this.buttonReloadDatabase.UseVisualStyleBackColor = true;
            this.buttonReloadDatabase.Click += new System.EventHandler(this.ButtonReloadDatabase_Click);
            // 
            // dataGridViewLinks
            // 
            this.dataGridViewLinks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLinks.Location = new System.Drawing.Point(12, 41);
            this.dataGridViewLinks.Name = "dataGridViewLinks";
            this.dataGridViewLinks.Size = new System.Drawing.Size(776, 156);
            this.dataGridViewLinks.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dataGridViewLinks);
            this.Controls.Add(this.buttonReloadDatabase);
            this.Controls.Add(this.buttonOpenWindow);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLinks)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenWindow;
        private System.Windows.Forms.Button buttonReloadDatabase;
        private System.Windows.Forms.DataGridView dataGridViewLinks;
    }
}

