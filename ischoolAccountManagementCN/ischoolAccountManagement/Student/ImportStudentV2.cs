using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Aspose.Cells;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Rendering;
using SmartSchool.API.PlugIn;
using K12.Data;
using FISCA.Presentation.Controls;


namespace ischoolAccountManagement.Student
{
    public partial class ImportStudentV2 : FISCA.Presentation.Controls.BaseForm, SmartSchool.API.PlugIn.Import.ImportWizard
    {
        private string _Title;
        // 储存学生状态
        private K12.Data.StudentRecord.StudentStatus _StudStatus = K12.Data.StudentRecord.StudentStatus.一般;

        private IntelliSchool.DSA.ClientFramework.ControlCommunication.ListViewCheckAllManager _CheckAllManager = new IntelliSchool.DSA.ClientFramework.ControlCommunication.ListViewCheckAllManager();
        private List<string> _StudentFields = new List<string>(new string[] { "学生系统编号", "学号", "班级", "座号", "姓名" });
        private Dictionary<string, int> _ImportFields = new Dictionary<string, int>();
        private Workbook _WorkBook;
        private Workbook _ErrorWB = null;
        Dictionary<RowData, Dictionary<string, string>> _ErrorRows, _WarningRows;
        private ButtonX advButton;
        private DevComponents.DotNetBar.ControlContainerItem advContainer;
        private int _PackageLimint = 500;
        private LinkLabel helpButton;
        private SmartSchool.API.PlugIn.Collections.FieldsCollection _RequiredFields;
        private SmartSchool.API.PlugIn.Collections.FieldsCollection _ImportableFields;
        private SmartSchool.API.PlugIn.Collections.FieldsCollection _SelectedFields;
        private SmartSchool.API.PlugIn.Collections.OptionCollection _Options;
        private PanelEx _OptionsContainer;
        private bool _Trim = true;


        public ImportStudentV2(string title, Image img)
        {
            InitializeComponent();


            #region 加入进阶按纽跟HELP按钮
            _OptionsContainer = new PanelEx();
            _OptionsContainer.Font = this.Font;
            _OptionsContainer.ColorSchemeStyle = eDotNetBarStyle.Office2007;
            _OptionsContainer.Size = new Size(100, 100);
            _OptionsContainer.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            _OptionsContainer.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            _OptionsContainer.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            _OptionsContainer.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            _OptionsContainer.Style.GradientAngle = 90;
            _Options = new SmartSchool.API.PlugIn.Collections.OptionCollection();
            _Options.ItemsChanged += new EventHandler(_Options_ItemsChanged);

            advContainer = new ControlContainerItem();
            advContainer.AllowItemResize = false;
            advContainer.GlobalItem = false;
            advContainer.MenuVisibility = eMenuVisibility.VisibleAlways;
            advContainer.Control = _OptionsContainer;

            ItemContainer itemContainer2 = new ItemContainer();
            itemContainer2.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            itemContainer2.MinimumSize = new System.Drawing.Size(0, 0);
            itemContainer2.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            advContainer});

            advButton = new ButtonX();
            advButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            advButton.Text = "    进阶";
            advButton.Top = this.wizard1.Controls[1].Controls[0].Top;
            advButton.Left = 5;
            advButton.Size = this.wizard1.Controls[1].Controls[0].Size;
            advButton.Visible = true;
            advButton.SubItems.Add(itemContainer2);
            advButton.PopupSide = ePopupSide.Top;
            advButton.SplitButton = true;
            advButton.Enabled = false;
            advButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            advButton.AutoExpandOnClick = true;
            advButton.SubItemsExpandWidth = 16;
            advButton.FadeEffect = false;
            advButton.FocusCuesEnabled = false;
            this.wizard1.Controls[1].Controls.Add(advButton);

            helpButton = new LinkLabel();
            helpButton.AutoSize = true;
            helpButton.BackColor = System.Drawing.Color.Transparent;
            helpButton.Location = new System.Drawing.Point(81, 10);
            helpButton.Size = new System.Drawing.Size(69, 17);
            helpButton.TabStop = true;
            helpButton.Text = "Help";
            helpButton.Visible = false;
            helpButton.Click += delegate { if (HelpButtonClick != null)HelpButtonClick(this, new EventArgs()); };
            helpButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.wizard1.Controls[1].Controls.Add(helpButton);
            #endregion

