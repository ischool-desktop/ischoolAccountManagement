using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization;

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

        [DataContract]
        public class DomainAdmin
        {
            [System.Runtime.Serialization.DataMember(Name = "name")]
            public string Name { get; set; }
            [System.Runtime.Serialization.DataMember(Name = "acc")]
            public string Acc { get; set; }
            [System.Runtime.Serialization.DataMember(Name = "pwd")]
            public string Pwd { get; set; }
        }

        [DataContract]
        public class RequestPackage
        {
            [DataMember(Name = "application")]
            public string Application { get; set; }
            [DataMember(Name = "domain")]
            public DomainAdmin Domain { get; set; }
            [DataMember(Name = "list")]
            public UserAccount[] List { get; set; }

            private string GetRequestJSON()
            {
                string value = "";
                System.Runtime.Serialization.Json.DataContractJsonSerializer sJSON = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(RequestPackage));
                MemoryStream ms = new MemoryStream();
                sJSON.WriteObject(ms, this);
                value = Encoding.UTF8.GetString(ms.ToArray());
                return value;
            }
            public string Send()
            {
                string url = Config.TaiwanUrl;
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "POST";
                req.Accept = "*/*";
                req.ContentType = "application/json";
                req.Referer = url;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                byte[] byteArray = Encoding.UTF8.GetBytes(GetRequestJSON());
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

                return responseFromServer;
            }
        }
    }
}
