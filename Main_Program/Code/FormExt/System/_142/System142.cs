using HuDongHeavyMachinery.Code.FormExt.Custom.COR020070;
using HuDongHeavyMachinery.Code.Util;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.System._142

{
    public class System142 : SwBaseForm
    {
       // private ButtonCombo buttonCombo;
        private ComboBox copyToBoxCombox;
        private Button loadButton;
        private DBDataSource OPOR, POR1;
        private Button reviewButton;
        private DataTable tmpTable;
        private Button viewButton;
        private UserDataSource QtySum;
        private StaticText QtyStatic;
        private EditText QtyEdit;
        private StaticText OwerLink;
        private EditText OwerCode;
        private Column QtyColumn;
        private Column ItemCodeColumn;
        private Matrix aMatrix;
        private Column iteNameColumn;
        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            copyToBoxCombox = (ComboBox) MyForm.Items.Item("10000329").Specific;
            copyToBoxCombox.ComboSelectAfter += copyToBoxCombox_ComboSelectAfter;

            //copyToBoxCombox.Item.FromPane = 10;
            //copyToBoxCombox.Item.ToPane = 10;

            //buttonCombo = (ButtonCombo) MyForm.Items.Add("inputB", BoFormItemTypes.it_BUTTON_COMBO).Specific;
            //buttonCombo.Item.Width = copyToBoxCombox.Item.Width;
            //buttonCombo.Item.Height = copyToBoxCombox.Item.Height;
            //buttonCombo.Item.AffectsFormMode = false;

            //for (var i = 0; i < copyToBoxCombox.ValidValues.Count; i++)
            //{
            //    var validValue = copyToBoxCombox.ValidValues.Item(i);
            //    buttonCombo.ValidValues.Add(validValue.Value, validValue.Description);
            //}
            //buttonCombo.ValidValues.Add("质检单", "COR020040");
            //buttonCombo.Caption = "复制到";
            //buttonCombo.ComboSelectAfter += buttonCombo_ComboSelectAfter;

            reviewButton = (Button) MyForm.Items.Add("review", BoFormItemTypes.it_BUTTON).Specific;
            reviewButton.Caption = "送检单查看";
            reviewButton.Item.Height = copyToBoxCombox.Item.Height;
            reviewButton.Item.Width = copyToBoxCombox.Item.Width;
            reviewButton.PressedAfter += reviewButton_PressedAfter;

            tmpTable = MyForm.DataSources.DataTables.Add("tmp");

            aMatrix = (Matrix)MyForm.Items.Item("38").Specific;
            iteNameColumn = aMatrix.Columns.Item("3");

            iteNameColumn.ChooseFromListAfter += ItemCodeColumn_ChooseFromListAfter;

            ItemCodeColumn = aMatrix.Columns.Item("1");

            ItemCodeColumn.ChooseFromListAfter += ItemCodeColumn_ChooseFromListAfter;

            QtyColumn = aMatrix.Columns.Item("11");
            QtyColumn.ValidateAfter += QtyColumn_ValidateAfter;


            OPOR = MyForm.DataSources.DBDataSources.Item("OPOR");
            POR1 = MyForm.DataSources.DBDataSources.Item("POR1");


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
                for (int i = 0; i < POR1.Size; i++)
                {
                    var value = POR1.GetValue("Quantity", i);
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
        private void reviewButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            //var sqlBuilder = new StringBuilder();
            //sqlBuilder.Append("SELECT distinct T0.\"DocEntry\", T0.\"DocNum\", T0.\"Status\", T0.\"CreateDate\" FROM \"@COR020040\"  T0 inner join \"@COR020041\"  T1 on T0.\"DocEntry\"=T1.\"DocEntry\" where T1.\"U_BaseEntry\"=" +
            //    OPOR.GetValue("DocEntry", 0));
            //tmpTable.ExecuteQuery(sqlBuilder.ToString());
            //if (!tmpTable.IsEmpty)
            //{
            //    const string formType = "COR020041";
            //    var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
            //    MySonUid = form.UniqueID;
            //    var swBaseForm = Globle.SwFormsList[form.UniqueID];
            //    swBaseForm.MyFatherUid = MyFormUid;
            //    ((COR020041) swBaseForm).SetInformation(tmpTable);
            //}

            var docEntry = OPOR.GetValue("DocEntry", 0).Trim();

            if (!string.IsNullOrEmpty(docEntry))
            {
                const string formType = "COR020072";
                var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
                MySonUid = form.UniqueID;
                var swBaseForm = Globle.SwFormsList[form.UniqueID];
                swBaseForm.MyFatherUid = MyFormUid;
                ((COR020072) swBaseForm).SetDataEventHandler(docEntry, this);
            }
        }

        private void buttonCombo_ComboSelectAfter(object sboObject, SBOItemEventArg pVal)
        {
            //var select = buttonCombo.Caption;
            //var flg = false;
            //if (copyToBoxCombox.Item.Enabled)
            //{
            //    for (var i = 0; i < copyToBoxCombox.ValidValues.Count; i++)
            //    {
            //        var validValue = copyToBoxCombox.ValidValues.Item(i);
            //        if (select == validValue.Value)
            //        {
            //            copyToBoxCombox.Select(select, BoSearchKey.psk_ByValue);
            //            flg = true;
            //            break;
            //        }
            //    }
            //    if (!flg)
            //    {
                    //if (select == "质检单")
                    //{
                    //    const string formType = "COR020040";

                    //    var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
                    //    MySonUid = form.UniqueID;
                    //    var swBaseForm = Globle.SwFormsList[form.UniqueID];
                    //    swBaseForm.MyFatherUid = MyFormUid;
                    //    ((COR020040) swBaseForm).SetInformation(OPOR, POR1);
            //        //}
            //    }
            //}
            //else
            //{
            //    Globle.Application.SetStatusBarMessage("此状态操作复制到" + select, BoMessageTime.bmt_Short, false);
            //}
        }

        private void copyToBoxCombox_ComboSelectAfter(object sboObject, SBOItemEventArg pVal)
        {

        }

        public override void ItemEventHandler(string formUid, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (pVal.BeforeAction)
            {
                if (MySonUid != null)
                {
                    var sonForm = Globle.Application.Forms.Item(MySonUid);
                    sonForm.Select();
                    bubbleEvent = false;
                }
            }
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                FormResize();
            }
        }

        private void FormResize()
        {
            reviewButton.Item.Top = copyToBoxCombox.Item.Top;
            reviewButton.Item.Left = copyToBoxCombox.Item.Left - copyToBoxCombox.Item.Width*2 - 8;
            //buttonCombo.Item.Top = copyToBoxCombox.Item.Top;
            //buttonCombo.Item.Left = copyToBoxCombox.Item.Left;
        }
    }
}