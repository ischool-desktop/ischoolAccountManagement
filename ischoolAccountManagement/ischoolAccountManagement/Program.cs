using FISCA;
using FISCA.Permission;
using FISCA.Presentation;
using ischoolAccountManagement.DAO;
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

            RoleAclSource.Instance["系統"].Add(new RibbonFeature("7CB6FA23-F378-41B2-AE6E-8271F9BB1F0A", "設定網域管理者帳密"));

            FISCA.Presentation.MotherForm.StartMenu["設定網域管理者帳密"].Enable = UserAcl.Current["7CB6FA23-F378-41B2-AE6E-8271F9BB1F0A"].Executable;
            FISCA.Presentation.MotherForm.StartMenu["設定網域管理者帳密"]["學生帳號網域"].Click += delegate
            {
                new Admin.AdminForm("student").ShowDialog();
            };
            FISCA.Presentation.MotherForm.StartMenu["設定網域管理者帳密"]["教師帳號網域"].Click += delegate
            {
                new Admin.AdminForm("teacher").ShowDialog();
            };


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
                var pass = false;
                // 確認已設定網路管理者
                if (Utility.GetAdminData("student") == null || !Utility.GetAdminData("student").Check())
                {
                    FISCA.Presentation.Controls.MsgBox.Show("需先設定網域管理者帳密");
                    if (UserAcl.Current["7CB6FA23-F378-41B2-AE6E-8271F9BB1F0A"].Executable)
                    {
                        Admin.AdminForm ad = new Admin.AdminForm("student");
                        ad.ShowDialog();
                        pass = (Utility.GetAdminData("student") != null && Utility.GetAdminData("student").Check());
                    }
                }
                else
                {
                    pass = true;
                }

                if (pass)
                {
                    SmartSchool.API.PlugIn.Import.Importer importer = new ImportStudentData();
                    ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
                    importer.InitializeImport(wizard);
                    wizard.ShowDialog();
                }
            };

            rbItemImport = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["資料統計"]["匯入"];
            rbItemImport["其它相關匯入"]["匯入教師帳號"].Enable = UserAcl.Current["ischoolAccountManagement.Teacher.ImportTeacherData"].Executable;
            rbItemImport["其它相關匯入"]["匯入教師帳號"].Click += delegate
            {
                var pass = false;
                // 確認已設定網路管理者
                if (Utility.GetAdminData("teacher") == null || !Utility.GetAdminData("teacher").Check())
                {
                    FISCA.Presentation.Controls.MsgBox.Show("需先設定網域管理者帳密");
                    if (UserAcl.Current["7CB6FA23-F378-41B2-AE6E-8271F9BB1F0A"].Executable)
                    {
                        Admin.AdminForm ad = new Admin.AdminForm("teacher");
                        ad.ShowDialog();
                        pass = (Utility.GetAdminData("teacher") != null && Utility.GetAdminData("teacher").Check());
                    }
                }
                else
                {
                    pass = true;
                }

                if (pass)
                {
                    SmartSchool.API.PlugIn.Import.Importer importer = new ImportTeacherData();
                    ImportTeacherV2 wizard = new ImportTeacherV2(importer.Text, importer.Image);
                    importer.InitializeImport(wizard);
                    wizard.ShowDialog();
                }
            };
        }
    }
}
