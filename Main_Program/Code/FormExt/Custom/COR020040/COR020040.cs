using System;
using System.Collections.Generic;
using System.Text;
using HuDongHeavyMachinery.Code.Util;
using SAPbobsCOM;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020040
{
    public class COR020040 : SwBaseForm
    {
        private Button GoodsReceiptPO;
        private DataTable tmpTable;
        private DBDataSource dbDataCor020040, dbDataCor020041, dbDataCor020042;
        private Matrix matrix;
        private Item rectangle;
        private Column checkColumn;
        private EditText cardCodeText;
        private Button copfromButton;
        private string conditionsOporXml;
        private string conditionsCustmerXml;
        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            KeyFieldList = new SortedList<string, string>();
            dbDataCor020040 = MyForm.DataSources.DBDataSources.Item("@COR020040");
            dbDataCor020041 = MyForm.DataSources.DBDataSources.Item("@COR020041");
            dbDataCor020042 = MyForm.DataSources.DBDataSources.Item("@COR020042");
            tmpTable = MyForm.DataSources.DataTables.Add("tmp");
            matrix = (Matrix) MyForm.Items.Item("15").Specific;
            KeyFieldList.Add("@COR020041", "U_ItemCode");
            rectangle = MyForm.Items.Item("16");
            GoodsReceiptPO = (Button) MyForm.Items.Item("17").Specific;
            GoodsReceiptPO.PressedAfter += GoodsReceiptPO_PressedAfter;
            GoodsReceiptPO.PressedBefore += GoodsReceiptPO_PressedBefore;
            checkColumn = matrix.Columns.Item("C5");
            checkColumn.KeyDownBefore += checkColumn_KeyDownBefore;
            cardCodeText = (EditText) MyForm.Items.Item("6").Specific;
            cardCodeText.ChooseFromListBefore += cardCodeText_ChooseFromListBefore;
            copfromButton = (Button) MyForm.Items.Item("27").Specific;
            copfromButton.ChooseFromListBefore += copfromButton_ChooseFromListBefore;
            var owerChooseFromList = MyForm.ChooseFromLists.Item(copfromButton.ChooseFromListUID);
            var conditions = owerChooseFromList.GetConditions();
            conditionsOporXml = conditions.GetAsXML();

            owerChooseFromList = MyForm.ChooseFromLists.Item(cardCodeText.ChooseFromListUID);
            conditions = owerChooseFromList.GetConditions();
            conditionsCustmerXml = conditions.GetAsXML();

            FormResize();
        }

        private void cardCodeText_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent)
        {
            var owerChooseFromList = MyForm.ChooseFromLists.Item(cardCodeText.ChooseFromListUID);
            var conditions = owerChooseFromList.GetConditions();
            conditions.LoadFromXML(conditionsCustmerXml);
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT distinct T0.\"CardCode\"  FROM OPOR T0  INNER JOIN POR1 T1 ON T0.\"DocEntry\" = T1.\"DocEntry\" WHERE T1.\"OpenQty\" >0 and  T1.\"Quantity\" > IFNULL(T1.\"U_CheckQty\" ,0.0) and   T1.\"LineStatus\" ='O' and T0.\"DocStatus\" ='O'");

            tmpTable.ExecuteQuery(sqlBuilder.ToString());
            if (!tmpTable.IsEmpty)
            {
                if (conditions.Count > 0)
                {
                    conditions.Item(conditions.Count - 1).Relationship = BoConditionRelationship.cr_AND;
                }
                for (var i = 0; i < tmpTable.Rows.Count; i++)
                {
                    var cardCdoe = tmpTable.GetValue("CardCode", i).ToString();
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
                    condition.CondVal = cardCdoe;

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
                owerChooseFromList.SetConditions(conditions);
            }
            bubbleEvent = true;
        }

        private void copfromButton_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent)
        {
            var cardCode = dbDataCor020040.GetValue("U_CardCode", 0).Trim();
            if (string.IsNullOrEmpty(cardCode))
            {
                Globle.Application.SetStatusBarMessage("请选择业务伙伴", BoMessageTime.bmt_Short);
                bubbleEvent = false;
                return;
            }
            if (MyForm.Mode!= BoFormMode.fm_ADD_MODE)
            {
                Globle.Application.SetStatusBarMessage("非添加状态不能操作复制从", BoMessageTime.bmt_Short);
                bubbleEvent = false;
                return;
            }
            var owerChooseFromList = MyForm.ChooseFromLists.Item(copfromButton.ChooseFromListUID);
            var conditions = owerChooseFromList.GetConditions();
            conditions.LoadFromXML(conditionsOporXml);

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT distinct T0.\"DocEntry\"  FROM OPOR T0  INNER JOIN POR1 T1 ON T0.\"DocEntry\" = T1.\"DocEntry\" WHERE T1.\"OpenQty\" >0 and  T1.\"Quantity\" > IFNULL(T1.\"U_CheckQty\" ,0.0) and   T1.\"LineStatus\" ='O' and T0.\"DocStatus\" ='O' and T0.\"CardCode\" ='").Append(cardCode)
                .Append("'");
            tmpTable.ExecuteQuery(sqlBuilder.ToString());
            if (!tmpTable.IsEmpty)
            {
                if (conditions.Count > 0)
                {
                    conditions.Item(conditions.Count - 1).Relationship = BoConditionRelationship.cr_AND;
                }
                for (var i = 0; i < tmpTable.Rows.Count; i++)
                {
                    var docEntry = tmpTable.GetValue("DocEntry", i).ToString();
                    var condition = conditions.Add();
                    if (0 == i && tmpTable.Rows.Count > 1)
                    {
                        condition.BracketOpenNum = 2;
                    }
                    else
                    {
                        condition.BracketOpenNum = 1;
                    }
                    condition.Alias = "DocEntry";
                    condition.Operation = BoConditionOperation.co_EQUAL;
                    condition.CondVal = docEntry;

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
                owerChooseFromList.SetConditions(conditions);
                bubbleEvent = true;
            }
            else
            {
                Globle.Application.SetStatusBarMessage("没有符合数据", BoMessageTime.bmt_Short);
                bubbleEvent = true;
            }
        }

        private void checkColumn_KeyDownBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent)
        {
            if (pVal.CharPressed == 9 && pVal.Modifiers == BoModifiersEnum.mt_CTRL)
            {
                matrix.FlushToDataSource();
                var row = pVal.Row - 1;
                var itemCode = dbDataCor020041.GetValue("U_ItemCode", row).Trim();
                var sql =
                    new StringBuilder(
                        "select * from OITM where \"ManBtchNum\"='Y' and \"MngMethod\"='A' and \"ItemCode\"='" +
                        itemCode + "'");
                tmpTable.ExecuteQuery(sql.ToString());
                if (!tmpTable.IsEmpty)
                {
                    const string formType = "COR020042";
                    var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
                    MySonUid = form.UniqueID;
                    var swBaseForm = Globle.SwFormsList[form.UniqueID];
                    swBaseForm.MyFatherUid = MyFormUid;
                    ((COR020042) swBaseForm).SetInformation(dbDataCor020041, dbDataCor020042, pVal.Row);
                    bubbleEvent = false;
                }
                else
                {
                    Globle.Application.SetStatusBarMessage("物料:" + itemCode + ",非批次管理。", BoMessageTime.bmt_Short, false);
                    bubbleEvent = true;
                }
            }
            else
            {
                bubbleEvent = true;
            }
        }


        private void GoodsReceiptPO_PressedBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent)
        {
            if (MyForm.Mode != BoFormMode.fm_OK_MODE)
            {
                bubbleEvent = false;
            }
            else
            {
                bubbleEvent = true;
            }
        }

        private void GoodsReceiptPO_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            if (string.IsNullOrEmpty(dbDataCor020040.GetValue("U_TarEntry", 0).Trim()))
            {
                var sqlBuilder =new StringBuilder("select distinct \"ItemCode\" from OITM where \"ManBtchNum\"='Y' and \"MngMethod\"='A' and \"ItemCode\" in (");
                for (var i = 0; i < dbDataCor020041.Size; i++)
                {
                    if (!string.IsNullOrEmpty(dbDataCor020041.GetValue("U_ItemCode", i).Trim()))
                    {
                        sqlBuilder.Append("'").Append(dbDataCor020041.GetValue("U_ItemCode", i).Trim()).Append("'");
                        sqlBuilder.Append(",");
                    }
                }
                sqlBuilder.Append(")");
                var sql = sqlBuilder.ToString();
                sql = sql.Remove(sql.Length - 2, 1);
                tmpTable.ExecuteQuery(sql);
                var tmpList = new List<string>();
                for (var i = 0; i < tmpTable.Rows.Count; i++)
                {
                    tmpList.Add(tmpTable.GetValue("ItemCode", i).ToString());
                }
                var goodsReceipt = (Documents) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oPurchaseDeliveryNotes);
                goodsReceipt.TaxDate = DateTime.Today;
                goodsReceipt.CardCode = dbDataCor020040.GetValue("U_CardCode", 0).Trim();
                goodsReceipt.CardName = dbDataCor020040.GetValue("U_CardName", 0).Trim();
                goodsReceipt.NumAtCard = dbDataCor020040.GetValue("U_NumAtCard", 0).Trim();
                goodsReceipt.DocDate = DateTime.Today;
                goodsReceipt.DocDueDate = DateTime.Today;
                var rid = 0;
                for (var i = 0; i < dbDataCor020041.Size; i++)
                {
                    var itemCode = dbDataCor020041.GetValue("U_ItemCode", i);
                    if (!string.IsNullOrEmpty(itemCode))
                    {
                        var cQty =double.Parse(dbDataCor020041.GetValue("U_CheckQty", i));
                        if (cQty>0.0)
                        {
                            goodsReceipt.Lines.SetCurrentLine(rid);

                            goodsReceipt.Lines.ItemCode = dbDataCor020041.GetValue("U_ItemCode", i).Trim();

                            goodsReceipt.Lines.Quantity = double.Parse(dbDataCor020041.GetValue("U_CheckQty", i).Trim());

                            goodsReceipt.Lines.BaseEntry = int.Parse(dbDataCor020041.GetValue("U_BaseEntry", i).Trim());

                            goodsReceipt.Lines.BaseType = int.Parse(dbDataCor020041.GetValue("U_BaseType", i).Trim());

                            goodsReceipt.Lines.BaseLine = int.Parse(dbDataCor020041.GetValue("U_BaseLine", i).Trim());

                            goodsReceipt.Lines.WarehouseCode = dbDataCor020041.GetValue("U_WhseCode", i).Trim();

                            var bathNum = 0;
                            if (tmpList.Contains(dbDataCor020041.GetValue("U_ItemCode", i).Trim()))
                            {
                                var lineId = dbDataCor020041.GetValue("LineId", i).Trim();
                                for (var j = 0; j < dbDataCor020042.Size - 1; j++)
                                {
                                    if (dbDataCor020042.GetValue("U_BaseLine", j).Trim() == lineId)
                                    {
                                        goodsReceipt.Lines.BatchNumbers.SetCurrentLine(bathNum);
                                        goodsReceipt.Lines.BatchNumbers.AddmisionDate = DateTime.Parse(dbDataCor020042.GetValue("U_InDate", j).Trim());
                                        goodsReceipt.Lines.BatchNumbers.BaseLineNumber = rid;
                                        goodsReceipt.Lines.BatchNumbers.BatchNumber =dbDataCor020042.GetValue("U_DistNumber", j).Trim();
                                        goodsReceipt.Lines.BatchNumbers.ExpiryDate =DateTime.Parse(dbDataCor020042.GetValue("U_ExpDate", j).Trim());
                                        goodsReceipt.Lines.BatchNumbers.ManufacturerSerialNumber = dbDataCor020042.GetValue("U_MnfSerial", j).Trim();
                                        goodsReceipt.Lines.BatchNumbers.InternalSerialNumber =dbDataCor020042.GetValue("U_LotNumber", j).Trim();
                                        goodsReceipt.Lines.BatchNumbers.Location =dbDataCor020042.GetValue("U_Location", j).Trim();
                                        goodsReceipt.Lines.BatchNumbers.Notes =dbDataCor020042.GetValue("U_Notes", j).Trim();
                                        goodsReceipt.Lines.BatchNumbers.Notes = dbDataCor020042.GetValue("U_Notes", j).Trim();
                                        goodsReceipt.Lines.BatchNumbers.Quantity = double.Parse(dbDataCor020042.GetValue("U_Quantity", j).Trim());
                                        goodsReceipt.Lines.BatchNumbers.Add();
                                        bathNum = bathNum + 1;
                                    }
                                }
                            }

                            goodsReceipt.Lines.Add();

                            rid++;
                        }
                    }
                }
                var retV = goodsReceipt.Add();
                if (retV != 0)
                {
                    int errCode;
                    string errMsg;
                    Globle.DiCompany.GetLastError(out errCode, out errMsg);
                    Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode, BoMessageTime.bmt_Short);
                }
                else
                {
                    var docEntry = Globle.DiCompany.GetNewObjectKey();
                    sqlBuilder.Clear();
                    sqlBuilder.Append("select \"DocNum\" from OPDN where \"DocEntry\"=" + docEntry);
                    tmpTable.ExecuteQuery(sqlBuilder.ToString());
                    var docNum = tmpTable.GetValue("DocNum", 0).ToString();
                    try
                    {
                        MyForm.Freeze(true);
                        dbDataCor020040.SetValue("U_TarNum", 0, docNum);
                        dbDataCor020040.SetValue("U_TarEntry", 0, docEntry);
                        MyForm.Mode = BoFormMode.fm_UPDATE_MODE;
                        MyForm.Items.Item("1").Click();
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                    finally
                    {
                        MyForm.Freeze(false);
                    }
                    Globle.Application.MessageBox("成功创建采购收货单:" + docNum, 1, "OK");
                    Globle.Application.StatusBar.SetSystemMessage("成功创建采购收货单" + docNum, BoMessageTime.bmt_Short,
                        BoStatusBarMessageType.smt_Success);
                    sqlBuilder.Clear();
                }
            }
            else
            {
                Globle.Application.SetStatusBarMessage("已经创建!", BoMessageTime.bmt_Short, false);
            }
        }


        public override void ItemEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                FormResize();
            }

            else if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                if (MyFatherUid != null)
                {
                    foreach (var swBaseForm in Globle.SwFormsList)
                    {
                        if (swBaseForm.Key == MyFatherUid)
                        {
                            Globle.SwFormsList[MyFatherUid].MySonUid = null;
                            break;
                        }
                    }
                }
            }

            else if (pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST)
            {
                if (!pVal.BeforeAction)
                {
                    var obj = ((ChooseFromListEvent) pVal).SelectedObjects;
                    if (!obj.IsEmpty)
                    {
                        if (pVal.ItemUID == "6")
                        {
                            var cardCode = obj.GetValue("CardCode", 0).ToString();
                            var cardName = obj.GetValue("CardName", 0).ToString();
                            dbDataCor020040.SetValue("U_CardCode", 0, cardCode);
                            dbDataCor020040.SetValue("U_CardName", 0, cardName);
                        }
                        else if (pVal.ItemUID == "27")
                        {
                            var sqlBuilder = new StringBuilder("(");
                            for (var i = 0; i < obj.Rows.Count; i++)
                            {
                                var docEntry = obj.GetValue("DocEntry", i).ToString();
                                if (!string.IsNullOrEmpty(docEntry))
                                {
                                    sqlBuilder.Append(docEntry);
                                    if (i != obj.Rows.Count - 1)
                                    {
                                        sqlBuilder.Append(",");
                                    }
                                }
                            }
                            sqlBuilder.Append(")");
                            var sql = sqlBuilder.ToString();
                            if (sql.Length > 2)
                            {
                                sql =
                                    "SELECT T0.\"DocNum\", T0.\"ObjType\", T1.\"LineNum\", T1.\"DocEntry\", T1.\"Quantity\", ifnull(T1.\"U_CheckQty\",0.0) as \"CheckQty\", T1.\"ItemCode\", T1.\"Dscription\", T1.\"WhsCode\",T1.\"UomCode\" FROM OPOR T0  INNER JOIN POR1 T1 ON T0.\"DocEntry\" = T1.\"DocEntry\" WHERE T1.\"OpenQty\" >0 and  T1.\"Quantity\" > IFNULL(T1.\"U_CheckQty\" ,0.0) and   T1.\"LineStatus\" ='O' and T0.\"DocStatus\" ='O' and T0.\"DocEntry\" in " +
                                    sql;
                                tmpTable.ExecuteQuery(sql);
                                if (!tmpTable.IsEmpty)
                                {
                                    dbDataCor020041.Clear();
                                    dbDataCor020042.Clear();
                                    for (var i = 0; i < tmpTable.Rows.Count; i++)
                                    {
                                        var rowIndex = dbDataCor020041.Size;
                                        dbDataCor020041.InsertRecord(rowIndex);
                                        dbDataCor020041.SetValue("LineId", rowIndex, (rowIndex + 1).ToString());
                                        dbDataCor020041.SetValue("U_ItemCode", rowIndex, tmpTable.GetValue("ItemCode", i).ToString());
                                        dbDataCor020041.SetValue("U_Dscription", rowIndex, tmpTable.GetValue("Dscription", i).ToString());
                                        dbDataCor020041.SetValue("U_Quantity", rowIndex,tmpTable.GetValue("Quantity", i).ToString());
                                        var value = double.Parse(tmpTable.GetValue("Quantity", i).ToString()) - double.Parse(tmpTable.GetValue("CheckQty", i).ToString());
                                        dbDataCor020041.SetValue("U_OpenCQty", rowIndex,value.ToString() );
                                        dbDataCor020041.SetValue("U_CheckQty", rowIndex,value.ToString());
                                        dbDataCor020041.SetValue("U_LineStatus", rowIndex, "O");
                                        dbDataCor020041.SetValue("U_WhseCode", rowIndex, tmpTable.GetValue("WhsCode", i).ToString());
                                        dbDataCor020041.SetValue("U_InvUoM", rowIndex, tmpTable.GetValue("UomCode", i).ToString());
                                        dbDataCor020041.SetValue("U_BaseLine", rowIndex,tmpTable.GetValue("LineNum", i).ToString());
                                        dbDataCor020041.SetValue("U_BaseEntry", rowIndex, tmpTable.GetValue("DocEntry", i).ToString());
                                        dbDataCor020041.SetValue("U_BaseType", rowIndex, tmpTable.GetValue("ObjType", i).ToString());
                                        dbDataCor020041.SetValue("U_BaseRef", rowIndex,tmpTable.GetValue("DocNum", i).ToString());
                                    }
                                    try
                                    {
                                        MyForm.Freeze(true);
                                        matrix.LoadFromDataSource();
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                    finally
                                    {
                                        MyForm.Freeze(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetInformation(DBDataSource opor, DBDataSource por1)
        {
            try
            {
                MyForm.Freeze(true);
                var row = 1;
                dbDataCor020040.SetValue("U_CardCode", 0, opor.GetValue("CardCode", 0));
                dbDataCor020040.SetValue("U_CardName", 0, opor.GetValue("CardName", 0));
                dbDataCor020040.SetValue("U_NumAtCard", 0, opor.GetValue("NumAtCard", 0));
                dbDataCor020040.SetValue("CreateDate", 0, DateTime.Today.ToString("yyyyMMdd"));

                for (var i = 0; i < por1.Size; i++)
                {
                    var value = por1.GetValue("ItemCode", i);
                    if (!string.IsNullOrEmpty(value))
                    {
                        dbDataCor020041.InsertRecord(dbDataCor020041.Size);
                        dbDataCor020041.SetValue("U_ItemCode", row - 1, por1.GetValue("ItemCode", i));
                        dbDataCor020041.SetValue("U_Dscription", row - 1, por1.GetValue("Dscription", i));
                        dbDataCor020041.SetValue("U_Quantity", row - 1, por1.GetValue("Quantity", i));
                        dbDataCor020041.SetValue("U_OpenCQty", row - 1,
                            (double.Parse(por1.GetValue("OpenQty", i)) -
                             double.Parse(string.IsNullOrEmpty(por1.GetValue("U_CheckQty", i))
                                 ? "0.0"
                                 : por1.GetValue("U_CheckQty", i))).ToString());
                        dbDataCor020041.SetValue("U_CheckQty", row - 1,
                            (double.Parse(por1.GetValue("OpenQty", i)) -
                             double.Parse(string.IsNullOrEmpty(por1.GetValue("U_CheckQty", i))
                                 ? "0.0"
                                 : por1.GetValue("U_CheckQty", i))).ToString());
                        dbDataCor020041.SetValue("LineId", row - 1, row.ToString());
                        dbDataCor020041.SetValue("U_BaseLine", row - 1, por1.GetValue("LineNum", i));
                        dbDataCor020041.SetValue("U_BaseEntry", row - 1, por1.GetValue("DocEntry", i));
                        dbDataCor020041.SetValue("U_BaseType", row - 1, por1.GetValue("ObjType", i));
                        dbDataCor020041.SetValue("U_BaseRef", row - 1, opor.GetValue("DocNum", 0));
                        dbDataCor020041.SetValue("U_WhseCode", row - 1, por1.GetValue("WhsCode", i));
                        dbDataCor020041.SetValue("U_InvUoM", row - 1, por1.GetValue("UomCode", i));
                        row++;
                    }
                }
                matrix.LoadFromDataSource();
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

        private void FormResize()
        {
            rectangle.Width = matrix.Item.Width + 8;
            rectangle.Height = matrix.Item.Height + 10;
        }
    }
}