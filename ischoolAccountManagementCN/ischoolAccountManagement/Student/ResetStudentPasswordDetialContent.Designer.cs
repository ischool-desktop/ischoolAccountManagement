namespace ischoolAccountManagement.Student
{
    partial class ResetStudentPasswordDetialContent
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnResetPassword = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // btnResetPassword
            // 
            this.btnResetPassword.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnResetPassword.AutoSize = true;
            this.btnResetPassword.BackColor = System.Drawing.Color.Transparent;
            this.btnResetPassword.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnResetPassword.Enabled = false;
            this.btnResetPassword.Location = new System.Drawing.Point(107, 0);
            this.btnResetPassword.Name = "btnResetPassword";
            this.btnResetPassword.Size = new System.Drawing.Size(75, 25);
            this.btnResetPassword.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnResetPassword.TabIndex = 4;
            this.btnResetPassword.Text = "重设密码";
            this.btnResetPassword.Click += new System.EventHandler(this.btnResetPassword_Click);
            // 
            // ResetStudentPasswordDetialContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnResetPassword);
            this.Group = "基本数据";
            this.Name = "ResetStudentPasswordDetialContent";
            this.Size = new System.Drawing.Size(550, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.ButtonX btnResetPassword;
    }
}
