using System.Text;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020070
{
    public class COR020072 : SwBaseForm
    {
        private Grid grid;
        private Item rectTem;
        private DataTable tmpTable;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            rectTem = MyForm.Items.Item("7");
            grid = (Grid) MyForm.Items.Item("6").Specific;
            tmpTable = MyForm.DataSources.DataTables.Add("tmp");
            Formresize();
        }

        public override void ItemEventHandler(string formUid, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                Formresize();
            }
            if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                var swBaseForm = Globle.SwFormsList[MyFatherUid];
                swBaseForm.MySonUid = null;
            }
        }

        private void Formresize()
        {
            rectTem.Width = grid.Item.Width + 8;
            rectTem.Height = grid.Item.Height + 8;
        }

        public override void SetDataEventHandler(object obj, SwBaseForm swBaseForm)
        {
            var baseEntry = (string) obj;
            if (!string.IsNullOrEmpty(baseEntry))
            {
                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append(
                    "select T1.\"DocEntry\",T1.\"DocNum\",T1.\"CreateDate\",T1.\"U_Comments\",(case when T1.\"Status\"='C' then '已清' else  '未清' end) as \"Status\" from \"@COR020070\" T1 where exists(select \"DocEntry\" from \"@COR020071\" T2 where T1.\"DocEntry\"=T2.\"DocEntry\" and T2.\"U_BaseType\"=22 and T2.\"U_BaseEntry\"=")
                    .Append(baseEntry)
                    .Append(")");
                tmpTable.ExecuteQuery(sqlBuilder.ToString());
                grid.DataTable = tmpTable;
                var column = grid.Columns.Item("DocEntry");
                column.TitleObject.Caption = "送检单链接";
                column.TitleObject.Sortable = true;

                var editTextColumn = (EditTextColumn) column;
                editTextColumn.LinkedObjectType = "COR020070";


                column = grid.Columns.Item("DocNum");
                column.TitleObject.Caption = "送检单编号";
                column.TitleObject.Sortable = true;

                column = grid.Columns.Item("CreateDate");
                column.TitleObject.Caption = "日期";
                column.TitleObject.Sortable = true;

                column = grid.Columns.Item("U_Comments");
                column.TitleObject.Caption = "备注";
                column.TitleObject.Sortable = true;

                column = grid.Columns.Item("Status");
                column.TitleObject.Caption = "状态";
                column.TitleObject.Sortable = true;
                
                for (var i = 0; i < grid.Rows.Count; i++)
                {
                    grid.RowHeaders.SetText(i, ( i + 1) + "");
                }
                grid.RowHeaders.TitleObject.Caption = "#";
                MyForm.Update();
            }
        }
    }
}