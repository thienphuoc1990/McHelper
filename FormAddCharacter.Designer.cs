namespace AutoVPT
{
    partial class FormAddCharacter
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
            this.textBoxID = new System.Windows.Forms.TextBox();
            this.textBoxLink = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonAddNewCharacter = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID";
            // 
            // textBoxID
            // 
            this.textBoxID.Location = new System.Drawing.Point(96, 10);
            this.textBoxID.Name = "textBoxID";
            this.textBoxID.Size = new System.Drawing.Size(230, 20);
            this.textBoxID.TabIndex = 1;
            // 
            // textBoxLink
            // 
            this.textBoxLink.Location = new System.Drawing.Point(96, 36);
            this.textBoxLink.Name = "textBoxLink";
            this.textBoxLink.Size = new System.Drawing.Size(230, 20);
            this.textBoxLink.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Link";
            // 
            // buttonAddNewCharacter
            // 
            this.buttonAddNewCharacter.Location = new System.Drawing.Point(108, 81);
            this.buttonAddNewCharacter.Name = "buttonAddNewCharacter";
            this.buttonAddNewCharacter.Size = new System.Drawing.Size(129, 23);
            this.buttonAddNewCharacter.TabIndex = 4;
            this.buttonAddNewCharacter.Text = "Thêm";
            this.buttonAddNewCharacter.UseVisualStyleBackColor = true;
            this.buttonAddNewCharacter.Click += new System.EventHandler(this.buttonAddNewCharacter_Click);
            // 
            // FormAddCharacter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 116);
            this.Controls.Add(this.buttonAddNewCharacter);
            this.Controls.Add(this.textBoxLink);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxID);
            this.Controls.Add(this.label1);
            this.Name = "FormAddCharacter";
            this.Text = "FormAddCharacter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxID;
        private System.Windows.Forms.TextBox textBoxLink;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonAddNewCharacter;
    }
}