using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using ischoolAccountManagement.DAO;
using FISCA.UDT;

namespace ischoolAccountManagement.Admin
{
    public partial class AdminForm : BaseForm
    {
        private string _Title = "网域管理者账号设定";
        private string _AccountType = "";
        UDT_AdminData _AdminData = null;

        public AdminForm(string AccountType)
        {
            _AccountType = AccountType;
            InitializeComponent();
            if (_AccountType == "student")
                _Title += "(学生)";

            if (_AccountType == "teacher")
                _Title += "(教师)";
            this.Text = _Title;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDomain.Text))
            {
                MsgBox.Show("网域必填。");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAccount.Text))
            {
                MsgBox.Show("账号必填。");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MsgBox.Show("密码必填。");
                return;
            }

            try
            {
                if (_AdminData == null)
                    _AdminData = new UDT_AdminData();

                _AdminData.Domain = txtDomain.Text;
                _AdminData.Account = txtAccount.Text;
                _AdminData.Password = Utility.ConvertStringToBase64String(txtPassword.Text);
                _AdminData.Type = _AccountType;
                _AdminData.Save();
                MsgBox.Show("储存完成");
                this.Close();
            }
            catch (Exception ex)
            {
                MsgBox.Show("储存过程发生错误" + ex.Message);
            }
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            txtAccount.Enabled = txtDomain.Enabled = txtPassword.Enabled = btnSave.Enabled = false;

            // 加载 udt 账号数据
            try
            {
                AccessHelper accHelper = new AccessHelper();
                string qry = "type='" + _AccountType + "'";
                List<UDT_AdminData> aData = accHelper.Select<UDT_AdminData>(qry);
                if (aData != null && aData.Count > 0)
                    _AdminData = aData[0];

                if (_AdminData != null)
                {
                    txtAccount.Text = _AdminData.Account;
                    txtDomain.Text = _AdminData.Domain;
                    txtPassword.Text = Utility.ConvertBase64StringToString(_AdminData.Password);
                }
            }
            catch (Exception ex)
            {

            }

            txtAccount.Enabled = txtDomain.Enabled = txtPassword.Enabled = btnSave.Enabled = true;
        }
    }
}

