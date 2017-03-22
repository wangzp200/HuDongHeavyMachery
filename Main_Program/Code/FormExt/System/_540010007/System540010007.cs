using System;
using System.Collections.Generic;
using System.Text;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.System._540010007
{
    internal class System540010007 : SwBaseForm
    {
        private readonly List<RowInfo> rowInfos = new List<RowInfo>();
        private bool auto;
        private Column cardCodeColumn;
        private string conditionsXml;
        private Column itemCodeColumn;
        private Matrix matrix1, matrix2;
        private bool needreckon = true;
        private int paneLevel;
        private Column priceColumn;
        private Column QuantityColumn;
        private CheckBox saveDraftBox;
        private ComboBox targetBox;
        private DataTable tmpTable;
        private Button wizNextButton, wizBackButton, cancleButton, reckonButton;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            matrix1 = (Matrix) MyForm.Items.Item("540000019").Specific;
            matrix2 = (Matrix) MyForm.Items.Item("540000012").Specific;
            tmpTable = MyForm.DataSources.DataTables.Add("tmp");
            cardCodeColumn = matrix1.Columns.Item("540000001");
            cardCodeColumn.ClickAfter += cardCodeColumn_ClickAfter;
            itemCodeColumn = matrix1.Columns.Item("540000005");
            priceColumn = matrix1.Columns.Item("540000021");


            QuantityColumn = matrix1.Columns.Item("540000017");
            QuantityColumn.ValidateAfter += QuantityColumn_ValidateAfter;


            targetBox = (ComboBox) MyForm.Items.Item("540000014").Specific;
            targetBox.ComboSelectBefore += targetBox_ComboSelectBefore;
            saveDraftBox = (CheckBox) MyForm.Items.Item("540000020").Specific;
            saveDraftBox.ClickBefore += saveDraftBox_ClickAfter;
            wizNextButton = (Button) MyForm.Items.Item("_wiz_next_").Specific;
            wizNextButton.ClickBefore += wizNextButton_ClickBefore;
            wizNextButton.PressedAfter += (wizNextButton_PressedAfter);


            wizBackButton = (Button) MyForm.Items.Item("_wiz_back_").Specific;
            wizBackButton.PressedAfter += wizBackButton_PressedAfter;


            var owerChooseFromList = MyForm.ChooseFromLists.Item(cardCodeColumn.ChooseFromListUID);
            var conditions = owerChooseFromList.GetConditions();
            conditionsXml = conditions.GetAsXML();
            cancleButton = (Button) MyForm.Items.Item("540000002").Specific;
            reckonButton = (Button) MyForm.Items.Add("reckon", BoFormItemTypes.it_BUTTON).Specific;


            reckonButton.PressedAfter += reckonButton_PressedAfter;
            reckonButton.Item.Height = cancleButton.Item.Height;
            reckonButton.Item.Width = cancleButton.Item.Width;
            reckonButton.Item.FromPane = matrix1.Item.FromPane;
            reckonButton.Item.ToPane = matrix1.Item.ToPane;
            reckonButton.Caption = "计算";
            reckonButton.Item.Top = cancleButton.Item.Top;
            reckonButton.Item.Left = cancleButton.Item.Left - cancleButton.Item.Width - 20;
            reckonButton.Item.LinkTo = "540000002";
           

        }

        private void QuantityColumn_ValidateAfter(object sboobject, SBOItemEventArg pval)
        {
            if (pval.ItemChanged && !auto)
            {
                var row = pval.Row;

                foreach (var rowInfo in rowInfos)
                {
                    if (rowInfo.LineNum == row)
                    {
                        var qty =
                            double.Parse(((EditText) (matrix1.Columns.Item("540000017").Cells.Item(row).Specific)).Value);
                        rowInfo.quantity = qty;
                        break;
                    }
                }
            }
        }

        private void wizBackButton_PressedAfter(object sboobject, SBOItemEventArg pval)
        {
            if (MyForm.PaneLevel == matrix2.Item.ToPane)
            {
                needreckon = true;
            }
        }

        private void reckonButton_PressedAfter(object sboobject, SBOItemEventArg pval)
        {
            if (!needreckon)
            {
                return;
            }

            if (rowInfos.Count > 0)
            {
                var paramsBuilder = new StringBuilder("(");
                for (var i = 0; i < rowInfos.Count; i++)
                {
                    var has = false;
                    for (var j = 0; j < i - 1; j++)
                    {
                        if (rowInfos[i].itemCode == rowInfos[j].itemCode)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (!has)
                    {
                        paramsBuilder.Append("'").Append(rowInfos[i].itemCode).Append("'").Append(",");
                    }
                }
                paramsBuilder = paramsBuilder.Remove(paramsBuilder.Length - 1, 1).Append(")");

                var sql = "select distinct \"ItemCode\",\"InvntItem\",\"OnHand\" from OITM where \"ItemCode\" in " +
                          paramsBuilder;

                tmpTable.ExecuteQuery(sql);
                if (!tmpTable.IsEmpty)
                {
                    var itemInfos = new List<ItemInfo>();
                    for (var i = 0; i < tmpTable.Rows.Count; i++)
                    {
                        var itemInfo = new ItemInfo
                        {
                            ItemCode = tmpTable.GetValue("ItemCode", i).ToString(),
                            InvntItem = tmpTable.GetValue("InvntItem", i).ToString(),
                            OnHand = double.Parse(tmpTable.GetValue("OnHand", i).ToString())
                        };
                        itemInfos.Add(itemInfo);
                    }
                    foreach (var rowInfo in rowInfos)
                    {
                        foreach (var itemInfo in itemInfos)
                        {
                            if (rowInfo.itemCode == itemInfo.ItemCode)
                            {
                                if (itemInfo.InvntItem != "Y")
                                {
                                    rowInfo.needQty = 0;
                                }
                                else
                                {
                                    if (rowInfo.quantity <=rowInfo.bookQty)
                                    {
                                        rowInfo.needQty = 0;
                                    }
                                    else
                                    {
                                        rowInfo.needQty = rowInfo.quantity - rowInfo.bookQty;
                                    }
                                }
                                if (itemInfo.OnHand <= 0)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    var columnNumber = 13;

                    try
                    {
                        auto = true;
                        for (var i = 0; i < rowInfos.Count; i++)
                        {
                            var info = rowInfos[i];
                            //var cardCode = ((EditText)(matrix1.Columns.Item("540000001").Cells.Item(i + 1).Specific)).Value.Trim();
                            //if (!string.IsNullOrEmpty(cardCode))
                            //{
                            //var qty = info.quantity - info.lockQty;
                            //if (qty < 0)
                            //{
                            //    qty = 0.0;
                            //}
                            if (matrix1.CommonSetting.GetCellEditable(i + 1, columnNumber))
                            {
                                ((EditText) (matrix1.Columns.Item("540000017").Cells.Item(i + 1).Specific)).Value =
                                    info.needQty + "";
                                if (info.needQty > 0.0)
                                {
                                    //matrix1.SelectRow(i + 1,true,true);
                                }
                            }
                        }
                        //}
                    }
                    catch (Exception exception)
                    {
                        Globle.Application.SetStatusBarMessage(exception.Message,
                            BoMessageTime.bmt_Short);
                    }
                    finally
                    {
                        auto = false;
                    }
                }
            }
        }

        public override void ItemEventHandler(string formUid, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                FormreSize();
            }
        }

        private void FormreSize()
        {
            reckonButton.Item.Top = cancleButton.Item.Top;
            reckonButton.Item.Left = cancleButton.Item.Left - cancleButton.Item.Width - 8;
        }

        private void wizNextButton_PressedAfter(object sboobject, SBOItemEventArg pval)
        {
            if (paneLevel != matrix1.Item.FromPane && MyForm.PaneLevel == matrix1.Item.FromPane &&
                paneLevel == matrix2.Item.FromPane)
            {
                //if (!saveDraftBox.Checked)
                //{
                //    saveDraftBox.Item.Click();
                //}
                if (targetBox.Value.Trim() != "1470000113")
                {
                    var validValues = targetBox.ValidValues;
                    for (var i = 0; i < validValues.Count; i++)
                    {
                        var validValue = validValues.Item(i);
                        if (validValue.Value == "1470000113")
                        {
                            targetBox.Select("1470000113");
                            break;
                        }
                    }
                }

                rowInfos.Clear();
                for (var i = 1; i <= matrix1.VisualRowCount; i++)
                {
                    var quantity =
                        double.Parse(
                            ((EditText) (matrix1.Columns.Item("540000017").Cells.Item(i).Specific)).Value.Trim());
                    var bookQty = 0.0;
                    if (!string.IsNullOrEmpty(((EditText)(matrix1.Columns.Item("U_BookQty").Cells.Item(i).Specific)).Value.Trim()))
                    {
                      bookQty=  double.Parse(((EditText)(matrix1.Columns.Item("U_BookQty").Cells.Item(i).Specific)).Value.Trim());
                    }
                   

                    var itemCode = ((EditText) (matrix1.Columns.Item("540000005").Cells.Item(i).Specific)).Value.Trim();
                    var cMyClass = new RowInfo
                    {
                        quantity = quantity,
                        bookQty = bookQty,
                        itemCode = itemCode,
                        LineNum = i
                    };
                    rowInfos.Add(cMyClass);
                }
            }
        }

        private void saveDraftBox_ClickAfter(object sboobject, SBOItemEventArg pval, out bool bubbleevent)
        {
            bubbleevent = true;
            if (saveDraftBox.Checked)
            {
                Globle.Application.SetStatusBarMessage("次控件已经锁定，不可更改");
                bubbleevent = false;
            }
        }

        private void targetBox_ComboSelectBefore(object sboobject, SBOItemEventArg pval, out bool bubbleevent)
        {
            bubbleevent = true;
            //if (targetBox.Value.Trim() == "22")
            //{
            //    Globle.Application.SetStatusBarMessage("次控件已经锁定，不可更改");
            //    bubbleevent = false;
            //}
        }

        private void wizNextButton_ClickBefore(object sboobject, SBOItemEventArg pval, out bool bubbleevent)
        {
            paneLevel = MyForm.PaneLevel;
            //if (MyForm.PaneLevel == matrix1.Item.FromPane)
            //{
            //    var messageBuilder = new StringBuilder();
            //    var selectRow = matrix1.GetNextSelectedRow();
            //    while (selectRow > 0)
            //    {
            //        var price = ((EditText) priceColumn.Cells.Item(selectRow).Specific).Value.Trim();
            //        if (string.IsNullOrEmpty(price))
            //        {
            //            messageBuilder.Append(selectRow).Append("、");
            //        }
            //        selectRow = matrix1.GetNextSelectedRow(selectRow);
            //    }
            //    var message = messageBuilder.ToString();
            //    if (!string.IsNullOrEmpty(message))
            //    {
            //        message.Remove(message.Length - 1);
            //        message = "第 " + message + " 行价格为空，不可进行下一步操作";
            //        Globle.Application.SetStatusBarMessage(message, BoMessageTime.bmt_Short);
            //        Globle.Application.MessageBox(message);
            //        bubbleevent = false;
            //        return;
            //    }

            //    if (needreckon)
            //    {
            //        needreckon = false;
            //        var result = Globle.Application.MessageBox("未进行数量计算,是否继续？", 2, "确定", "取消");
            //        if (result == 2)
            //        {
            //            bubbleevent = false;
            //            return;
            //        }
            //    }
            //}
            bubbleevent = true;
        }

        private void cardCodeColumn_ClickAfter(object sboobject, SBOItemEventArg pval)
        {
            var owerChooseFromList = MyForm.ChooseFromLists.Item(cardCodeColumn.ChooseFromListUID);
            var conditions = owerChooseFromList.GetConditions();
            conditions.LoadFromXML(conditionsXml);

            var itemCode = ((EditText) itemCodeColumn.Cells.Item(pval.Row).Specific).Value.Trim();
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select \"VendorCode\" from ITM2 where \"ItemCode\"='").Append(itemCode).Append("'");
            tmpTable.ExecuteQuery(sqlBuilder.ToString());
            if (!tmpTable.IsEmpty)
            {
                if (conditions.Count > 0)
                {
                    conditions.Item(conditions.Count - 1).Relationship = BoConditionRelationship.cr_AND;
                }
                for (var i = 0; i < tmpTable.Rows.Count; i++)
                {
                    var vendorCode = tmpTable.GetValue("VendorCode", i).ToString();
                    var condition = conditions.Add();
                    if (0 == i && tmpTable.Rows.Count > 1)
                    {
                        condition.BracketOpenNum = 2;
                    }
                    else
                    {
                        condition.BracketOpenNum = 1;
                    }
                    condition.Alias = "CardCode";
                    condition.Operation = BoConditionOperation.co_EQUAL;
                    condition.CondVal = vendorCode;

                    if (0 == i && tmpTable.Rows.Count > 1)
                    {
                        condition.Relationship = BoConditionRelationship.cr_OR;
                    }
                    else if (i < tmpTable.Rows.Count - 1)
                    {
                        condition.Relationship = BoConditionRelationship.cr_OR;
                    }


                    if (tmpTable.Rows.Count - 1 == i && tmpTable.Rows.Count > 1)
                    {
                        condition.BracketCloseNum = 2;
                    }
                    else
                    {
                        condition.BracketCloseNum = 1;
                    }
                }
            }
            owerChooseFromList.SetConditions(conditions);
        }

        private class RowInfo
        {
            public double quantity { get; set; }
            //public double lockQty { get; set; }
            public string itemCode { get; set; }
            public double needQty { get; set; }
            public int LineNum { get; set; }

            public double bookQty { get; set; }
        }

        private class ItemInfo
        {
            public string ItemCode { get; set; }
            public string InvntItem { get; set; }
            public double OnHand { get; set; }
        }
    }
}