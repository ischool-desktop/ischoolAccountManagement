using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using System.Net;
using System.IO;

namespace ischoolAccountManagement.DAO
{
    [TableName("ischool_domain_account_data")]
    public class UDT_AdminData : ActiveRecord
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Field(Field = "account", Indexed = true)]
        public string Account { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Field(Field = "password", Indexed = false)]
        public string Password { get; set; }


        /// <summary>
        /// Domain
        /// </summary>
        [Field(Field = "domain", Indexed = false)]
        public string Domain { get; set; }

        /// <summary>
        /// Type:student,teacher
        /// </summary>
        [Field(Field = "type", Indexed = false)]
        public string Type { get; set; }


        public bool Check()
        {
            try
            {
                var dName = Domain;
                var dAccount = Account;
                var dPwd = Utility.ConvertBase64StringToString(Password);
                string dsns = FISCA.Authentication.DSAServices.AccessPoint;

                string url = Config.ChinaUrl;
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "POST";
                req.Accept = "*/*";
                req.ContentType = "application/json";
                string titleStr = "{'application':'" + dsns + "','domain':{'name':'" + dName + "','acc':'" + dAccount + "','pwd':'" + dPwd + "'}}";
                // 取代'""
                string cc = "\"";
                titleStr = titleStr.Replace("'", cc);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                byte[] byteArray = Encoding.UTF8.GetBytes(titleStr);
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
                try
                {
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                    rsp.Close();
                    if (!responseFromServer.Contains("success"))
                        return false;
                }
                catch (Exception ex)
                {
                    reader.Close();
                    dataStream.Close();
                    rsp.Close();
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public void uploadAcc(List<Service.UserAccount> list)
        {
            var dName = Domain;
            var dAccount = Account;
            var dPwd = Utility.ConvertBase64StringToString(Password);

            string dsns = FISCA.Authentication.DSAServices.AccessPoint;
            string url = Config.ChinaUrl;


            StringBuilder sendSB = new StringBuilder();
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
            sendSB.Append(Service.GetUserAccountJSONString(list));
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
                throw new Exception("上传网域账号失败," + responseFromServer);
        }
    }
}
