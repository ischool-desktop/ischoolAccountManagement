using FISCA.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ischoolAccountManagement
{
    public partial class TypePassword : BaseForm
    {
        public TypePassword()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("密碼不得空白");
            }
            else if (txtPassword.Text != txtCPassword.Text)
            {
                DevComponents.DotNetBar.MessageBoxEx.Show("新密碼與確認密碼不相符");
            }
            else
            {
                this.Tag = txtPassword.Text;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }
    }
}
