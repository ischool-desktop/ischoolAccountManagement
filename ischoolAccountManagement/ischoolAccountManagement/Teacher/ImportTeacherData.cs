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
    class ImportTeacherData : SmartSchool.API.PlugIn.Import.Importer
    {
        // 系統內學生帳號
        Dictionary<string, string> _ExistingAccount = new Dictionary<string, string>();
        Dictionary<string, TeacherRecord> _DicTeacherRecord = new Dictionary<string, TeacherRecord>();
        Dictionary<TeacherRecord, string> _DicOldAccount = new Dictionary<TeacherRecord, string>();
        FISCA.LogAgent.LogSaver _LogSaver = FISCA.LogAgent.ApplicationLog.CreateLogSaverInstance();
        List<string> _ImportList;
        UDT_AdminData _AdminData;

        public ImportTeacherData()
        {
            this.Image = null;
            this.Text = "匯入教師帳號";

            _AdminData = Utility.GetAdminData("teacher");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            wizard.PackageLimit = 3000;
            //可匯入的欄位
            wizard.ImportableFields.AddRange("登入帳號", "密碼");

            wizard.ValidateStart += Wizard_ValidateStart;
            wizard.ValidateRow += Wizard_ValidateRow;

            wizard.ImportStart += Wizard_ImportStart;
            wizard.ImportPackage += Wizard_ImportPackage;
            wizard.ImportComplete += Wizard_ImportComplete;
        }

        private void Wizard_ValidateStart(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
        {
            // 系統內學生帳號
            _ExistingAccount.Clear();
            List<string> idList = new List<string>();
            foreach (string id in e.List)
                idList.Add(id);

            foreach (TeacherRecord rec in K12.Data.Teacher.SelectAll())
            {
                if (idList.Contains(rec.ID))
                    rec.TALoginName = "";

                if (rec.TALoginName != "")
                {
                    string sKey = rec.TALoginName.ToLower().Replace(" ", "");
                    if (!_ExistingAccount.ContainsKey(sKey))
                        _ExistingAccount.Add(sKey, rec.ID);
                }
            }
            _ImportList = new List<string>(e.List);
        }


        private void Wizard_ValidateRow(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
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
                                    e.ErrorFields.Add(field, "教師登入帳號重複，請修正");
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
            _DicTeacherRecord.Clear();
            List<TeacherRecord> list = K12.Data.Teacher.SelectByIDs(_ImportList);
            foreach (var item in list)
            {
                _DicOldAccount.Add(item, "" + item.TALoginName);
                item.TALoginName = "";
                _DicTeacherRecord.Add(item.ID, item);
            }
            K12.Data.Teacher.Update(list);
        }

        private void Wizard_ImportPackage(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
        {
            List<Service.UserAccount> uploadList = new List<Service.UserAccount>();
            var list = new List<TeacherRecord>();
            foreach (var row in e.Items)
            {
                var teacherRec = _DicTeacherRecord[row.ID];
                list.Add(teacherRec);
                string value = "" + row["登入帳號"];
                if (value != "")
                {
                    if (!value.Contains("@"))
                        value = value + "@" + _AdminData.Domain;
                    value = value.Replace(" ", "");
                    teacherRec.TALoginName = value;
                    list.Add(teacherRec);
                    if (value.ToLower().EndsWith("@" + _AdminData.Domain.ToLower()))
                    {
                        string pwd = "" + row["密碼"];
                        if (pwd != "")
                            uploadList.Add(new Service.UserAccount() { Account = value, FirstName = teacherRec.Name, LastName = "", Password = pwd });
                    }
                }
                if (_DicOldAccount[teacherRec].ToLower() != teacherRec.TALoginName.ToLower())
                {
                    if (_DicOldAccount[teacherRec] == "")
                        _LogSaver.AddBatch("匯入教師帳號", "匯入", "teacher", row.ID, string.Format("登入帳號設定為「{1}」", _DicOldAccount[teacherRec], teacherRec.TALoginName));
                    else
                        _LogSaver.AddBatch("匯入教師帳號", "匯入", "teacher", row.ID, string.Format("登入帳號由「{0}」改為「{1}」", _DicOldAccount[teacherRec], teacherRec.TALoginName));
                }
            }
            if (list.Count > 0)
            {
                K12.Data.Teacher.Update(list);
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
