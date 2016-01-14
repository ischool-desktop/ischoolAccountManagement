using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace ischoolAccountManagement
{
    class ExportTeacherAccount : SmartSchool.API.PlugIn.Export.Exporter
    {
        List<string> _FieldNameList;
        public ExportTeacherAccount()
        {
            this.Image = null;
            this.Text = "汇出教师账号";
            _FieldNameList = new List<string>();
            _FieldNameList.Add("登入账号");
            _FieldNameList.Add("密码");
            _FieldNameList.Add("姓");
            _FieldNameList.Add("名");
        }

        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            wizard.ExportableFields.AddRange(_FieldNameList);
            wizard.ExportPackage += (sender, e) =>
            {
                // 取得教师资料
                List<TeacherRecord> TeacherRecList = K12.Data.Teacher.SelectByIDs(e.List);

                foreach (TeacherRecord TeacherRec in TeacherRecList)
                {
                    RowData row = new RowData();
                    row.ID = TeacherRec.ID;

                    foreach (string field in e.ExportFields)
                    {
                        if (wizard.ExportableFields.Contains(field))
                        {
                            switch (field)
                            {
                                case "登入账号": row.Add(field, TeacherRec.TALoginName); break;
                            }
                        }
                    }
                    e.Items.Add(row);
                }

            };
        }
    }
}

