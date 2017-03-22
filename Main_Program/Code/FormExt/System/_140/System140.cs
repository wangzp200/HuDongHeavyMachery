using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.System._140
{
    public class System140 : SwBaseForm
    {
        private Matrix aMatrix;
        private DBDataSource ODLN, DLN1;
        private UserDataSource QtySum;
        private StaticText QtyStatic;
        private EditText QtyEdit;
        private StaticText OwerLink;
        private EditText OwerCode;
        private Column QtyColumn;
        private Column ItemCodeColumn;
        private Column iteNameColumn;
        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            ODLN = MyForm.DataSources.DBDataSources.Item("ODLN");
            DLN1 = MyForm.DataSources.DBDataSources.Item("DLN1");
            aMatrix = (Matrix)MyForm.Items.Item("38").Specific;
            iteNameColumn = aMatrix.Columns.Item("3");

         

            iteNameColumn.ChooseFromListAfter += ItemCodeColumn_ChooseFromListAfter;

            ItemCodeColumn = aMatrix.Columns.Item("1");

            ItemCodeColumn.ChooseFromListAfter += ItemCodeColumn_ChooseFromListAfter;

            QtyColumn = aMatrix.Columns.Item("11");
            QtyColumn.ValidateAfter += QtyColumn_ValidateAfter;



            OwerLink = MyForm.Items.Item("230").Specific as StaticText;
            OwerCode = MyForm.Items.Item("222").Specific as EditText;


            QtySum = MyForm.DataSources.UserDataSources.Add("Sum", BoDataType.dt_QUANTITY, 254);

            QtyEdit = MyForm.Items.Add("Qty", BoFormItemTypes.it_EDIT).Specific as EditText;
            QtyEdit.DataBind.SetBound(true, "", "Sum");
            QtyEdit.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1, BoModeVisualBehavior.mvb_False);

            QtyStatic = MyForm.Items.Add("LinkQty", BoFormItemTypes.it_STATIC).Specific as StaticText;
            QtyStatic.Item.LinkTo = "Qty";
            QtyStatic.Caption = "数量总计";



            QtyStatic.Item.Left = OwerLink.Item.Left;
            QtyStatic.Item.Top = OwerLink.Item.Top + 16;
            QtyStatic.Item.Width = OwerLink.Item.Width;
            QtyStatic.Item.Height = OwerLink.Item.Height;

            QtyEdit.Item.Left = OwerCode.Item.Left;
            QtyEdit.Item.Top = OwerCode.Item.Top + 16;
            QtyEdit.Item.Width = OwerCode.Item.Width;
            QtyEdit.Item.Height = OwerCode.Item.Height;
        }
        public override void MenuEventHandler(ref MenuEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && (pVal.MenuUID == "1284" || pVal.MenuUID == "1286" || pVal.MenuUID == "1292" || pVal.MenuUID == "1293" || pVal.MenuUID == "1294"))
            {
                var sum = 0.0;
                for (int i = 1; i <= aMatrix.RowCount; i++)
                {
                    var value = (aMatrix.Columns.Item("11").Cells.Item(i).Specific as EditText).Value.Trim();
                    if (!string.IsNullOrEmpty(value))
                    {
                        sum += double.Parse(value);
                    }
                }
                if (sum > 0.0)
                {
                    QtySum.Value = sum.ToString();
                }
            }
        }
        private void ItemCodeColumn_ChooseFromListAfter(object sboObject, SBOItemEventArg pVal)
        {
            var sum = 1.0;
            for (int i = 1; i <= aMatrix.RowCount; i++)
            {
                var value = (aMatrix.Columns.Item("11").Cells.Item(i).Specific as EditText).Value.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    sum += double.Parse(value);
                }
            }
            if (sum > 0.0)
            {
                QtySum.Value = sum.ToString();
            }
        }

        private void QtyColumn_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            if (pVal.ItemChanged)
            {
                var sum = 0.0;
                for (int i = 1; i <= aMatrix.VisualRowCount; i++)
                {
                    var value = (aMatrix.Columns.Item("11").Cells.Item(i).Specific as EditText).Value.Trim();
                    if (!string.IsNullOrEmpty(value))
                    {
                        sum += double.Parse(value);
                    }
                }
                if (sum > 0.0)
                {
                    QtySum.Value = sum.ToString();
                }
            }
        }
        public override void FormDataLoad(ref BusinessObjectInfo businessobjectinfo, ref bool bubbleevent)
        {
            if (!businessobjectinfo.BeforeAction && businessobjectinfo.ActionSuccess)
            {
                var sum = 0.0;
                for (int i = 0; i < DLN1.Size; i++)
                {
                    var value = DLN1.GetValue("Quantity", i);
                    if (!string.IsNullOrEmpty(value))
                    {
                        sum += double.Parse(value);
                    }
                }
                if (sum > 0.0)
                {
                    QtySum.Value = sum.ToString();
                }
            }

        }
    }
}