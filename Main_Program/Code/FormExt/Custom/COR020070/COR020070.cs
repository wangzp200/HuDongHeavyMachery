using System;
using System.Runtime.InteropServices;
using HuDongHeavyMachinery.Code.Util;
using SAPbobsCOM;
using SAPbouiCOM;
using Items = SAPbobsCOM.Items;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020070
{
    public class COR020070 : SwBaseForm
    {
        private Column columnQuaQty, columnUnQuaQty, columnIsCheck;
        private Button copyButton, checkAllButton;
        private DBDataSource dbDataCor020070, dbDataCor020071;
        private Matrix matrix;
        private Item rectangle;
        private ComboBox seriesBox;
        private DataTable TMP;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            matrix = (Matrix) MyForm.Items.Item("10").Specific;

            columnQuaQty = matrix.Columns.Item("C4");
            columnQuaQty.ValidateAfter += columnQuaQty_ValidateAfter;
            columnUnQuaQty = matrix.Columns.Item("C5");
            columnUnQuaQty.ValidateAfter += columnUnQuaQty_ValidateAfter;

            MyForm.ReportType = "A001";


            columnIsCheck = matrix.Columns.Item("C6");
            //columnIsCheck.ClickAfter += columnIsCheck_ClickAfter;
            //columnIsCheck.PressedAfter+=new _IColumnEvents_PressedAfterEventHandler(columnIsCheck_PressedAfter);
            //columnIsCheck.DoubleClickAfter += columnIsCheck_PressedAfter;
            columnIsCheck.ClickAfter += columnIsCheck_PressedAfter;
            columnIsCheck.DoubleClickAfter += columnIsCheck_PressedAfter;
            rectangle = MyForm.Items.Item("24");
            dbDataCor020070 = MyForm.DataSources.DBDataSources.Item("@COR020070");
            dbDataCor020071 = MyForm.DataSources.DBDataSources.Item("@COR020071");
            TMP = MyForm.DataSources.DataTables.Add("TMP");
            seriesBox = (ComboBox) MyForm.Items.Item("25").Specific;
            CommonUtil.SeriesValidValues(seriesBox.ValidValues, MyForm);
            copyButton = (Button) MyForm.Items.Item("3").Specific;
            copyButton.ClickAfter += copyButton_ClickAfter;
            FormResize();

            //var rptTypeService =
            //    (ReportTypesService)
            //        Globle.DiCompany.GetCompanyService().GetBusinessService(ServiceTypes.ReportTypesService);
            //var newType = (ReportType) rptTypeService.GetDataInterface(ReportTypesServiceDataInterfaces.rtsReportType);

            //newType.TypeName = "采购到货送检单";
            //newType.AddonName = "coresuit";
            //newType.AddonFormType = "COR020070";
            //newType.MenuID = "MySubMenu01";
            ////newType.DefaultReportLayout = "A0010001";
            //SAPbobsCOM.ReportTypeParams newTypeParam = rptTypeService.AddReportType(newType); 

            checkAllButton = MyForm.Items.Item("23").Specific as Button;
            if (checkAllButton != null) checkAllButton.PressedAfter += checkAllButton_PressedAfter;



        }

        private void checkAllButton_PressedAfter(object sboobject, SBOItemEventArg pval)
        {


           

            matrix.FlushToDataSource();
            for (var i = 0; i < dbDataCor020071.Size; i++)
            {
                var isCheck = dbDataCor020071.GetValue("U_IsCheck", i).Trim();
                if (isCheck == "Y")
                {
                    var qty = dbDataCor020071.GetValue("U_Quantity", i);
                    dbDataCor020071.SetValue("U_QuaQty", i, qty);
                    dbDataCor020071.SetValue("U_UNQuaQty", i, "0.0");
                }
            }
            try
            {
                MyForm.Freeze(true);
                matrix.LoadFromDataSource();
                if (MyForm.Mode == BoFormMode.fm_OK_MODE)
                {
                    MyForm.Mode = BoFormMode.fm_UPDATE_MODE;
                }
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

        private void columnIsCheck_PressedAfter(object sboobject, SBOItemEventArg pVal)
        {
            if (pVal.Row > 0)
            {
                var isCheck = (CheckBox) matrix.Columns.Item("C6").Cells.Item(pVal.Row).Specific;
                if (isCheck.Checked)
                {
                    for (var i = 0; i < matrix.Columns.Count; i++)
                    {
                        var column = matrix.Columns.Item(i);
                        if (column.UniqueID != "C6" && column.Visible && column.Editable)
                        {
                            if (!matrix.CommonSetting.GetCellEditable(pVal.Row, i))
                            {
                                matrix.CommonSetting.SetCellEditable(pVal.Row, i, true);
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < matrix.Columns.Count; i++)
                    {
                        var column = matrix.Columns.Item(i);
                        if (column.UniqueID != "C6" && column.Visible && column.Editable)
                        {
                            if (matrix.CommonSetting.GetCellEditable(pVal.Row, i))
                            {
                                matrix.CommonSetting.SetCellEditable(pVal.Row, i, false);
                            }
                        }
                    }
                }
            }
        }

        private void copyButton_ClickAfter(object sboobject, SBOItemEventArg pval)
        {

            //var sql = "SELECT T0.\"ItemCode\" FROM ITM1 T0 WHERE T0.\"PriceList\" =11 and  ifnull(T0.\"Currency\",'') <>'RMB'";

            //TMP.ExecuteQuery(sql);



            const string formType = "COR020071";
            var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
            MySonUid = form.UniqueID;
            var swBaseForm = Globle.SwFormsList[MySonUid];
            swBaseForm.MyFatherUid = MyFormUid;
        }

        private void columnUnQuaQty_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            if (pVal.ItemChanged)
            {
                var currentRow = pVal.Row;
                dbDataCor020071.Offset = currentRow - 1;
                matrix.GetLineData(currentRow);
                var quantity = double.Parse(dbDataCor020071.GetValue("U_Quantity", currentRow - 1).Trim());
                var unQuaQty = double.Parse(dbDataCor020071.GetValue("U_UNQuaQty", currentRow - 1).Trim());
                if (unQuaQty > quantity || unQuaQty < 0)
                {
                    dbDataCor020071.SetValue("U_UNQuaQty", currentRow - 1, "0.0");
                    dbDataCor020071.SetValue("U_QuaQty", currentRow - 1, quantity.ToString());
                    Globle.Application.SetStatusBarMessage("数据不合理！", BoMessageTime.bmt_Short);
                }
                else
                {
                    var quaQty = quantity - unQuaQty;
                    dbDataCor020071.SetValue("U_QuaQty", currentRow - 1, quaQty.ToString());
                }
                matrix.SetLineData(currentRow);
            }
        }

        private void columnQuaQty_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            if (pVal.ItemChanged)
            {
                var currentRow = pVal.Row;
                dbDataCor020071.Offset = currentRow - 1;
                matrix.GetLineData(currentRow);
                var quantity = double.Parse(dbDataCor020071.GetValue("U_Quantity", currentRow - 1).Trim());
                var quaQty = double.Parse(dbDataCor020071.GetValue("U_QuaQty", currentRow - 1).Trim());
                if (quaQty > quantity || quaQty < 0)
                {
                    dbDataCor020071.SetValue("U_QuaQty", currentRow - 1, "0.0");
                    dbDataCor020071.SetValue("U_UNQuaQty", currentRow - 1, quantity.ToString());
                    Globle.Application.SetStatusBarMessage("数据不合理！", BoMessageTime.bmt_Short);
                }
                else
                {
                    var unQuaQty = quantity - quaQty;
                    dbDataCor020071.SetValue("U_UNQuaQty", currentRow - 1, unQuaQty.ToString());
                }
                matrix.SetLineData(currentRow);
            }
        }

        public override void SonFormCloseEventHandler(object obj, SwBaseForm sonSwBaseForm)
        {
            var result = (DataTable) obj;
            if (result.Rows.Count > 0)
            {
                dbDataCor020071.Clear();
                for (var i = 0; i < result.Rows.Count; i++)
                {
                    var rowIndex = dbDataCor020071.Size;
                    dbDataCor020071.InsertRecord(rowIndex);
                    dbDataCor020071.SetValue("LineId", rowIndex, (rowIndex + 1).ToString());
                    dbDataCor020071.SetValue("U_ItemCode", rowIndex, result.GetValue("ItemCode", i).ToString());
                    dbDataCor020071.SetValue("U_Dscription", rowIndex, result.GetValue("Dscription", i).ToString());
                    dbDataCor020071.SetValue("U_Quantity", rowIndex, result.GetValue("Quantity", i).ToString());
                    dbDataCor020071.SetValue("U_QuaQty", rowIndex, "0.0");
                    dbDataCor020071.SetValue("U_UNQuaQty", rowIndex, "0.0");
                    dbDataCor020071.SetValue("U_IsCheck", rowIndex, "Y");
                    dbDataCor020071.SetValue("U_Quantity", rowIndex, result.GetValue("Quantity", i).ToString());
                    dbDataCor020071.SetValue("U_BaseEntry", rowIndex, result.GetValue("DocEntry", i).ToString());
                    dbDataCor020071.SetValue("U_BaseTaxDate", rowIndex,
                        ((DateTime) result.GetValue("TaxDate", i)).ToString("yyyyMMdd"));
                    dbDataCor020071.SetValue("U_CardCode", rowIndex, result.GetValue("CardCode", i).ToString());
                    dbDataCor020071.SetValue("U_CardName", rowIndex, result.GetValue("CardName", i).ToString());
                    dbDataCor020071.SetValue("U_SlpName", rowIndex, result.GetValue("SlpName", i).ToString());
                    dbDataCor020071.SetValue("U_BaseType", rowIndex, result.GetValue("ObjType", i).ToString());
                    dbDataCor020071.SetValue("U_BaseLine", rowIndex, result.GetValue("LineNum", i).ToString());
                    dbDataCor020071.SetValue("U_BaseRef", rowIndex, result.GetValue("DocNum", i).ToString());
                    dbDataCor020071.SetValue("U_ArmyCheck", rowIndex, result.GetValue("U_ArmyCheck", i).ToString());
                    dbDataCor020071.SetValue("U_SalesOrderNo", rowIndex, result.GetValue("U_SalesOrderNo", i).ToString());
                }
                try
                {
                    MyForm.Freeze(true);
                    matrix.LoadFromDataSource();
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
            sonSwBaseForm.MyForm.Close();
        }

        public override void ItemEventHandler(string formUid, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                FormResize();
            }
            if (pVal.BeforeAction && MySonUid != null)
            {
                Globle.Application.Forms.Item(MySonUid).Select();
                bubbleEvent = false;
            }
            //else if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST)
            //{
            //    var cardCode = dbDataCor020070.GetValue("U_CardCode", 0).Trim();
            //    if (pVal.ItemUID == "3")
            //    {
            //        var oConds = oporChooseFromList.GetConditions();
            //        var stringBuilder = new StringBuilder();
            //        if (!string.IsNullOrEmpty(cardCode))
            //        {
            //            stringBuilder.Append("select distinct T0.\"DocEntry\" from OPOR T0 inner join POR1 T1 on T0.\"DocEntry\"=T1.\"DocEntry\" and T1.\"Quantity\">IFNULL(T1.\"U_CheckQty\",0.0) where T0.\"CANCELED\"='N' and T0.\"DocStatus\"='O' and T0.\"CardCode\"='" + cardCode + "'");
            //        }
            //        else
            //        {
            //            stringBuilder.Append("select distinct T0.\"DocEntry\" from OPOR T0 inner join POR1 T1 on T0.\"DocEntry\"=T1.\"DocEntry\" and T1.\"Quantity\">IFNULL(T1.\"U_CheckQty\",0.0) where T0.\"CANCELED\"='N' and T0.\"DocStatus\"='O'");
            //        }
            //        TMP.ExecuteQuery(stringBuilder.ToString());
            //        if (!TMP.IsEmpty)
            //        {
            //            if (oConds.Count > 0)
            //            {
            //                oConds.Item(oConds.Count - 1).Relationship = BoConditionRelationship.cr_AND;
            //            }


            //            for (var i = 0; i < TMP.Rows.Count; i++)
            //            {
            //                var oCond = oConds.Add();
            //                if (i == 0)
            //                {
            //                    oCond.BracketOpenNum = 2;
            //                }
            //                else
            //                {
            //                    oCond.BracketOpenNum = 1;
            //                }
            //                oCond.Alias = "DocEntry";
            //                oCond.Operation = BoConditionOperation.co_EQUAL;
            //                oCond.CondVal = TMP.GetValue("DocEntry", 0).ToString().Trim();
            //                if (i == TMP.Rows.Count - 1)
            //                {
            //                    oCond.BracketCloseNum = 2;
            //                }
            //                else
            //                {
            //                    oCond.BracketCloseNum = 1;
            //                    oCond.Relationship = BoConditionRelationship.cr_OR;
            //                }
            //            }
            //        }

            //        oporChooseFromList.SetConditions(oConds);
            //    }
            //}
            //else if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST)
            //{
            //    try
            //    {
            //        MyForm.Freeze(true);
            //        var obj = ((ChooseFromListEvent)pVal).SelectedObjects;
            //        if (obj == null)
            //        {
            //            return;
            //        }
            //        if (pVal.ItemUID == "14")
            //        {
            //            var cardCode = obj.GetValue("CardCode", 0).ToString();
            //            var cardName = obj.GetValue("CardName", 0).ToString();
            //            dbDataCor020070.SetValue("U_CardCode", 0, cardCode);
            //            dbDataCor020070.SetValue("U_CardName", 0, cardName);
            //        }
            //        else if (pVal.ItemUID == "3")
            //        {
            //            var docEnrty = obj.GetValue("DocEntry", 0);
            //            var taxDate = obj.GetValue("TaxDate", 0);
            //            var slpCode = obj.GetValue("SlpCode", 0);
            //            var baseref = obj.GetValue("DocNum", 0).ToString();
            //            var cardCode = obj.GetValue("CardCode", 0).ToString();
            //            var cardName = obj.GetValue("CardName", 0).ToString();
            //            dbDataCor020070.SetValue("U_CardCode", 0, cardCode);
            //            dbDataCor020070.SetValue("U_CardName", 0, cardName);
            //            dbDataCor020070.SetValue("U_BaseEntry", 0, docEnrty.ToString());
            //            dbDataCor020070.SetValue("U_BaseTaxDate", 0, ((DateTime)taxDate).ToString("yyyyMMdd"));
            //            var sql = "select \"SlpName\" from OSLP where \"SlpCode\"='" + slpCode + "'";
            //            TMP.ExecuteQuery(sql);
            //            if (!TMP.IsEmpty)
            //            {
            //                var slpName = TMP.GetValue("SlpName", 0).ToString();
            //                dbDataCor020070.SetValue("U_SlpName", 0, slpName);
            //            }
            //            sql = "select * from POR1 where \"DocEntry\"=" + docEnrty;
            //            TMP.ExecuteQuery(sql);
            //            if (!TMP.IsEmpty)
            //            {
            //                dbDataCor020071.Clear();
            //                for (var i = 0; i < TMP.Rows.Count; i++)
            //                {
            //                    var rowIndex = dbDataCor020071.Size;
            //                    dbDataCor020071.InsertRecord(rowIndex);
            //                    var value = (i + 1) + "";
            //                    dbDataCor020071.SetValue("lineId", rowIndex, value);
            //                    value = TMP.GetValue("ItemCode", i).ToString();
            //                    dbDataCor020071.SetValue("U_ItemCode", rowIndex, value);
            //                    value = TMP.GetValue("Dscription", i).ToString();
            //                    dbDataCor020071.SetValue("U_Dscription", rowIndex, value);
            //                    value = TMP.GetValue("Quantity", i).ToString();
            //                    dbDataCor020071.SetValue("U_Quantity", rowIndex, value);
            //                    dbDataCor020071.SetValue("U_QuaQty", rowIndex, value);
            //                    dbDataCor020071.SetValue("U_UNQuaQty", rowIndex, "0.0");
            //                    dbDataCor020071.SetValue("U_IsCheck", rowIndex, "Y");

            //                    value = TMP.GetValue("DocEntry", i).ToString();
            //                    dbDataCor020071.SetValue("U_BaseEntry", rowIndex, value);

            //                    value = TMP.GetValue("ObjType", i).ToString();
            //                    dbDataCor020071.SetValue("U_BaseType", rowIndex, value);

            //                    value = TMP.GetValue("LineNum", i).ToString();
            //                    dbDataCor020071.SetValue("U_BaseLine", rowIndex, value);
            //                    dbDataCor020071.SetValue("U_BaseRef", rowIndex, baseref);
            //                }
            //                matrix.LoadFromDataSource();
            //                matrix.AutoResizeColumns();

            //                for (var j = 1; j <= matrix.RowCount; j++)
            //                {
            //                    for (var i = 0; i < matrix.Columns.Count; i++)
            //                    {
            //                        var column = matrix.Columns.Item(i);
            //                        if (column.UniqueID != "C6" && column.Visible && column.Editable)
            //                        {
            //                            if (!matrix.CommonSetting.GetCellEditable(j, i))
            //                            {
            //                                matrix.CommonSetting.SetCellEditable(j, i, true);
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception exception)
            //    {
            //        Globle.Application.SetStatusBarMessage(exception.Message);
            //    }
            //    finally
            //    {
            //        MyForm.Freeze(false);
            //    }
            //}
        }

        public override void FormDataLoad(ref BusinessObjectInfo businessobjectinfo, ref bool bubbleevent)
        {
            if (!businessobjectinfo.BeforeAction)
            {
                for (var j = 1; j <= matrix.VisualRowCount; j++)
                {
                    var isCheck = (CheckBox) matrix.Columns.Item("C6").Cells.Item(j).Specific;
                    if (isCheck.Checked)
                    {
                        for (var i = 0; i < matrix.Columns.Count; i++)
                        {
                            var column = matrix.Columns.Item(i);
                            if (column.UniqueID != "C6" && column.Visible && column.Editable)
                            {
                                if (!matrix.CommonSetting.GetCellEditable(j, i))
                                {
                                    matrix.CommonSetting.SetCellEditable(j, i, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (var i = 0; i < matrix.Columns.Count; i++)
                        {
                            var column = matrix.Columns.Item(i);
                            if (column.UniqueID != "C6" && column.Visible && column.Editable)
                            {
                                if (matrix.CommonSetting.GetCellEditable(j, i))
                                {
                                    matrix.CommonSetting.SetCellEditable(j, i, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void LayoutKeyEventHandler(ref LayoutKeyInfo eventinfo, ref bool bubbleevent)
        {
            if (eventinfo.BeforeAction && eventinfo.ReportCode == MyForm.ReportType)
            {
                var docEntry = dbDataCor020070.GetValue("DocEntry", 0).Trim();
                if (!string.IsNullOrEmpty(docEntry))
                {
                    eventinfo.LayoutKey = docEntry;
                }
                else
                {
                    bubbleevent = false;
                }
            }
        }

        private void FormResize()
        {
            rectangle.Width = matrix.Item.Width + 10;
            rectangle.Height = matrix.Item.Height + 10;
        }
    }
}