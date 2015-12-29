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
            rbItemExport["匯出學生帳號"].Enable = true;
            rbItemExport["匯出學生帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportStudentAccount();
                ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            rbItemExport = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["資料統計"]["匯出"];
            rbItemExport["匯出教師帳號"].Enable = true;
            rbItemExport["匯出教師帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportTeacherAccount();
                ExportTeacherV2 wizard = new ExportTeacherV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            RibbonBarButton rbItemImport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯入"];
            rbItemImport["匯入學生帳號"].Enable = true;
            rbItemImport["匯入學生帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Import.Importer importer = new ImportStudentData();
                ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };

        }
    }
}
