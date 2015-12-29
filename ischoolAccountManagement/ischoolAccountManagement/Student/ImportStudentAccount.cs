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
        public ImportStudentData()
        {
            this.Image = null;
            this.Text = "匯入學生帳號";
            _FieldNameList.Add("登入帳號");
            _FieldNameList.Add("密碼");
            _FieldNameList.Add("姓");
            _FieldNameList.Add("名");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {    
            VirtualCheckBox setAccount = new VirtualCheckBox("設定網域管理者帳號", false);
            setAccount.CheckedChanged += delegate { 
            if(setAccount.Checked)
            {

                Admin.AdminForm ad = new Admin.AdminForm("student");          
                ad.Show();
                setAccount.Checked = false;
            }
            };            

            wizard.Options.Add(setAccount);            
            wizard.PackageLimit = 3000;
            //必需要有的欄位
            wizard.RequiredFields.AddRange("學號");
            //可匯入的欄位
            wizard.ImportableFields.AddRange(_FieldNameList);

            //驗證每行資料的事件
            wizard.ValidateRow += wizard_ValidateRow;

            //實際匯入資料的事件
            wizard.ImportPackage += wizard_ImportPackage;

            //匯入完成
            wizard.ImportComplete += (sender, e) => MessageBox.Show("匯入完成!");
        }

        void wizard_ImportPackage(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
        {
            // 尋找主要Key來判斷，如果有學生系統編號先用系統編號，沒有使用學號，
            Dictionary<string, RowData> RowDataDict = new Dictionary<string, RowData>();
            Dictionary<string, int> chkSidDict = new Dictionary<string, int>();
            Dictionary<string, string> chkSnumDict = new Dictionary<string, string>();
            List<StudentRecord> InsertStudentRecList = new List<StudentRecord>();
            List<StudentRecord> StudentRecAllList = K12.Data.Student.SelectAll();
            // 系統內學生帳號
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

            // 先處理學生基本資料
            foreach (RowData Row in e.Items)
            {
                string StudentID = "";

                if (Row.ContainsKey("學生系統編號"))
                {
                    string id = Row["學生系統編號"].ToString();
                    if (chkSidDict.ContainsKey(id))
                        StudentID = id;
                }

                if (StudentID == "")
                {
                    string ssNum = "", snum = "";
                    if (Row.ContainsKey("學號"))
                    {
                        snum = Row["學號"].ToString();
                        string status = "一般";
                        if (Row.ContainsKey("狀態"))
                        {
                            if (Row["狀態"].ToString() != "")
                                status = Row["狀態"].ToString();
                        }
                        ssNum = snum + status;
                    }

                    if (chkSnumDict.ContainsKey(ssNum))
                        StudentID = chkSnumDict[ssNum];
                    else
                    {
                        // 新增
                        if (chkSnumDict.ContainsKey(snum + "一般"))
                        {
                            // 有重複
                        }
                        else
                        {                       
                    
                        }
                    }
                }


                // 檢查帳號
                

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
            //比對
            foreach (RowData Row in e.Items)
            {
                string StudentID = "";

                if (Row.ContainsKey("學生系統編號"))
                {
                    string id = Row["學生系統編號"].ToString();
                    if (chkSidDict.ContainsKey(id))
                        StudentID = id;
                }

                if (StudentID == "")
                {
                    string ssNum = "", snum = "";
                    if (Row.ContainsKey("學號"))
                    {
                        snum = Row["學號"].ToString();
                        string status = "一般";
                        if (Row.ContainsKey("狀態"))
                        {
                            if (Row["狀態"].ToString() != "")
                                status = Row["狀態"].ToString();
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
            // 取得學生基本
            List<StudentRecord> StudentRecordList = K12.Data.Student.SelectByIDs(StudentIDList);
            Dictionary<string, StudentRecord> StudentRecordDict = new Dictionary<string, StudentRecord>();
            foreach (StudentRecord rec in StudentRecordList)
                if (!StudentRecordDict.ContainsKey(rec.ID))
                    StudentRecordDict.Add(rec.ID, rec);


            List<Service.UserAccount> UserAccountList = new List<Service.UserAccount>();

            // 開始處理
            List<StudentRecord> updateStudentRecList = new List<StudentRecord>();
            
            #region 上傳到Domain
            string dName = "", dAccount = "", dPwd = "";

            bool chkSend = false;
            // 取得帳號UDT
            UDT_AdminData udtAdmin = Utility.GetAdminData("student");
            if (udtAdmin != null)
            {
                dName = udtAdmin.Domain;
                dAccount = udtAdmin.Account;
                dPwd = Utility.ConvertBase64StringToString(udtAdmin.Password);
                chkSend = true;
            }

            foreach (string StudentID in RowDataDict.Keys)
            {
                RowData rd = RowDataDict[StudentID];
                if (rd.ContainsKey("登入帳號"))
                {
                    string value = rd["登入帳號"].ToString();
                    //if (StudentRecordDict[StudentID].SALoginName != value)
                    //    sbLog.AppendLine(string.Format("登入帳號由「{0}」改為「{1}」", StudentRecordDict[StudentID].SALoginName, value));
                    StudentRecordDict[StudentID].SALoginName = value;
                    updateStudentRecList.Add(StudentRecordDict[StudentID]);
                }
            }

            if (updateStudentRecList.Count > 0)
                K12.Data.Student.Update(updateStudentRecList);


            if(chkSend)
            {
                StringBuilder sendSB = new StringBuilder();

                foreach (RowData Row in e.Items)
                {
                    Service.UserAccount uAcc = new Service.UserAccount();
                    if (Row.ContainsKey("登入帳號"))
                        uAcc.Account = Row["登入帳號"].ToString();

                    if (Row.ContainsKey("密碼"))
                        uAcc.Password = Row["密碼"].ToString();

                    if (Row.ContainsKey("姓"))
                        uAcc.LastName = Row["姓"].ToString();

                    if (Row.ContainsKey("名"))
                        uAcc.FirstName = Row["名"].ToString();

                    UserAccountList.Add(uAcc);
                }
                string dsns = FISCA.Authentication.DSAServices.AccessPoint;
                string url = @"https://auth.ischool.com.tw/c/1campus.service/applicationAccounts.php";
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "POST";
                req.Accept = "*/*";
                req.ContentType = "application/json";

                sendSB.Append("{");
                string titleStr = "'application':'"+dsns+"','domain':{'name':'"+dName+"','acc':'"+dAccount+"','pwd':'"+dPwd+"'},'list':";
                // 取代'""
                string cc="\"";
                titleStr=titleStr.Replace("'", cc);
                sendSB.Append(titleStr);
                sendSB.Append(Service.GetUserAccountJSONString(UserAccountList));
                sendSB.Append("}");

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

            }

            #endregion
           

            //FISCA.LogAgent.ApplicationLog.Log("匯入學生帳號)", "匯入", sbLog.ToString());

        }
        void wizard_ValidateRow(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
        {
            #region 驗各欄位填寫格式

            List<StudentRecord> StudentRecAllList = K12.Data.Student.SelectAll();
            // 系統內學生帳號
            Dictionary<string, string> StudSANameDict = new Dictionary<string, string>();
            Dictionary<string, string> StudSANameSnumDict = new Dictionary<string, string>();

            foreach (StudentRecord rec in StudentRecAllList)
            {
                if (!StudSANameDict.ContainsKey(rec.SALoginName))
                    StudSANameDict.Add(rec.SALoginName, rec.ID);

                if(rec.Status == StudentRecord.StudentStatus.一般)
                {
                    if (!StudSANameSnumDict.ContainsKey(rec.SALoginName))
                        StudSANameSnumDict.Add(rec.SALoginName, rec.StudentNumber);
                }
            }

            foreach (string field in e.SelectFields)
            {
                string value = e.Data[field];
                switch (field)
                {
                    default:
                        break;

                    case "學號":
                        if (value.Replace(" ", "") == "")
                            e.ErrorFields.Add(field, "此欄為必填欄位。");
                        break;
              
                    case "登入帳號":
                        if(value !="")
                        {
                            if (StudSANameSnumDict.ContainsKey(value))
                            {
                                if (e.Data.ContainsKey("學號"))
                                {
                                    string SysNum = e.Data["學號"].ToString();
                                    if (SysNum != StudSANameSnumDict[value])
                                        e.ErrorFields.Add(field, "學生登入帳號已被使用，請修正");
                                }
                            }

                            if(StudSANameDict.ContainsKey(value))
                            {
                                if(e.Data.ContainsKey("學生系統編號"))
                                {
                                    string SysID = e.Data["學生系統編號"].ToString();
                                    if(SysID !=StudSANameDict[value])
                                        e.ErrorFields.Add(field, "學生登入帳號已被使用，請修正");
                                }
                            }

                        }
                        break;
                }
            }
            #endregion
            #region 驗證主鍵

            string Key = e.Data.ID;
            string errorMessage = string.Empty;

            if (_Keys.Contains(Key))
                errorMessage = "";
            else
                _Keys.Add(Key);

            e.ErrorMessage = errorMessage;

            #endregion
        }
    }
}
