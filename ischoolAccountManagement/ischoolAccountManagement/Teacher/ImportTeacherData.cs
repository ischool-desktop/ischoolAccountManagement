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
    class ImportTeacherData : SmartSchool.API.PlugIn.Import.Importer
    {
        List<string> _FieldNameList = new List<string>();
        List<string> _Keys = new List<string>();
        Dictionary<string, string> StudSANameDict = new Dictionary<string, string>();
        Dictionary<string, string> StudSANameSnumDict = new Dictionary<string, string>();
        List<TeacherRecord> TeacherRecAllList = new List<TeacherRecord>();

        public ImportTeacherData()
        {
            this.Image = null;
            this.Text = "匯入教師帳號";
            _FieldNameList.Add("登入帳號");
            _FieldNameList.Add("密碼");
            _FieldNameList.Add("姓");
            _FieldNameList.Add("名");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            VirtualCheckBox setAccount = new VirtualCheckBox("設定網域管理者帳號", false);
            setAccount.CheckedChanged += delegate
            {
                if (setAccount.Checked)
                {
                    Admin.AdminForm ad = new Admin.AdminForm("teacher");
                    ad.Show();
                    setAccount.Checked = false;
                }
            };

            // 檢查是否已設定網路管理者
            UDT_AdminData chkAdData = Utility.GetAdminData("teacher");
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
                string msg = "網路管理者帳號未設定或設定不完整，無法上傳教師網域帳號，請至進階 設定網域管理者帳號";
                FISCA.Presentation.Controls.MsgBox.Show(msg);
            }

            // 檢查帳號是否可以登入
            if (chkAdDataErr == false)
            {
                Utility.CheckAdminPWD("teacher");
            }     

            wizard.Options.Add(setAccount);
            wizard.PackageLimit = 3000;
            //必需要有的欄位
            wizard.RequiredFields.AddRange("教師姓名","暱稱");
            //可匯入的欄位
            wizard.ImportableFields.AddRange(_FieldNameList);

            wizard.ValidateStart += wizard_ValidateStart;
            //驗證每行資料的事件
            wizard.ValidateRow += wizard_ValidateRow;
            
            //實際匯入資料的事件
            wizard.ImportPackage += wizard_ImportPackage;

            //匯入完成
            wizard.ImportComplete += (sender, e) => MessageBox.Show("匯入完成!");
        }

        void wizard_ValidateStart(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
        {
            _Keys.Clear();
            TeacherRecAllList = K12.Data.Teacher.SelectAll();
            StudSANameDict.Clear();
            StudSANameSnumDict.Clear();
            List<string> idList = new List<string>();
            foreach(string id in e.List)
            {
                idList.Add(id);
            }

            // 系統內教師帳號
            foreach (TeacherRecord rec in TeacherRecAllList)
            {
                if (idList.Contains(rec.ID))
                    rec.TALoginName = "";

                string sKey = rec.TALoginName.ToLower().Replace(" ", "");

                if (sKey != "")
                {
                    if (!StudSANameDict.ContainsKey(sKey))
                        StudSANameDict.Add(sKey, rec.ID);

                    if (rec.Status == TeacherRecord.TeacherStatus.一般)
                    {
                        if (!StudSANameSnumDict.ContainsKey(sKey))
                            StudSANameSnumDict.Add(sKey, rec.Name + rec.Nickname);
                    }
                }
            }
        }

        void wizard_ImportPackage(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
        {

            // 尋找主要Key來判斷，如果有教師系統編號先用系統編號，沒有使用學號，
            Dictionary<string, RowData> RowDataDict = new Dictionary<string, RowData>();
            Dictionary<string, int> chkSidDict = new Dictionary<string, int>();
            Dictionary<string, string> chkSnumDict = new Dictionary<string, string>();
            List<TeacherRecord> InsertStudentRecList = new List<TeacherRecord>();
            List<TeacherRecord> TeacherRecAllList = K12.Data.Teacher.SelectAll();
            // 系統內教師帳號
            Dictionary<string, string> StudSANameDict = new Dictionary<string, string>();

            foreach (TeacherRecord rec in TeacherRecAllList)
            {
                chkSidDict.Add(rec.ID, 0);
                if (!StudSANameDict.ContainsKey(rec.TALoginName))
                    StudSANameDict.Add(rec.TALoginName, rec.ID);
            }

            // 再次建立索引
            Dictionary<string, TeacherRecord> TeacherRecAllDict = new Dictionary<string, TeacherRecord>();
            TeacherRecAllList = K12.Data.Teacher.SelectAll();
            chkSidDict.Clear();
            chkSnumDict.Clear();
            foreach (TeacherRecord rec in TeacherRecAllList)
            {             
                string key = rec.Name+rec.Nickname + rec.StatusStr;
                if (!chkSnumDict.ContainsKey(key))
                    chkSnumDict.Add(key, rec.ID);

                TeacherRecAllDict.Add(rec.ID, rec);
            }

            List<string> TeacherIDList = new List<string>();
            //比對
            foreach (RowData Row in e.Items)
            {
                string TeacherID = "";

                if (Row.ContainsKey("教師系統編號"))
                {
                    string id = Row["教師系統編號"].ToString();
                    if (chkSidDict.ContainsKey(id))
                        TeacherID = id;
                }

                if (TeacherID == "")
                {                   
                    if (Row.ContainsKey("教師姓名") && Row.ContainsKey("暱稱"))
                    {
                        string key = Row["教師姓名"].ToString() + Row["暱稱"].ToString() + "一般";
                        if (chkSnumDict.ContainsKey(key))
                            TeacherID = chkSnumDict[key];
                    }
                }

                if (!string.IsNullOrEmpty(TeacherID))
                {
                    if (!RowDataDict.ContainsKey(TeacherID))
                        RowDataDict.Add(TeacherID, Row);

                    TeacherIDList.Add(TeacherID);
                }
            }
            // 取得教師基本
            List<TeacherRecord> TeacherRecordList = K12.Data.Teacher.SelectByIDs(TeacherIDList);
            Dictionary<string, TeacherRecord> TeacherRecordDict = new Dictionary<string, TeacherRecord>();
            foreach (TeacherRecord rec in TeacherRecordList)
                if (!TeacherRecordDict.ContainsKey(rec.ID))
                    TeacherRecordDict.Add(rec.ID, rec);


            List<Service.UserAccount> UserAccountList = new List<Service.UserAccount>();

            // 開始處理
            List<TeacherRecord> updateTeacherRecList = new List<TeacherRecord>();

            #region 上傳到Domain
            string dName = "", dAccount = "", dPwd = "";

            bool chkSend = false;
            // 取得帳號UDT
            UDT_AdminData udtAdmin = Utility.GetAdminData("teacher");
            if (udtAdmin != null)
            {
                dName = udtAdmin.Domain;
                dAccount = udtAdmin.Account;
                dPwd = Utility.ConvertBase64StringToString(udtAdmin.Password);
                chkSend = true;
            }

            // 記log
            StringBuilder sbLog = new StringBuilder();
            foreach (string TeacherID in RowDataDict.Keys)
            {
                if (TeacherRecordDict.ContainsKey(TeacherID))
                {
                    string tt = "教師系統編號：" + TeacherID + ",教師姓名：" + TeacherRecordDict[TeacherID].Name + ", 暱稱：" + TeacherRecordDict[TeacherID].Nickname;
                    RowData rd = RowDataDict[TeacherID];
                    if (rd.ContainsKey("登入帳號"))
                    {
                        string value = rd["登入帳號"].ToString();
                        if (TeacherRecordDict[TeacherID].TALoginName != value)
                            sbLog.AppendLine(string.Format("登入帳號由「{0}」改為「{1}」", TeacherRecordDict[TeacherID].TALoginName, value));                          
                    }
                }
            }

            // 清除所選帳號
            foreach (string TeacherID in RowDataDict.Keys)
            {
                if (TeacherRecordDict.ContainsKey(TeacherID))
                {
                    RowData rd = RowDataDict[TeacherID];
                    if (rd.ContainsKey("登入帳號"))
                    {
                        string value = rd["登入帳號"].ToString();
                        if(value!="")
                            TeacherRecordDict[TeacherID].TALoginName ="";
                        updateTeacherRecList.Add(TeacherRecordDict[TeacherID]);
                    }
                }
            }
            K12.Data.Teacher.Update(updateTeacherRecList);
            updateTeacherRecList.Clear();

            // 寫入新帳號
            foreach (string TeacherID in RowDataDict.Keys)
            {
                if (TeacherRecordDict.ContainsKey(TeacherID))
                {
                    RowData rd = RowDataDict[TeacherID];
                    if (rd.ContainsKey("登入帳號"))
                    {
                        string value = rd["登入帳號"].ToString();
                        if (value != "")
                        {
                            if (!value.Contains("@") && dName != "")
                                value = value + "@" + dName;

                            TeacherRecordDict[TeacherID].TALoginName = value;
                        }
                        updateTeacherRecList.Add(TeacherRecordDict[TeacherID]);
                    }
                }
            }

            if (updateTeacherRecList.Count > 0)
                K12.Data.Teacher.Update(updateTeacherRecList);

            FISCA.LogAgent.ApplicationLog.Log("匯入教師帳號", "匯入", sbLog.ToString());


            if (chkSend)
            {
                StringBuilder sendSB = new StringBuilder();

                foreach (RowData Row in e.Items)
                {
                    Service.UserAccount uAcc = new Service.UserAccount();
                    if (Row.ContainsKey("登入帳號"))
                    {
                        uAcc.Account = Row["登入帳號"].ToString();

                        // 檢查Account 是否有帶@，沒有自動加入。
                        if (!uAcc.Account.Contains("@") && dName !="")
                            uAcc.Account += uAcc.Account + "@" + dName;

                    }

                    if (Row.ContainsKey("密碼"))
                        uAcc.Password = Row["密碼"].ToString();

                    if (Row.ContainsKey("姓"))
                        uAcc.LastName = Row["姓"].ToString();

                    if (Row.ContainsKey("名"))
                        uAcc.FirstName = Row["名"].ToString();

                    UserAccountList.Add(uAcc);
                }
                string dsns = FISCA.Authentication.DSAServices.AccessPoint;
                                
                string url = Config.TaiwanUrl;
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "POST";
                req.Accept = "*/*";
                req.ContentType = "application/json";

                sendSB.Append("{");
                string titleStr = "'application':'" + dsns + "','domain':{'name':'" + dName + "','acc':'" + dAccount + "','pwd':'" + dPwd + "'},'list':";
                // 取代'""
                string cc = "\"";
                titleStr = titleStr.Replace("'", cc);
                sendSB.Append(titleStr);
                sendSB.Append(Service.GetUserAccountJSONString(UserAccountList));
                sendSB.Append("}");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                byte[] byteArray = Encoding.UTF8.GetBytes(sendSB.ToString());
                req.ContentLength = byteArray.Length;
                Stream dataStream = req.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                HttpWebResponse rsp;
                rsp = (HttpWebResponse)req.GetResponse();
                //= req.GetResponse();
                dataStream = rsp.GetResponseStream();

                // Console.WriteLine(((HttpWebResponse)rsp).StatusDescription);
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                rsp.Close();
                if (!responseFromServer.Contains("success"))
                    FISCA.Presentation.Controls.MsgBox.Show("上傳網域帳號失敗," + responseFromServer);
            }
            #endregion    
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

                    case "教師姓名":
                        if (value.Replace(" ", "") == "")
                            e.ErrorFields.Add(field, "此欄為必填欄位。");
                        break;

                    case "登入帳號":
                        if (value != "")
                        {
                            value = value.ToLower().Replace(" ", "");

                            if (StudSANameSnumDict.ContainsKey(value))
                            {
                                if (e.Data.ContainsKey("教師姓名") && e.Data.ContainsKey("暱稱"))
                                {
                                    string SysNum = e.Data["教師姓名"].ToString() + e.Data["暱稱"].ToString();
                                    if (SysNum != StudSANameSnumDict[value])
                                        e.ErrorFields.Add(field, "教師登入帳號已被" + e.Data["教師姓名"].ToString() + "使用，請修正");
                                }
                            }

                            if (StudSANameDict.ContainsKey(value))
                            {
                                if (e.Data.ContainsKey("教師系統編號"))
                                {
                                    string SysID = e.Data["教師系統編號"].ToString();
                                    if (SysID != StudSANameDict[value])
                                        e.ErrorFields.Add(field, "教師登入帳號已被使用，請修正");
                                }
                            }

                        }
                        break;
                }
            }
            #endregion
            #region 驗證主鍵


            string Key = "";
            if (e.Data.ContainsKey("登入帳號"))
            {
                Key = e.Data["登入帳號"].ToLower().Replace(" ", "");
            }
            string errorMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(Key))
                if (_Keys.Contains(Key))
                    errorMessage = "登入帳號重複，無法匯入。";
                else
                    _Keys.Add(Key);

            e.ErrorMessage = errorMessage;

            #endregion
        }
    }
}
