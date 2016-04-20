using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ischoolAccountManagement.DAO;

namespace ischoolAccountManagement.Student
{
    public partial class ResetStudentPasswordDetialContent : FISCA.Presentation.DetailContent
    {
        public ResetStudentPasswordDetialContent()
        {
            InitializeComponent();

            string loadingID = "";
            bool isAllowReset = false;
            BackgroundWorker bkwLoader = new BackgroundWorker();
            bkwLoader.DoWork += delegate
            {
                var stuRec = K12.Data.Student.SelectByID(loadingID);
                UDT_AdminData udtAdmin = Utility.GetAdminData("student");
                if (stuRec.SALoginName != "" && udtAdmin.Check() && stuRec.SALoginName.ToLower().EndsWith("@" + udtAdmin.Domain.ToLower()))
                    isAllowReset = true;
                else
                    isAllowReset = false;
            };
            bkwLoader.RunWorkerCompleted += delegate
            {
                if (loadingID != this.PrimaryKey)
                {
                    this.btnResetPassword.Enabled = false;
                    this.Loading = true;
                    loadingID = this.PrimaryKey;
                    bkwLoader.RunWorkerAsync();
                }
                else
                {
                    this.Loading = false;
                    this.btnResetPassword.Enabled = isAllowReset;
                }
            };
            this.PrimaryKeyChanged += delegate
            {
                if (!bkwLoader.IsBusy)
                {
                    this.btnResetPassword.Enabled = false;
                    this.Loading = true;
                    loadingID = this.PrimaryKey;
                    bkwLoader.RunWorkerAsync();
                }
            };
            this.SaveButtonClick += delegate
            {
                if (!bkwLoader.IsBusy)
                {
                    this.btnResetPassword.Enabled = false;
                    this.Loading = true;
                    loadingID = this.PrimaryKey;
                    bkwLoader.RunWorkerAsync();
                }
            };
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            var newPWD = new TypePassword();
            if (newPWD.ShowDialog() == DialogResult.OK)
            {
                var stuRec = K12.Data.Student.SelectByID(PrimaryKey);
                UDT_AdminData udtAdmin = Utility.GetAdminData("student");
                udtAdmin.uploadAcc(new List<Service.UserAccount>() { new Service.UserAccount() { Account = stuRec.SALoginName, Password = "" + newPWD.Tag } });
                DevComponents.DotNetBar.MessageBoxEx.Show("密码已设定。");
            }
        }
    }
}
