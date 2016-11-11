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
using static ischoolAccountManagement.Service;

namespace ischoolAccountManagement
{
    class ImportStudentData : SmartSchool.API.PlugIn.Import.Importer
    {
        // 系統內學生帳號
        Dictionary<string, string> _ExistingAccount = new Dictionary<string, string>();
        Dictionary<string, StudentRecord> _DicStudentRecord = new Dictionary<string, StudentRecord>();
        Dictionary<StudentRecord, string> _DicOldAccount = new Dictionary<StudentRecord, string>();
        FISCA.LogAgent.LogSaver _LogSaver = FISCA.LogAgent.ApplicationLog.CreateLogSaverInstance();
        List<string> _ImportList;
        UDT_AdminData _AdminData;

        public ImportStudentData()
        {
            this.Image = null;
            this.Text = "匯入學生帳號";

            _AdminData = Utility.GetAdminData("student");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            wizard.PackageLimit = 3000;
            //可匯入的欄位
            wizard.ImportableFields.AddRange("登入帳號", "密碼");

            wizard.ValidateStart += wizard_ValidateStart;
            //驗證每行資料的事件
            wizard.ValidateRow += wizard_ValidateRow;

            wizard.ImportStart += Wizard_ImportStart;

            //實際匯入資料的事件
            wizard.ImportPackage += wizard_ImportPackage;

            //匯入完成
            wizard.ImportComplete += Wizard_ImportComplete;
        }


        void wizard_ValidateStart(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
        {
            // 系統內學生帳號
            _ExistingAccount.Clear();
            List<string> idList = new List<string>();
            foreach (string id in e.List)
                idList.Add(id);

            foreach (StudentRecord rec in K12.Data.Student.SelectAll())
            {
                if (idList.Contains(rec.ID))
                    rec.SALoginName = "";

                if (rec.SALoginName != "")
                {
                    string sKey = rec.SALoginName.ToLower().Replace(" ", "");
                    if (!_ExistingAccount.ContainsKey(sKey))
                        _ExistingAccount.Add(sKey, rec.ID);
                }
            }
            _ImportList = new List<string>(e.List);
        }

        void wizard_ValidateRow(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
        {
            #region 驗各欄位填寫格式

            foreach (string field in e.SelectFields)
            {
                string value = e.Data[field];
                switch (field)
                {
                    default:
                        break;

                    case "登入帳號":
                        if (value != "")
                        {
                            if (!value.Contains("@"))
                                value = value + "@" + _AdminData.Domain;
                            value = value.ToLower().Replace(" ", "");
                            lock (_ExistingAccount)
                            {
                                if (_ExistingAccount.ContainsKey(value))
                                {
                                    e.ErrorFields.Add(field, "學生登入帳號重複，請修正");
                                }
                                else
                                {
                                    _ExistingAccount.Add(value, e.Data.ID);
                                }
                            }
                            if (value.Contains("@") && !value.EndsWith("@" + _AdminData.Domain.ToLower()))
                            {
                                e.WarningFields.Add(field, "輸入帳號不屬於設定的網域，將不會進行帳號密碼的開設");
                            }
                        }
                        break;
                }
            }
            #endregion
        }

        private void Wizard_ImportStart(object sender, EventArgs e)
        {
            _LogSaver.ClearBatch();
            _DicOldAccount.Clear();
            _DicStudentRecord.Clear();
            List<StudentRecord> list = K12.Data.Student.SelectByIDs(_ImportList);
            foreach (var item in list)
            {
                _DicOldAccount.Add(item, "" + item.SALoginName);
                item.SALoginName = "";
                _DicStudentRecord.Add(item.ID, item);
            }
            K12.Data.Student.Update(list);
        }

        void wizard_ImportPackage(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
        {
            List<Service.UserAccount> uploadList = new List<Service.UserAccount>();
            var list = new List<StudentRecord>();
            foreach (var row in e.Items)
            {
                var stuRec = _DicStudentRecord[row.ID];
                list.Add(stuRec);
                string value = "" + row["登入帳號"];
                if (value != "")
                {
                    if (!value.Contains("@"))
                        value = value + "@" + _AdminData.Domain;
                    value = value.Replace(" ", "");
                    stuRec.SALoginName = value;
                    list.Add(stuRec);
                    if (value.ToLower().EndsWith("@" + _AdminData.Domain.ToLower()))
                    {
                        string pwd = "" + row["密碼"];
                        if (pwd != "")
                            uploadList.Add(new Service.UserAccount() { Account = value, FirstName = stuRec.Name, LastName = "", Password = pwd });
                    }
                }
                if (_DicOldAccount[stuRec].ToLower() != stuRec.SALoginName.ToLower())
                {
                    if (_DicOldAccount[stuRec] == "")
                        _LogSaver.AddBatch("匯入學生帳號", "匯入", "student", row.ID, string.Format("登入帳號設定為「{1}」", _DicOldAccount[stuRec], stuRec.SALoginName));
                    else
                        _LogSaver.AddBatch("匯入學生帳號", "匯入", "student", row.ID, string.Format("登入帳號由「{0}」改為「{1}」", _DicOldAccount[stuRec], stuRec.SALoginName));
                }
            }
            if (list.Count > 0)
            {
                K12.Data.Student.Update(list);
            }
            if (uploadList.Count > 0)
            {
                new RequestPackage()
                {
                    Application = FISCA.Authentication.DSAServices.AccessPoint,
                    Domain = new DomainAdmin() { Name = _AdminData.Domain, Acc = _AdminData.Account, Pwd = Utility.ConvertBase64StringToString(_AdminData.Password) },
                    List = uploadList.ToArray()
                }.Send();
            }
        }
        private void Wizard_ImportComplete(object sender, EventArgs e)
        {
            _LogSaver.LogBatch();
            MessageBox.Show("匯入完成!");
        }
    }
}
