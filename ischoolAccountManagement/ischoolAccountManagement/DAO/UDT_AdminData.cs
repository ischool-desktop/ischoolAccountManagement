using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using System.Net;
using System.IO;
using static ischoolAccountManagement.Service;

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
                if (new RequestPackage()
                {
                    Application = FISCA.Authentication.DSAServices.AccessPoint,
                    Domain = new DomainAdmin() { Name = Domain, Acc = Account, Pwd = Utility.ConvertBase64StringToString(Password) },
                    List = null
                }.Send().Contains("success"))
                    return true;
                else
                    return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
