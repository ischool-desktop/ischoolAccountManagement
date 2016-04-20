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

            Catalog StudentCatalog1 = RoleAclSource.Instance["学生"]["功能按钮"];
            StudentCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Student.ExportStudentData", "汇出学生账号"));
            StudentCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Student.ImportStudentData", "汇入学生账号"));

            Catalog TeacherCatalog1 = RoleAclSource.Instance["教师"]["功能按钮"];
            TeacherCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Teacher.ExportTeacherData", "汇出教师账号"));
            TeacherCatalog1.Add(new RibbonFeature("ischoolAccountManagement.Teacher.ImportTeacherData", "汇入教师账号"));



            RibbonBarButton rbItemExport = K12.Presentation.NLDPanels.Student.RibbonBarItems["资料统计"]["汇出"];
            rbItemExport["汇出学生账号"].Enable = UserAcl.Current["ischoolAccountManagement.Student.ExportStudentData"].Executable;
            rbItemExport["汇出学生账号"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportStudentAccount();
                ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            rbItemExport = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["资料统计"]["汇出"];
            rbItemExport["汇出教师账号"].Enable = UserAcl.Current["ischoolAccountManagement.Teacher.ExportTeacherData"].Executable;
            rbItemExport["汇出教师账号"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportTeacherAccount();
                ExportTeacherV2 wizard = new ExportTeacherV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            RibbonBarButton rbItemImport = K12.Presentation.NLDPanels.Student.RibbonBarItems["资料统计"]["汇入"];
            rbItemImport["汇入学生账号"].Enable = UserAcl.Current["ischoolAccountManagement.Student.ImportStudentData"].Executable;
            rbItemImport["汇入学生账号"].Click += delegate
            {
                SmartSchool.API.PlugIn.Import.Importer importer = new ImportStudentData();
                ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };

            rbItemImport = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["资料统计"]["汇入"];
            rbItemImport["汇入教师账号"].Enable = UserAcl.Current["ischoolAccountManagement.Teacher.ImportTeacherData"].Executable;
            rbItemImport["汇入教师账号"].Click += delegate
            {
                SmartSchool.API.PlugIn.Import.Importer importer = new ImportTeacherData();
                ImportTeacherV2 wizard = new ImportTeacherV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };

            K12.Presentation.NLDPanels.Student.AddDetailBulider<ResetStudentPasswordDetialContent>();

            K12.Presentation.NLDPanels.Teacher.AddDetailBulider<ResetTeacherPasswordDetialContent>();
        }
    }
}

