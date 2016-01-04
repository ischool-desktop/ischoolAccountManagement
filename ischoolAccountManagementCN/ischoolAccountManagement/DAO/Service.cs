using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;


namespace ischoolAccountManagement
{
    public class Service
    {
        [System.Runtime.Serialization.DataContract]        
        public class UserAccount
        {
            [System.Runtime.Serialization.DataMember(Name = "acc")]
            public string Account { get; set; }

            [System.Runtime.Serialization.DataMember(Name = "pwd")]
            public string Password { get; set; }

            [System.Runtime.Serialization.DataMember(Name = "firstName")]
            public string FirstName { get; set; }

            [System.Runtime.Serialization.DataMember(Name = "lastName")]
            public string LastName { get; set; }

        }



        /// <summary>
        /// 取得 帳號資料轉成 JSON String
        /// </summary>
        /// <param name="accountList"></param>
        /// <returns></returns>
        public static string GetUserAccountJSONString(List<UserAccount> accountList)
        {
            string value = "";
            System.Runtime.Serialization.Json.DataContractJsonSerializer sJSON = new System.Runtime.Serialization.Json.DataContractJsonSerializer(accountList.GetType());
            MemoryStream ms = new MemoryStream();
            sJSON.WriteObject(ms, accountList);
            value = Encoding.UTF8.GetString(ms.ToArray());
            return value;
        }
    }
}
