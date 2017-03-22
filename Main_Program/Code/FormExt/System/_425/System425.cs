using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.System._425
{
    public class System425 : SwBaseForm
    {
        private StaticText qtyStaticText;
        private Matrix matrix;
        private EditText qtyEditText;
        private Column rowColumn, qtyColumn;
        private UserDataSource sumQtyUserDataSource;
        private Button nextButton;
        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            sumQtyUserDataSource = MyForm.DataSources.UserDataSources.Add("sumQty", BoDataType.dt_QUANTITY, 0);
            matrix = MyForm.Items.Item("3").Specific as Matrix;
            if (matrix != null)
            {

               

                rowColumn = matrix.Columns.Item("0");
                rowColumn.ClickAfter += rowColumn_ClickAfter;
                rowColumn.DoubleClickAfter += rowColumn_ClickAfter;
                qtyColumn = matrix.Columns.Item("5");
                qtyColumn.ValidateAfter += qtyColumn_ValidateAfter;

                qtyEditText = MyForm.Items.Add("Qty", BoFormItemTypes.it_EDIT).Specific as EditText;
                if (qtyEditText != null)
                {
                    qtyEditText.DataBind.SetBound(true, "", "sumQty");
                    qtyEditText.Item.FromPane = matrix.Item.FromPane;
                    qtyEditText.Item.ToPane = matrix.Item.ToPane;

                    qtyEditText.Item.Top = matrix.Item.Top + matrix.Item.Height + 10;
                    qtyEditText.Item.Left = matrix.Item.Left + 100;
                    qtyEditText.Item.Width = 120;
                    qtyEditText.Item.Height = 15;
                    qtyEditText.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1, BoModeVisualBehavior.mvb_False);

                }

                qtyStaticText = MyForm.Items.Add("lQty", BoFormItemTypes.it_STATIC).Specific as StaticText;
                if (qtyStaticText != null)
                {
                    qtyStaticText.Item.FromPane = matrix.Item.FromPane;
                    qtyStaticText.Caption = "数量总计";
                    qtyStaticText.Item.ToPane = matrix.Item.ToPane;
                    qtyStaticText.Item.LinkTo = "Qty";

                    qtyStaticText.Item.Top = matrix.Item.Top + matrix.Item.Height + 10;
                    qtyStaticText.Item.Left = matrix.Item.Left ;
                    qtyStaticText.Item.Width = 80;
                    qtyStaticText.Item.Height = 15;
                }
                nextButton = MyForm.Items.Item("43").Specific as Button;
               
                nextButton.PressedAfter+=nextButton_PressedAfter;
            }
            EventForm.ResizeAfter += eventForm_ResizeAfter;
        }

        private void nextButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            if (MyForm.PaneLevel == matrix.Item.FromPane && pVal.ActionSuccess)
            {
                var sum = 0.0;
                var selectRow = matrix.GetNextSelectedRow(0, BoOrderType.ot_SelectionOrder);
                while (selectRow > 0)
                {
                    var cell = matrix.Columns.Item("5").Cells.Item(selectRow).Specific as EditText;
                    if (cell != null)
                    {
                        var value = cell.Value.Trim();
                        if (!string.IsNullOrEmpty(value))
                        {
                            sum += double.Parse(value);
                        }
                    }
                    selectRow = matrix.GetNextSelectedRow(selectRow, BoOrderType.ot_SelectionOrder);
                }
                qtyEditText.Value = sum.ToString();
            }
        }
        public void rowColumn_ClickAfter(object sboObject, SBOItemEventArg pVal)
        {
                var sum = 0.0;
                var selectRow = matrix.GetNextSelectedRow(0, BoOrderType.ot_SelectionOrder);
                while (selectRow > 0)
                {
                    var cell = matrix.Columns.Item("5").Cells.Item(selectRow).Specific as EditText;
                    if (cell != null)
                    {
                        var value = cell.Value.Trim();
                        if (!string.IsNullOrEmpty(value))
                        {
                            sum += double.Parse(value);
                        }
                    }
                    selectRow = matrix.GetNextSelectedRow(selectRow, BoOrderType.ot_SelectionOrder);
                }
                qtyEditText.Value = sum.ToString();
        }

        public void qtyColumn_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            var row = pVal.Row;
            var selected = matrix.IsRowSelected(row);
            if (selected)
            {
                var sum = 0.0;
                var selectRow = matrix.GetNextSelectedRow(0, BoOrderType.ot_SelectionOrder);
                while (selectRow > 0)
                {
                    var cell = matrix.Columns.Item("5").Cells.Item(selectRow).Specific as EditText;
                    if (cell != null)
                    {
                        var value = cell.Value.Trim();
                        if (string.IsNullOrEmpty(value))
                        {
                            sum += double.Parse(value);
                        }
                    }
                    selectRow = matrix.GetNextSelectedRow(selectRow, BoOrderType.ot_SelectionOrder);
                }
                qtyEditText.Value = sum.ToString();
            }
        }

        public void eventForm_ResizeAfter(SBOItemEventArg pVal)
        {
            qtyStaticText.Item.Top = matrix.Item.Top + matrix.Item.Height + 5;
            qtyStaticText.Item.Left = matrix.Item.Left;
            qtyStaticText.Item.Height = 15;
            qtyStaticText.Item.Width = 80;

            qtyEditText.Item.Top = matrix.Item.Top + matrix.Item.Height + 5;
            qtyEditText.Item.Left = matrix.Item.Left+90;
            qtyEditText.Item.Height = 15;
            qtyEditText.Item.Width = 100;
        }
    }
}