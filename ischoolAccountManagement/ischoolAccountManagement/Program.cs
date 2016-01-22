using FISCA;
using FISCA.Permission;
using FISCA.Presentation;
using ischoolAccountManagement.Student;
using ischoolAccountManagement.Teacher;
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

            Catalog StudentCatalog1 = RoleAclSource.Instance["學生"]["功能按鈕"];
            StudentCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Student.ExportStudentData", "匯出學生帳號"));
            StudentCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Student.ImportStudentData", "匯入學生帳號"));

            Catalog TeacherCatalog1 = RoleAclSource.Instance["教師"]["功能按鈕"];
            TeacherCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Teacher.ExportTeacherData", "匯出教師帳號"));
            TeacherCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Teacher.ImportTeacherData", "匯入教師帳號"));



            RibbonBarButton rbItemExport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯出"];
            rbItemExport["其它相關匯出"]["匯出學生帳號"].Enable = UserAcl.Current["ischoolAccountManagement.Student.ExportStudentData"].Executable;
            rbItemExport["其它相關匯出"]["匯出學生帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportStudentAccount();
                ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            rbItemExport = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["資料統計"]["匯出"];
            rbItemExport["其它相關匯出"]["匯出教師帳號"].Enable = UserAcl.Current["ischoolAccountManagement.Teacher.ExportTeacherData"].Executable;
            rbItemExport["其它相關匯出"]["匯出教師帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportTeacherAccount();
                ExportTeacherV2 wizard = new ExportTeacherV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            RibbonBarButton rbItemImport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯入"];
            rbItemImport["其它相關匯入"]["匯入學生帳號"].Enable = UserAcl.Current["ischoolAccountManagement.Student.ImportStudentData"].Executable;
            rbItemImport["其它相關匯入"]["匯入學生帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Import.Importer importer = new ImportStudentData();
                ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };

            rbItemImport = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["資料統計"]["匯入"];
            rbItemImport["其它相關匯入"]["匯入教師帳號"].Enable = UserAcl.Current["ischoolAccountManagement.Teacher.ImportTeacherData"].Executable;
            rbItemImport["其它相關匯入"]["匯入教師帳號"].Click += delegate
            {
                SmartSchool.API.PlugIn.Import.Importer importer = new ImportTeacherData();
                ImportTeacherV2 wizard = new ImportTeacherV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };
        }
    }
}
