using System;
using System.Collections.Generic;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020040
{
    public class COR020041 : SwBaseForm
    {
        private readonly List<RowInfo> myRowInfos = new List<RowInfo>();
        private int currentPage;
        private Button dowmButton;
        private DataTable dt;
        private Grid grid;
        private Button noButton;
        private Button okButton;
        private int pageNum = 40;
        private Item rectangle;
        private StaticText showText;
        private int totalPages;
        private Button upButton;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            okButton = (Button) MyForm.Items.Item("5").Specific;
            okButton.PressedAfter += okButton_PressedAfter;
            noButton = (Button) MyForm.Items.Item("6").Specific;
            noButton.PressedAfter += noButton_PressedAfter;
            grid = (Grid) MyForm.Items.Item("7").Specific;
            rectangle = MyForm.Items.Item("9");
            showText = (StaticText) MyForm.Items.Item("8").Specific;

            upButton = (Button) MyForm.Items.Item("1000001").Specific;
            upButton.PressedAfter += upButton_PressedAfter;
            dowmButton = (Button) MyForm.Items.Item("1000002").Specific;
            dowmButton.PressedAfter += dowmButton_PressedAfter;
            dt = MyForm.DataSources.DataTables.Add("dt");
            dt.Columns.Add("DocEntry", BoFieldsType.ft_Integer);
            dt.Columns.Add("DocNum", BoFieldsType.ft_Integer);
            dt.Columns.Add("CreateDate", BoFieldsType.ft_Date);
            dt.Columns.Add("Status", BoFieldsType.ft_AlphaNumeric);
        }

        private void dowmButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            currentPage = currentPage + 1;
            SetGridInfo(currentPage);
        }

        private void upButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            currentPage = currentPage - 1;
            SetGridInfo(currentPage);
        }

        /// <summary>
        ///     初始化本窗体的参数
        /// </summary>
        /// <param name="obj"></param>
        public void SetInformation(object obj)
        {
            var dataTable = (DataTable) obj;
            totalPages = (dataTable.Rows.Count/pageNum) + (dataTable.Rows.Count%pageNum > 0 ? 1 : 0);

            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var value = "";
                var rowInfo = new RowInfo();
                rowInfo.DocEntry = int.Parse(dataTable.GetValue("DocEntry", i).ToString());
                rowInfo.DocNum = int.Parse(dataTable.GetValue("DocNum", i).ToString());
                rowInfo.CreateDate = DateTime.Parse(dataTable.GetValue("CreateDate", i).ToString());
                rowInfo.Status = dataTable.GetValue("Status", i).ToString();
                myRowInfos.Add(rowInfo);
            }
            currentPage = 0;
            SetGridInfo(currentPage);
        }

        /// <summary>
        ///     设置Grid的值
        /// </summary>
        /// <param name="page"></param>
        private void SetGridInfo(int page)
        {
            try
            {
                MyForm.Freeze(true);


                if (currentPage == 0)
                {
                    upButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_False);
                }
                else
                {
                    upButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_True);
                }
                if (currentPage + 1 >= totalPages)
                {
                    dowmButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_False);
                }
                else
                {
                    dowmButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_True);
                }

                dt.Rows.Clear();
                var row = 0;
                for (var i = page*pageNum;
                    i < ((page + 1)*pageNum > myRowInfos.Count ? myRowInfos.Count : (page + 1)*pageNum);
                    i++)
                {
                    var excelRowInfo = myRowInfos[i];
                    dt.Rows.Add(1);

                    dt.SetValue("DocEntry", row, excelRowInfo.DocEntry);
                    dt.SetValue("DocNum", row, excelRowInfo.DocNum);
                    dt.SetValue("CreateDate", row, excelRowInfo.CreateDate);
                    dt.SetValue("Status", row, excelRowInfo.Status);
                    row ++;
                }
                grid.DataTable = dt;
                grid.RowHeaders.TitleObject.Caption = "#";
                for (var i = 0; i < grid.Rows.Count; i++)
                {
                    grid.RowHeaders.SetText(i, (page*pageNum + i + 1) + "");
                }

                var editTextColumn = (EditTextColumn)grid.Columns.Item("DocEntry");
                editTextColumn.LinkedObjectType = "COR020040";
                editTextColumn.TitleObject.Caption = "质检单序号";
                editTextColumn.TitleObject.Sortable=true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("DocNum");
                editTextColumn.TitleObject.Caption = "质检单编号";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("CreateDate");
                editTextColumn.TitleObject.Caption = "创建日期";
                editTextColumn.TitleObject.Sortable = true;

                var column =grid.Columns.Item("Status");
                column.Type= BoGridColumnType.gct_ComboBox;
                column.TitleObject.Caption = "状态";
                column.TitleObject.Sortable = true;

                var comboBoxColumn = (ComboBoxColumn)column;
                comboBoxColumn.DisplayType = BoComboDisplayType.cdt_Description;
                var validValues = comboBoxColumn.ValidValues;

                validValues.Add("O", "未清");
                validValues.Add("C", "已清");

                for (var i = 0; i < grid.Columns.Count; i++)
                {
                    grid.Columns.Item(i).Editable = false;
                }

                showText.Caption = (page + 1) + "/" + totalPages;
                MyForm.Resize(MyForm.Width, MyForm.Height);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                MyForm.Freeze(false);
            }
        }

        private void noButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            Globle.SwFormsList[MyFatherUid].MySonUid = null;
            MyForm.Close();
        }

        private void okButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            var swBase = Globle.SwFormsList[MyFatherUid];
            swBase.SonFormCloseEventHandler(myRowInfos, this);
        }

        public override void ItemEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                rectangle.Width = grid.Item.Width + 8;
                rectangle.Height = grid.Item.Height + 10;
            }
            if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                Globle.SwFormsList[MyFatherUid].MySonUid = null;
            }
        }

        private class RowInfo
        {
            public int DocEntry { get; set; }

            public int DocNum { get; set; }

            public DateTime CreateDate { get; set; }

            public string Status { get; set; }
        }
    }
}