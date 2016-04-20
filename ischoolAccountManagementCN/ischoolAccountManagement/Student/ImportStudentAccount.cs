using System.Collections.Generic;
using System.Windows.Forms;
using FISCA.UDT;
using SmartSchool.API.PlugIn;
using System;
using System.Data;
using K12.Data;
using System.Text;
using ischoolAccountManagement.DAO;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace ischoolAccountManagement
{
    class ImportStudentData : SmartSchool.API.PlugIn.Import.Importer
    {
        List<string> _FieldNameList = new List<string>();

        public ImportStudentData()
        {
            this.Image = null;
            this.Text = "汇入学生账号";
            _FieldNameList.Add("登入账号");
            _FieldNameList.Add("密码");
            //_FieldNameList.Add("姓");
            //_FieldNameList.Add("名");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            // 检查是否已设定网络管理者
            UDT_AdminData chkAdData = Utility.GetAdminData("student");
            bool chkAdDataErr = false;
            if (chkAdData == null)
                chkAdDataErr = true;

            if (chkAdData != null)
            {
                if (chkAdData.Account.Trim() == "" || chkAdData.Password.Trim() == "" || chkAdData.Domain.Trim() == "")
                    chkAdDataErr = true;
            }

            if (chkAdDataErr)
            {
                string msg = "网络管理者账号未设定或设定不完整，无法上传学生网域账号，请至进阶 设定网域管理者账号";
                FISCA.Presentation.Controls.MsgBox.Show(msg);
            }

            // 检查账号是否可以登入
            if (chkAdDataErr == false)
            {
                Utility.CheckAdminPWD("student");
            }

            //利用移除再新增按鈕可以正常設定功能
            addOption(wizard);

            wizard.PackageLimit = 3000;
            //必需要有的字段
            //wizard.RequiredFields.AddRange("学号");
            //可汇入的字段
            wizard.ImportableFields.AddRange(_FieldNameList);
            //設定驗證
            setupValidate(wizard);
            //設定匯入
            setupImport(wizard);
        }

        void addOption(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            VirtualRadioButton setAccount = new VirtualRadioButton("设定网域管理者账号", false);
            setAccount.CheckedChanged += delegate
            {
                if (setAccount.Checked)
                {
                    wizard.Options.Remove(setAccount);

                    Admin.AdminForm ad = new Admin.AdminForm("student");
                    ad.ShowDialog();
                    addOption(wizard);
                }
            };
            wizard.Options.Add(setAccount);
        }

        void setupValidate(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            UDT_AdminData udtAdmin = Utility.GetAdminData("student");
            // 系统内学生账号檢查
            List<string> accountKeysCheck = new List<string>();
            // 檢查domain帳號設定錯誤訊息
            string domainAccErr = "";
            wizard.ValidateStart += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
            {
                accountKeysCheck.Clear();
                #region 建立匯入帳號重複檢查排除表
                List<string> idList = new List<string>(e.List);

                foreach (StudentRecord rec in K12.Data.Student.SelectAll())
                {
                    if (!idList.Contains(rec.ID) && rec.SALoginName != "")
                    {
                        //不匯入的學生帳號加入排除
                        accountKeysCheck.Add(rec.SALoginName.ToLower().Trim());
                    }
                }
                #endregion

                #region 檢查設定的domain管理者帳號正確
                udtAdmin = Utility.GetAdminData("student");
                if (udtAdmin != null)
                {
                    if (!udtAdmin.Check())
                    {
                        domainAccErr = "网络管理者账号错误，无法上传学生网域账号，请至进阶 设定网域管理者账号";
                    }
                }
                else
                {
                    domainAccErr = "网络管理者账号未设定或设定不完整，无法上传学生网域账号，请至进阶 设定网域管理者账号";
                }
                #endregion
            };

            //验证每行资料的事件
            wizard.ValidateRow += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
            {
                if (domainAccErr != "")
                {
                    e.ErrorMessage += domainAccErr;
                }
                if (e.SelectFields.Contains("密码") && !e.SelectFields.Contains("登入账号"))
                    e.ErrorMessage += "汇入密码需同时汇入帐号";
                #region 验證各筆資料

                foreach (string field in e.SelectFields)
                {
                    string value = "" + e.Data[field];
                    switch (field)
                    {
                        default:
                            break;
                        case "登入账号":
                            value = value.ToLower().Trim();
                            if (value != "")
                            {
                                if (accountKeysCheck.Contains(value))
                                    e.ErrorFields.Add(field, "登入账号重复，无法汇入。");
                                else
                                    accountKeysCheck.Add(value);

                                if (value.Contains("@") && !value.EndsWith("@" + udtAdmin.Domain.ToLower().Trim()))
                                    e.ErrorFields.Add(field, "帳號必須是 @" + udtAdmin.Domain + "。");
                            }
                            break;
                        case "密码":
                            if (value.Replace(" ", "") != value)
                            {
                                e.ErrorFields.Add(field, "密码不得包含空白字元。");
                            }
                            break;
                    }
                }
                #endregion
            };

        }

        void setupImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            Dictionary<StudentRecord, string> updateStudentAcc = new Dictionary<StudentRecord, string>();
            wizard.ImportStart += delegate
            {
                updateStudentAcc = new Dictionary<StudentRecord, string>();
            };
            //实际汇入资料的事件
            wizard.ImportPackage += delegate(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
            {
                if (e.ImportFields.Contains("登入账号"))
                {
                    #region 取得學生資料
                    Dictionary<string, StudentRecord> StudentRecordDict = new Dictionary<string, StudentRecord>();
                    List<string> StudentIDList = new List<string>();
                    //比对
                    foreach (RowData Row in e.Items)
                    {
                        StudentIDList.Add(Row.ID);
                    }
                    List<StudentRecord> StudentRecordList = K12.Data.Student.SelectByIDs(StudentIDList);
                    foreach (StudentRecord rec in StudentRecordList)
                        if (!StudentRecordDict.ContainsKey(rec.ID))
                            StudentRecordDict.Add(rec.ID, rec);
                    #endregion

                    UDT_AdminData udtAdmin = Utility.GetAdminData("student");
                    List<Service.UserAccount> UserAccountList = new List<Service.UserAccount>();
                    foreach (RowData Row in e.Items)
                    {
                        Service.UserAccount uAcc = new Service.UserAccount();
                        if (Row.ContainsKey("登入账号"))
                        {
                            uAcc.Account = "" + Row["登入账号"];

                            // 检查Account 是否有带@，没有自动加入。
                            if (!uAcc.Account.Contains("@") && udtAdmin.Domain != "")
                                uAcc.Account = uAcc.Account + "@" + udtAdmin.Domain;

                        }

                        if (e.ImportFields.Contains("密码"))
                            uAcc.Password = "" + Row["密码"];
                        else
                            uAcc.Password = null;

                        if (StudentRecordDict.ContainsKey(Row.ID))
                            uAcc.LastName = StudentRecordDict[Row.ID].Name;

                        if (StudentRecordDict[Row.ID].SALoginName.ToLower().Trim() != uAcc.Account.ToLower())
                        {
                            updateStudentAcc.Add(StudentRecordDict[Row.ID], uAcc.Account);
                        }

                        UserAccountList.Add(uAcc);
                    }
                    udtAdmin.uploadAcc(UserAccountList);
                }
            };
            wizard.ImportComplete += delegate
            {
                if (updateStudentAcc.Count > 0)
                {
                    // 记log
                    StringBuilder sbLog = new StringBuilder();
                    foreach (var stuRec in updateStudentAcc.Keys)
                    {
                        sbLog.AppendLine(string.Format("登入账号由「{0}」改为「{1}」", stuRec.SALoginName, updateStudentAcc[stuRec]));
                        stuRec.SALoginName = "";
                    }
                    //清空相關學生帳號
                    K12.Data.Student.Update(updateStudentAcc.Keys);
                    foreach (var stuRec in updateStudentAcc.Keys)
                    {
                        stuRec.SALoginName = updateStudentAcc[stuRec];
                    }
                    //寫入相關學生帳號
                    K12.Data.Student.Update(updateStudentAcc.Keys);
                    FISCA.LogAgent.ApplicationLog.Log("汇入学生账号", "汇入", sbLog.ToString());
                }
                FISCA.Presentation.MotherForm.SetStatusBarMessage("汇入完成!");
            };
        }
    }
}

