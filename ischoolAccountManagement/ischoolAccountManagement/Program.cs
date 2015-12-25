using FISCA;
using FISCA.Presentation;
using ischoolAccountManagement.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ischoolAccountManagement
{
    public class Program
    {
        [MainMethod()]
        static public void Main()
        {

            RibbonBarButton rbItemExport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯出"];
            rbItemExport["學籍相關匯出"]["匯出學生帳號"].Enable = true;
            rbItemExport["學籍相關匯出"]["匯出學生帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportStudentAccount();
                ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };
        }
    }
}
