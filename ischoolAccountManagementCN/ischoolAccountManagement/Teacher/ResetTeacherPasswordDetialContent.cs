using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ischoolAccountManagement.DAO;

namespace ischoolAccountManagement.Teacher
{
    public partial class ResetTeacherPasswordDetialContent : FISCA.Presentation.DetailContent
    {
        public ResetTeacherPasswordDetialContent()
        {
            InitializeComponent();

            string loadingID = "";
            bool isAllowReset = false;
            BackgroundWorker bkwLoader = new BackgroundWorker();
            bkwLoader.DoWork += delegate
            {
                var teacherRec = K12.Data.Teacher.SelectByID(loadingID);
                UDT_AdminData udtAdmin = Utility.GetAdminData("teacher");
                if (teacherRec.TALoginName != "" && udtAdmin.Check() && teacherRec.TALoginName.ToLower().EndsWith("@" + udtAdmin.Domain.ToLower()))
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
                var teacherRec = K12.Data.Teacher.SelectByID(PrimaryKey);
                UDT_AdminData udtAdmin = Utility.GetAdminData("teacher");
                udtAdmin.uploadAcc(new List<Service.UserAccount>() { new Service.UserAccount() { Account = teacherRec.TALoginName, Password = "" + newPWD.Tag } });

                DevComponents.DotNetBar.MessageBoxEx.Show("密码已设定。");
            }
        }
    }
}
