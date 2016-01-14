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
        List<string> _Keys = new List<string>();
        List<StudentRecord> StudentRecAllList = new List<StudentRecord>();
        // 系统内学生账号
        Dictionary<string, string> StudSANameDict = new Dictionary<string, string>();
        Dictionary<string, string> StudSANameSnumDict = new Dictionary<string, string>();

        public ImportStudentData()
        {
            this.Image = null;
            this.Text = "汇入学生账号";
            _FieldNameList.Add("登入账号");
            _FieldNameList.Add("密码");
            _FieldNameList.Add("姓");
            _FieldNameList.Add("名");
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


            VirtualCheckBox setAccount = new VirtualCheckBox("设定网域管理者账号", false);
            setAccount.CheckedChanged += delegate
            {
                if (setAccount.Checked)
                {

                    Admin.AdminForm ad = new Admin.AdminForm("student");
                    ad.Show();
                    setAccount.Checked = false;
                }
            };

            wizard.Options.Add(setAccount);
            wizard.PackageLimit = 3000;
            //必需要有的字段
            wizard.RequiredFields.AddRange("学号");
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
            StudentRecAllList = K12.Data.Student.SelectAll();
            // 系统内学生账号
            StudSANameDict.Clear();
            StudSANameSnumDict.Clear();
            List<string> idList = new List<string>();
            foreach (string id in e.List)
                idList.Add(id);

            foreach (StudentRecord rec in StudentRecAllList)
            {
                if (idList.Contains(rec.ID))
                    rec.SALoginName = "";

                if (rec.SALoginName != "")
                {
                    string sKey = rec.SALoginName.ToLower().Replace(" ", "");
                    if (!StudSANameDict.ContainsKey(sKey))
                        StudSANameDict.Add(sKey, rec.ID);

                    if (rec.Status == StudentRecord.StudentStatus.一般)
                    {
                        if (!StudSANameSnumDict.ContainsKey(sKey))
                            StudSANameSnumDict.Add(sKey, rec.StudentNumber);
                    }
                }
            }

        }

        void wizard_ImportPackage(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
        {
            // 寻找主要Key来判断，如果有学生系统编号先用系统编号，没有使用学号，
            Dictionary<string, RowData> RowDataDict = new Dictionary<string, RowData>();
            Dictionary<string, int> chkSidDict = new Dictionary<string, int>();
            Dictionary<string, string> chkSnumDict = new Dictionary<string, string>();
            List<StudentRecord> InsertStudentRecList = new List<StudentRecord>();
            List<StudentRecord> StudentRecAllList = K12.Data.Student.SelectAll();
            // 系统内学生账号
            Dictionary<string, string> StudSANameDict = new Dictionary<string, string>();

            foreach (StudentRecord rec in StudentRecAllList)
            {
                chkSidDict.Add(rec.ID, 0);
                string key = rec.StudentNumber + rec.StatusStr;
                if (!chkSnumDict.ContainsKey(key))
                    chkSnumDict.Add(key, rec.ID);

                if (!StudSANameDict.ContainsKey(rec.SALoginName))
                    StudSANameDict.Add(rec.SALoginName, rec.ID);
            }

            // 再次建立索引
            Dictionary<string, StudentRecord> StudRecAllDict = new Dictionary<string, StudentRecord>();
            StudentRecAllList = K12.Data.Student.SelectAll();
            chkSidDict.Clear();
            chkSnumDict.Clear();
            foreach (StudentRecord rec in StudentRecAllList)
            {
                chkSidDict.Add(rec.ID, 0);
                string key = rec.StudentNumber + rec.StatusStr;
                if (!chkSnumDict.ContainsKey(key))
                    chkSnumDict.Add(key, rec.ID);

                StudRecAllDict.Add(rec.ID, rec);
            }

            List<string> StudentIDList = new List<string>();
            //比对
            foreach (RowData Row in e.Items)
            {
                string StudentID = "";

                if (Row.ContainsKey("学生系统编号"))
                {
                    string id = Row["学生系统编号"].ToString();
                    if (chkSidDict.ContainsKey(id))
                        StudentID = id;
                }

                if (StudentID == "")
                {
                    string ssNum = "", snum = "";
                    if (Row.ContainsKey("学号"))
                    {
                        snum = Row["学号"].ToString();
                        string status = "一般";
                        if (Row.ContainsKey("状态"))
                        {
                            if (Row["状态"].ToString() != "")
                                status = Row["状态"].ToString();
                        }
                        ssNum = snum + status;
                    }

                    if (chkSnumDict.ContainsKey(ssNum))
                        StudentID = chkSnumDict[ssNum];
                }

                if (!string.IsNullOrEmpty(StudentID))
                {
                    if (!RowDataDict.ContainsKey(StudentID))
                        RowDataDict.Add(StudentID, Row);

                    StudentIDList.Add(StudentID);
                }
            }
            // 取得学生基本
            List<StudentRecord> StudentRecordList = K12.Data.Student.SelectByIDs(StudentIDList);
            Dictionary<string, StudentRecord> StudentRecordDict = new Dictionary<string, StudentRecord>();
            foreach (StudentRecord rec in StudentRecordList)
                if (!StudentRecordDict.ContainsKey(rec.ID))
                    StudentRecordDict.Add(rec.ID, rec);


            List<Service.UserAccount> UserAccountList = new List<Service.UserAccount>();

            // 开始处理
            List<StudentRecord> updateStudentRecList = new List<StudentRecord>();

            #region 上传到Domain
            string dName = "", dAccount = "", dPwd = "";

            bool chkSend = false;
            // 取得账号UDT
            UDT_AdminData udtAdmin = Utility.GetAdminData("student");
            if (udtAdmin != null)
            {
                dName = udtAdmin.Domain;
                dAccount = udtAdmin.Account;
                dPwd = Utility.ConvertBase64StringToString(udtAdmin.Password);
                chkSend = true;
            }

            // 记log
            StringBuilder sbLog = new StringBuilder();
            foreach (string StudentID in RowDataDict.Keys)
            {
                if (StudentRecordDict.ContainsKey(StudentID))
                {
                    string tt = "学生系统编号：" + StudentID + ",学号：" + StudentRecordDict[StudentID].StudentNumber;
                    if (StudentRecordDict[StudentID].Class != null)
                        tt += "班级：" + StudentRecordDict[StudentID].Class.Name;

                    if (StudentRecordDict[StudentID].SeatNo.HasValue)
                        tt += "座号：" + StudentRecordDict[StudentID].SeatNo.Value;
                    RowData rd = RowDataDict[StudentID];
                    if (rd.ContainsKey("登入账号"))
                    {
                        string value = rd["登入账号"].ToString();
                        string oldValue = StudentRecordDict[StudentID].SALoginName;
                        if (oldValue != value)
                            sbLog.AppendLine(string.Format("登入账号由「{0}」改为「{1}」", oldValue, value));
                    }
                }
            }

            // 清空这学生原本账号
            foreach (string StudentID in RowDataDict.Keys)
            {
                if (StudentRecordDict.ContainsKey(StudentID))
                {
                    RowData rd = RowDataDict[StudentID];
                    if (rd.ContainsKey("登入账号"))
                    {
                        string value = rd["登入账号"].ToString();
                        if (value != "")
                        {
                            StudentRecordDict[StudentID].SALoginName = "";
                            updateStudentRecList.Add(StudentRecordDict[StudentID]);
                        }
                    }
                }
            }
            if (updateStudentRecList.Count > 0)
                K12.Data.Student.Update(updateStudentRecList);


            updateStudentRecList.Clear();
            // 放入新账号
            foreach (string StudentID in RowDataDict.Keys)
            {
                if (StudentRecordDict.ContainsKey(StudentID))
                {
                    RowData rd = RowDataDict[StudentID];
                    if (rd.ContainsKey("登入账号"))
                    {
                        string value = rd["登入账号"].ToString();
                        if (!value.Contains("@") && dName != "")
                            value = value + "@" + dName;
                        StudentRecordDict[StudentID].SALoginName = value;
                        updateStudentRecList.Add(StudentRecordDict[StudentID]);
                    }
                }
            }

            if (updateStudentRecList.Count > 0)
                K12.Data.Student.Update(updateStudentRecList);

            FISCA.LogAgent.ApplicationLog.Log("汇入学生账号", "汇入", sbLog.ToString());


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
                            uAcc.Account += uAcc.Account + "@" + dName;

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
                //req.Referer = url;
                //req.Host = "auth.ischoolcenter.com";              
                //req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";                
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
                try
                {
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
                catch (Exception ex)
                {

                }

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

                    case "学号":
                        if (value.Replace(" ", "") == "")
                            e.ErrorFields.Add(field, "此栏为必填字段。");
                        break;

                    case "登入账号":
                        if (value != "")
                        {
                            value = value.ToLower().Replace(" ", "");
                            if (StudSANameSnumDict.ContainsKey(value))
                            {
                                if (e.Data.ContainsKey("学号"))
                                {
                                    string SysNum = e.Data["学号"].ToString();
                                    if (SysNum != StudSANameSnumDict[value])
                                        e.ErrorFields.Add(field, "学生登入账号已被使用，请修正。");
                                }
                            }

                            if (StudSANameDict.ContainsKey(value))
                            {
                                if (e.Data.ContainsKey("学生系统编号"))
                                {
                                    string SysID = e.Data["学生系统编号"].ToString();
                                    if (SysID != StudSANameDict[value])
                                        e.ErrorFields.Add(field, "学生登入账号已被使用，请修正");
                                }
                            }

                        }
                        break;
                }
            }
            #endregion
            #region 验证主键


            //string Key = e.Data.ID;

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

