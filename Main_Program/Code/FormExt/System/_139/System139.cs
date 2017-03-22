using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using HuDongHeavyMachinery.Code.FormExt.Custom.COR020020;
using HuDongHeavyMachinery.Code.Model;
using HuDongHeavyMachinery.Code.Util;
using SAPbouiCOM;
using SwissAddonFramework.Utils.Windows;

namespace HuDongHeavyMachinery.Code.FormExt.System._139

{
    public class System139 : SwBaseForm
    {
        private readonly object[] loactionInts = { 0, "" };
        private DBDataSource ORDR, RDR1;
        private Matrix aMatrix;
        private ComboBox copyFroCombox;
        private ComboBox copyToBoxCombox;
        private Button inputButton;
        private Column iteNameColumn;
        private Button loadButton;
        private UserDataSource QtySum;
        private StaticText QtyStatic;
        private EditText QtyEdit;
        private StaticText OwerLink;
        private EditText OwerCode;
        private Column QtyColumn;
        private Column ItemCodeColumn;
        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            inputButton = (Button)MyForm.Items.Add("inputB", BoFormItemTypes.it_BUTTON).Specific;
            loadButton = (Button)MyForm.Items.Add("loadB", BoFormItemTypes.it_BUTTON).Specific;
            copyFroCombox = (ComboBox)MyForm.Items.Item("10000330").Specific;
            copyToBoxCombox = (ComboBox)MyForm.Items.Item("10000329").Specific;

