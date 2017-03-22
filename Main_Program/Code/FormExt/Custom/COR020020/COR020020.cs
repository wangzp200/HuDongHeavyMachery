using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HuDongHeavyMachinery.Code.Model;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020020
{
    public class COR020020 : SwBaseForm
    {
        private readonly List<GridRowInfo> gridRowInfos = new List<GridRowInfo>();
        private int currentPage;
        private Button dowmButton;
        private DataTable dt;
        private ExcelRowInfo[] excelRowInfos;
        private Grid grid;
        private Button noButton;
        private Button okButton;
        private int pageNum = 40;
        private Item rectangle;
        private StaticText showText;
        private int totalPages;
        private Button upButton;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            okButton = (Button) MyForm.Items.Item("5").Specific;
            okButton.PressedAfter += okButton_PressedAfter;
            noButton = (Button) MyForm.Items.Item("6").Specific;
            noButton.PressedAfter += noButton_PressedAfter;
            grid = (Grid) MyForm.Items.Item("7").Specific;
            rectangle = MyForm.Items.Item("9");
            showText = (StaticText) MyForm.Items.Item("8").Specific;

            upButton = (Button) MyForm.Items.Item("1000001").Specific;
            upButton.PressedAfter += upButton_PressedAfter;
            dowmButton = (Button) MyForm.Items.Item("1000002").Specific;
            dowmButton.PressedAfter += dowmButton_PressedAfter;

            dt = MyForm.DataSources.DataTables.Add("dt");
            dt.Columns.Add("select", BoFieldsType.ft_Text);
            dt.Columns.Add("MachineryNo", BoFieldsType.ft_Text);
            dt.Columns.Add("PurchaseNo", BoFieldsType.ft_Text);
            dt.Columns.Add("ItemName", BoFieldsType.ft_Text);
            dt.Columns.Add("FrgnName", BoFieldsType.ft_Text);
            dt.Columns.Add("PriceBeforeVat", BoFieldsType.ft_Quantity);
            dt.Columns.Add("LeadTime", BoFieldsType.ft_Quantity);
            dt.Columns.Add("Quantity", BoFieldsType.ft_Quantity);
            dt.Columns.Add("ItemCode", BoFieldsType.ft_Text);
            dt.Columns.Add("Price", BoFieldsType.ft_Quantity);
            dt.Columns.Add("Rate", BoFieldsType.ft_Quantity);
            dt.Columns.Add("Memo", BoFieldsType.ft_Text);
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
                    if (!exists && !string.IsNullOrEmpty(itemCode))
                    {
                        var gridRowInfo = new GridRowInfo
                        {
                            ItemCode = itemCode,
                            ItemName = dt.GetValue("ItemName", i).ToString(),
                            MachineryNo = dt.GetValue("MachineryNo", i).ToString(),
                            PurchaseNo = dt.GetValue("PurchaseNo", i).ToString(),
                            Quantity = double.Parse(dt.GetValue("Quantity", i).ToString()),
                            LeadTime = double.Parse(dt.GetValue("LeadTime", i).ToString()),
                            Memo = dt.GetValue("Memo", i).ToString(),
                            FrgnName = dt.GetValue("FrgnName", i).ToString()
                        };
                        gridRowInfos.Add(gridRowInfo);
                    }
                }
            }
            currentPage = currentPage + 1;
            SetGridInfo(currentPage);
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
                    if (!exists && !string.IsNullOrEmpty(itemCode))
                    {
                        var gridRowInfo = new GridRowInfo
                        {
                            ItemCode = itemCode,
                            ItemName = dt.GetValue("ItemName", i).ToString(),
                            MachineryNo = dt.GetValue("MachineryNo", i).ToString(),
                            PurchaseNo = dt.GetValue("PurchaseNo", i).ToString(),
                            Quantity = double.Parse(dt.GetValue("Quantity", i).ToString()),
                            LeadTime = double.Parse(dt.GetValue("LeadTime", i).ToString()),
                            Memo = dt.GetValue("Memo", i).ToString(),
                            FrgnName = dt.GetValue("FrgnName", i).ToString()
                        };
                        gridRowInfos.Add(gridRowInfo);
                    }
                }
            }
            currentPage = currentPage - 1;
            SetGridInfo(currentPage);
        }

        /// <summary>
        ///     初始化本窗体的参数
        /// </summary>
        /// <param name="obj"></param>
        public void SetInformation(object obj)
        {
            var excelRowInfolList = (List<ExcelRowInfo>) obj;
            excelRowInfos = new ExcelRowInfo[excelRowInfolList.Count()];
            excelRowInfolList.CopyTo(excelRowInfos);
            totalPages = (excelRowInfos.Length/pageNum) + (excelRowInfos.Length%pageNum > 0 ? 1 : 0);

            foreach (var excelRowInfo in excelRowInfos)
            {
                if (!string.IsNullOrEmpty(excelRowInfo.MachineryNo) && !string.IsNullOrEmpty(excelRowInfo.PurchaseNo))
                {
                    var gridRowInfo = new GridRowInfo
                    {
                        ItemCode = excelRowInfo.ItemCode,
                        ItemName = excelRowInfo.ItemName,
                        MachineryNo = excelRowInfo.MachineryNo,
                        PurchaseNo = excelRowInfo.PurchaseNo,
                        Quantity = excelRowInfo.Quantity,
                        LeadTime = excelRowInfo.LeadTime,
                        Memo = excelRowInfo.Memo,
                        FrgnName=excelRowInfo.FrgnName
                    };
                    gridRowInfos.Add(gridRowInfo);
                }
               
            }

            currentPage = 0;
            SetGridInfo(currentPage);
        }

        /// <summary>
        ///     设置Grid的值
        /// </summary>
        /// <param name="page"></param>
        private void SetGridInfo(int page)
        {
            try
            {
                MyForm.Freeze(true);


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


                dt.Rows.Clear();
                var row = 0;
                for (var i = page*pageNum;
                    i < ((page + 1)*pageNum > excelRowInfos.Length ? excelRowInfos.Length : (page + 1)*pageNum);
                    i++)
                {
                    var excelRowInfo = excelRowInfos[i];
                    dt.Rows.Add();
                    dt.SetValue("select", row, "Y");
                    dt.SetValue("MachineryNo", row, excelRowInfo.MachineryNo);
                    dt.SetValue("PurchaseNo", row, excelRowInfo.PurchaseNo);
                    dt.SetValue("ItemCode", row, excelRowInfo.ItemCode);
                    dt.SetValue("ItemName", row, excelRowInfo.ItemName);
                    dt.SetValue("FrgnName", row, excelRowInfo.FrgnName);
                    dt.SetValue("PriceBeforeVat", row, excelRowInfo.PriceBeforeVat);
                    dt.SetValue("LeadTime", row, excelRowInfo.LeadTime);
                    dt.SetValue("Quantity", row, excelRowInfo.Quantity);
                    dt.SetValue("Price", row, excelRowInfo.Price);
                    dt.SetValue("Rate", row, excelRowInfo.Rate);
                    dt.SetValue("Memo", row, excelRowInfo.Memo);
                    row ++;
                }

                grid.DataTable = dt;
                grid.RowHeaders.TitleObject.Caption = "#";

                var column = grid.Columns.Item("ItemCode");
                column.Visible = false;

                column = grid.Columns.Item("select");
                column.Type = BoGridColumnType.gct_CheckBox;
                column.TitleObject.Caption = "选择";

                var editTextColumn = (EditTextColumn)grid.Columns.Item("MachineryNo");
                editTextColumn.TitleObject.Caption = "机号";
                editTextColumn.TitleObject.Sortable=true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("PurchaseNo");
                editTextColumn.TitleObject.Caption = "订货号";
                editTextColumn.TitleObject.Sortable = true;


                editTextColumn = (EditTextColumn)grid.Columns.Item("ItemName");
                editTextColumn.TitleObject.Caption = "物料描述";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("FrgnName");
                editTextColumn.TitleObject.Caption = "物料外文名";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("PriceBeforeVat");
                editTextColumn.TitleObject.Caption = "税前价格";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("LeadTime");
                editTextColumn.TitleObject.Caption = "采购提前期";
                editTextColumn.TitleObject.Sortable = true;
                editTextColumn.Editable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("Quantity");
                editTextColumn.TitleObject.Caption = "数量";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("Price");
                editTextColumn.TitleObject.Caption = "税后价格";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("Rate");
                editTextColumn.TitleObject.Caption = "税率";
                editTextColumn.TitleObject.Sortable = true;

                editTextColumn = (EditTextColumn)grid.Columns.Item("Memo");
                editTextColumn.TitleObject.Caption = "备注";
                editTextColumn.TitleObject.Sortable = true;

                for (var i = 0; i < grid.Rows.Count; i++)
                {
                    grid.RowHeaders.SetText(i, (page*pageNum + i + 1) + "");
                }

                for (var i = 1; i < grid.Columns.Count; i++)
                {
                    grid.Columns.Item(i).Editable = false;
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
                    var machineryNo1 = dt.GetValue("MachineryNo", i).ToString();
                    var purchaseNo1 = dt.GetValue("PurchaseNo", i).ToString();

                    for (var j = i + 1; j < dt.Rows.Count; j++)
                    {
                        var machineryNo2 = dt.GetValue("MachineryNo", j).ToString();
                        var purchaseNo2 = dt.GetValue("PurchaseNo", j).ToString();

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
                    if (!exists && !string.IsNullOrEmpty(itemCode))
                    {
                        var gridRowInfo = new GridRowInfo
                        {
                            ItemCode = itemCode,
                            ItemName = dt.GetValue("ItemName", i).ToString(),
                            MachineryNo = dt.GetValue("MachineryNo", i).ToString(),
                            PurchaseNo = dt.GetValue("PurchaseNo", i).ToString(),
                            Quantity = double.Parse(dt.GetValue("Quantity", i).ToString()),
                            LeadTime = double.Parse(dt.GetValue("LeadTime", i).ToString()),
                            Memo = dt.GetValue("Memo", i).ToString(),
                            FrgnName = dt.GetValue("FrgnName", i).ToString()
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
                rectangle.Width = grid.Item.Width + 8;
                rectangle.Height = grid.Item.Height + 10;
            }
            if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                Globle.SwFormsList[MyFatherUid].MySonUid = null;
            }
        }
    }
}