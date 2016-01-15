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
            this.Text = "汇入教师账号";
            _FieldNameList.Add("登入账号");
            _FieldNameList.Add("密码");
            _FieldNameList.Add("姓");
            _FieldNameList.Add("名");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            VirtualCheckBox setAccount = new VirtualCheckBox("设定网域管理者账号", false);
            setAccount.CheckedChanged += delegate
            {
                if (setAccount.Checked)
                {
                    Admin.AdminForm ad = new Admin.AdminForm("teacher");
                    ad.Show();
                    setAccount.Checked = false;
                }
            };

            // 检查是否已设定网络管理者
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
                string msg = "网络管理者账号未设定或设定不完整，无法上传教师网域账号，请至进阶 设定网域管理者账号";

                FISCA.Presentation.Controls.MsgBox.Show(msg);
            }

            // 检查账号是否可以登入
            if (chkAdDataErr == false)
            {
                Utility.CheckAdminPWD("teacher");
            }

            wizard.Options.Add(setAccount);
            wizard.PackageLimit = 3000;
            //必需要有的字段
            wizard.RequiredFields.AddRange("教师姓名", "昵称");
            //可汇入的字段
            wizard.ImportableFields.AddRange(_FieldNameList);

            wizard.ValidateStart += wizard_ValidateStart;
            //验证每行资料的事件
            wizard.ValidateRow += wizard_ValidateRow;

            //实际汇入资料的事件
            wizard.ImportPackage += wizard_ImportPackage;

            //汇入完成
            wizard.ImportComplete += (sender, e) => MessageBox.Show("汇入完成!");
        }

        void wizard_ValidateStart(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
        {
            _Keys.Clear();
            TeacherRecAllList = K12.Data.Teacher.SelectAll();
            StudSANameDict.Clear();
            StudSANameSnumDict.Clear();
            List<string> idList = new List<string>();
            foreach (string id in e.List)
            {
                idList.Add(id);
            }

            // 系统内教师账号
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

            // 寻找主要Key来判断，如果有教师系统编号先用系统编号，没有使用学号，
            Dictionary<string, RowData> RowDataDict = new Dictionary<string, RowData>();
            Dictionary<string, int> chkSidDict = new Dictionary<string, int>();
            Dictionary<string, string> chkSnumDict = new Dictionary<string, string>();
            List<TeacherRecord> InsertStudentRecList = new List<TeacherRecord>();
            List<TeacherRecord> TeacherRecAllList = K12.Data.Teacher.SelectAll();
            // 系统内教师账号
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
                string key = rec.Name + rec.Nickname + rec.StatusStr;
                if (!chkSnumDict.ContainsKey(key))
                    chkSnumDict.Add(key, rec.ID);

                TeacherRecAllDict.Add(rec.ID, rec);
            }

            List<string> TeacherIDList = new List<string>();
            //比对
            foreach (RowData Row in e.Items)
            {
                string TeacherID = "";

                if (Row.ContainsKey("教师系统编号"))
                {
                    string id = Row["教师系统编号"].ToString();
                    if (chkSidDict.ContainsKey(id))
                        TeacherID = id;
                }

                if (TeacherID == "")
                {
                    if (Row.ContainsKey("教师姓名") && Row.ContainsKey("昵称"))
                    {
                        string key = Row["教师姓名"].ToString() + Row["昵称"].ToString() + "一般";
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
            // 取得教师基本
            List<TeacherRecord> TeacherRecordList = K12.Data.Teacher.SelectByIDs(TeacherIDList);
            Dictionary<string, TeacherRecord> TeacherRecordDict = new Dictionary<string, TeacherRecord>();
            foreach (TeacherRecord rec in TeacherRecordList)
                if (!TeacherRecordDict.ContainsKey(rec.ID))
                    TeacherRecordDict.Add(rec.ID, rec);


            List<Service.UserAccount> UserAccountList = new List<Service.UserAccount>();

            // 开始处理
            List<TeacherRecord> updateTeacherRecList = new List<TeacherRecord>();

            #region 上传到Domain
            string dName = "", dAccount = "", dPwd = "";

            bool chkSend = false;
            // 取得账号UDT
            UDT_AdminData udtAdmin = Utility.GetAdminData("teacher");
            if (udtAdmin != null)
            {
                dName = udtAdmin.Domain;
                dAccount = udtAdmin.Account;
                dPwd = Utility.ConvertBase64StringToString(udtAdmin.Password);
                chkSend = true;
            }

            // 记log
            StringBuilder sbLog = new StringBuilder();
            foreach (string TeacherID in RowDataDict.Keys)
            {
                if (TeacherRecordDict.ContainsKey(TeacherID))
                {
                    string tt = "教师系统编号：" + TeacherID + ",教师姓名：" + TeacherRecordDict[TeacherID].Name + ", 昵称：" + TeacherRecordDict[TeacherID].Nickname;
                    RowData rd = RowDataDict[TeacherID];
                    if (rd.ContainsKey("登入账号"))
                    {
                        string value = rd["登入账号"].ToString();
                        if (TeacherRecordDict[TeacherID].TALoginName != value)
                            sbLog.AppendLine(string.Format("登入账号由「{0}」改为「{1}」", TeacherRecordDict[TeacherID].TALoginName, value));
                    }
                }
            }

            // 清除所选账号
            foreach (string TeacherID in RowDataDict.Keys)
            {
                if (TeacherRecordDict.ContainsKey(TeacherID))
                {
                    RowData rd = RowDataDict[TeacherID];
                    if (rd.ContainsKey("登入账号"))
                    {
                        string value = rd["登入账号"].ToString();
                        if (value != "")
                            TeacherRecordDict[TeacherID].TALoginName = "";
                        updateTeacherRecList.Add(TeacherRecordDict[TeacherID]);
                    }
                }
            }
            K12.Data.Teacher.Update(updateTeacherRecList);
            updateTeacherRecList.Clear();

            // 写入新账号
            foreach (string TeacherID in RowDataDict.Keys)
            {
                if (TeacherRecordDict.ContainsKey(TeacherID))
                {
                    RowData rd = RowDataDict[TeacherID];
                    if (rd.ContainsKey("登入账号"))
                    {
                        string value = rd["登入账号"].ToString();
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

            FISCA.LogAgent.ApplicationLog.Log("汇入教师账号", "汇入", sbLog.ToString());


            if (chkSend)
            {
                StringBuilder sendSB = new StringBuilder();

                foreach (RowData Row in e.Items)
                {
                    Service.UserAccount uAcc = new Service.UserAccount();
                    if (Row.ContainsKey("登入账号"))
                    {
                        uAcc.Account = Row["登入账号"].ToString();

                        // 检查Account 是否有带@，没有自动加入。
                        if (!uAcc.Account.Contains("@") && dName != "")
                            uAcc.Account = uAcc.Account + "@" + dName;

                    }

                    if (Row.ContainsKey("密码"))
                        uAcc.Password = Row["密码"].ToString();

                    if (Row.ContainsKey("姓"))
                        uAcc.LastName = Row["姓"].ToString();

                    if (Row.ContainsKey("名"))
                        uAcc.FirstName = Row["名"].ToString();

                    UserAccountList.Add(uAcc);
                }
                string dsns = FISCA.Authentication.DSAServices.AccessPoint;
                string url = Config.ChinaUrl;

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
                    FISCA.Presentation.Controls.MsgBox.Show("上传网域账号失败," + responseFromServer);
            }
            #endregion
        }

        void wizard_ValidateRow(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
        {
            #region 验各字段填写格式


            foreach (string field in e.SelectFields)
            {
                string value = e.Data[field];
                switch (field)
                {
                    default:
                        break;

                    case "教师姓名":
                        if (value.Replace(" ", "") == "")
                            e.ErrorFields.Add(field, "此栏为必填字段。");
                        break;

                    case "登入账号":
                        if (value != "")
                        {
                            value = value.ToLower().Replace(" ", "");

                            if (StudSANameSnumDict.ContainsKey(value))
                            {
                                if (e.Data.ContainsKey("教师姓名") && e.Data.ContainsKey("昵称"))
                                {
                                    string SysNum = e.Data["教师姓名"].ToString() + e.Data["昵称"].ToString();
                                    if (SysNum != StudSANameSnumDict[value])
                                        e.ErrorFields.Add(field, "教师登入账号已被" + e.Data["教师姓名"].ToString() + "使用，请修正");
                                }
                            }

                            if (StudSANameDict.ContainsKey(value))
                            {
                                if (e.Data.ContainsKey("教师系统编号"))
                                {
                                    string SysID = e.Data["教师系统编号"].ToString();
                                    if (SysID != StudSANameDict[value])
                                        e.ErrorFields.Add(field, "教师登入账号已被使用，请修正");
                                }
                            }

                        }
                        break;
                }
            }
            #endregion
            #region 验证主键


            string Key = "";
            if (e.Data.ContainsKey("登入账号"))
            {
                Key = e.Data["登入账号"].ToLower().Replace(" ", "");
            }
            string errorMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(Key))
                if (_Keys.Contains(Key))
                    errorMessage = "登入账号重复，无法汇入。";
                else
                    _Keys.Add(Key);

            e.ErrorMessage = errorMessage;

            #endregion
        }
    }
}