            inputButton.Caption = "批量导入数据";
            inputButton.Item.Width = copyFroCombox.Item.Width;
            inputButton.Item.Height = copyFroCombox.Item.Height;
            inputButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1, BoModeVisualBehavior.mvb_False);
            inputButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, 1, BoModeVisualBehavior.mvb_True);
            inputButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, 2, BoModeVisualBehavior.mvb_True);
            inputButton.PressedAfter += _IButtonEvents_PressedAfterEventHandler_inputButton;

            loadButton.Caption = "导出模板";
            loadButton.Item.Width = copyFroCombox.Item.Width;
            loadButton.Item.Height = copyFroCombox.Item.Height;
            loadButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1, BoModeVisualBehavior.mvb_False);
            loadButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, 1, BoModeVisualBehavior.mvb_True);
            loadButton.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, 2, BoModeVisualBehavior.mvb_True);
            loadButton.PressedAfter += _IButtonEvents_PressedAfterEventHandler_loadButton;


            aMatrix = (Matrix)MyForm.Items.Item("38").Specific;
            iteNameColumn = aMatrix.Columns.Item("3");

            iteNameColumn.DoubleClickAfter += iteNameColumn_DoubleClickAfter;

            iteNameColumn.ChooseFromListAfter += ItemCodeColumn_ChooseFromListAfter;

            ItemCodeColumn = aMatrix.Columns.Item("1");

            ItemCodeColumn.ChooseFromListAfter+=ItemCodeColumn_ChooseFromListAfter;

            QtyColumn = aMatrix.Columns.Item("11");
            QtyColumn.ValidateAfter+=QtyColumn_ValidateAfter;



            ORDR = MyForm.DataSources.DBDataSources.Item("ORDR");
            RDR1 = MyForm.DataSources.DBDataSources.Item("RDR1");

            OwerLink = MyForm.Items.Item("230").Specific as StaticText;
            OwerCode = MyForm.Items.Item("222").Specific as EditText;


            QtySum = MyForm.DataSources.UserDataSources.Add("Sum", BoDataType.dt_QUANTITY,254);

            QtyEdit = MyForm.Items.Add("Qty", BoFormItemTypes.it_EDIT).Specific as EditText;
            QtyEdit.DataBind.SetBound(true, "", "Sum");
            QtyEdit.Item.SetAutoManagedAttribute(BoAutoManagedAttr.ama_Editable, -1, BoModeVisualBehavior.mvb_False);

            QtyStatic = MyForm.Items.Add("LinkQty", BoFormItemTypes.it_STATIC).Specific as StaticText;
            QtyStatic.Item.LinkTo = "Qty";
            QtyStatic.Caption = "数量总计";



            QtyStatic.Item.Left = OwerLink.Item.Left;
            QtyStatic.Item.Top = OwerLink.Item.Top+16;
            QtyStatic.Item.Width = OwerLink.Item.Width;
            QtyStatic.Item.Height = OwerLink.Item.Height;

            QtyEdit.Item.Left = OwerCode.Item.Left;
            QtyEdit.Item.Top = OwerCode.Item.Top + 16;
            QtyEdit.Item.Width = OwerCode.Item.Width;
            QtyEdit.Item.Height = OwerCode.Item.Height;

         
        }





        public override void MenuEventHandler(ref MenuEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction&& (pVal.MenuUID=="1284"||pVal.MenuUID=="1286"||pVal.MenuUID=="1292"||pVal.MenuUID=="1293"||pVal.MenuUID=="1294"))
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
                    var value =( aMatrix.Columns.Item("11").Cells.Item(i).Specific as EditText).Value.Trim();
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
            if (!businessobjectinfo.BeforeAction&& businessobjectinfo.ActionSuccess)
            {
               var sum = 0.0;
                for (int i = 0; i < RDR1.Size; i++) {
                    var value = RDR1.GetValue("Quantity", i);
                    if (!string.IsNullOrEmpty(value))
                    {
                        sum += double.Parse(value);
                    }
                }
                if (sum>0.0)
                {
                    QtySum.Value = sum.ToString();
                }
            }
 
        }
        private void iteNameColumn_DoubleClickAfter(object sboObject, SBOItemEventArg pVal)
        {
            loactionInts[0] = pVal.Row;
            loactionInts[1] = pVal.ColUID;
            const string formType = "COR020030";
            var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
            MySonUid = form.UniqueID;
            var swBaseForm = Globle.SwFormsList[form.UniqueID];
            swBaseForm.MyFatherUid = MyFormUid;
        }


        private void openFileDialog_FileSetEvent(string path)
        {
            var checkFile = CommonUtil.IsFileInUse(path);

            if (checkFile)
            {
                Globle.Application.SetStatusBarMessage("文件正在被使用,不能读取数据!", BoMessageTime.bmt_Short);
                return;
            }
            var filePath = path;
            var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var datatable = NpoiHelper.RenderDataTableFromExcel(file, 0, 0);
            var excelRowInfos = new List<ExcelRowInfo>();
            var sql = new StringBuilder();
            foreach (DataRow row in datatable.Rows)
            {
                if (string.IsNullOrEmpty(row[0].ToString()) && string.IsNullOrEmpty(row[1].ToString()))
                {
                    continue;
                }
                sql.Append(
                    "select T0.\"U_MachineryNo\",T1.\"U_PurchaseNo\",T2.\"ItemCode\",T2.\"ItemName\",T2.\"FrgnName\",T3.\"Price\"/((T5.\"Rate\"+100)/100.0),T2.\"LeadTime\",T3.\"Price\",T5.\"Rate\",ifnull(T1.\"U_Memo\",'') as \"Memo\" ");
                sql.Append("from \"@COR020000\" T0 inner join \"@COR020001\" T1 on T0.\"Code\"=T1.\"Code\" ");
                sql.Append("INNER join OITM T2 on T1.\"U_ItemCode\"=T2.\"ItemCode\" ");
                sql.Append("LEFT JOIN ITM1 T3 ON T3.\"ItemCode\" = T2.\"ItemCode\" ");
                sql.Append("LEFT JOIN OPLN T4 ON T4.\"ListNum\" = T3.\"PriceList\" ");
                sql.Append("LEFT JOIN OVTG T5 ON T5.\"Code\" = T2.\"VatGourpSa\" ");
                sql.Append("where T2.\"SellItem\"='Y' and T0.\"U_MachineryNo\"='" + row[0] +
                           "' and T1.\"U_PurchaseNo\"='" + row[1] + "' ").Append("and T4.\"ListName\"='").Append("销售表价").Append("' "); ;
                Globle.ORecordSet.DoQuery(sql.ToString());

                if (Globle.ORecordSet.EoF == true)
                {
                    var excelRowInfo = new ExcelRowInfo();
                    excelRowInfo.MachineryNo = row[0].ToString();
                    excelRowInfo.PurchaseNo = row[1].ToString();
                    excelRowInfo.ItemCode = "";
                    excelRowInfo.ItemName = "";
                    excelRowInfo.FrgnName = "";
                    excelRowInfos.Add(excelRowInfo);
                }
                else
                {
                    while (Globle.ORecordSet.EoF == false)
                    {
                        var excelRowInfo = new ExcelRowInfo();

                        excelRowInfo.MachineryNo = Globle.ORecordSet.Fields.Item(0).Value.ToString();
                        excelRowInfo.PurchaseNo = Globle.ORecordSet.Fields.Item(1).Value.ToString();
                        excelRowInfo.ItemCode = Globle.ORecordSet.Fields.Item(2).Value.ToString();
                        excelRowInfo.ItemName = Globle.ORecordSet.Fields.Item(3).Value.ToString();
                        excelRowInfo.FrgnName = (Globle.ORecordSet.Fields.Item(4).Value == null
                            ? ""
                            : Globle.ORecordSet.Fields.Item(4).Value.ToString());
                        excelRowInfo.PriceBeforeVat =
                            double.Parse((Globle.ORecordSet.Fields.Item(5).Value == null
                                ? "0.0"
                                : Globle.ORecordSet.Fields.Item(5).Value.ToString()));
                        excelRowInfo.LeadTime = (Globle.ORecordSet.Fields.Item(6).Value == null
                            ? 0.0
                            : double.Parse(Globle.ORecordSet.Fields.Item(6).Value.ToString()));
                        excelRowInfo.Quantity = double.Parse(row[2].ToString());

                        excelRowInfo.Price = (Globle.ORecordSet.Fields.Item(7).Value == null
                            ? 0.0
                            : double.Parse(Globle.ORecordSet.Fields.Item(7).Value.ToString()));
                        excelRowInfo.Rate = (Globle.ORecordSet.Fields.Item(8).Value == null
                            ? 0.0
                            : double.Parse(Globle.ORecordSet.Fields.Item(8).Value.ToString()));
                        excelRowInfo.Memo = Globle.ORecordSet.Fields.Item(9).Value.ToString();
                        excelRowInfos.Add(excelRowInfo);

                        Globle.ORecordSet.MoveNext();
                    }
                }


              
                sql.Clear();
            }

            if (excelRowInfos.Count > 0)
            {
                const string formType = "COR020020";
                var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
                MySonUid = form.UniqueID;
                var swBaseForm = Globle.SwFormsList[form.UniqueID];
                swBaseForm.MyFatherUid = MyFormUid;
                ((COR020020)swBaseForm).SetInformation(excelRowInfos);
            }
        }

        private void _IButtonEvents_PressedAfterEventHandler_inputButton(object sboObject, SBOItemEventArg pVal)
        {
            if ((MyForm.Mode == BoFormMode.fm_UPDATE_MODE || MyForm.Mode == BoFormMode.fm_ADD_MODE ||
                 MyForm.Mode == BoFormMode.fm_OK_MODE) && pVal.ActionSuccess)
            {
                var cardCode = ((EditText)MyForm.Items.Item("4").Specific).Value.Trim();
                if (!string.IsNullOrEmpty(cardCode))
                {
                    var openFileDialog = new OpenFileDialog
                    {
                        InitialDirectory = "C:\\",
                        Filter = ".xls文件(*.xls)|*.xls",
                        FilterIndex = 1,
                        RestoreDirectory = true
                    };
                    openFileDialog.FileSetEvent += openFileDialog_FileSetEvent;
                    openFileDialog.ShowDialog();
                }
                else
                {
                    Globle.Application.SetStatusBarMessage("请选择客户！", BoMessageTime.bmt_Short);
                }
            }
        }

        private void _IButtonEvents_PressedAfterEventHandler_loadButton(object sboObject, SBOItemEventArg pVal)
        {
            if ((MyForm.Mode == BoFormMode.fm_UPDATE_MODE || MyForm.Mode == BoFormMode.fm_ADD_MODE ||
                 MyForm.Mode == BoFormMode.fm_OK_MODE) && pVal.ActionSuccess)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    InitialDirectory = "C:\\",
                    Filter = ".xls文件(*.xls)|*.xls",
                    FilterIndex = 1,
                    FileName = "Template.xls",
                    RestoreDirectory = true
                };

                saveFileDialog.FileSetEvent += saveFileDialog_FileSetEvent;
                saveFileDialog.ShowDialog();
            }
        }

        private void saveFileDialog_FileSetEvent(string path)
        {
            var thisExe = Assembly.GetExecutingAssembly();
            var readStream = thisExe.GetManifestResourceStream("HuDongHeavyMachinery.File.Template.xls");
            const int length = 1024;
            var buffer = new byte[length];
            var tempfile = Path.GetTempPath() + Path.GetRandomFileName();
            var writeStream = new FileStream(tempfile, FileMode.Create, FileAccess.Write);
            if (readStream != null)
            {
                var bytesRead = readStream.Read(buffer, 0, length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = readStream.Read(buffer, 0, length);
                }
            }
            if (readStream != null) readStream.Close();
            writeStream.Close();

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempfile, path);
            Globle.Application.StatusBar.SetSystemMessage("文件导出成功！", BoMessageTime.bmt_Short,
                BoStatusBarMessageType.smt_Success);
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
                inputButton.Item.Left = copyFroCombox.Item.Left;
                inputButton.Item.Top = copyFroCombox.Item.Top - copyFroCombox.Item.Height - 2;


                loadButton.Item.Left = copyToBoxCombox.Item.Left;
                loadButton.Item.Top = copyToBoxCombox.Item.Top - copyToBoxCombox.Item.Height - 2;
            }
            if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                if (MySonUid != null)
                {
                    var sonForm = Globle.Application.Forms.Item(MySonUid);
                    sonForm.Select();
                    bubbleEvent = false;
                }
            }
        }

        public override void SonFormCloseEventHandler(object obj, SwBaseForm sonSwBaseForm)
        {
            var owerChooseFromList = MyForm.ChooseFromLists.Item(iteNameColumn.ChooseFromListUID);
            var conditions = owerChooseFromList.GetConditions();
            var conditionsXml = conditions.GetAsXML();

            MySonUid = null;
            var gridRowInfos = (List<GridRowInfo>)obj;

            var gridRowInfoss = new GridRowInfo[gridRowInfos.Count];
            gridRowInfos.CopyTo(gridRowInfoss);

            sonSwBaseForm.MyForm.Close();

            try
            {
                MyForm.Freeze(true);
                foreach (var gridRowInfo in gridRowInfoss)
                {
                    if (string.IsNullOrEmpty(gridRowInfo.ItemCode))
                    {
                        continue;
                    }
                    if (conditions.Count > 0)
                    {
                        conditions.Item(conditions.Count - 1).Relationship = BoConditionRelationship.cr_AND;
                    }
                    var condition = conditions.Add();
                    condition.Alias = "ItemCode";
                    condition.Operation = BoConditionOperation.co_EQUAL;
                    condition.CondVal = gridRowInfo.ItemCode;
                    owerChooseFromList.SetConditions(conditions);
                    var rowIndex = aMatrix.VisualRowCount;


                    ((EditText)aMatrix.Columns.Item("3").Cells.Item(rowIndex).Specific).Value = gridRowInfo.ItemName;

                    Globle.Application.SendKeys("{TAB}");
                    ((EditText)aMatrix.Columns.Item("U_MachineryNo").Cells.Item(rowIndex).Specific).Value =
                        gridRowInfo.MachineryNo;
                    ((EditText)aMatrix.Columns.Item("U_PurchaseNo").Cells.Item(rowIndex).Specific).Value =
                        gridRowInfo.PurchaseNo;
                    ((EditText)aMatrix.Columns.Item("U_FrgnName").Cells.Item(rowIndex).Specific).Value =
                      gridRowInfo.FrgnName;
                    ((EditText)aMatrix.Columns.Item("11").Cells.Item(rowIndex).Specific).Value =
                        gridRowInfo.Quantity.ToString();
                    ((EditText)aMatrix.Columns.Item("U_LeadTime").Cells.Item(rowIndex).Specific).Value =
                       gridRowInfo.LeadTime.ToString();
                    ((EditText)aMatrix.Columns.Item("U_Memo").Cells.Item(rowIndex).Specific).Value =
                       gridRowInfo.Memo;
                    conditions.LoadFromXML(conditionsXml);
                    owerChooseFromList.SetConditions(conditions);
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

            var msg = "数据导入完成";
            Globle.Application.StatusBar.SetSystemMessage(msg, BoMessageTime.bmt_Short,
                BoStatusBarMessageType.smt_Success);
            Globle.Application.MessageBox(msg);
        }
    }
}