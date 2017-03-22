using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.System._1470000200
{
    public class System1470000200 : SwBaseForm
    {
        private EditText DocNum;
        private DBDataSource OPRQ, PRQ1;
        private Button taskAllocation, requestC;
        //private Button subgroup;
        private DataTable tmpTable;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            OPRQ = MyForm.DataSources.DBDataSources.Item("OPRQ");
            PRQ1 = MyForm.DataSources.DBDataSources.Item("PRQ1");
            tmpTable = MyForm.DataSources.DataTables.Add("tmp");

            taskAllocation = MyForm.Items.Add("task", BoFormItemTypes.it_BUTTON).Specific as Button;
            if (taskAllocation != null)
            {
                taskAllocation.Caption = "任务分配";
                taskAllocation.Item.Height = MyForm.Items.Item("1").Height;
                taskAllocation.Item.Width = MyForm.Items.Item("1").Width;
                taskAllocation.ClickAfter += taskAllocation_ClickAfter;
            }


            requestC = MyForm.Items.Add("requestC", BoFormItemTypes.it_BUTTON).Specific as Button;
            if (requestC != null)
            {
                requestC.Caption = "生成采购订单";
                requestC.Item.Height = MyForm.Items.Item("1").Height;
                requestC.Item.Width = MyForm.Items.Item("1").Width + 10;
                requestC.ClickAfter += requestC_ClickAfter;
            }

            DocNum = MyForm.Items.Item("8").Specific as EditText;
        }

        private void requestC_ClickAfter(object sboobject, SBOItemEventArg pval)
        {
            if (MyForm.Mode != BoFormMode.fm_OK_MODE)
            {
                Globle.Application.SetStatusBarMessage("非确定状态", BoMessageTime.bmt_Short);
                return;
            }

            var pqType = OPRQ.GetValue("U_PQType", 0);

            if (pqType != "T")
            {
                Globle.Application.SetStatusBarMessage("此单据非分配类型！", BoMessageTime.bmt_Short);
                return;
            }
            var lineVendors = new List<string>();

            for (var i = 0; i < PRQ1.Size; i++)
            {
                var item = PRQ1.GetValue("ItemCode", i).Trim();
                var flg = PRQ1.GetValue("U_Select", i).Trim();
                var lineStatus = PRQ1.GetValue("LineStatus", i).Trim();
                var lineVendor = PRQ1.GetValue("LineVendor", i).Trim();
                if (!string.IsNullOrEmpty(item) && lineStatus == "O" && !string.IsNullOrEmpty(lineVendor)&&flg=="Y")
                {
                    if (!lineVendors.Contains(lineVendor))
                    {
                        lineVendors.Add(lineVendor);
                    }
                }
            }

            var lineListSum = new List<int>();

            foreach (var lineVendor in lineVendors)
            {
                var tmpList = new List<int>();
                var oPurchaseOrders =
                    (Documents) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oPurchaseOrders);
                oPurchaseOrders.CardCode = lineVendor;
                oPurchaseOrders.TaxDate = DateTime.Today;
                oPurchaseOrders.DocDate = DateTime.ParseExact(OPRQ.GetValue("DocDate", 0).Trim(),
                    "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                oPurchaseOrders.DocDueDate = DateTime.ParseExact(OPRQ.GetValue("DocDueDate", 0).Trim(), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                oPurchaseOrders.RequriedDate = DateTime.ParseExact(OPRQ.GetValue("ReqDate", 0).Trim(),
                    "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                oPurchaseOrders.DocumentsOwner = Convert.ToInt32(OPRQ.GetValue("OwnerCode", 0).Trim());
             
              

                var docLines = oPurchaseOrders.Lines;
                var hasOne = true;

                var priceList = new List<double>();
                for (var i = 0; i < PRQ1.Size; i++)
                {
                    var item = PRQ1.GetValue("ItemCode", i).Trim();
                    var lineStatus = PRQ1.GetValue("LineStatus", i).Trim();
                    var tlineVendor = PRQ1.GetValue("LineVendor", i).Trim();
                    var flg = PRQ1.GetValue("U_Select", i).Trim();
                    if (!string.IsNullOrEmpty(item) && lineStatus == "O" && lineVendor == tlineVendor && flg == "Y")
                    {
                        oPurchaseOrders.SalesPersonCode = Convert.ToInt32(PRQ1.GetValue("SlpCode", i).Trim());
                        var count = docLines.Count;
                        if (hasOne)
                        {
                            count = docLines.Count - 1;
                            hasOne = false;
                        }
                        else
                        {
                            docLines.Add();
                        }
                        docLines.SetCurrentLine(count);
                        docLines.BaseType = Convert.ToInt32(OPRQ.GetValue("ObjType", 0).Trim());
                        docLines.BaseEntry = Convert.ToInt32(OPRQ.GetValue("DocEntry", 0).Trim());
                        docLines.BaseLine = Convert.ToInt32(PRQ1.GetValue("LineNum", i).Trim());
                        docLines.ItemCode = PRQ1.GetValue("ItemCode", i).Trim();
                        docLines.ShipDate = DateTime.ParseExact(PRQ1.GetValue("PQTReqDate", i).Trim(), "yyyyMMdd",
                            CultureInfo.CurrentCulture, DateTimeStyles.None);
                        docLines.Quantity = double.Parse(PRQ1.GetValue("Quantity", i).Trim());
                        docLines.PriceAfterVAT = double.Parse(PRQ1.GetValue("PriceAfVAT", i).Trim());

                        docLines.VatGroup = PRQ1.GetValue("VatGroup", i).Trim();
                        docLines.WarehouseCode = PRQ1.GetValue("WhsCode", i).Trim();
                        docLines.FreeText = PRQ1.GetValue("FreeTxt", i).Trim();
                        docLines.SalesPersonCode = Convert.ToInt32(PRQ1.GetValue("SlpCode", i).Trim());
                        for (var j = 0; j < docLines.UserFields.Fields.Count; j++)
                        {
                            docLines.UserFields.Fields.Item(j).Value =
                                PRQ1.GetValue(docLines.UserFields.Fields.Item(j).Name, i).Trim();
                        }
                        priceList.Add(double.Parse(PRQ1.GetValue("PriceAfVAT", i).Trim()));
                        tmpList.Add(i);
                    }
                }

                var retV = oPurchaseOrders.Add();
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
                    foreach (var line in tmpList)
                    {
                        lineListSum.Add(line);
                    }
                    var docEntry = Convert.ToInt32(Globle.DiCompany.GetNewObjectKey());
                    var r = oPurchaseOrders.GetByKey(docEntry);
                    var lines = oPurchaseOrders.Lines;

                    for (var i = 0; i < priceList.Count; i++)
                    {
                        lines.SetCurrentLine(i);

                        lines.PriceAfterVAT = priceList[i];
                    }

                    retV = oPurchaseOrders.Update();

                    if (retV != 0)
                    {
                        var errCode = 0;
                        string errMsg;
                        Globle.DiCompany.GetLastError(out errCode, out errMsg);
                        Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                            BoMessageTime.bmt_Short);
                    }
                }
            }


            if (lineListSum.Count < PRQ1.Size - 1 && lineListSum.Count > 0)
            {
                var oPurchaseRequests = (Documents) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oPurchaseRequest);
                oPurchaseRequests.DocumentsOwner = Convert.ToInt32(OPRQ.GetValue("OwnerCode", 0).Trim());
                oPurchaseRequests.TaxDate = DateTime.Today;
                oPurchaseRequests.DocDate = DateTime.ParseExact(OPRQ.GetValue("DocDate", 0).Trim(),"yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                oPurchaseRequests.DocDueDate = DateTime.ParseExact(OPRQ.GetValue("DocDueDate", 0).Trim(), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                oPurchaseRequests.RequriedDate = DateTime.ParseExact(OPRQ.GetValue("ReqDate", 0).Trim(),"yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                oPurchaseRequests.UserFields.Fields.Item("U_PQType").Value = "T";
                var docLines = oPurchaseRequests.Lines;
                var hasOne = true;

                for (var i = 0; i < PRQ1.Size; i++)
                {
                    var item = PRQ1.GetValue("ItemCode", i).Trim();
                    var tslpCode = PRQ1.GetValue("SlpCode", i).Trim();
                    var lineStatus = PRQ1.GetValue("LineStatus", i).Trim();
                    if (!string.IsNullOrEmpty(item) && lineStatus == "O" && !lineListSum.Contains(i))
                    {
                        var count = docLines.Count;
                        if (hasOne)
                        {
                            count = docLines.Count - 1;
                            hasOne = false;
                        }
                        else
                        {
                            docLines.Add();
                        }

                        docLines.SetCurrentLine(count);
                        docLines.ItemCode = item;
                        docLines.LineVendor = PRQ1.GetValue("LineVendor", i).Trim();
                        docLines.RequiredDate =  DateTime.ParseExact(PRQ1.GetValue("PQTReqDate", i).Trim(), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                        docLines.Quantity = double.Parse(PRQ1.GetValue("Quantity", i).Trim());
                        docLines.VatGroup = PRQ1.GetValue("VatGroup", i).Trim();
                        docLines.PriceAfterVAT =double.Parse(PRQ1.GetValue("PriceAfVAT", i).Trim());
                        docLines.WarehouseCode = PRQ1.GetValue("WhsCode", i).Trim();
                        docLines.SalesPersonCode = Convert.ToInt32(tslpCode);
                        docLines.FreeText = PRQ1.GetValue("FreeTxt", i).Trim();
                       
                        for (var j = 0; j < docLines.UserFields.Fields.Count; j++)
                        {
                            docLines.UserFields.Fields.Item(j).Value =
                                PRQ1.GetValue(docLines.UserFields.Fields.Item(j).Name, i).Trim();
                        }
                        docLines.UserFields.Fields.Item("U_DocDate").Value = DateTime.ParseExact(OPRQ.GetValue("DocDate", 0).Trim(), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                    }
                }
                var retV = oPurchaseRequests.Add();
                if (retV != 0)
                {
                    var errCode = 0;
                    string errMsg;
                    Globle.DiCompany.GetLastError(out errCode, out errMsg);
                    Globle.Application.SetStatusBarMessage(errMsg + " errorCode:" + errCode,
                        BoMessageTime.bmt_Short);
                }
            }

            if (lineListSum.Count > 0)
            {
                var oPurchaseRequests =(Documents) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oPurchaseRequest);
                var hasFind = oPurchaseRequests.GetByKey(int.Parse(OPRQ.GetValue("DocEntry", 0).Trim()));
                if (hasFind)
                {
                    var lines = oPurchaseRequests.Lines;
                    foreach (var line in lineListSum)
                    {
                        lines.SetCurrentLine(line);
                        lines.LineStatus = BoStatus.bost_Close;
                    }


                    for (var i = PRQ1.Size - 1; i > -1; i--)
                    {
                        var item = PRQ1.GetValue("ItemCode", i).Trim();
                        if (!string.IsNullOrEmpty(item) && !lineListSum.Contains(i))
                        {
                            lines.SetCurrentLine(i);
                            if (lines.LineStatus== BoStatus.bost_Open)
                            {
                                lines.Delete();
                            }
                        }
                    }

                    var retV = oPurchaseRequests.Update();
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
                        Globle.Application.Menus.Item("1304").Activate();
                    }
                }
            }
        }

        private void taskAllocation_ClickAfter(object sboobject, SBOItemEventArg pval)
        {
            if (MyForm.Mode != BoFormMode.fm_OK_MODE)
            {
                Globle.Application.SetStatusBarMessage("非确定状态", BoMessageTime.bmt_Short);
                return;
            }

            var pqType = OPRQ.GetValue("U_PQType", 0);

            if (pqType != "R")
            {
                Globle.Application.SetStatusBarMessage("此单据非任务类型！", BoMessageTime.bmt_Short);
                return;
            }
            var slpCodes = new List<string>();

            for (var i = 0; i < PRQ1.Size; i++)
            {
                var item = PRQ1.GetValue("ItemCode", i).Trim();
                var lineStatus = PRQ1.GetValue("LineStatus", i).Trim();
                if (!string.IsNullOrEmpty(item) && lineStatus == "O")
                {
                    var slpCode = PRQ1.GetValue("SlpCode", i).Trim();
                    if (slpCode != "-1" && !slpCodes.Contains(slpCode))
                    {
                        slpCodes.Add(slpCode);
                    }
                }
            }
            if (slpCodes.Count > 0)
            {
                var tmpBuilder = new StringBuilder("(");
                for (var i = 0; i < slpCodes.Count; i++)
                {
                    tmpBuilder.Append(slpCodes[i]).Append(",");
                }
                tmpBuilder = tmpBuilder.Remove(tmpBuilder.Length - 1, 1).Append(")");
                var sql =
                    "SELECT T0.\"salesPrson\",IFNULL(T2.\"DocEntry\",'-1') as \"DocEntry\",T0.\"empID\" FROM OHEM T0  INNER JOIN OSLP T1 ON T0.\"salesPrson\" = T1.\"SlpCode\" LEFT JOIN OPRQ T2 ON T0.\"empID\"= T2.\"OwnerCode\" AND T2.\"U_PQType\"='T' AND T2.\"CANCELED\"='N' AND T2.\"DocStatus\"='O'  where T0.\"salesPrson\" in " +
                    tmpBuilder;

                tmpTable.ExecuteQuery(sql);


                if (!tmpTable.IsEmpty)
                {
                    var lineListSum = new List<int>();

                    foreach (var slpCode in slpCodes)
                    {
                        var has = false;
                        var docEntry = -1;
                        for (var i = 0; i < tmpTable.Rows.Count; i++)
                        {
                            if (tmpTable.GetValue("salesPrson", i).ToString() == slpCode &&
                                tmpTable.GetValue("DocEntry", i).ToString() != "-1")
                            {
                                docEntry = (int) tmpTable.GetValue("DocEntry", i);
                                has = true;
                                break;
                            }
                        }
                        var oPurchaseRequests =
                            (Documents) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oPurchaseRequest);
                        var tmpList = new List<int>();
                        if (has)
                        {
                            if (oPurchaseRequests != null)
                            {
                                var hasFind = oPurchaseRequests.GetByKey(docEntry);

                                if (hasFind)
                                {
                                    var docLines = oPurchaseRequests.Lines;
                                    for (var i = 0; i < PRQ1.Size; i++)
                                    {
                                        var item = PRQ1.GetValue("ItemCode", i).Trim();
                                        var lineStatus = PRQ1.GetValue("LineStatus", i).Trim();
                                        if (!string.IsNullOrEmpty(item) && lineStatus == "O")
                                        {
                                            var tslpCode = PRQ1.GetValue("SlpCode", i).Trim();
                                            if (slpCode == tslpCode)
                                            {
                                                var count = docLines.Count;
                                                docLines.Add();
                                                docLines.SetCurrentLine(count);
                                                docLines.ItemCode = item;
                                                docLines.LineVendor = PRQ1.GetValue("LineVendor", i).Trim();
                                                docLines.RequiredDate =
                                                    DateTime.ParseExact(PRQ1.GetValue("PQTReqDate", i).Trim(),
                                                        "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                                                docLines.Quantity = double.Parse(PRQ1.GetValue("Quantity", i).Trim());
                                                docLines.VatGroup = PRQ1.GetValue("VatGroup", i).Trim();
                                                docLines.PriceAfterVAT =
                                                    double.Parse(PRQ1.GetValue("PriceAfVAT", i).Trim());
                                                docLines.WarehouseCode = PRQ1.GetValue("WhsCode", i).Trim();
                                                docLines.SalesPersonCode = Convert.ToInt32(slpCode);
                                                docLines.FreeText = PRQ1.GetValue("FreeTxt", i).Trim();
                                                for (var j = 0; j < docLines.UserFields.Fields.Count; j++)
                                                {
                                                    docLines.UserFields.Fields.Item(j).Value =
                                                        PRQ1.GetValue(docLines.UserFields.Fields.Item(j).Name, i).Trim();
                                                }
                                                docLines.UserFields.Fields.Item("U_DocDate").Value = DateTime.ParseExact(OPRQ.GetValue("DocDate", 0).Trim(), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                                                tmpList.Add(i);
                                            }
                                        }
                                    }
                                }
                                var retV = oPurchaseRequests.Update();
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
                                    foreach (var line in tmpList)
                                    {
                                        lineListSum.Add(line);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var ownerCode = "";
                            for (var i = 0; i < tmpTable.Rows.Count; i++)
                            {
                                if (tmpTable.GetValue("salesPrson", i).ToString() == slpCode)
                                {
                                    ownerCode = tmpTable.GetValue("empID", i).ToString();
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(ownerCode) && oPurchaseRequests != null)
                            {
                                oPurchaseRequests.DocumentsOwner = Convert.ToInt32(ownerCode);

                                oPurchaseRequests.TaxDate = DateTime.Today;
                                oPurchaseRequests.DocDate = DateTime.ParseExact(OPRQ.GetValue("DocDate", 0).Trim(),
                                    "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                                oPurchaseRequests.DocDueDate = DateTime.ParseExact(
                                    OPRQ.GetValue("DocDueDate", 0).Trim(),
                                    "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                                oPurchaseRequests.RequriedDate = DateTime.ParseExact(OPRQ.GetValue("ReqDate", 0).Trim(),
                                    "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                                oPurchaseRequests.UserFields.Fields.Item("U_PQType").Value = "T";
                                oPurchaseRequests.SalesPersonCode = Convert.ToInt32(slpCode);
                                var docLines = oPurchaseRequests.Lines;
                                var hasOne = true;

                                for (var i = 0; i < PRQ1.Size; i++)
                                {
                                    var item = PRQ1.GetValue("ItemCode", i).Trim();
                                    var lineStatus = PRQ1.GetValue("LineStatus", i).Trim();
                                    if (!string.IsNullOrEmpty(item) && lineStatus == "O")
                                    {
                                        var tslpCode = PRQ1.GetValue("SlpCode", i).Trim();
                                        if (slpCode == tslpCode)
                                        {
                                            var count = docLines.Count;
                                            if (hasOne)
                                            {
                                                count = docLines.Count - 1;
                                                hasOne = false;
                                            }
                                            else
                                            {
                                                docLines.Add();
                                            }

                                            docLines.SetCurrentLine(count);
                                            docLines.ItemCode = item;
                                            docLines.LineVendor = PRQ1.GetValue("LineVendor", i).Trim();
                                            docLines.RequiredDate =
                                                DateTime.ParseExact(PRQ1.GetValue("PQTReqDate", i).Trim(),
                                                    "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                                            docLines.Quantity = double.Parse(PRQ1.GetValue("Quantity", i).Trim());
                                            docLines.VatGroup = PRQ1.GetValue("VatGroup", i).Trim();
                                            docLines.PriceAfterVAT =
                                                double.Parse(PRQ1.GetValue("PriceAfVAT", i).Trim());
                                            docLines.WarehouseCode = PRQ1.GetValue("WhsCode", i).Trim();
                                            docLines.SalesPersonCode = Convert.ToInt32(slpCode);
                                            docLines.FreeText = PRQ1.GetValue("FreeTxt", i).Trim();
                                            for (var j = 0; j < docLines.UserFields.Fields.Count; j++)
                                            {
                                                docLines.UserFields.Fields.Item(j).Value =
                                                    PRQ1.GetValue(docLines.UserFields.Fields.Item(j).Name, i).Trim();
                    
                                            }
                                            docLines.UserFields.Fields.Item("U_DocDate").Value = DateTime.ParseExact(OPRQ.GetValue("DocDate", 0).Trim(), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                                            tmpList.Add(i);
                                        }
                                    }
                                }


                                var retV = oPurchaseRequests.Add();
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
                                    foreach (var line in tmpList)
                                    {
                                        lineListSum.Add(line);
                                    }
                                }
                            }
                        }
                    }


                    if (lineListSum.Count > 0)
                    {
                        var oPurchaseRequests =
                            (Documents) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oPurchaseRequest);
                        var hasFind = oPurchaseRequests.GetByKey(int.Parse(OPRQ.GetValue("DocEntry", 0).Trim()));
                        if (hasFind)
                        {
                            var lines = oPurchaseRequests.Lines;
                            foreach (var line in lineListSum)
                            {
                                lines.SetCurrentLine(line);
                                lines.LineStatus = BoStatus.bost_Close;
                            }
                            var retV = oPurchaseRequests.Update();
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
                                Globle.Application.Menus.Item("1304").Activate();
                            }
                        }
                    }
                }
            }
        }

        public override void ItemEventHandler(string formUid, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                Formresize();
            }
        }

        private void Formresize()
        {
            taskAllocation.Item.Top = MyForm.Items.Item("1").Top;
            taskAllocation.Item.Left = MyForm.Items.Item("1").Width*2 + MyForm.Items.Item("1").Left + 10;
            requestC.Item.Top = MyForm.Items.Item("1").Top;
            requestC.Item.Left = MyForm.Items.Item("1").Width*3 + MyForm.Items.Item("1").Left + 15;
        }

        public override void FormDataLoad(ref BusinessObjectInfo businessobjectinfo, ref bool bubbleevent)
        {
            if (!businessobjectinfo.BeforeAction)
            {
                var pqType = OPRQ.GetValue("U_PQType", 0) == "" || OPRQ.GetValue("U_PQType", 0) == null
                    ? "T"
                    : OPRQ.GetValue("U_PQType", 0);

                switch (pqType)
                {
                    case "T":
                        requestC.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,BoModeVisualBehavior.mvb_False);
                        requestC.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, 1, BoModeVisualBehavior.mvb_True);
                        taskAllocation.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1, BoModeVisualBehavior.mvb_False);
                        break;
                    default:
                        requestC.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,BoModeVisualBehavior.mvb_False);
                        taskAllocation.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,BoModeVisualBehavior.mvb_False);
                        taskAllocation.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, 1, BoModeVisualBehavior.mvb_True);
                        break;
                }
            }
        }
    }
}