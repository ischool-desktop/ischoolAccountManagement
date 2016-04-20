namespace ischoolAccountManagement
{
    partial class TypePassword
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
            this.btnExit = new DevComponents.DotNetBar.ButtonX();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.txtPassword = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.btnSubmit = new DevComponents.DotNetBar.ButtonX();
            this.txtCPassword = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.SuspendLayout();
            // 
            // btnExit
            // 
            this.btnExit.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnExit.AutoSize = true;
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Location = new System.Drawing.Point(187, 80);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 25);
            this.btnExit.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "取消";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // labelX3
            // 
            this.labelX3.AutoSize = true;
            this.labelX3.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX3.BackgroundStyle.Class = "";
            this.labelX3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX3.Location = new System.Drawing.Point(12, 51);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(60, 21);
            this.labelX3.TabIndex = 11;
            this.labelX3.Text = "确认密码";
            // 
            // txtPassword
            // 
            // 
            // 
            // 
            this.txtPassword.Border.Class = "TextBoxBorder";
            this.txtPassword.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtPassword.Location = new System.Drawing.Point(78, 10);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(184, 25);
            this.txtPassword.TabIndex = 0;
            // 
            // labelX2
            // 
            this.labelX2.AutoSize = true;
            this.labelX2.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX2.BackgroundStyle.Class = "";
            this.labelX2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX2.Location = new System.Drawing.Point(12, 12);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(47, 21);
            this.labelX2.TabIndex = 8;
            this.labelX2.Text = "新密码";
            // 
            // btnSubmit
            // 
            this.btnSubmit.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnSubmit.AutoSize = true;
            this.btnSubmit.BackColor = System.Drawing.Color.Transparent;
            this.btnSubmit.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnSubmit.Location = new System.Drawing.Point(104, 80);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 25);
            this.btnSubmit.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnSubmit.TabIndex = 2;
            this.btnSubmit.Text = "确认";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // txtCPassword
            // 
            // 
            // 
            // 
            this.txtCPassword.Border.Class = "TextBoxBorder";
            this.txtCPassword.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtCPassword.Location = new System.Drawing.Point(78, 49);
            this.txtCPassword.Name = "txtCPassword";
            this.txtCPassword.PasswordChar = '*';
            this.txtCPassword.Size = new System.Drawing.Size(184, 25);
            this.txtCPassword.TabIndex = 0;
            // 
            // TypePassword
            // 
            this.AcceptButton = this.btnSubmit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(276, 112);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.labelX3);
            this.Controls.Add(this.txtCPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.labelX2);
            this.Controls.Add(this.btnSubmit);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "TypePassword";
            this.Text = "重设密码";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.ButtonX btnExit;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.Controls.TextBoxX txtPassword;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.ButtonX btnSubmit;
        private DevComponents.DotNetBar.Controls.TextBoxX txtCPassword;
    }
}