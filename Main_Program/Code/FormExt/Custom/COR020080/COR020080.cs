using System;
using System.Collections.Generic;
using HuDongHeavyMachinery.Code.FormExt.Other.MessageInfo;
using HuDongHeavyMachinery.Code.Util;
using SAPbobsCOM;
using SAPbouiCOM;
using Items = SAPbobsCOM.Items;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020080
{
    public class COR020080 : SwBaseForm
    {
        private readonly List<string> itemCodeList = new List<string>();
        private readonly List<int> updateRows = new List<int>();
        private DataTable doc, tmpTable;
        private Matrix matrix;
        private Button okButton;
        private Column priceColumn;
        private string priceList;
        private EditText searchEdit;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            matrix = MyForm.Items.Item("3").Specific as Matrix;
            matrix.SelectionMode = BoMatrixSelect.ms_Single;
            priceColumn = matrix.Columns.Item("5");
            priceColumn.ValidateAfter += priceColumn_ValidateAfter;

            okButton = MyForm.Items.Item("1").Specific as Button;
            okButton.PressedBefore += okButton_PressedBefore;

            searchEdit = MyForm.Items.Item("5").Specific as EditText;
            searchEdit.KeyDownAfter += searchEdit_KeyDownAfter;

            doc = MyForm.DataSources.DataTables.Item("DOC");
            tmpTable = MyForm.DataSources.DataTables.Add("tmp");
            var priceListColumn = matrix.Columns.Item("3");

            var sql = "select * from OPLN";

            tmpTable.ExecuteQuery(sql);

            if (!tmpTable.IsEmpty)
            {
                var v = priceListColumn.ValidValues;
                for (var i = 0; i < tmpTable.Rows.Count; i++)
                {
                    v.Add(tmpTable.GetValue("ListNum", i).ToString(), tmpTable.GetValue("ListName", i).ToString());
                }
            }
        }

        private void searchEdit_KeyDownAfter(object sboObject, SBOItemEventArg pVal)
        {
            var value = searchEdit.Value.Trim();
            for (var i = 0; i < itemCodeList.Count; i++)
            {
                var itemCode = itemCodeList[i];
                if (itemCode.StartsWith(value))
                {
                    matrix.SelectRow(i + 1, true, false);
                    break;
                }
            }
        }

        private void okButton_PressedBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            matrix.FlushToDataSource();
            if (MyForm.Mode == BoFormMode.fm_UPDATE_MODE && updateRows.Count > 0)
            {
                var company = CommonUtil.getSCompany();
                if (company != null)
                {
                    var progressBar = Globle.Application.StatusBar.CreateProgressBar("正在更新价格清单....", updateRows.Count,
                        false);
                    try
                    {
                        foreach (var row in updateRows)
                        {
                            var itemCode = doc.GetValue("ItemCode", row - 1).ToString().Trim();
                            var price = doc.GetValue("Price", row - 1).ToString().Trim();
                            var oitm = (Items) company.GetBusinessObject(BoObjectTypes.oItems);
                            var result = oitm.GetByKey(itemCode);
                            if (result)
                            {
                                var priceList = oitm.PriceList;

                                for (var j = 0; j < priceList.Count; j++)
                                {
                                    priceList.SetCurrentLine(j);
                                    if (priceList.PriceList == Convert.ToInt16(this.priceList))
                                    {
                                        priceList.Currency = "RMB";
                                        priceList.Price = Convert.ToDouble(price);
                                    }
                                }
                            }
                            var update = oitm.Update();
                            if (update == 0)
                            {
                                progressBar.Value = progressBar.Value + 1;
                                Globle.Application.StatusBar.SetSystemMessage("成功更新" + oitm.ItemCode + "价格清单!",
                                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        updateRows.Clear();
                        progressBar.Stop();
                    }
                }
                if (company.Connected)
                {
                    company.Disconnect();
                    Globle.Application.StatusBar.SetSystemMessage("DI连接断开!",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }
            }
        }

        private void priceColumn_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            if (pVal.ItemChanged && pVal.ActionSuccess)
            {
                if (!updateRows.Contains(pVal.Row))
                {
                    updateRows.Add(pVal.Row);
                }
            }
        }

        public void setInfo(string priceList)
        {
            if (priceList.Length == 1)
            {
                MyForm.Title = MyForm.Title + "0" + priceList;
            }
            else
            {
                MyForm.Title = MyForm.Title + " " + priceList;
            }
            this.priceList = priceList;

            var sql =
                "select  1 as \"RowId\",T0.\"ItemCode\",T1.\"ItemName\",T0.\"PriceList\",T0.\"Factor\",T0.\"Price\",T0.\"Currency\",T0.\"Ovrwritten\" from  dummy T12,itm1 T0 inner join oitm T1 on T0.\"ItemCode\"=T1.\"ItemCode\"  WHERE T0.\"PriceList\"=" +
                priceList + " Order By T0.\"ItemCode\"";
            doc.ExecuteQuery(sql);
            if (!doc.IsEmpty)
            {
                //var xml = tmpTable.SerializeAsXML(BoDataTableXmlSelect.dxs_All);
                //var oXmlDoc = new DOMDocument();
                //oXmlDoc.loadXML(xml);
                ////var n=oXmlDoc.selectSingleNode("");
                //var nodes = oXmlDoc.selectNodes("DataTable/Rows/Row/Cells");
                //var rowcount = 1;
                //foreach (IXMLDOMNode node in nodes)
                //{
                //    var childNodes = node.childNodes;
                //    foreach (IXMLDOMNode childNode in childNodes)
                //    {
                //        var column = childNode.firstChild.nodeTypedValue;
                //        //if (column == "ItemCode")
                //        //{
                //        //    var value = childNode.lastChild.nodeTypedValue;
                //        //    itemCodeList.Add(value);
                //        //    break;
                //        //}
                //        if (column == "RowId")
                //        {
                //            childNode.lastChild.nodeTypedValue = rowcount;
                //            rowcount = rowcount + 1;
                //            break;
                //        }
                //    }
                //}
                //xml = oXmlDoc.xml;

                //doc.LoadSerializedXML(BoDataTableXmlSelect.dxs_All, xml);

                const string formType = "MessageInfo";
                var form = CreateNewFormUtil.CreateNewForm(formType, MyForm.Top + MyForm.Height/2 - 45/2,
                    MyForm.Left + MyForm.Width/2 - 55);
                var messageInfo = Globle.SwFormsList[form.UniqueID] as MessageInfo;
                try
                {
                    if (messageInfo != null) messageInfo.SetMessage("加载数据中....");
                    for (var i = 0; i < doc.Rows.Count; i++)
                    {
                        var value = doc.GetValue("ItemCode", i).ToString();
                        itemCodeList.Add(value);
                        doc.SetValue("RowId", i, i + 1);
                    }
                    MyForm.Freeze(true);
                    matrix.LoadFromDataSource();
                }
                catch (Exception ex)
                {
                    Globle.Application.SetStatusBarMessage(ex.Message, BoMessageTime.bmt_Medium, true);
                }
                finally
                {
                    MyForm.Freeze(false);
                    form.Close();
                }
            }
        }
    }
}