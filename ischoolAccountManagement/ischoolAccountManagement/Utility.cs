using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ischoolAccountManagement.DAO;
using FISCA.UDT;

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
    }
}
