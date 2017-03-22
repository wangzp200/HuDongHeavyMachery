using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020070
{
    public class COR020071 : SwBaseForm
    {
        private readonly List<int> paramList = new List<int>();
        private ComboBox combo;
        private int docFounded;
        private DataTable docTable, docdTable, tmpTable;
        private XmlDocument docXmlData;
        private Folder folder1, folder2;
        private Matrix matrix1, matrix2, ioTmpMtx;
        private CheckBox mutCheckBox;
        private Button okButton, cancleButton;
        private Item rectTem;
        private EditText searchText;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            mutCheckBox = (CheckBox) MyForm.Items.Item("9").Specific;
            docTable = MyForm.DataSources.DataTables.Item("DOC");
            docdTable = MyForm.DataSources.DataTables.Item("Dtc");
            tmpTable = MyForm.DataSources.DataTables.Add("tmp");
            matrix1 = (Matrix) MyForm.Items.Item("10").Specific;
            matrix1.SelectionMode = BoMatrixSelect.ms_Auto;

            matrix2 = (Matrix) MyForm.Items.Item("11").Specific;
            matrix2.SelectionMode = BoMatrixSelect.ms_Auto;
            folder1 = (Folder) MyForm.Items.Item("3").Specific;
            folder1.ClickBefore += folder1_ClickBefore;
            folder1.ClickAfter += folder1_ClickAfter;
            folder2 = (Folder) MyForm.Items.Item("4").Specific;
            folder2.ClickBefore += folder2_ClickBefore;
            folder2.ClickAfter += folder2_ClickAfter;
            rectTem = MyForm.Items.Item("5");
            combo = (ComboBox) MyForm.Items.Item("8").Specific;
            searchText = (EditText) MyForm.Items.Item("7").Specific;
            searchText.KeyDownAfter += searchText_KeyDownAfter;
            okButton = (Button) MyForm.Items.Item("100").Specific;
            okButton.PressedAfter += okButton_PressedAfter;
            cancleButton = (Button) MyForm.Items.Item("200").Specific;
            cancleButton.PressedAfter += cancleButton_PressedAfter;
            docXmlData = new XmlDocument();

            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(
                "SELECT 0 AS \"RowId\",T1.\"DocEntry\",T1.\"DocNum\",T1.\"ObjType\",T1.\"CardCode\",T1.\"CardName\",T1.\"TaxDate\",T3.\"SlpName\" FROM dummy T12,OPOR T1 LEFT JOIN OSLP T3 ON T3.\"SlpCode\" = T1.\"SlpCode\" WHERE EXISTS(SELECT T2.\"DocEntry\" FROM POR1 T2 WHERE T1.\"DocEntry\"=T2.\"DocEntry\" AND T2.\"LineStatus\"='O' AND T2.\"Quantity\">ifnull(T2.\"U_CQty\",0.0)+ifnull(T2.\"U_QuaQty\",0.0)) AND T1.\"DocStatus\"='O' AND T1.\"CANCELED\"='N'");
            tmpTable.ExecuteQuery(sqlBuilder.ToString());
            if (!tmpTable.IsEmpty)
            {
                docTable.Rows.Clear();
                for (var i = 0; i < tmpTable.Rows.Count; i++)
                {
                    docTable.Rows.Add();
                    var rowIndex = docTable.Rows.Count - 1;
                    for (var j = 0; j < tmpTable.Columns.Count; j++)
                    {
                        var column = tmpTable.Columns.Item(j);
                        if (column.Name != "RowId")
                        {
                            docTable.SetValue(column.Name, rowIndex, tmpTable.GetValue(column.Name, i));
                        }
                    }
                    docTable.SetValue("RowId", rowIndex, (rowIndex + 1));
                }
                try
                {
                    MyForm.Freeze(true);
                    matrix1.LoadFromDataSource();
                    matrix1.AutoResizeColumns();
                    docXmlData.LoadXml(matrix1.SerializeAsXML(BoMatrixXmlSelect.mxs_All));
                }
                catch (Exception exception)
                {
                    Globle.Application.SetStatusBarMessage(exception.Message, BoMessageTime.bmt_Short);
                }
                finally
                {
                    MyForm.Freeze(false);
                }
            }
            folder1.Item.Click();
        }

        private void cancleButton_PressedAfter(object sboobject, SBOItemEventArg pval)
        {
            MyForm.Close();
        }

        private void okButton_PressedAfter(object sboobject, SBOItemEventArg pval)
        {
            if (MyForm.PaneLevel==2)
            {
                var lsXmlString = docdTable.SerializeAsXML(BoDataTableXmlSelect.dxs_DataOnly);
                tmpTable.LoadSerializedXML(BoDataTableXmlSelect.dxs_DataOnly, lsXmlString);
                tmpTable.Rows.Clear();

                var selectIndex = matrix2.GetNextSelectedRow();
                while (selectIndex > 0)
                {
                    tmpTable.Rows.Add();
                    var rowIndex = tmpTable.Rows.Count - 1;
                    for (var j = 0; j < docdTable.Columns.Count; j++)
                    {
                        var column = docdTable.Columns.Item(j);
                        tmpTable.SetValue(column.Name, rowIndex, docdTable.GetValue(column.Name, selectIndex - 1));
                    }

                    selectIndex = matrix2.GetNextSelectedRow(selectIndex);
                }
            }
            else
            {
                var selectRow = matrix1.GetNextSelectedRow();
                matrix1.FlushToDataSource();
                if (selectRow > 0)
                {
                    var paBuilder = new StringBuilder("(");
                    while (selectRow > 0)
                    {
                        var docEntry = docTable.GetValue("DocEntry", selectRow - 1).ToString().Trim();
                        paBuilder.Append(docEntry).Append(",");
                        selectRow = matrix1.GetNextSelectedRow(selectRow);
                    }
                    var paramss = paBuilder.ToString();
                    paramss = paramss.Substring(0, paramss.Length - 1);
                    paramss = paramss + ")";

                    var sqlBuilder = new StringBuilder();
                    sqlBuilder.Append(
                        "SELECT 1 as \"RowId\",T0.\"DocEntry\",T0.\"DocNum\",T0.\"ObjType\",T0.\"CardCode\",T0.\"CardName\",T0.\"TaxDate\",T1.\"U_SalesOrderNo\",T1.\"LineNum\",T1.\"ItemCode\",T1.\"Dscription\",T1.\"Quantity\"-ifnull(T1.\"U_CQty\",0)-ifnull(T1.\"U_QuaQty\",0) as \"Quantity\",T1.\"U_QuaQty\",T1.\"U_UNQuaQty\",T2.\"SlpName\",T3.\"U_ArmyCheck\" FROM dummy T12,OPOR T0 INNER JOIN POR1 T1 ON	T0.\"DocEntry\"=T1.\"DocEntry\" AND T1.\"Quantity\">ifnull(T1.\"U_CQty\",0.0)+ifnull(T1.\"U_QuaQty\",0.0) AND T1.\"LineStatus\"='O' INNER JOIN OSLP T2 ON T2.\"SlpCode\" = T1.\"SlpCode\" INNER JOIN OITM T3 ON T1.\"ItemCode\"=T3.\"ItemCode\" WHERE T0.\"DocStatus\"='O' AND T0.\"CANCELED\"='N' and T0.\"DocEntry\" in ");
                    sqlBuilder.Append(paramss);
                    tmpTable.ExecuteQuery(sqlBuilder.ToString());
                }
            }


            var swBaseForm = Globle.SwFormsList[MyFatherUid];


            swBaseForm.SonFormCloseEventHandler(tmpTable, this);
        }

        private void searchText_KeyDownAfter(object sboobject, SBOItemEventArg pval)
        {
            if (pval.CharPressed == 13)
            {
                var found = 0;
                var colUid = combo.Value.Trim();
                var findVlaue = searchText.Value.Trim();
                var isMul = mutCheckBox.Checked;
                if (!string.IsNullOrEmpty(colUid) && !string.IsNullOrEmpty(findVlaue) &&!string.IsNullOrEmpty(docXmlData.InnerXml))
                {
                    XmlNode foundNode;
                    if (docFounded > 0)
                    {
                        foundNode =
                            docXmlData.SelectSingleNode("./Matrix/Rows/Row[position()>" + docFounded +
                                                        "]/Columns/Column[ID='" + colUid + "' and contains(Value,'" +
                                                        findVlaue + "')]");
                        if (docFounded >= ioTmpMtx.RowCount)
                        {
                            docFounded = 0;
                        }
                    }
                    else
                    {
                        foundNode =
                            docXmlData.SelectSingleNode("./Matrix/Rows/Row/Columns/Column[ID='" + colUid +
                                                        "' and contains(Value,'" + findVlaue + "')]");
                    }
                    if (foundNode != null)
                    {
                        if (foundNode.ParentNode != null)
                            foundNode = foundNode.ParentNode.SelectSingleNode("./Column[ID='C0']");
                        if (foundNode != null)
                        {
                            var selectSingleNode = foundNode.SelectSingleNode("./Value");
                            if (selectSingleNode != null)
                                found = Convert.ToInt32(selectSingleNode.InnerText);
                            if (found > 0)
                            {
                                ioTmpMtx.SelectRow(found, true, isMul);
                                docFounded = found;
                            }
                        }
                    }
                }
            }
            else
            {
                docFounded = 0;
            }
        }

        private void folder2_ClickBefore(object sboobject, SBOItemEventArg pval, out bool bubbleevent)
        {
            paramList.Clear();
            bubbleevent = true;
            matrix1.FlushToDataSource();
            var selectRow = matrix1.GetNextSelectedRow();
            if (selectRow < 0)
            {
                Globle.Application.SetStatusBarMessage("请选择采购订单行", BoMessageTime.bmt_Short);
                bubbleevent = false;
            }
            else
            {
                while (selectRow > 0)
                {
                    paramList.Add(selectRow);
                    selectRow = matrix1.GetNextSelectedRow(selectRow);
                }
            }
        }

        private void folder1_ClickBefore(object sboobject, SBOItemEventArg pval, out bool bubbleevent)
        {
            bubbleevent = true;
        }

        private void folder2_ClickAfter(object sboobject, SBOItemEventArg pval)
        {
            if (MyForm.PaneLevel == 2)
            {
                return;
            }
            MyForm.PaneLevel = 2;
            searchText.Value = "";
            var validValues = combo.ValidValues;
            for (var i = validValues.Count - 1; i > -1; i--)
            {
                validValues.Remove(i, BoSearchKey.psk_Index);
            }
            for (var i = 0; i < matrix2.Columns.Count; i++)
            {
                var column = matrix2.Columns.Item(i);
                if (column.Visible)
                {
                    validValues.Add(column.UniqueID, column.TitleObject.Caption);
                }
            }
            docdTable.Rows.Clear();
            if (paramList.Count > 0)
            {
                var paBuilder = new StringBuilder("(");
                for (var i = 0; i < paramList.Count; i++)
                {
                    var docEntry = docTable.GetValue("DocEntry", paramList[i] - 1).ToString().Trim();
                    paBuilder.Append(docEntry).Append(",");
                }

                var paramss = paBuilder.ToString();
                paramss = paramss.Substring(0, paramss.Length - 1);
                paramss = paramss + ")";

                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append(
                    "SELECT 1 as \"RowId\",T0.\"DocEntry\",T0.\"DocNum\",T0.\"ObjType\",T0.\"CardCode\",T0.\"CardName\",T0.\"TaxDate\",T1.\"U_SalesOrderNo\",T1.\"LineNum\",T1.\"ItemCode\",T1.\"Dscription\",T1.\"Quantity\"-ifnull(T1.\"U_CQty\",0)-ifnull(T1.\"U_QuaQty\",0) as \"Quantity\",T1.\"U_QuaQty\",T1.\"U_UNQuaQty\",T2.\"SlpName\",T3.\"U_ArmyCheck\" FROM dummy T12,OPOR T0 INNER JOIN POR1 T1 ON	T0.\"DocEntry\"=T1.\"DocEntry\" AND T1.\"Quantity\">ifnull(T1.\"U_CQty\",0.0)+ifnull(T1.\"U_QuaQty\",0.0) AND T1.\"LineStatus\"='O' INNER JOIN OSLP T2 ON T2.\"SlpCode\" = T1.\"SlpCode\" INNER JOIN OITM T3 ON T1.\"ItemCode\"=T3.\"ItemCode\" WHERE T0.\"DocStatus\"='O' AND T0.\"CANCELED\"='N' and T0.\"DocEntry\" in ");
                sqlBuilder.Append(paramss);
                tmpTable.ExecuteQuery(sqlBuilder.ToString());
                if (!tmpTable.IsEmpty)
                {
                    for (var i = 0; i < tmpTable.Rows.Count; i++)
                    {
                        docdTable.Rows.Add();
                        var rowIndex = docdTable.Rows.Count - 1;

                        for (var j = 0; j < tmpTable.Columns.Count; j++)
                        {
                            var column = tmpTable.Columns.Item(j);
                            if (column.Name != "RowId")
                            {
                                docdTable.SetValue(column.Name, rowIndex, tmpTable.GetValue(column.Name, i));
                            }
                        }
                        docdTable.SetValue("RowId", rowIndex, (rowIndex + 1));
                    }
                    try
                    {
                        MyForm.Freeze(true);
                        matrix2.LoadFromDataSource();
                        matrix2.AutoResizeColumns();
                        docXmlData.LoadXml(matrix2.SerializeAsXML(BoMatrixXmlSelect.mxs_All));
                        ioTmpMtx = matrix2;
                    }
                    catch (Exception exception)
                    {
                        Globle.Application.SetStatusBarMessage(exception.Message, BoMessageTime.bmt_Short);
                    }
                    finally
                    {
                        MyForm.Freeze(false);
                    }
                }
            }
        }

        private void folder1_ClickAfter(object sboobject, SBOItemEventArg pval)
        {
            if (MyForm.PaneLevel == 1)
            {
                return;
            }
            MyForm.PaneLevel = 1;
            searchText.Value = "";

            var validValues = combo.ValidValues;
            for (var i = validValues.Count - 1; i > -1; i--)
            {
                validValues.Remove(i, BoSearchKey.psk_Index);
            }
            for (var i = 0; i < matrix1.Columns.Count; i++)
            {
                var column = matrix1.Columns.Item(i);
                if (column.Visible)
                {
                    validValues.Add(column.UniqueID, column.TitleObject.Caption);
                }
            }
            ioTmpMtx = matrix1;
            try
            {
                MyForm.Freeze(true);
                docTable.Rows.Clear();
                matrix2.LoadFromDataSource();
            }
            catch (Exception exception)
            {
                Globle.Application.SetStatusBarMessage(exception.Message, BoMessageTime.bmt_Short);
            }
            finally
            {
                MyForm.Freeze(false);
            }
        }

        public override void ItemEventHandler(string formUid, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                ResizeForm();
            }
            else if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                var swBaseForm = Globle.SwFormsList[MyFatherUid];
                swBaseForm.MySonUid = null;
            }
        }

        private void ResizeForm()
        {
            rectTem.Width = matrix1.Item.Width + 8;
            rectTem.Height = matrix1.Item.Height + 8;
        }
    }
}