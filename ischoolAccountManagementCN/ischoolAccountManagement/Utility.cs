using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ischoolAccountManagement.DAO;
using FISCA.UDT;
using System.Net;
using System.IO;

namespace ischoolAccountManagement
{
    public class Utility
    {

        public static string ConvertStringToBase64String(string str)
        {
            string value = "";
            byte[] b = System.Text.Encoding.GetEncoding("utf-8").GetBytes(str);
            value = Convert.ToBase64String(b);
            return value;
        }

        public static string ConvertBase64StringToString(string str)
        {
            string value = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(str));
            return value;
        }

        public static UDT_AdminData GetAdminData(string type)
        {
            UDT_AdminData value = null;
            AccessHelper accHelper = new AccessHelper();
            string qry = "type='" + type + "'";
            List<UDT_AdminData> aData = accHelper.Select<UDT_AdminData>(qry);
            if (aData != null && aData.Count > 0)
                value = aData[0];

            return value;
        }

        /// <summary>
        /// 检查账号是否可以登入
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CheckAdminPWD(string type)
        {
            bool pass = true;
            try
            {
                // 取得主机账号密码
                UDT_AdminData admin = GetAdminData(type);

                string dsns = FISCA.Authentication.DSAServices.AccessPoint;
                string url = Config.ChinaUrl;

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "POST";
                req.Accept = "*/*";
                req.ContentType = "application/json";
                StringBuilder sendSB = new StringBuilder();
                sendSB.Append("{");
                string titleStr = "'application':'" + dsns + "','domain':{'name':'" + admin.Domain + "','acc':'" + admin.Account + "','pwd':'" + Utility.ConvertBase64StringToString(admin.Password) + "'}";
                // 取代'""
                string cc = "\"";
                titleStr = titleStr.Replace("'", cc);
                sendSB.Append(titleStr);
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
                {
                    pass = false;
                    FISCA.Presentation.Controls.MsgBox.Show("网域账号登入失败," + responseFromServer);
                }
            }
            catch (Exception ex)
            {
                pass = false;
                FISCA.Presentation.Controls.MsgBox.Show("网域账号登入失败," + ex.Message);
            }            
            // 登入测试
            return pass;
        }
    }
}

