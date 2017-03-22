using System;
using System.Collections.Generic;
using System.Text;
using HuDongHeavyMachinery.Code.Util;
using SAPbobsCOM;
using SAPbouiCOM;
using Company = SAPbobsCOM.Company;
using Items = SAPbobsCOM.Items;

namespace HuDongHeavyMachinery.Code.FormExt.System._540000988

{
    public class System540000988 : SwBaseForm
    {
        private ComboBox copyCombox;
        private CheckBox hasUpdateBox;
        private DBDataSource OPQT, PQT1;
        private StaticText owerText;
        private DataTable tmpTable;
        private DataTable tmpTable2;
        private Button updteButton;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            updteButton = (Button) MyForm.Items.Add("updteB", BoFormItemTypes.it_BUTTON).Specific;
            copyCombox = (ComboBox) MyForm.Items.Item("10000330").Specific;
            hasUpdateBox = (CheckBox) MyForm.Items.Add("hasUpdate", BoFormItemTypes.it_CHECK_BOX).Specific;
            hasUpdateBox.Item.Height = 15;
            hasUpdateBox.Item.Width = 120;
            hasUpdateBox.Caption = "已更新特殊价格清单";
            hasUpdateBox.Item.RightJustified = true;
            hasUpdateBox.Item.TextStyle = 4;
            owerText = (StaticText) MyForm.Items.Item("230").Specific;
            updteButton.Caption = "更新特殊价格";
            updteButton.Item.Width = copyCombox.Item.Width;
            updteButton.Item.Height = copyCombox.Item.Height;
            updteButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1, BoModeVisualBehavior.mvb_False);
            updteButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, 1, BoModeVisualBehavior.mvb_True);
            OPQT = MyForm.DataSources.DBDataSources.Item("OPQT");
            PQT1 = MyForm.DataSources.DBDataSources.Item("PQT1");
            tmpTable = MyForm.DataSources.DataTables.Add("tmp1");
            tmpTable2 = MyForm.DataSources.DataTables.Add("tmp2");
            hasUpdateBox.DataBind.SetBound(true, "OPQT", "U_hasUpdate");
            updteButton.PressedAfter += _IButtonEvents_PressedAfterEventHandler;
        }

        private void _IButtonEvents_PressedAfterEventHandler(object sboObject, SBOItemEventArg pVal)
        {
            if (MyForm.Mode == BoFormMode.fm_OK_MODE && pVal.ActionSuccess)
            {
                UpdateSupplier();
                if (hasUpdateBox.Checked)
                {
                    Globle.Application.StatusBar.SetSystemMessage("已经更新，不可重复!", BoMessageTime.bmt_Short,
                        BoStatusBarMessageType.smt_Warning);
                    return;
                }
                var mg = Globle.Application.MessageBox("确认更新特殊价格清单?", 2, "是", "否");
                if (mg == 2)
                {
                    return;
                }
                const string formType = "COR020050";
                var top = MyForm.Top + MyForm.Height/2 - 54;
                var left = MyForm.Left + MyForm.Width/2 - 106;
                var form = CreateNewFormUtil.CreateNewForm(formType, top, left);
                MySonUid = form.UniqueID;
                var swBaseForm = Globle.SwFormsList[form.UniqueID];
                swBaseForm.MyFatherUid = MyFormUid;
            }
            else
            {
                Globle.Application.MessageBox("必须在确定状态才可更新特殊价格！");
            }
        }

        private void UpdateSupplier()
        {
            var cardCode = OPQT.GetValue("CardCode", 0).Trim();

            var sqlBuilder = new StringBuilder("(");


            for (var i = 0; i < PQT1.Size; i++)
            {
                var itemCode = PQT1.GetValue("ItemCode", i).Trim();
                if (!string.IsNullOrEmpty(itemCode))
                {
                    sqlBuilder.Append("'").Append(itemCode).Append("'").Append(",");
                }
            }
            sqlBuilder = sqlBuilder.Remove(sqlBuilder.Length - 1, 1).Append(")");

            var sql =
                "select distinct T0.\"ItemCode\",T0.\"CardCode\",T1.\"VendorCode\" from OITM T0 left join ITM2 T1 on T0.\"ItemCode\"=T1.\"ItemCode\" where T0.\"ItemCode\" in " +
                sqlBuilder;

            tmpTable.ExecuteQuery(sql);

            var vendorCodeInfos = new List<VendorCodeInfo>();

            if (!tmpTable.IsEmpty)
            {
                for (var i = 0; i < tmpTable.Rows.Count; i++)
                {
                    var itemCode = tmpTable.GetValue("ItemCode", i).ToString().Trim();
                    var scardCode = tmpTable.GetValue("CardCode", i).ToString().Trim();
                    var vendorCode = tmpTable.GetValue("VendorCode", i).ToString().Trim();
                    var vendorCodeInfo = new VendorCodeInfo
                    {
                        ItemCode = itemCode,
                        CardCode = scardCode,
                        VendorCode = vendorCode
                    };
                    vendorCodeInfos.Add(vendorCodeInfo);
                }
            }
            if (vendorCodeInfos.Count > 0)
            {
                var tmpList = new List<string>();
                for (var i = 0; i < vendorCodeInfos.Count; i++)
                {
                    var vendorCodeInfo = vendorCodeInfos[i];

                    if (tmpList.Contains(vendorCodeInfo.ItemCode))
                    {
                        continue;
                    }
                    tmpList.Add(vendorCodeInfo.ItemCode);

                    if (string.IsNullOrEmpty(vendorCodeInfo.CardCode))
                    {
                        var itemObj = Globle.DiCompany.GetBusinessObject(BoObjectTypes.oItems) as Items;
                        if (itemObj != null)
                        {
                            var hasFind = itemObj.GetByKey(vendorCodeInfo.ItemCode);
                            if (hasFind)
                            {
                                itemObj.Mainsupplier = cardCode;

                                itemObj.Update();
                                var retV = itemObj.Update();
                                if (retV != 0)
                                {
                                    var errCode = 0;
                                    string errMsg;
                                    Globle.DiCompany.GetLastError(out errCode, out errMsg);
                                    Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                                        BoMessageTime.bmt_Short);
                                }
                                else
                                {
                                    Globle.Application.StatusBar.SetSystemMessage("成功更新"+vendorCodeInfo.ItemCode+" 首选供应商！", BoMessageTime.bmt_Short,BoStatusBarMessageType.smt_Success);
                                }
                            }
                        }
                    }
                    else
                    {
                        var has = false;
                        for (var j = i; j < vendorCodeInfos.Count; j++)
                        {
                            var tmPvendorCodeInfo = vendorCodeInfos[j];
                            if (tmPvendorCodeInfo.ItemCode == vendorCodeInfo.ItemCode)
                            {
                                if (tmPvendorCodeInfo.VendorCode == cardCode)
                                {
                                    has = true;
                                    break;
                                }
                            }
                        }
                        if (!has)
                        {
                            var itemObj = Globle.DiCompany.GetBusinessObject(BoObjectTypes.oItems) as Items;
                            if (itemObj != null)
                            {
                                var hasFind = itemObj.GetByKey(vendorCodeInfo.ItemCode);
                                if (hasFind)
                                {
                                    var preferredVendors = itemObj.PreferredVendors;
                                    var count = preferredVendors.Count;
                                    preferredVendors.Add();
                                    preferredVendors.SetCurrentLine(count);
                                    preferredVendors.BPCode = cardCode;
                                    itemObj.Update();
                                    var retV = itemObj.Update();
                                    if (retV != 0)
                                    {
                                        var errCode = 0;
                                        string errMsg;
                                        Globle.DiCompany.GetLastError(out errCode, out errMsg);
                                        Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                                            BoMessageTime.bmt_Short);
                                    }
                                    else
                                    {
                                        Globle.Application.StatusBar.SetSystemMessage("成功更新" + vendorCodeInfo.ItemCode + " 首选供应商！", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Globle.Application.StatusBar.SetSystemMessage("供应商更新完成！", BoMessageTime.bmt_Short,
                BoStatusBarMessageType.smt_Success);
        }

        private void UpdateSpecialPrice(string docEntry, string changeCode)
        {
            if (!string.IsNullOrEmpty(docEntry))
            {
                var specialPriceList = -1;
                var sql = "select \"ListNum\" from OPLN where \"ListName\"='PurchaseRefPrice'";
                tmpTable.ExecuteQuery(sql);
                if (tmpTable.IsEmpty)
                {
                    sql =
                        "select ifnull(max(\"ListNum\"),0) as \"ListNum\" from OPLN where \"ListName\"='PurchaseRefPrice'";
                    tmpTable.ExecuteQuery(sql);
                    var priceLists = (PriceLists) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oPriceLists);
                    priceLists.PriceListName = "PurchaseRefPrice";
                    priceLists.IsGrossPrice = BoYesNoEnum.tYES;
                    priceLists.Factor = 1;
                    priceLists.RoundingMethod = BoRoundingMethod.borm_NoRounding;
                    priceLists.GroupNum = BoPriceListGroupNum.boplgn_Group2;
                    priceLists.DefaultPrimeCurrency = "RMB";
                    var retV = priceLists.Add();
                    if (retV != 0)
                    {
                        var errCode = 0;
                        string errMsg;
                        Globle.DiCompany.GetLastError(out errCode, out errMsg);
                        Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode, BoMessageTime.bmt_Short);
                        return;
                    }
                    sql = "select \"ListNum\" from OPLN where \"ListName\"='PurchaseRefPrice'";
                    tmpTable.ExecuteQuery(sql);
                    specialPriceList = int.Parse(tmpTable.GetValue("ListNum", 0).ToString());
                }
                else
                {
                    specialPriceList = int.Parse(tmpTable.GetValue("ListNum", 0).ToString());
                }


                var sqlBuilder = new StringBuilder();

                sqlBuilder.Append("select DISTINCT T1.\"ItemCode\" from  PQT1 T1  inner join ITM1 T2 on T1.\"ItemCode\"=T2.\"ItemCode\" where ");
                sqlBuilder.Append("T1.\"DocEntry\"=").Append(docEntry);
                sqlBuilder.Append(" and ").Append("T2.\"PriceList\"=").Append(specialPriceList);
                sqlBuilder.Append(" and ").Append("ifnull(T2.\"Currency\",'')<>").Append("'RMB'");

                tmpTable2.ExecuteQuery(sqlBuilder.ToString());
                if (!tmpTable2.IsEmpty)
                {
                    for (var i = 0; i < tmpTable2.Rows.Count; i++)
                    {
                        var itemCode = tmpTable2.GetValue("ItemCode", i).ToString();
                        if (!string.IsNullOrEmpty(itemCode))
                        {
                            var oitm = (Items) Globle.DiCompany.GetBusinessObject(  BoObjectTypes.oItems);
                            var result = oitm.GetByKey(itemCode);
                            if (result)
                            {
                                var priceList = oitm.PriceList;

                                for (var j = 0; j < priceList.Count; j++)
                                {
                                    priceList.SetCurrentLine(j);
                                    if (priceList.PriceList == specialPriceList)
                                    {
                                        priceList.Currency = "RMB";
                                        break;
                                    }
                                }
                            }
                            var update = oitm.Update();
                            if (update == 0)
                            {
                                Globle.Application.StatusBar.SetSystemMessage("成功更新" + oitm.ItemCode + "价格清单RMB!",
                                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            }
                        }
                    }
                }


                sql = "select distinct \"ItemCode\",\"PriceAfVAT\",\"Currency\",T1.\"CardCode\",T1.\"TaxDate\",T0.\"U_LeadTime\" from PQT1 T0 inner join OPQT T1 on T0.\"DocEntry\"=T1.\"DocEntry\" where T0.\"DocEntry\"=" +
                    docEntry;

                Globle.ORecordSet.DoQuery(sql);
                if (Globle.ORecordSet.EoF == false)
                {
                    var rowInfos = new List<RowInfo>();
                    while (Globle.ORecordSet.EoF == false)
                    {
                        var rowInfo = new RowInfo();
                        rowInfo.ItemCode = Globle.ORecordSet.Fields.Item(0).Value.ToString();
                        rowInfo.Price = double.Parse(Globle.ORecordSet.Fields.Item(1).Value.ToString());
                        rowInfo.Currency = Globle.ORecordSet.Fields.Item(2).Value.ToString();
                        rowInfo.CardCode = Globle.ORecordSet.Fields.Item(3).Value.ToString();
                        rowInfo.TaxDate = DateTime.Parse(Globle.ORecordSet.Fields.Item(4).Value.ToString());
                        rowInfo.LeadTime = Globle.ORecordSet.Fields.Item(5).Value.ToString();
                        ;
                        rowInfos.Add(rowInfo);
                        Globle.ORecordSet.MoveNext();
                    }
                    var changeCodeInfos = new List<ChangeCodeInfo>();
                    SpecialPrices oSpp;
                    foreach (var rowInfo in rowInfos)
                    {
                        oSpp = (SpecialPrices) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oSpecialPrices);
                        var result = oSpp.GetByKey(rowInfo.ItemCode, rowInfo.CardCode);

                        if (result)
                        {
                            var rowNumber = 0;
                            var specialPricesDataAreas = oSpp.SpecialPricesDataAreas;
                            var itemNo = "";
                            for (var i = 0; i < specialPricesDataAreas.Count; i++)
                            {
                                specialPricesDataAreas.SetCurrentLine(i);
                                if (specialPricesDataAreas.DateFrom <= rowInfo.TaxDate &&
                                    specialPricesDataAreas.Dateto >= rowInfo.TaxDate)
                                {
                                    var hasUpdate = UpdateItemCurrency(rowInfo.ItemCode, rowInfo.Currency,
                                        specialPriceList);

                                    if (!hasUpdate)
                                    {
                                        continue;
                                    }

                                    specialPricesDataAreas.PriceCurrency = rowInfo.Currency;
                                    specialPricesDataAreas.SpecialPrice = rowInfo.Price;
                                    itemNo = specialPricesDataAreas.ItemNo;
                                    rowNumber = specialPricesDataAreas.RowNumber;
                                    break;
                                }
                                if (specialPricesDataAreas.DateFrom < rowInfo.TaxDate &&
                                    specialPricesDataAreas.Dateto.Equals(DateTime.Parse("1899/12/30 0:00:00")))
                                {
                                    var hasUpdate = UpdateItemCurrency(rowInfo.ItemCode, rowInfo.Currency,
                                        specialPriceList);

                                    if (!hasUpdate)
                                    {
                                        continue;
                                    }
                                    specialPricesDataAreas.Dateto = rowInfo.TaxDate.AddDays(-1);
                                    specialPricesDataAreas.Add();
                                    specialPricesDataAreas.PriceCurrency = rowInfo.Currency;
                                    specialPricesDataAreas.SpecialPrice = rowInfo.Price;
                                    specialPricesDataAreas.DateFrom = rowInfo.TaxDate;
                                    specialPricesDataAreas.Discount = 0.0;
                                    itemNo = specialPricesDataAreas.ItemNo;
                                    rowNumber = specialPricesDataAreas.RowNumber;

                                    break;
                                }
                                if (specialPricesDataAreas.DateFrom == rowInfo.TaxDate &&
                                    specialPricesDataAreas.Dateto.Equals(DateTime.Parse("1899/12/30 0:00:00")))
                                {
                                    var hasUpdate = UpdateItemCurrency(rowInfo.ItemCode, rowInfo.Currency,
                                        specialPriceList);
                                    if (!hasUpdate)
                                    {
                                        continue;
                                    }

                                    specialPricesDataAreas.PriceCurrency = rowInfo.Currency;
                                    specialPricesDataAreas.SpecialPrice = rowInfo.Price;
                                    itemNo = specialPricesDataAreas.ItemNo;
                                    rowNumber = specialPricesDataAreas.RowNumber;
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(itemNo))
                            {
                                var changeCodeInfo = new ChangeCodeInfo();
                                changeCodeInfo.ItemCode = itemNo;
                                changeCodeInfo.CardCode = rowInfo.CardCode;
                                changeCodeInfo.ChangeCode = changeCode;
                                changeCodeInfo.LeadTime = rowInfo.LeadTime;
                                changeCodeInfo.LineNum = rowNumber;
                                changeCodeInfo.Price = rowInfo.Price;
                                changeCodeInfos.Add(changeCodeInfo);
                            }

                            var tmpList = new List<double>();

                            for (var i = 0; i < specialPricesDataAreas.Count; i++)
                            {
                                specialPricesDataAreas.SetCurrentLine(i);
                                tmpList.Add(specialPricesDataAreas.SpecialPrice);
                            }

                            for (var i = 0; i < specialPricesDataAreas.Count; i++)
                            {
                                specialPricesDataAreas.SetCurrentLine(i);
                                var has = false;
                                foreach (var changeCodeInfo in changeCodeInfos)
                                {
                                    if (specialPricesDataAreas.BPCode == changeCodeInfo.CardCode &&
                                        specialPricesDataAreas.RowNumber == changeCodeInfo.LineNum &&
                                        specialPricesDataAreas.ItemNo == changeCodeInfo.ItemCode)
                                    {
                                        has = true;
                                        break;
                                    }
                                }
                                if (!has)
                                {
                                    sqlBuilder.Clear();
                                    sqlBuilder.Append(
                                        "SELECT T0.\"U_ChangeCode\",T0.\"U_LeadTime\" FROM SPP1 T0 WHERE T0.\"ItemCode\"='")
                                        .Append(specialPricesDataAreas.ItemNo)
                                        .Append("'");
                                    sqlBuilder.Append(" And ")
                                        .Append("T0.\"CardCode\" ='")
                                        .Append(specialPricesDataAreas.BPCode)
                                        .Append("'");
                                    sqlBuilder.Append(" And ")
                                        .Append("T0.\"LINENUM\" =")
                                        .Append(specialPricesDataAreas.RowNumber);
                                    tmpTable.ExecuteQuery(sqlBuilder.ToString());
                                    var lchangeCode = tmpTable.GetValue("U_ChangeCode", 0).ToString();
                                    var leadTime = tmpTable.GetValue("U_LeadTime", 0).ToString();
                                    var changeCodeInfo = new ChangeCodeInfo();
                                    changeCodeInfo.ItemCode = specialPricesDataAreas.ItemNo;
                                    changeCodeInfo.CardCode = specialPricesDataAreas.BPCode;
                                    changeCodeInfo.ChangeCode = lchangeCode;
                                    changeCodeInfo.LeadTime = leadTime;
                                    changeCodeInfo.Price = rowInfo.Price;
                                    changeCodeInfo.LineNum = specialPricesDataAreas.RowNumber;
                                    changeCodeInfos.Add(changeCodeInfo);
                                }


                                specialPricesDataAreas.PriceListNo = 0;
                                specialPricesDataAreas.SpecialPrice = 0;
                            }

                            var retV = oSpp.Update();
                            if (retV != 0)
                            {
                                var errCode = 0;
                                string errMsg;
                                Globle.DiCompany.GetLastError(out errCode, out errMsg);
                                Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                                    BoMessageTime.bmt_Short);
                            }
                            else
                            {
                                specialPricesDataAreas = oSpp.SpecialPricesDataAreas;
                                for (var i = 0; i < tmpList.Count; i++)
                                {
                                    specialPricesDataAreas.SetCurrentLine(i);
                                    specialPricesDataAreas.Discount = 0.0;
                                    specialPricesDataAreas.SpecialPrice = tmpList[i];
                                    specialPricesDataAreas.PriceListNo = specialPriceList;
                                    specialPricesDataAreas.AutoUpdate = BoYesNoEnum.tNO;
                                }
                                retV = oSpp.Update();
                                if (retV != 0)
                                {
                                    var errCode = 0;
                                    string errMsg;
                                    Globle.DiCompany.GetLastError(out errCode, out errMsg);
                                    Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                                        BoMessageTime.bmt_Short);
                                }
                                else
                                {
                                    Globle.Application.StatusBar.SetSystemMessage("成功更新" + rowInfo.ItemCode + "价格清单!",  BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                }
                            }
                        }
                        else
                        {
                            const int rowNumber = 0;

                            var hasUpdate = UpdateItemCurrency(rowInfo.ItemCode, rowInfo.Currency, specialPriceList);

                            if (!hasUpdate)
                            {
                                continue;
                            }

                            oSpp.AutoUpdate = BoYesNoEnum.tNO;
                            oSpp.ItemCode = rowInfo.ItemCode;
                            oSpp.CardCode = rowInfo.CardCode;
                            oSpp.Price = rowInfo.Price;
                            oSpp.Currency = rowInfo.Currency;
                            oSpp.PriceListNum = specialPriceList;
                            oSpp.DiscountPercent = 0;

                            var specialPricesDataAreas = oSpp.SpecialPricesDataAreas;

                            specialPricesDataAreas.SetCurrentLine(0);
                            specialPricesDataAreas.AutoUpdate = BoYesNoEnum.tNO;
                            specialPricesDataAreas.DateFrom = DateTime.Today;
                            specialPricesDataAreas.PriceCurrency = rowInfo.Currency;
                            specialPricesDataAreas.SpecialPrice = rowInfo.Price;
                            specialPricesDataAreas.Discount = 0.0;
                            specialPricesDataAreas.PriceListNo = specialPriceList;

                            var retV = oSpp.Add();
                            if (retV != 0)
                            {
                                var errCode = 0;
                                string errMsg;
                                Globle.DiCompany.GetLastError(out errCode, out errMsg);
                                Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                                    BoMessageTime.bmt_Short);
                            }
                            else
                            {
                                var changeCodeInfo = new ChangeCodeInfo();
                                changeCodeInfo.ItemCode = rowInfo.ItemCode;
                                changeCodeInfo.CardCode = rowInfo.CardCode;
                                changeCodeInfo.ChangeCode = changeCode;
                                changeCodeInfo.LeadTime = rowInfo.LeadTime;
                                changeCodeInfo.LineNum = rowNumber;
                                changeCodeInfos.Add(changeCodeInfo);
                                Globle.Application.StatusBar.SetSystemMessage("成功添加" + rowInfo.ItemCode + "价格清单!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                            }
                        }
                    }
                    if (changeCodeInfos.Count > 0)
                    {
                        sqlBuilder.Clear();
                        foreach (var changeCodeInfo in changeCodeInfos)
                        {
                            sqlBuilder.Clear();
                            sqlBuilder.Append("update SPP1 set \"U_ChangeCode\"='")
                                .Append(changeCodeInfo.ChangeCode).Append("'");
                            sqlBuilder.Append(",\"U_LeadTime\"='").Append(changeCodeInfo.LeadTime).Append("'");
                            sqlBuilder.Append(" where \"CardCode\"='").Append(changeCodeInfo.CardCode).Append("'");
                            sqlBuilder.Append(" and \"ItemCode\"='").Append(changeCodeInfo.ItemCode).Append("'");
                            sqlBuilder.Append(" and \"LINENUM\"=").Append(changeCodeInfo.LineNum);
                            tmpTable.ExecuteQuery(sqlBuilder.ToString());
                        }
                    }
                }
            }
            try
            {
                MyForm.Freeze(true);
                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append("update OPQT set \"U_hasUpdate\"='Y' where \"DocEntry\"=")
                    .Append(OPQT.GetValue("DocEntry", 0).Trim());
                tmpTable.ExecuteQuery(sqlBuilder.ToString());
                if (Globle.Application.Menus.Exists("1304"))
                {
                    var menuItem = Globle.Application.Menus.Item("1304");
                    if (menuItem.Enabled)
                    {
                        menuItem.Activate();
                    }
                }
            }
            catch (Exception exception)
            {
                Globle.Application.StatusBar.SetSystemMessage(exception.Message, BoMessageTime.bmt_Short);
            }
            finally
            {
                MyForm.Freeze(false);
            }
        }

        private bool UpdateItemCurrency(string itemCode, string currency, int listNum)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select \"ItemCode\" from ITM1 where \"ItemCode\"='").Append(itemCode).Append("'");
            sqlBuilder.Append(" and \"PriceList\"=").Append(listNum);
            sqlBuilder.Append(" and \"Currency\"='").Append(currency).Append("'");
            tmpTable.ExecuteQuery(sqlBuilder.ToString());
            if (!tmpTable.IsEmpty)
            {
                return true;
            }
            var oitm = (Items) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oItems);
            var priceList = oitm.PriceList;
            for (var i = 0; i < priceList.Count; i++)
            {
                priceList.SetCurrentLine(i);
                if (priceList.PriceList == listNum)
                {
                    priceList.Currency = currency;
                    var retv = oitm.Update();
                    if (retv != 0)
                    {
                        var errCode = 0;
                        string errMsg;
                        Globle.DiCompany.GetLastError(out errCode, out errMsg);
                        Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                            BoMessageTime.bmt_Short);
                        return false;
                    }
                    return true;
                }
            }
            return false;
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
                updteButton.Item.Top = copyCombox.Item.Top;
                updteButton.Item.Left = copyCombox.Item.Left - copyCombox.Item.Width - 5;
                updteButton.Item.Top = copyCombox.Item.Top;
                hasUpdateBox.Item.Top = owerText.Item.Top + owerText.Item.Height + 4;
                hasUpdateBox.Item.Width = owerText.Item.Width;
                hasUpdateBox.Item.Left = owerText.Item.Left;
            }
        }

        public override void SonFormCloseEventHandler(object obj, SwBaseForm sonSwBaseForm)
        {
            var changeCode = (string) obj;
            MySonUid = null;
            sonSwBaseForm.MyForm.Close();
            if (!string.IsNullOrEmpty(changeCode))
            {
                var docEntry = OPQT.GetValue("DocEntry", 0);
                UpdateSpecialPrice(docEntry, changeCode);
                Globle.Application.StatusBar.SetSystemMessage("更新特殊价格完成!", BoMessageTime.bmt_Short,
                    BoStatusBarMessageType.smt_Success);
            }
        }

        private class ChangeCodeInfo
        {
            public string ItemCode { get; set; }
            public string CardCode { get; set; }
            public string ChangeCode { get; set; }
            public int LineNum { get; set; }
            public double Price { get; set; }

            public string LeadTime { get; set; }
        }

        private class RowInfo
        {
            public string ItemCode { get; set; }
            public double Price { get; set; }
            public string Currency { get; set; }
            public string CardCode { get; set; }
            public DateTime TaxDate { get; set; }
            public string LeadTime { get; set; }
        }

        private class VendorCodeInfo
        {
            public string ItemCode { get; set; }
            public string CardCode { get; set; }
            public string VendorCode { get; set; }
        }
    }
}