            #region 设定Wizard会跟着Style跑
            //this.wizard1.FooterStyle.ApplyStyle(( GlobalManager.Renderer as Office2007Renderer ).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.wizard1.HeaderStyle.ApplyStyle((GlobalManager.Renderer as Office2007Renderer).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.wizard1.FooterStyle.BackColorGradientAngle = -90;
            this.wizard1.FooterStyle.BackColorGradientType = eGradientType.Linear;
            this.wizard1.FooterStyle.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.wizard1.FooterStyle.BackColor2 = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.End;
            this.wizard1.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.wizard1.BackgroundImage = null;
            for (int i = 0; i < 6; i++)
            {
                (this.wizard1.Controls[1].Controls[i] as ButtonX).ColorTable = eButtonColor.OrangeWithBackground;
            }
            (this.wizard1.Controls[0].Controls[1] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.MouseOver.TitleText;
            (this.wizard1.Controls[0].Controls[2] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TitleText;
            _CheckAllManager.TargetComboBox = this.checkBox1;
            _CheckAllManager.TargetListView = this.listView1;
            #endregion

            _Title = this.Text = title;

            lblReqFields.Text = "<font color=\"#7A4E2B\"><b>学生系统编号 或 学号</b></font>";

            foreach (WizardPage page in wizard1.WizardPages)
            {
                page.PageTitle = _Title;
                if (img != null)
                {
                    Bitmap b = new Bitmap(48, 48);
                    using (Graphics g = Graphics.FromImage(b))
                        g.DrawImage(img, 0, 0, 48, 48);
                    page.PageHeaderImage = b;
                }
            }
            _RequiredFields = new SmartSchool.API.PlugIn.Collections.FieldsCollection();
            _ImportableFields = new SmartSchool.API.PlugIn.Collections.FieldsCollection();
            _SelectedFields = new SmartSchool.API.PlugIn.Collections.FieldsCollection();
            _RequiredFields.ItemsChanged += new EventHandler(FieldsChanged);
            _ImportableFields.ItemsChanged += new EventHandler(FieldsChanged);


        }
        Dictionary<VirtualCheckBox, DevComponents.DotNetBar.Controls.CheckBoxX> _CheckBoxs = new Dictionary<VirtualCheckBox, DevComponents.DotNetBar.Controls.CheckBoxX>();
        Dictionary<VirtualRadioButton, System.Windows.Forms.RadioButton> _RadioButtons = new Dictionary<VirtualRadioButton, System.Windows.Forms.RadioButton>();
        List<VirtualCheckItem> _AllItems = new List<VirtualCheckItem>();
        void _Options_ItemsChanged(object sender, EventArgs e)
        {
            _CheckBoxs.Clear();
            _RadioButtons.Clear();
            int width = 0;
            int Y = 1;
            int speace = 1;
            int visibleCount = 0;
            foreach (Control control in _OptionsContainer.Controls)
            {
                control.Dispose();
            }
            _OptionsContainer.Controls.Clear();
            foreach (VirtualCheckItem item in _Options)
            {
                if (!_AllItems.Contains(item))
                {
                    _AllItems.Add(item);
                    item.VisibleChanged += new EventHandler(item_VisibleChanged);
                }
                if (item.Visible)
                {
                    visibleCount++;
                    if (item is VirtualCheckBox)
                    {
                        #region 加入CheckBox
                        DevComponents.DotNetBar.Controls.CheckBoxX checkbox = new DevComponents.DotNetBar.Controls.CheckBoxX();
                        checkbox.TabIndex = 0;
                        checkbox.TabStop = true;
                        checkbox.AutoSize = true;
                        checkbox.Text = item.Text;
                        checkbox.Checked = item.Checked;
                        checkbox.Enabled = item.Enabled;
                        checkbox.Tag = item;
                        checkbox.CheckedChanged += new EventHandler(checkbox_CheckedChanged);
                        item.CheckedChanged += new EventHandler(syncCheckBox);
                        item.TextChanged += new EventHandler(syncCheckBox);
                        item.EnabledChanged += new EventHandler(syncCheckBox);
                        checkbox.Location = new Point(9, Y);
                        _OptionsContainer.Controls.Add(checkbox);//要先加入Panel后抓Size才准
                        Y += checkbox.Height + speace;
                        if (checkbox.PreferredSize.Width + 25 > width)
                            width = checkbox.PreferredSize.Width + 25;
                        _CheckBoxs.Add(item as VirtualCheckBox, checkbox);
                        #endregion
                    }
                    if (item is VirtualRadioButton)
                    {
                        #region 加入RadioButton
                        System.Windows.Forms.RadioButton radioButton = new System.Windows.Forms.RadioButton();
                        radioButton.TabIndex = 0;
                        radioButton.TabStop = true;
                        radioButton.AutoSize = true;
                        radioButton.Text = item.Text;
                        radioButton.Checked = item.Checked;
                        radioButton.Enabled = item.Enabled; ;
                        radioButton.Tag = item;
                        radioButton.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
                        item.CheckedChanged += new EventHandler(syncRadioButton);
                        item.TextChanged += new EventHandler(syncRadioButton);
                        item.EnabledChanged += new EventHandler(syncRadioButton);
                        radioButton.Location = new Point(12, Y);
                        _OptionsContainer.Controls.Add(radioButton);
                        //radioButton.Invalidate();
                        //radioButton.PerformLayout();
                        Y += radioButton.Height + speace;
                        if (radioButton.PreferredSize.Width + 25 > width)
                            width = radioButton.PreferredSize.Width + 25;
                        _RadioButtons.Add(item as VirtualRadioButton, radioButton);
                        #endregion
                    }
                }
            }
            _OptionsContainer.Size = new Size(width, Y);
            advButton.Enabled = visibleCount > 0;
            advContainer.RecalcSize();
            SetForeColor(_OptionsContainer);
        }

        void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            DevComponents.DotNetBar.Controls.CheckBoxX checkbox = (DevComponents.DotNetBar.Controls.CheckBoxX)sender;
            VirtualCheckItem item = (VirtualCheckItem)checkbox.Tag;
            if (item.Checked != checkbox.Checked)
                item.Checked = checkbox.Checked;
        }

