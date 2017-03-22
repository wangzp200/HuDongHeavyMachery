using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using HuDongHeavyMachinery.Code.Model;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020030
{
    public class COR020030 : SwBaseForm
    {
        private const int pageNum = 50;
        private readonly List<GridRowInfo> gridRowInfos = new List<GridRowInfo>();
        private EditText MachineNo;
        private string MachineryNoValue;
        private EditText PurchaseNo;
        private string PurchaseNoValue;

        private int currentPage;
        private Button dowmButton;
        private DataTable dt;
        private Grid grid;
        private UserDataSource mUserDataSource;
        private Button noButton;
        private Button okButton;
        private UserDataSource pUserDataSource;
        private Item rectangle;
        private Button searchButton;
        private StaticText showText;
        private int totalPages;
        private Button upButton;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            okButton = (Button) MyForm.Items.Item("5").Specific;
            okButton.PressedAfter += okButton_PressedAfter;
            noButton = (Button) MyForm.Items.Item("6").Specific;
            noButton.PressedAfter += noButton_PressedAfter;
            searchButton = (Button) MyForm.Items.Item("8").Specific;
            searchButton.PressedAfter += searchButton_PressedAfter;
            grid = (Grid) MyForm.Items.Item("7").Specific;
            rectangle = MyForm.Items.Item("9");
            dt = MyForm.DataSources.DataTables.Add("dt");

            showText = (StaticText) MyForm.Items.Item("13").Specific;


            MachineNo = (EditText) MyForm.Items.Item("3").Specific;
            mUserDataSource = MyForm.DataSources.UserDataSources.Item("3");
            PurchaseNo = (EditText) MyForm.Items.Item("4").Specific;
            pUserDataSource = MyForm.DataSources.UserDataSources.Item("4");

            upButton = (Button) MyForm.Items.Item("10").Specific;
            upButton.PressedAfter += upButton_PressedAfter;
            dowmButton = (Button) MyForm.Items.Item("11").Specific;
            dowmButton.PressedAfter += dowmButton_PressedAfter;
            MachineryNoValue = string.Empty;
            PurchaseNoValue = string.Empty;
            var sqlbuffer = new StringBuilder();
            //sqlbuffer.Append("select count(*) as \"totalPages\" from OITM where \"SellItem\"='Y' and 1=0");
            //dt.ExecuteQuery(sqlbuffer.ToString());
            //if (!dt.IsEmpty)
            //{
            //    var totalPagess = int.Parse(dt.GetValue("totalPages", 0).ToString());
            //    totalPages = (totalPagess/pageNum) + (totalPagess%pageNum > 0 ? 1 : 0);
            //    SetInformation(currentPage);
            //}
            FormResize();
        }

        private void dowmButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var itemCode = dt.GetValue("ItemCode", i).ToString();
                var select = dt.GetValue("select", i).ToString();

                if (select == "N")
                {
                    foreach (var gridRowInfo in gridRowInfos)
                    {
                        if (gridRowInfo.ItemCode == itemCode)
                        {
                            gridRowInfos.Remove(gridRowInfo);
                            break;
                        }
                    }
                }
                else
                {
                    var exists = false;
                    foreach (var gridRowInfo in gridRowInfos)
                    {
                        if (gridRowInfo.ItemCode == itemCode)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        var gridRowInfo = new GridRowInfo
                        {
                            ItemCode = itemCode,
                            ItemName = dt.GetValue("ItemName", i).ToString(),
                            MachineryNo = dt.GetValue("U_MachineryNo", i) == null ? "" : dt.GetValue("U_MachineryNo", i).ToString(),
                            PurchaseNo = dt.GetValue("U_PurchaseNo", i) == null ? "" : dt.GetValue("U_PurchaseNo", i).ToString(),
                            Quantity = double.Parse(dt.GetValue("Quantity", i).ToString()),
                            LeadTime = double.Parse(dt.GetValue("LeadTime", i).ToString()),
                            FrgnName = dt.GetValue("FrgnName", i) == null ? "" : dt.GetValue("FrgnName", i).ToString()
                        };
                        gridRowInfos.Add(gridRowInfo);
                    }
                }
            }

            currentPage = currentPage + 1;
            SetInformation(currentPage);
        }

        private void upButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var itemCode = dt.GetValue("ItemCode", i).ToString();
                var select = dt.GetValue("select", i).ToString();

                if (select == "N")
                {
                    foreach (var gridRowInfo in gridRowInfos)
                    {
                        if (gridRowInfo.ItemCode == itemCode)
                        {
                            gridRowInfos.Remove(gridRowInfo);
                            break;
                        }
                    }
                }
                else
                {
                    var exists = false;
                    foreach (var gridRowInfo in gridRowInfos)
                    {
                        if (gridRowInfo.ItemCode == itemCode)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        var gridRowInfo = new GridRowInfo
                        {
                            ItemCode = itemCode,
                            ItemName = dt.GetValue("ItemName", i).ToString(),
                            MachineryNo = dt.GetValue("U_MachineryNo", i) == null ? "" : dt.GetValue("U_MachineryNo", i).ToString(),
                            PurchaseNo = dt.GetValue("U_PurchaseNo", i) == null ? "" : dt.GetValue("U_PurchaseNo", i).ToString(),
                            Quantity = double.Parse(dt.GetValue("Quantity", i).ToString()),
                            LeadTime = double.Parse(dt.GetValue("LeadTime", i).ToString()),
                            FrgnName = dt.GetValue("FrgnName", i) == null ? "" : dt.GetValue("FrgnName", i).ToString()
                        };
                        gridRowInfos.Add(gridRowInfo);
                    }
                }
            }
            currentPage = currentPage - 1;
            SetInformation(currentPage);
        }

        private void searchButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            PurchaseNoValue = pUserDataSource.Value;
            MachineryNoValue = mUserDataSource.Value;
            var sqlbuffer = new StringBuilder();

            //if (string.IsNullOrEmpty(PurchaseNoValue) && string.IsNullOrEmpty(MachineryNoValue))
            //{
            //    sqlbuffer.Append("select count(*) as \"totalPages\" from OITM where \"SellItem\"='Y'");
            //}
            //else 
            if (!string.IsNullOrEmpty(PurchaseNoValue) && !string.IsNullOrEmpty(MachineryNoValue))
            {
                sqlbuffer.Append(
                    "select count(T5.\"U_ItemCode\") as \"totalPages\" from (select distinct \"U_ItemCode\",T2.\"ItemName\" from \"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" inner join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" where T2.\"SellItem\"='Y' and T0.\"U_MachineryNo\"='");
                sqlbuffer.Append(MachineryNoValue).Append("' and T1.\"U_PurchaseNo\"='");
                sqlbuffer.Append(PurchaseNoValue).Append("')T5");
                dt.ExecuteQuery(sqlbuffer.ToString());
                if (!dt.IsEmpty)
                {
                    var totalPagess = int.Parse(dt.GetValue("totalPages", 0).ToString());
                    totalPages = (totalPagess / pageNum) + (totalPagess % pageNum > 0 ? 1 : 0);
                    SetInformation(currentPage);
                }

                currentPage = 0;
                SetInformation(currentPage);
            }
            //else if (!string.IsNullOrEmpty(PurchaseNoValue) && string.IsNullOrEmpty(MachineryNoValue))
            //{
            //    sqlbuffer.Append(
            //        "select count(T5.\"U_ItemCode\") as \"totalPages\" from (select distinct \"U_ItemCode\",T2.\"ItemName\"  from \"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" inner join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" where T2.\"SellItem\"='Y' ");
            //    sqlbuffer.Append("and T1.\"U_PurchaseNo\"='");
            //    sqlbuffer.Append(PurchaseNoValue).Append("')T5");
            //}
            //else if (string.IsNullOrEmpty(PurchaseNoValue) && !string.IsNullOrEmpty(MachineryNoValue))
            //{
            //    sqlbuffer.Append(
            //        "select count(T5.\"U_ItemCode\") as \"totalPages\" from (select distinct \"U_ItemCode\",T2.\"ItemName\"  from \"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" inner join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" where T2.\"SellItem\"='Y' and T0.\"U_MachineryNo\"='");
            //    sqlbuffer.Append(PurchaseNoValue).Append("')T5");
            //}


            //dt.ExecuteQuery(sqlbuffer.ToString());
            //if (!dt.IsEmpty)
            //{
            //    var totalPagess = int.Parse(dt.GetValue("totalPages", 0).ToString());
            //    totalPages = (totalPagess/pageNum) + (totalPagess%pageNum > 0 ? 1 : 0);
            //    SetInformation(currentPage);
            //}

            //currentPage = 0;
            //SetInformation(currentPage);
        }

        private void noButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            Globle.SwFormsList[MyFatherUid].MySonUid = null;
            MyForm.Close();
        }

        private void okButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var itemCode = dt.GetValue("ItemCode", i).ToString();
                var select = dt.GetValue("select", i).ToString();

                if (select == "Y")
                {
                    var exists = false;
                    foreach (var gridRowInfo in gridRowInfos)
                    {
                        if (gridRowInfo.ItemCode == itemCode)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists && !string.IsNullOrEmpty(itemCode))
                    {
                        var gridRowInfo = new GridRowInfo
                        {
                            ItemCode = itemCode,
                            ItemName = dt.GetValue("ItemName", i).ToString(),
                            MachineryNo = dt.GetValue("U_MachineryNo", i) == null ? "" : dt.GetValue("U_MachineryNo", i).ToString(),
                            PurchaseNo = dt.GetValue("U_PurchaseNo", i) == null ? "" : dt.GetValue("U_PurchaseNo", i).ToString(),
                            Quantity = double.Parse(dt.GetValue("Quantity", i).ToString()),
                            LeadTime = double.Parse(dt.GetValue("LeadTime", i).ToString()),
                            FrgnName = dt.GetValue("FrgnName", i) == null ? "" : dt.GetValue("FrgnName", i).ToString()
                        };
                        gridRowInfos.Add(gridRowInfo);
                    }
                }
            }
            var swBase = Globle.SwFormsList[MyFatherUid];

            swBase.SonFormCloseEventHandler(gridRowInfos, this);
        }

        public override void ItemEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                FormResize();
            }
            if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                Globle.SwFormsList[MyFatherUid].MySonUid = null;
            }
        }

        public void SetInformation(int page)
        {
            try
            {
                MyForm.Freeze(true);
                var sqlbuffer = new StringBuilder();
                //if (string.IsNullOrEmpty(PurchaseNoValue) && string.IsNullOrEmpty(MachineryNoValue))
                //{
                //    sqlbuffer.Append("select 'N' as \"select\",T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\",T2.\"ItemName\" ,T2.\"FrgnName\",T3.\"Price\"/((T5.\"Rate\"+100)/100.0) as \"PriceBeforeVat\",T2.\"LeadTime\",'1.0' as \"Quantity\",T3.\"Price\",T5.\"Rate\",T2.\"ItemCode\" ");
                //    sqlbuffer.Append(
                //        "from dummy T12,\"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" ");
                //    sqlbuffer.Append("right join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" ");
                //    sqlbuffer.Append("LEFT JOIN ITM1 T3 ON T3.\"ItemCode\" = T2.\"ItemCode\" ");
                //    sqlbuffer.Append("LEFT JOIN OPLN T4 ON T4.\"ListNum\" = T3.\"PriceList\" ");
                //    sqlbuffer.Append("LEFT JOIN OVTG T5 ON T5.\"Code\" = T2.\"VatGourpSa\" ");
                //    sqlbuffer.Append("where T2.\"SellItem\"='Y'");
                //    sqlbuffer.Append("and T4.\"ListName\"='").Append("销售表价").Append("' ");
                //    sqlbuffer.Append(" order by T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\""); 
                //}
                //else 
                if (!string.IsNullOrEmpty(PurchaseNoValue) && !string.IsNullOrEmpty(MachineryNoValue))
                {
                    sqlbuffer.Append(
                        "select 'N' as \"select\",T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\",T2.\"ItemName\" ,T2.\"FrgnName\",T3.\"Price\"/((T5.\"Rate\"+100)/100.0) as \"PriceBeforeVat\",T2.\"LeadTime\",'1.0' as \"Quantity\",T3.\"Price\",T5.\"Rate\",T2.\"ItemCode\" ");
                    sqlbuffer.Append(
                        "from dummy T12,\"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" ");
                    sqlbuffer.Append("inner join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" ");
                    sqlbuffer.Append("LEFT JOIN ITM1 T3 ON T3.\"ItemCode\" = T2.\"ItemCode\" ");
                    sqlbuffer.Append("LEFT JOIN OPLN T4 ON T4.\"ListNum\" = T3.\"PriceList\" ");
                    sqlbuffer.Append("LEFT JOIN OVTG T5 ON T5.\"Code\" = T2.\"VatGourpSa\" ");
                    sqlbuffer.Append("where T2.\"SellItem\"='Y' ");
                    sqlbuffer.Append("and T4.\"ListName\"='").Append("销售表价").Append("' ");
                    sqlbuffer.Append("and T0.\"U_MachineryNo\"='").Append(MachineryNoValue).Append("' ");
                    sqlbuffer.Append("and T1.\"U_PurchaseNo\"='").Append(PurchaseNoValue).Append("' order by T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\"");
                }
                else { return; }
                //else if (!string.IsNullOrEmpty(PurchaseNoValue) && string.IsNullOrEmpty(MachineryNoValue))
                //{
                //    sqlbuffer.Append(
                //        "select 'N' as \"select\",T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\",T2.\"ItemName\" ,T2.\"FrgnName\",T3.\"Price\"/((T5.\"Rate\"+100)/100.0) as \"PriceBeforeVat\",T2.\"LeadTime\",'1.0' as \"Quantity\",T3.\"Price\",T5.\"Rate\",T2.\"ItemCode\" ");
                //    sqlbuffer.Append(
                //        "from dummy T12,\"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" ");
                //    sqlbuffer.Append("inner join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" ");
                //    sqlbuffer.Append("LEFT JOIN ITM1 T3 ON T3.\"ItemCode\" = T2.\"ItemCode\" ");
                //    sqlbuffer.Append("LEFT JOIN OPLN T4 ON T4.\"ListNum\" = T3.\"PriceList\" ");
                //    sqlbuffer.Append("LEFT JOIN OVTG T5 ON T5.\"Code\" = T2.\"VatGourpSa\" ");
                //    sqlbuffer.Append("where T2.\"SellItem\"='Y' ");
                //    sqlbuffer.Append("and T4.\"ListName\"='").Append("销售表价").Append("' ");
                //    sqlbuffer.Append("and T1.\"U_PurchaseNo\"='").Append(PurchaseNoValue).Append("' order by T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\"");
                //}
                //else if (string.IsNullOrEmpty(PurchaseNoValue) && !string.IsNullOrEmpty(MachineryNoValue))
                //{
                //    sqlbuffer.Append(
                //        "select 'N' as \"select\",T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\",T2.\"ItemName\" ,T2.\"FrgnName\",T3.\"Price\"/((T5.\"Rate\"+100)/100.0) as \"PriceBeforeVat\",T2.\"LeadTime\",'1.0' as \"Quantity\",T3.\"Price\",T5.\"Rate\",T2.\"ItemCode\" ");
                //    sqlbuffer.Append(
                //        "from dummy T12,\"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" ");
                //    sqlbuffer.Append("inner join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" ");
                //    sqlbuffer.Append("LEFT JOIN ITM1 T3 ON T3.\"ItemCode\" = T2.\"ItemCode\" ");
                //    sqlbuffer.Append("LEFT JOIN OPLN T4 ON T4.\"ListNum\" = T3.\"PriceList\" ");
                //    sqlbuffer.Append("LEFT JOIN OVTG T5 ON T5.\"Code\" = T2.\"VatGourpSa\" ");
                //    sqlbuffer.Append("where T2.\"SellItem\"='Y' ");
                //    sqlbuffer.Append("and T4.\"ListName\"='").Append("销售表价").Append("' ");
                //    sqlbuffer.Append("and T0.\"U_MachineryNo\"='").Append(MachineryNoValue).Append("' order by T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\"");
                //}
                sqlbuffer.Append("limit").Append(" ");
                sqlbuffer.Append(pageNum).Append(" ");
                sqlbuffer.Append("OFFSET").Append(" ");
                sqlbuffer.Append(currentPage*pageNum);
                dt.ExecuteQuery(sqlbuffer.ToString());
                if (currentPage == 0)
                {
                    upButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_False);
                }
                else
                {
                    upButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_True);
                }
                if (currentPage + 1 >= totalPages)
                {
                    dowmButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_False);
                }
                else
                {
                    dowmButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1,
                        BoModeVisualBehavior.mvb_True);
                }

                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    var itemCode = dt.GetValue("ItemCode", i).ToString();
                    foreach (var gridRowInfo in gridRowInfos)
                    {
                        if (itemCode == gridRowInfo.ItemCode)
                        {
                            dt.SetValue("select", i, "Y");
                        }
                    }
                }

                grid.DataTable = dt;
                grid.RowHeaders.TitleObject.Caption = "#";

                var column = grid.Columns.Item("ItemCode");
                column.Visible = false;

                column = grid.Columns.Item("select");
                column.Type = BoGridColumnType.gct_CheckBox;
                column.TitleObject.Caption = "选择";

                var editTextColumn = (EditTextColumn) grid.Columns.Item("U_MachineryNo");
                editTextColumn.TitleObject.Caption = "机号";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn) grid.Columns.Item("U_PurchaseNo");
                editTextColumn.TitleObject.Caption = "订货号";
                editTextColumn.TitleObject.Sortable = true;


                editTextColumn = (EditTextColumn) grid.Columns.Item("ItemName");
                editTextColumn.TitleObject.Caption = "物料描述";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn) grid.Columns.Item("FrgnName");
                editTextColumn.TitleObject.Caption = "物料外文名";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn) grid.Columns.Item(6);
                editTextColumn.TitleObject.Caption = "税前价格";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn) grid.Columns.Item("LeadTime");
                editTextColumn.TitleObject.Caption = "提前期";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn) grid.Columns.Item("Quantity");
                editTextColumn.TitleObject.Caption = "数量";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("Price");
                editTextColumn.TitleObject.Caption = "税后价格";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("Rate");
                editTextColumn.TitleObject.Caption = "税率";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("PriceBeforeVat");
                editTextColumn.TitleObject.Caption = "税前价格";
                editTextColumn.TitleObject.Sortable = true;

                for (var i = 0; i < grid.Rows.Count; i++)
                {
                    grid.RowHeaders.SetText(i, (page*pageNum + i + 1) + "");
                }

                for (var i = 1; i < grid.Columns.Count; i++)
                {
                    if (grid.Columns.Item(i).TitleObject.Caption != "数量")
                    {
                        grid.Columns.Item(i).Editable = false;
                    }
                }


                var newRgbColor1 = Color.FromArgb(255, 144, 230, 255);
                var rowForeColor1 = newRgbColor1.R | (newRgbColor1.G << 8) | (newRgbColor1.B << 16);

                var newRgbColor2 = Color.FromArgb(255, 202, 200, 255);
                var rowForeColor2 = newRgbColor2.R | (newRgbColor2.G << 8) | (newRgbColor2.B << 16);

                var colorIndex = 0;

                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    var tmp = i;
                    var hasSame = false;

                    if (dt.GetValue("U_MachineryNo", i) != null && dt.GetValue("U_PurchaseNo", i) != null)
                    {
                        var machineryNo1 = dt.GetValue("U_MachineryNo", i).ToString();
                        var purchaseNo1 = dt.GetValue("U_PurchaseNo", i).ToString();

                        for (var j = i + 1; j < dt.Rows.Count; j++)
                        {
                            var machineryNo2 = dt.GetValue("U_MachineryNo", j).ToString();
                            var purchaseNo2 = dt.GetValue("U_PurchaseNo", j).ToString();

                            if (machineryNo1 == machineryNo2 && purchaseNo1 == purchaseNo2)
                            {
                                for (var k = 1; k < grid.Columns.Count; k++)
                                {
                                    var gColumn = grid.Columns.Item(k);
                                    if (gColumn.Visible)
                                    {
                                        var rowForeColor = colorIndex%2 == 0 ? rowForeColor1 : rowForeColor2;
                                        grid.CommonSetting.SetCellBackColor(j + 1, k, rowForeColor);
                                    }
                                }
                                hasSame = true;
                                tmp = tmp + 1;
                            }
                        }

                        if (hasSame)
                        {
                            for (var k = 1; k < grid.Columns.Count; k++)
                            {
                                var gColumn = grid.Columns.Item(k);
                                if (gColumn.Visible)
                                {
                                    var rowForeColor = colorIndex%2 == 0 ? rowForeColor1 : rowForeColor2;
                                    grid.CommonSetting.SetCellBackColor(i + 1, k, rowForeColor);
                                }
                            }
                            colorIndex = colorIndex + 1;
                        }
                        i = tmp;
                    }
                }
                showText.Caption = (page + 1) + "/" + totalPages;
                MyForm.Resize(MyForm.Width, MyForm.Height);
            }
            catch (Exception exception)
            {
                Globle.Application.SetStatusBarMessage(exception.Message);
            }
            finally
            {
                MyForm.Freeze(false);
            }
        }

        private void FormResize()
        {
            rectangle.Width = grid.Item.Width + 8;
            rectangle.Height = grid.Item.Height + 10;
        }
    }
}