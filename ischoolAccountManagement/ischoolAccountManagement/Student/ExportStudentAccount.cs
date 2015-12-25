using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace ischoolAccountManagement.Student
{
    class ExportStudentAccount : SmartSchool.API.PlugIn.Export.Exporter
    {
        List<string> _FieldNameList;

        public ExportStudentAccount()
        {
            this.Image = null;
            this.Text = "匯出學生帳號";
            _FieldNameList = new List<string>();
            _FieldNameList.Add("登入帳號");
        }
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            wizard.ExportableFields.AddRange(_FieldNameList);
            wizard.ExportPackage += (sender, e) =>
            {
                // 取得學生資料
                List<StudentRecord> StudentRecList = K12.Data.Student.SelectByIDs(e.List);
                               
                foreach (StudentRecord StudRec in StudentRecList)
                {
                    RowData row = new RowData();
                    row.ID = StudRec.ID;

                    foreach (string field in e.ExportFields)
                    {
                        if (wizard.ExportableFields.Contains(field))
                        {
                            switch (field)
                            {
                                case "登入帳號": row.Add(field, StudRec.SALoginName); break;                  
                            }
                        }
                    }
                    e.Items.Add(row);
                }

            };
        }

        private int SortStudent(StudentRecord s1, StudentRecord s2)
        {
            if (s1.Class == null || s2.Class == null)
                return 1;

            if (s1.Class.GradeYear.HasValue == false || s2.Class.GradeYear.HasValue == false)
                return 1;

            string strS1 = "", strS2 = "";

            strS1 += s1.Class.GradeYear.HasValue ? s1.Class.GradeYear.Value.ToString().PadLeft(3, '0') : "999";
            strS2 += s2.Class.GradeYear.HasValue ? s2.Class.GradeYear.Value.ToString().PadLeft(3, '0') : "999";

            strS1 += s1.Class.Name.PadLeft(20, '0');
            strS2 += s2.Class.Name.PadLeft(20, '0');

            strS1 += s1.SeatNo.HasValue ? s1.SeatNo.Value.ToString().PadLeft(3, '0') : "999";
            strS2 += s2.SeatNo.HasValue ? s2.SeatNo.Value.ToString().PadLeft(3, '0') : "999";

            return strS1.CompareTo(strS2);
        }
    }
}