        void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton radioButton = (System.Windows.Forms.RadioButton)sender;
            VirtualCheckItem item = (VirtualCheckItem)radioButton.Tag;
            if (item.Checked != radioButton.Checked)
                item.Checked = radioButton.Checked;
        }

        void item_VisibleChanged(object sender, EventArgs e)
        {
            _Options_ItemsChanged(null, null);
        }

        void syncCheckBox(object sender, EventArgs e)
        {
            VirtualCheckBox item = (VirtualCheckBox)sender;
            if (!_CheckBoxs.ContainsKey(item)) return;
            DevComponents.DotNetBar.Controls.CheckBoxX checkbox = _CheckBoxs[item];
            checkbox.Text = item.Text;
            checkbox.Enabled = item.Enabled;
            if (item.Checked != checkbox.Checked)
                checkbox.Checked = item.Checked;
            if (checkbox.PreferredSize.Width + 25 > _OptionsContainer.Width)
                _OptionsContainer.Width = checkbox.PreferredSize.Width + 25;
        }

        void syncRadioButton(object sender, EventArgs e)
        {
            VirtualRadioButton item = (VirtualRadioButton)sender;
            if (!_RadioButtons.ContainsKey(item)) return;
            System.Windows.Forms.RadioButton radioButton = _RadioButtons[item];
            radioButton.Text = item.Text;
            radioButton.Enabled = item.Enabled;
            if (item.Checked != radioButton.Checked)
                radioButton.Checked = item.Checked;
            if (radioButton.PreferredSize.Width + 25 > _OptionsContainer.Width)
                _OptionsContainer.Width = radioButton.PreferredSize.Width + 25;
        }

        private void SetForeColor(Control parent)
        {
            foreach (Control var in parent.Controls)
            {
                if (var is System.Windows.Forms.RadioButton)
                    var.ForeColor = ((Office2007Renderer)GlobalManager.Renderer).ColorTable.CheckBoxItem.Default.Text;
                SetForeColor(var);
            }
        }

        void FieldsChanged(object sender, EventArgs e)
        {
            this.wizardPage1.NextButtonEnabled = eWizardButtonState.False;
            linkLabel2.Visible = false;
            errorFile.Clear();
            Application.DoEvents();
            foreach (string field in _RequiredFields)
            {
                if (!_ImportableFields.Contains(field))
                {
                    _ImportableFields.Add(field);
                }
            }
            lblReqFields.Text = "<font color=\"#7A4E2B\"><b>学生系统编号 或 学号</b></font>";
            foreach (string req in _RequiredFields)
            {
                lblReqFields.Text += " 、 " + "<font color=\"#7A4E2B\"><b>" + req + "</b></font>";
            }
            //if ( _ImportableFields.Count == _RequiredFields.Count )
            //    wizard1.WizardPages.RemoveAt(1);
            //else if ( !wizard1.WizardPages.Contains(wizardPage2) )
            //    wizard1.WizardPages.Insert(0, wizardPage2);
            if (txtFile.Text != "")
                CheckFile();
        }

        private void CheckFile()
        {
            this.wizardPage1.NextButtonEnabled = eWizardButtonState.False;
            string messingFields = "";
            foreach (string key in _RequiredFields)
            {
                if (!_ImportFields.ContainsKey(key))
                {
                    messingFields += (messingFields == "" ? "" : ",") + key;
                }
            }
            if (!_ImportFields.ContainsKey("学生系统编号") && !_ImportFields.ContainsKey("学号"))
            {
                messingFields += (messingFields == "" ? "" : ",") + "学生系统编号或学号";
            }
            if (messingFields != "")
            {
                errorFile.SetIconAlignment(txtFile, ErrorIconAlignment.MiddleLeft);
                errorFile.SetError(txtFile, "缺少必要字段:\n" + messingFields);
                return;
            }
            listView1.SuspendLayout();
            listView1.Items.Clear();
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (string field in _ImportableFields)
            {
                if (!_RequiredFields.Contains(field) && _ImportFields.ContainsKey(field))
                {
                    ListViewItem item = new ListViewItem(field);
                    item.Checked = true;
                    items.Add(item);
                }
            }
            listView1.Items.AddRange(items.ToArray());
            listView1.ResumeLayout();
            this.wizardPage1.NextButtonEnabled = eWizardButtonState.True;
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (SelectSourceFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (txtFile.Text == SelectSourceFileDialog.FileName)
                    txtFile_TextChanged(null, null);
                else
                    txtFile.Text = SelectSourceFileDialog.FileName;
            }
        }

        private void txtFile_TextChanged(object sender, EventArgs e)
        {
            this.UseWaitCursor = true;
            linkLabel2.Visible = false;
            errorFile.Clear();
            Application.DoEvents();
            _ImportFields.Clear();
            _WorkBook = new Workbook();
            try
            {
                _WorkBook.Open(txtFile.Text);
            }
            catch
            {
                errorFile.SetIconAlignment(txtFile, ErrorIconAlignment.MiddleLeft);
                errorFile.SetError(txtFile, "档案不存在、格式错误或档案开启中无法读取。");
                this.UseWaitCursor = false;
                return;
            }
            linkLabel2.Visible = true;
            this.UseWaitCursor = false;
            //读取档案内所有的字段
            for (int i = 0; i <= (int)_WorkBook.Worksheets[0].Cells.MaxColumn; i++)
            {
                if (GetTrimText("" + _WorkBook.Worksheets[0].Cells[0, i].StringValue) != "")
                    _ImportFields.Add(GetTrimText("" + _WorkBook.Worksheets[0].Cells[0, i].StringValue), i);
            }
            if (LoadSource != null)
            {
                SmartSchool.API.PlugIn.Import.LoadSourceEventArgs args = new SmartSchool.API.PlugIn.Import.LoadSourceEventArgs();
                args.Fields.AddRange(_ImportFields.Keys);
                LoadSource(this, args);
            }
            CheckFile();
        }

        private BackgroundWorker _BKWValidate;

        private void wizardPage3_AfterPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            this.progressBarX1.Value = 0;
            // 读取画面上选取学生状态


            lblWarningCount.Text = lblErrCount.Text = "0";
            this.wizardPage3.FinishButtonEnabled = eWizardButtonState.False;
            linkLabel1.Visible = false;
            labelX2.Text = "数据验证中";
            linkLabel3.Tag = null;
            linkLabel3.Visible = false;
            Application.DoEvents();

            _BKWValidate = new BackgroundWorker();
            _BKWValidate.WorkerReportsProgress = true;
            _BKWValidate.WorkerSupportsCancellation = true;
            _BKWValidate.DoWork += new DoWorkEventHandler(_BKWValidate_DoWork);
            _BKWValidate.ProgressChanged += new ProgressChangedEventHandler(_BKWValidate_ProgressChanged);
            _BKWValidate.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BKWValidate_RunWorkerCompleted);

            List<string> fields = new List<string>();
            string fileName = txtFile.Text;
            fields.AddRange(_RequiredFields);
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Checked)
                    fields.Add(item.Text);
            }
            _ErrorRows = new Dictionary<RowData, Dictionary<string, string>>();
            _WarningRows = new Dictionary<RowData, Dictionary<string, string>>();
            Workbook wb = new Workbook();
            wb.Copy(_WorkBook);
            _BKWValidate.RunWorkerAsync(new object[] { fields, _ImportFields, wb });
        }

        private void wizardPage3_BackButtonClick(object sender, CancelEventArgs e)
        {
            if (_BKWValidate != null && _BKWValidate.IsBusy)
                _BKWValidate.CancelAsync();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_BKWValidate != null && _BKWValidate.IsBusy)
                _BKWValidate.CancelAsync();
            linkLabel3.Tag = "!";
        }

        private void ImportStudent_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_BKWValidate != null && _BKWValidate.IsBusy)
                _BKWValidate.CancelAsync();
        }

        private void _BKWValidate_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = (BackgroundWorker)sender;
            List<string> selectedFields = (List<string>)((object[])e.Argument)[0];
            Dictionary<string, int> importFields = (Dictionary<string, int>)((object[])e.Argument)[1];
            Workbook wb = (Workbook)((object[])e.Argument)[2];
            wb.Worksheets[0].Comments.Clear();
            wb.Worksheets[0].Hyperlinks.Clear();
            int errorSheetIndex = wb.Worksheets.Add();
            {
                int errc = 0;
                #region 命名
                for (; ; errc++)
                {
                    bool pass = true;
                    string n = "错误&警告说明" + (errc == 0 ? "" : "(" + errc + ")");
                    foreach (Aspose.Cells.Worksheet var in wb.Worksheets)
                    {
                        if (n == var.Name)
                        {
                            pass = false;
                            break;
                        }
                    }
                    if (pass) break;
                }
                #endregion
                wb.Worksheets[errorSheetIndex].Name = "错误&警告说明" + (errc == 0 ? "" : "(" + errc + ")");
            }
            string errorSheetName = wb.Worksheets[errorSheetIndex].Name;
            Worksheet errorSheet = wb.Worksheets[errorSheetIndex];
            errorSheet.Cells[0, 0].PutValue("行号");
            errorSheet.Cells[0, 1].PutValue("种类");
            errorSheet.Cells[0, 2].PutValue("说明");
            int errorSheetRowIndex = 1;

            Style errorStyle = wb.Styles[wb.Styles.Add()];
            Style warningStyle = wb.Styles[wb.Styles.Add()];
            Style passStyle = wb.Styles[wb.Styles.Add()];

            Style errorStyle2 = wb.Styles[wb.Styles.Add()];
            Style warningStyle2 = wb.Styles[wb.Styles.Add()];

            //errorStyle.Pattern = BackgroundType.Solid;
            //errorStyle.ForegroundColor = Color.Red;
            errorStyle.Font.Color = Color.Red;
            errorStyle.Font.Underline = FontUnderlineType.Single;

            //warningStyle.Pattern = BackgroundType.Solid;
            //warningStyle.ForegroundColor = Color.Yellow;
            warningStyle.Font.Color = wb.GetMatchingColor(Color.Goldenrod);
            warningStyle.Font.Underline = FontUnderlineType.Single;

            //passStyle.Pattern = BackgroundType.Solid;
            //passStyle.ForegroundColor = Color.Green;
            passStyle.Font.Color = Color.Green;

            warningStyle2.Font.Color = wb.GetMatchingColor(Color.Goldenrod);
            errorStyle2.Font.Color = Color.Red;


            int errorCount = 0;
            int warningCount = 0;
            Dictionary<RowData, int> rowDataIndex = new Dictionary<RowData, int>();

            Dictionary<int, string> fieldIndex = new Dictionary<int, string>();
            foreach (string field in selectedFields)
            {
                fieldIndex.Add(importFields[field], field);
            }

            double progress = 0.0;

            // 取得所有学生建立所引
            Dictionary<string, StudentRecord> chkStudRecDict = new Dictionary<string, StudentRecord>();
            List<StudentRecord> AllStudenRecList = K12.Data.Student.SelectAll();
            foreach (StudentRecord rec in AllStudenRecList)
                chkStudRecDict.Add(rec.ID, rec);


            #region 产生RowData数据
            if (importFields.ContainsKey("学生系统编号"))
            {
                List<string> chkStudentIDSameList = new List<string>();
                #region 用编号验证数据
                for (int i = 1; i <= wb.Worksheets[0].Cells.MaxDataRow; i++)
                {
                    // , "学号", "班级", "座号", "科别", "姓名"
                    string id = GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["学生系统编号"]].StringValue);
                    if (chkStudRecDict.ContainsKey(id))
                    {
                        string rowError = "";
                        if (!chkStudentIDSameList.Contains(id))
                            chkStudentIDSameList.Add(id);
                        else
                            rowError = "工作表内 学生系统编号：" + id + " 数据重复";
                        #region 验明正身
                        StudentRecord stu = chkStudRecDict[id];

                        if (importFields.ContainsKey("学号") && GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["学号"]].StringValue) != stu.StudentNumber)
                        {
                            //rowError = "学号与系统内学生数据不同!!";
                            rowError += (rowError == "" ? "" : "、\n") + "系统内学生学号为\"" + stu.StudentNumber + "\"";
                        }

                        if (importFields.ContainsKey("姓名") && GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["姓名"]].StringValue) != stu.Name)
                        {
                            //rowError = "姓名与系统内学生数据不同!!";
                            rowError += (rowError == "" ? "" : "、\n") + "系统内学生姓名为\"" + stu.Name + "\"";
                        }

                        #endregion
                        if (rowError == "")
                        {
                            RowData rowdata = new RowData();
                            rowdata.ID = id;
                            foreach (int index in fieldIndex.Keys)
                            {
                                if (wb.Worksheets[0].Cells[i, index].Type == CellValueType.IsDateTime)
                                {
                                    rowdata.Add(fieldIndex[index], wb.Worksheets[0].Cells[i, index].DateTimeValue.ToString());
                                }
                                else
                                    rowdata.Add(fieldIndex[index], GetTrimText("" + wb.Worksheets[0].Cells[i, index].StringValue));
                            }
                            rowDataIndex.Add(rowdata, i);
                        }
                        else
                        {
                            errorCount++;
                            errorSheet.Cells[errorSheetRowIndex, 0].PutValue(i + 1);
                            errorSheet.Cells[errorSheetRowIndex, 1].PutValue("错误");
                            errorSheet.Cells[errorSheetRowIndex, 2].PutValue(rowError);
                            errorSheet.Cells[errorSheetRowIndex, 0].Style = errorStyle;
                            errorSheet.Cells[errorSheetRowIndex, 1].Style = errorStyle2;
                            errorSheet.Cells[errorSheetRowIndex, 2].Style = errorStyle2;
                            errorSheet.Hyperlinks.Add(errorSheetRowIndex, 0, 1, 1, "'" + wb.Worksheets[0].Name + "'!" + wb.Worksheets[0].Cells[i, 0].Name);
                            wb.Worksheets[0].Hyperlinks.Add(i, 0, 1, 1, "'" + errorSheetName + "'!" + errorSheet.Cells[errorSheetRowIndex, 0].Name);
                            errorSheet.AutoFitRow(errorSheetRowIndex);
                            errorSheetRowIndex++;
                            wb.Worksheets[0].Cells[i, 0].Style = errorStyle;
                        }
                    }
                    else
                    {
                        bool isNullRow = true;
                        for (byte c = 0; c < wb.Worksheets[0].Cells.MaxDataColumn; c++)
                        {
                            if (GetTrimText("" + wb.Worksheets[0].Cells[i, c].StringValue) != "")
                                isNullRow = false;
                        }
                        if (!isNullRow)
                        {
                            errorCount++;
                            errorSheet.Cells[errorSheetRowIndex, 0].PutValue(i + 1);
                            errorSheet.Cells[errorSheetRowIndex, 1].PutValue("错误");
                            errorSheet.Cells[errorSheetRowIndex, 2].PutValue("验证字段(学生系统编号)不得空白或不存在系统内");
                        }
                        else
                        {
                            errorCount++;
                            errorSheet.Cells[errorSheetRowIndex, 0].PutValue(i + 1);
                            errorSheet.Cells[errorSheetRowIndex, 1].PutValue("错误");
                            errorSheet.Cells[errorSheetRowIndex, 2].PutValue("验证字段(学生系统编号)不存在系统内");
                        }

                        errorSheet.Cells[errorSheetRowIndex, 0].Style = errorStyle;
                        errorSheet.Cells[errorSheetRowIndex, 1].Style = errorStyle2;
                        errorSheet.Cells[errorSheetRowIndex, 2].Style = errorStyle2;
                        errorSheet.Hyperlinks.Add(errorSheetRowIndex, 0, 1, 1, "'" + wb.Worksheets[0].Name + "'!" + wb.Worksheets[0].Cells[i, 0].Name);
                        wb.Worksheets[0].Hyperlinks.Add(i, 0, 1, 1, "'" + errorSheetName + "'!" + errorSheet.Cells[errorSheetRowIndex, 0].Name);
                        errorSheet.AutoFitRow(errorSheetRowIndex);
                        errorSheetRowIndex++;
                        wb.Worksheets[0].Cells[i, 0].Style = errorStyle;
                    }
                    if (bkw.CancellationPending)
                    {
                        e.Cancel = true;
                        _ErrorWB = wb;
                        return;
                    }
                    progress = ((double)i) * 100.0 / 100000;
                    bkw.ReportProgress((int)progress, new int[] { errorCount, warningCount });
                }
                #endregion
            }
            else if (importFields.ContainsKey("学号"))
            {
                // 建立学号状态所引
                Dictionary<string, StudentRecord> StudNumberStatusDict = new Dictionary<string, StudentRecord>();
                List<string> chkWstSnumList = new List<string>();
                foreach (StudentRecord rec in AllStudenRecList)
                {
                    string key = rec.StudentNumber + rec.StatusStr;
                    if (!StudNumberStatusDict.ContainsKey(key))
                        StudNumberStatusDict.Add(key, rec);
                }

                for (int i = 1; i <= wb.Worksheets[0].Cells.MaxDataRow; i++)
                {
                    string StuStatus = "一般";
                    if (importFields.ContainsKey("状态") && GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["状态"]].StringValue) != "")
                        StuStatus = GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["状态"]].StringValue);
                    string wtNum = GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["学号"]].StringValue);
                    string num = wtNum + StuStatus;
                    if (num != "")
                    {
                        string rowError = "";
                        #region 验明正身
                        if (!chkWstSnumList.Contains(num))
                            chkWstSnumList.Add(num);
                        else
                        {
                            rowError = "工作表内 学号：" + wtNum + ",状态：" + StuStatus + " 数据重复";

                        }
                        StudentRecord stu = null;
                        if (StudNumberStatusDict.ContainsKey(num))
                        {
                            StudentRecord var = StudNumberStatusDict[num];
                            bool pass = true;

                            if (importFields.ContainsKey("姓名") && GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["姓名"]].StringValue) != var.Name)
                            {
                                rowError += (rowError == "" ? "" : "、\n") + "系统内学生姓名为\"" + var.Name + "\"";
                                pass = false;
                            }
                            if (pass)
                            {
                                stu = var;
                            }
                        }
                        else
                        {
                            // 当作新增，给后面程序处理
                            if (stu == null)
                            {
                                // 检查一般状态是否存在，不存在新增一笔
                                if (StudNumberStatusDict.ContainsKey(wtNum + "一般"))
                                {
                                    rowError = "学号" + wtNum + " 已经有学生使用，无法新增。";
                                }
                                //else
                                //{
                                //    StudentRecord newStu = new StudentRecord();
                                //    newStu.StudentNumber = wtNum;
                                //    if (importFields.ContainsKey("姓名") && GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["姓名"]].StringValue) != "")
                                //    {
                                //        newStu.Name = GetTrimText("" + wb.Worksheets[0].Cells[i, importFields["姓名"]].StringValue);                                       
                                //    }
                                //    newStu.Status = K12.Data.StudentRecord.StudentStatus.一般;
                                //    string sid=JHStudent.Insert(newStu);
                                //    stu = JHStudent.SelectByID(sid);
                                //}
                            }
                        }

                        #endregion
                        if (rowError == "")
                        {
                            if (stu != null)
                            {
                                RowData rowdata = new RowData();
                                rowdata.ID = stu.ID;
                                foreach (int index in fieldIndex.Keys)
                                {
                                    if (wb.Worksheets[0].Cells[i, index].Type == CellValueType.IsDateTime)
                                    {
                                        rowdata.Add(fieldIndex[index], wb.Worksheets[0].Cells[i, index].DateTimeValue.ToString());
                                    }
                                    else
                                        rowdata.Add(fieldIndex[index], GetTrimText("" + wb.Worksheets[0].Cells[i, index].StringValue));
                                }
                                rowDataIndex.Add(rowdata, i);
                            }
                        }
                        else
                        {
                            errorCount++;
                            errorSheet.Cells[errorSheetRowIndex, 0].PutValue(i + 1);
                            errorSheet.Cells[errorSheetRowIndex, 1].PutValue("错误");
                            errorSheet.Cells[errorSheetRowIndex, 2].PutValue(rowError);
                            errorSheet.Cells[errorSheetRowIndex, 0].Style = errorStyle;
                            errorSheet.Cells[errorSheetRowIndex, 1].Style = errorStyle2;
                            errorSheet.Cells[errorSheetRowIndex, 2].Style = errorStyle2;
                            errorSheet.Hyperlinks.Add(errorSheetRowIndex, 0, 1, 1, "'" + wb.Worksheets[0].Name + "'!" + wb.Worksheets[0].Cells[i, 0].Name);
                            wb.Worksheets[0].Hyperlinks.Add(i, 0, 1, 1, "'" + errorSheetName + "'!" + errorSheet.Cells[errorSheetRowIndex, 0].Name);
                            errorSheet.AutoFitRow(errorSheetRowIndex);
                            errorSheetRowIndex++;
                            wb.Worksheets[0].Cells[i, 0].Style = errorStyle;
                        }
                    }
                    else
                    {
                        bool isNullRow = true;
                        for (byte c = 0; c < wb.Worksheets[0].Cells.MaxDataColumn; c++)
                        {
                            if (GetTrimText("" + wb.Worksheets[0].Cells[i, c].StringValue) != "")
                                isNullRow = false;
                        }
                        if (!isNullRow)
                        {
                            errorCount++;
                            errorSheet.Cells[errorSheetRowIndex, 0].PutValue(i + 1);
                            errorSheet.Cells[errorSheetRowIndex, 1].PutValue("错误");
                            errorSheet.Cells[errorSheetRowIndex, 2].PutValue("验证字段(学号)不得空白");
                            errorSheet.Cells[errorSheetRowIndex, 0].Style = errorStyle;
                            errorSheet.Cells[errorSheetRowIndex, 1].Style = errorStyle2;
                            errorSheet.Cells[errorSheetRowIndex, 2].Style = errorStyle2;
                            errorSheet.Hyperlinks.Add(errorSheetRowIndex, 0, 1, 1, "'" + wb.Worksheets[0].Name + "'!" + wb.Worksheets[0].Cells[i, 0].Name);
                            wb.Worksheets[0].Hyperlinks.Add(i, 0, 1, 1, "'" + errorSheetName + "'!" + errorSheet.Cells[errorSheetRowIndex, 0].Name);
                            errorSheet.AutoFitRow(errorSheetRowIndex);
                            errorSheetRowIndex++;
                            wb.Worksheets[0].Cells[i, 0].Style = errorStyle;
                        }
                    }

                    if (bkw.CancellationPending)
                    {
                        e.Cancel = true;
                        _ErrorWB = wb;
                        return;
                    }
                    progress = ((double)i) * 100.0 / 100000;
                    bkw.ReportProgress((int)progress, new int[] { errorCount, warningCount });
                }
            #endregion
            }


            #region 验证数据
            List<string> list = new List<string>();
            foreach (RowData row in rowDataIndex.Keys)
            {
                if (!list.Contains(row.ID))
                    list.Add(row.ID);
            }
            //_Process.StartValidate(list);
            if (ValidateStart != null)
            {
                SmartSchool.API.PlugIn.Import.ValidateStartEventArgs args = new SmartSchool.API.PlugIn.Import.ValidateStartEventArgs();
                args.List = list.ToArray();
                ValidateStart(this, args);
            }
            double totleCount = (double)rowDataIndex.Count;
            double count = 0.0;
            foreach (RowData row in rowDataIndex.Keys)
            {
                #region 验证
                string rowError = "";
                Dictionary<string, string> errorFields, warningFields;
                //RowDataValidatedEventArgs args = _Process.ValidateRow(row, selectedFields);
                SmartSchool.API.PlugIn.Import.ValidateRowEventArgs args = new SmartSchool.API.PlugIn.Import.ValidateRowEventArgs();
                args.Data = row;
                args.SelectFields.AddRange(selectedFields);
                if (ValidateRow != null)
                {
                    ValidateRow(this, args);
                }
                errorFields = args.ErrorFields;
                warningFields = args.WarningFields;
                rowError += args.ErrorMessage;
                if (rowError == "" && errorFields.Count == 0 && warningFields.Count == 0)
                {
                    wb.Worksheets[0].Cells[rowDataIndex[row], 0].Style = passStyle;
                }
                else
                {
                    string message = "";
                    bool hasError = false;
                    if (rowError != "" || errorFields.Count != 0)
                    {
                        errorCount++;
                        hasError = true;
                        message = rowError;
                        message += (message == "" ? "" : "\n") + "";
                        foreach (string key in errorFields.Keys)
                        {
                            message += (message == "" ? "" : "\n") + "  " + key + "：" + errorFields[key];
                        }
                        errorSheet.Cells[errorSheetRowIndex, 0].PutValue(rowDataIndex[row] + 1);
                        errorSheet.Cells[errorSheetRowIndex, 1].PutValue(hasError ? "错误" : "警告");
                        errorSheet.Cells[errorSheetRowIndex, 2].PutValue(message);
                        errorSheet.Cells[errorSheetRowIndex, 0].Style = errorStyle;
                        errorSheet.Cells[errorSheetRowIndex, 1].Style = errorStyle2;
                        errorSheet.Cells[errorSheetRowIndex, 2].Style = errorStyle2;
                        errorSheet.Hyperlinks.Add(errorSheetRowIndex, 0, 1, 1, "'" + wb.Worksheets[0].Name + "'!" + wb.Worksheets[0].Cells[rowDataIndex[row], 0].Name);
                        wb.Worksheets[0].Hyperlinks.Add(rowDataIndex[row], 0, 1, 1, "'" + errorSheetName + "'!" + errorSheet.Cells[errorSheetRowIndex, 0].Name);
                        errorSheet.AutoFitRow(errorSheetRowIndex);
                        errorSheetRowIndex++;
                    }
                    message = "";
                    if (warningFields.Count != 0)
                    {
                        if (!hasError)
                            warningCount++;
                        message += (message == "" ? "" : "\n") + "";
                        foreach (string key in warningFields.Keys)
                        {
                            message += (message == "" ? "" : "\n") + "  " + key + "：" + warningFields[key];
                        }
                        errorSheet.Cells[errorSheetRowIndex, 0].PutValue(rowDataIndex[row] + 1);
                        errorSheet.Cells[errorSheetRowIndex, 1].PutValue("警告");
                        errorSheet.Cells[errorSheetRowIndex, 2].PutValue(message);
                        errorSheet.Cells[errorSheetRowIndex, 0].Style = warningStyle;
                        errorSheet.Cells[errorSheetRowIndex, 1].Style = warningStyle2;
                        errorSheet.Cells[errorSheetRowIndex, 2].Style = warningStyle2;
                        errorSheet.Hyperlinks.Add(errorSheetRowIndex, 0, 1, 1, "'" + wb.Worksheets[0].Name + "'!" + wb.Worksheets[0].Cells[rowDataIndex[row], 0].Name);
                        if (!hasError)
                            wb.Worksheets[0].Hyperlinks.Add(rowDataIndex[row], 0, 1, 1, "'" + errorSheetName + "'!" + errorSheet.Cells[errorSheetRowIndex, 0].Name);
                        errorSheet.AutoFitRow(errorSheetRowIndex);
                        errorSheetRowIndex++;
                    }
                    wb.Worksheets[0].Cells[rowDataIndex[row], 0].Style = hasError ? errorStyle : warningStyle;
                }
                #endregion
                if (bkw.CancellationPending)
                {
                    e.Cancel = true;
                    _ErrorWB = wb;
                    return;
                }
                count++;
                bkw.ReportProgress((int)(progress + count * (100.0 - progress) / totleCount), new int[] { errorCount, warningCount });
            }


            //_Process.FinishValidate();
            if (ValidateComplete != null)
                ValidateComplete(this, new EventArgs());
            #endregion

            List<RowData> rows = new List<RowData>();
            rows.AddRange(rowDataIndex.Keys);

            bkw.ReportProgress(100, new int[] { errorCount, warningCount });

            errorSheet.AutoFitColumn(0);
            errorSheet.AutoFitColumn(1, 1, 500);
            errorSheet.AutoFitColumn(2, 1, 500);

            e.Result = new object[] { wb, errorCount == 0, rows, selectedFields };
        }

        private void _BKWValidate_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker bkw = (BackgroundWorker)sender;
            if (bkw.CancellationPending)
                return;
            progressBarX1.Value = e.ProgressPercentage;
            lblErrCount.Text = "" + (int)((int[])e.UserState)[0];
            lblWarningCount.Text = "" + (int)((int[])e.UserState)[1];
            if (linkLabel3.Visible == false && (int)((int[])e.UserState)[0] > 0 || (int)((int[])e.UserState)[1] > 0)
                linkLabel3.Visible = true;
        }

        private void _BKWValidate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bkw = (BackgroundWorker)sender;

            linkLabel3.Visible = false;
            if (!bkw.CancellationPending && !e.Cancelled)
            {
                linkLabel1.Visible = true;
                linkLabel1.Tag = ((object[])e.Result)[0];
                wizardPage3.FinishButtonEnabled = ("" + linkLabel3.Tag != "!" && (bool)((object[])e.Result)[1]) ? eWizardButtonState.True : eWizardButtonState.False;
                this.Tag = ((object[])e.Result)[2];
                wizard1.Tag = ((object[])e.Result)[3];
                if (wizardPage3.FinishButtonEnabled == eWizardButtonState.True)
                    labelX2.Text = "数据验证完成";
                else
                    labelX2.Text = "数据验证失败";
            }
            else
            {
                if (_ErrorWB != null)
                {
                    linkLabel1.Visible = true;
                    linkLabel1.Tag = _ErrorWB;
                    wizardPage3.FinishButtonEnabled = eWizardButtonState.False;
                    labelX2.Text = "数据验证失败";
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Workbook wb = new Workbook();
            wb.Copy((Workbook)((Control)sender).Tag);
            Completed(_Title + "_验证", wb);
        }

        private void Completed(string inputReportName, Workbook inputDoc)
        {
            string reportName = inputReportName;

            string path = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xls");

            Workbook doc = inputDoc;

            if (File.Exists(path))
            {
                int i = 1;
                while (true)
                {
                    string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                    if (!File.Exists(newPath))
                    {
                        path = newPath;
                        break;
                    }
                }
            }

            try
            {
                doc.Save(path);
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".xls";
                sd.Filter = "Excel档案 (*.xls)|*.xls|所有档案 (*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        doc.Save(sd.FileName);
                    }
                    catch
                    {
                        MsgBox.Show("指定路径无法存取。", "建立档案失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        private void wizardPage3_FinishButtonClick(object sender, CancelEventArgs e)
        {
            Dictionary<string, List<RowData>> ID_Rows = new Dictionary<string, List<RowData>>();
            #region 将资料依ID群组
            List<RowData> rows = (List<RowData>)this.Tag;
            foreach (RowData row in rows)
            {
                if (!ID_Rows.ContainsKey(row.ID))
                    ID_Rows.Add(row.ID, new List<RowData>());
                ID_Rows[row.ID].Add(row);
            }
            #endregion
            List<List<RowData>> packages = new List<List<RowData>>();
            #region 将数据分割成数个Package
            {
                List<RowData> package = null;
                int packageCount = 0;
                foreach (string id in ID_Rows.Keys)
                {
                    if (packageCount <= 0)
                    {
                        package = new List<RowData>();
                        packages.Add(package);
                        packageCount = _PackageLimint;
                    }
                    package.AddRange(ID_Rows[id]);
                    packageCount -= ID_Rows[id].Count;
                }
            }
            #endregion
            BackgroundWorker bkwImport = new BackgroundWorker();
            bkwImport.WorkerReportsProgress = true;
            bkwImport.DoWork += new DoWorkEventHandler(bkwImport_DoWork);
            bkwImport.ProgressChanged += new ProgressChangedEventHandler(bkwImport_ProgressChanged);
            bkwImport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bkwImport_RunWorkerCompleted);
            bkwImport.RunWorkerAsync(new object[] { packages, wizard1.Tag });
            this.Close();
        }

        void bkwImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                throw e.Error;
            else
            {
                //_Process.FinishImport();
                if (ImportComplete != null)
                    ImportComplete(this, new EventArgs());
                FISCA.Presentation.MotherForm.SetStatusBarMessage(_Title + " 完成", 100);
            }
        }

        void bkwImport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FISCA.Presentation.MotherForm.SetStatusBarMessage(_Title + "...", e.ProgressPercentage);
        }

        void bkwImport_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkw = (BackgroundWorker)sender;
            List<List<RowData>> packages = (List<List<RowData>>)((object[])e.Argument)[0];
            List<string> importFields = (List<string>)((object[])e.Argument)[1];
            //_Process.StartImport();
            if (ImportStart != null)
                ImportStart(this, new EventArgs());
            double totle = 0.0;
            double packageProgress = 100.0 / packages.Count;
            bkw.ReportProgress(1);
            foreach (List<RowData> package in packages)
            {
                //_Process.ImportPackage(package, importFields);
                if (ImportPackage != null)
                {
                    SmartSchool.API.PlugIn.Import.ImportPackageEventArgs args = new SmartSchool.API.PlugIn.Import.ImportPackageEventArgs();
                    args.ImportFields.AddRange(importFields);
                    args.Items.AddRange(package);
                    ImportPackage(this, args);
                }
                totle += packageProgress;
                bkw.ReportProgress((int)totle);
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            txtFile_TextChanged(null, null);
        }

        #region ImportWizard 成员

        public void AddError(SmartSchool.API.PlugIn.RowData rowData, string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AddWarning(SmartSchool.API.PlugIn.RowData rowData, string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        //public Control ControlPanel
        //{
        //    get
        //    {
        //        return advContainer.Control;
        //    }
        //    set
        //    {
        //        advContainer.Control = value;
        //        advButton.Enabled = ( advContainer.Control != null );
        //    }
        //}
        public SmartSchool.API.PlugIn.Collections.OptionCollection Options
        {
            get { return _Options; }
        }

        public event EventHandler ControlPanelClose;

        public event EventHandler ControlPanelOpen;

        public event EventHandler HelpButtonClick;

        public bool HelpButtonVisible
        {
            get
            {
                return helpButton.Visible;
            }
            set
            {
                helpButton.Visible = value;
            }
        }

        public event EventHandler ImportComplete;

        public event EventHandler<SmartSchool.API.PlugIn.Import.ImportPackageEventArgs> ImportPackage;

        public event EventHandler ImportStart;

        public SmartSchool.API.PlugIn.Collections.FieldsCollection ImportableFields
        {
            get { return _ImportableFields; }
        }

        public event EventHandler<SmartSchool.API.PlugIn.Import.LoadSourceEventArgs> LoadSource;

        public int PackageLimit
        {
            get
            {
                return _PackageLimint;
            }
            set
            {
                _PackageLimint = value;
            }
        }

        public SmartSchool.API.PlugIn.Collections.FieldsCollection RequiredFields
        {
            get { return _RequiredFields; }
        }

        public SmartSchool.API.PlugIn.Collections.FieldsCollection SelectedFields
        {
            get { return _SelectedFields; }
        }

        public event EventHandler ValidateComplete;

        public event EventHandler<SmartSchool.API.PlugIn.Import.ValidateRowEventArgs> ValidateRow;

        public event EventHandler<SmartSchool.API.PlugIn.Import.ValidateStartEventArgs> ValidateStart;

        #endregion

        private void wizard1_WizardPageChanged(object sender, WizardPageChangeEventArgs e)
        {

            advButton.Visible = (e.NewPage == wizardPage1);
        }

        private void chkTrim_CheckedChanged(object sender, EventArgs e)
        {
            _Trim = chkTrim.Checked;
            txtFile_TextChanged(null, null);
        }

        private string GetTrimText(string text)
        {
            return _Trim ? text.Trim() : text;
        }
    }
}

