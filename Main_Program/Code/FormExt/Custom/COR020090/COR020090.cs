using HuDongHeavyMachinery.Code.Util;
using SAPbouiCOM;
using SwissAddonFramework.Utils.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Row = System.Data.DataRow;
namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020090
{
    class COR020090 : SwBaseForm
    {
        private Folder folder1, folder2;
        private Button loadbutton, Createbutton, Openbutton;
        private Matrix mtx1, mtx2;
        private DataTable DTL1, DTL2, TmpTable;
        private SAPbobsCOM.GeneralService generalService;
        private Item Rec;
        public override void FormCreate(string formUId, ref SAPbouiCOM.ItemEvent pVal, ref bool bubbleEvent)
        {
            folder1 = MyForm.Items.Item("4").Specific as Folder;
            folder1.ClickAfter += folder1_ClickAfter;
            folder2 = MyForm.Items.Item("3").Specific as Folder;
            folder2.ClickAfter += folder2_ClickAfter;
            loadbutton = MyForm.Items.Item("5").Specific as Button;
            loadbutton.PressedAfter += loadbutton_PressedAfter;
            Createbutton = MyForm.Items.Item("6").Specific as Button;
            Createbutton.PressedAfter += Createbutton_PressedAfter;
            Openbutton = MyForm.Items.Item("1000004").Specific as Button;
            Openbutton.PressedAfter += Openbutton_PressedAfter;

            mtx1 = MyForm.Items.Item("1000002").Specific as Matrix;
            mtx2 = MyForm.Items.Item("1000003").Specific as Matrix;
            DTL1 = MyForm.DataSources.DataTables.Item("DTL1");
            DTL2 = MyForm.DataSources.DataTables.Item("DTL2");
            TmpTable = MyForm.DataSources.DataTables.Add("Tmp");
            generalService = Globle.DiCompany.GetCompanyService().GetGeneralService("COR020000");
            Rec = MyForm.Items.Item("1000001");
            Rec.Width = mtx1.Item.Width + 8;
            Rec.Height = mtx1.Item.Height + 8;
            folder1.Select();
        }

        private void Openbutton_PressedAfter(object sboObject, SBOItemEventArg pVal)
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

        private void Createbutton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            try
            {


                //return;

                MyForm.Freeze(true);
                var task = new Task(() =>
                {
                    var oGeneralParams = generalService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams) as SAPbobsCOM.GeneralDataParams;
                    var oGeneralData = generalService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData) as SAPbobsCOM.GeneralData;
                    SAPbobsCOM.GeneralDataCollection oSons;
                    SAPbobsCOM.GeneralData oSon;
                    //var progressBar = Globle.Application.StatusBar.CreateProgressBar("更新重....", DTL1.Rows.Count, false);
             
                    var dTL1column = new string[DTL1.Columns.Count];
                    var dTL2column = new string[DTL2.Columns.Count];

                    for (int j = 0; j < DTL1.Columns.Count; j++)
                    {
                        dTL1column[j] = DTL1.Columns.Item(j).Name;
                    }
                    for (int j = 0; j < DTL2.Columns.Count; j++)
                    {
                        dTL2column[j] = DTL2.Columns.Item(j).Name;
                    }
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(DTL2.SerializeAsXML(BoDataTableXmlSelect.dxs_All));
                    var xmlColumns = xmlDoc.SelectNodes("DataTable/Columns/Column");
                    var columnInfos = new SortedList<string, int>();
                    for (int i = 0; i < xmlColumns.Count; i++)
                    {
                        var xmlColumn = xmlColumns.Item(i);
                        columnInfos.Add(xmlColumn.Attributes.Item(0).Value, i);
                    }
                    var xmlRows = xmlDoc.SelectNodes("DataTable/Rows/Row");

                    for (int i = 0; i < DTL1.Rows.Count; i++) {
                        var code = DTL1.GetValue("Code", i).ToString();
                        var sql = "SELECT \"Code\" FROM \"@COR020000\" WHERE \"Code\"='" + code + "'";
                        TmpTable.ExecuteQuery(sql);
                        if (!TmpTable.IsEmpty)
                        {
                            //progressBar.Text = "正在更新:" + code;
                            Globle.Application.StatusBar.SetSystemMessage("正在更新:" + code + ",已完成" + ((1+i) * 100.0 / DTL1.Columns.Count) + "%", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Success);
                            code = TmpTable.GetValue("Code", 0).ToString();
                            oGeneralParams.SetProperty("Code", code);
                            oGeneralData = generalService.GetByParams(oGeneralParams);
                            for (int j = 0; j < dTL1column.Length; j++)
                            {
                                var column = dTL1column[j];
                                if (column.StartsWith("U_"))
                                {
                                    oGeneralData.SetProperty(column, DTL1.GetValue(column, i));
                                }
                            }
                            oSons = oGeneralData.Child("COR020001");
                            for (int j = 0; j < xmlRows.Count; j++)
                            {
                                var xmlRow = xmlRows.Item(j);
                                var cells = xmlRow.SelectNodes("Cells/Cell");
                                if (cells.Item(columnInfos["Code"]).InnerText==code)
                                {
                                     var type = 0;
                                    for (int n = 0; n < oSons.Count; n++)
                                    {
                                        oSon = oSons.Item(n);
                                        if (oSon.GetProperty("U_PurchaseNo").ToString().Equals(cells.Item(columnInfos["U_PurchaseNo"]).InnerText))
                                        {
                                            type = 1;
                                            for (int m = 0; m < dTL2column.Length; m++)
                                            {
                                                var column = dTL2column[m];
                                                if (column.StartsWith("U_"))
                                                {
                                                    oSon.SetProperty(column, cells.Item(columnInfos[column]).InnerText);
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    if (type == 0)
                                    {
                                        oSon = oSons.Add();
                                        for (int m = 0; m < dTL2column.Length; m++)
                                        {
                                            var column = dTL2column[m];
                                            if (column.StartsWith("U_"))
                                            {
                                                oSon.SetProperty(column, cells.Item(columnInfos[column]).InnerText);
                                            }
                                        }
                                    }
                                }
                            }
                            generalService.Update(oGeneralData);
                        }
                        else
                        {
                            oGeneralData = generalService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData) as SAPbobsCOM.GeneralData;
                            //progressBar.Text = "正在添加:" + code;
                            Globle.Application.StatusBar.SetSystemMessage("正在添加:" + code + ",已完成" + ((1 + i) * 100.0 / DTL1.Columns.Count) + "%", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Success);
                            for (int j = 0; j < dTL1column.Length; j++)
                            {
                                var column = dTL1column[j];
                                if (!column.Equals("LineId"))
                                {
                                    oGeneralData.SetProperty(column, DTL1.GetValue(column, i));
                                }
                            }
                            oSons = oGeneralData.Child("COR020001");
                            for (int j = 0; j < xmlRows.Count; j++)
                            {
                                var xmlRow = xmlRows.Item(j);
                                var cells = xmlRow.SelectNodes("Cells/Cell");
                                if (cells.Item(columnInfos["U_PurchaseNo"]).InnerText.Equals(code))
                                {
                                    oSon = oSons.Add();
                                    for (int m = 0; m < dTL2column.Length; m++)
                                    {
                                        var column = dTL2column[m];
                                        if (column.StartsWith("U_"))
                                        {
                                            oSon.SetProperty(column, cells.Item(columnInfos[column]).InnerText);
                                        }
                                    }
                                }
                            }
                            generalService.Add(oGeneralData);
                        }
                        //progressBar.Value = i;
                    }
                    //progressBar.Stop();
                    //Marshal.ReleaseComObject(progressBar);
                    Globle.Application.SetStatusBarMessage("更新完毕!", BoMessageTime.bmt_Short, false);
                });
                task.Start();
            }
            catch (Exception ex)
            {
                Globle.Application.SetStatusBarMessage(ex.Message, BoMessageTime.bmt_Short);
            }
            finally
            {
                MyForm.Freeze(false);
            }
        }

        private void openFileDialog_FileSetEvent(string path)
        {
            DTL1.Rows.Clear();
            DTL2.Rows.Clear();
            var itemInfos = new SortedList<string, string>();
            var checkFile = CommonUtil.IsFileInUse(path);

            if (checkFile)
            {
                Globle.Application.SetStatusBarMessage("文件正在被使用,不能读取数据!", BoMessageTime.bmt_Short);
                return;
            }
            var file = new FileStream(path, FileMode.Open, FileAccess.Read);
            var headTable = NpoiHelper.RenderDataTableFromExcel(file, 0, 0);
            file = new FileStream(path, FileMode.Open, FileAccess.Read);
            var lineTable = NpoiHelper.RenderDataTableFromExcel(file, 1, 0);

            StringBuilder itemCodes = new StringBuilder("(");

            foreach (Row row in lineTable.Rows)
            {
                var itemCode = row[3].ToString();
                if (!string.IsNullOrEmpty(itemCode))
                {
                    itemCodes.Append("'").Append(itemCode).Append("',");
                }

            }

            if (itemCodes.Length > 0)
            {
                var sql = itemCodes.ToString();
                sql = "SELECT Distinct  \"ItemCode\",\"ItemName\" FROM OITM WHERE \"ItemCode\" IN " + sql.Substring(0, sql.Length - 1) + ")";
                TmpTable.ExecuteQuery(sql);
                if (!TmpTable.IsEmpty)
                {

                    for (int i = 0; i < TmpTable.Rows.Count; i++)
                    {
                        var itemCode = TmpTable.GetValue("ItemCode", i).ToString();
                        var itemName = TmpTable.GetValue("ItemName", i).ToString();
                        if (!itemInfos.Keys.Contains(itemCode))
                        {
                            itemInfos.Add(itemCode, itemName);
                        }
                    }
                }
            }



            StringBuilder machinerys = new StringBuilder("(");

            var progressBar = Globle.Application.StatusBar.CreateProgressBar("加载头信息中....", headTable.Rows.Count, false);
            var pIndex = 0;
            foreach (Row row in headTable.Rows)
            {
                var code = row[0].ToString();
                if (!string.IsNullOrEmpty(code))
                {
                    machinerys.Append("'").Append(code.Trim()).Append("',");

                    var index = DTL1.Rows.Count;
                    DTL1.Rows.Add();
                    for (int i = 1; i < DTL1.Columns.Count; i++)
                    {
                        var column = DTL1.Columns.Item(i);
                        DTL1.SetValue(column.Name, index, row[i - 1].ToString());
                    }
                    DTL1.SetValue("LineId", index, (index + 1));
                }
                progressBar.Value = pIndex;
                pIndex++;
            }
            progressBar.Stop();

            if (machinerys.Length > 1)
            {
                var sql = machinerys.ToString();
                sql = sql.Substring(0, sql.Length - 1) + ")";
                sql = "SELECT * FROM \"@COR020080\" WHERE \"U_MachineryNo\" IN " + sql;
                TmpTable.ExecuteQuery(sql);
                if (!TmpTable.IsEmpty)
                {
                    for (int i = 0; i < TmpTable.Rows.Count; i++)
                    {
                        var machineryNo = TmpTable.GetValue("U_MachineryNo", i).ToString();
                        for (int j = 0; j < DTL1.Rows.Count; j++)
                        {
                            if (machineryNo.Equals(DTL1.GetValue("U_MachineryNo", j).ToString()))
                            {
                                DTL1.SetValue("U_MachineryType", j, TmpTable.GetValue("U_MachineryType", i));
                                DTL1.SetValue("U_ShipName", j, TmpTable.GetValue("U_HullNo", i));
                                DTL1.SetValue("U_Vendor", j, TmpTable.GetValue("U_Vendor", i));
                            }
                        }
                    }
                }
            }
            progressBar = Globle.Application.StatusBar.CreateProgressBar("加载行信息中....", lineTable.Rows.Count, false);
            pIndex = 0;
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(DTL2.SerializeAsXML(BoDataTableXmlSelect.dxs_DataOnly));
            var xmlRows = xmlDoc.SelectSingleNode("DataTable/Rows");
            int rowIndex = 0;
            var columns = new string[DTL2.Columns.Count];
            for (int i = 0; i < DTL2.Columns.Count; i++)
            {
                columns[i] = DTL2.Columns.Item(i).Name;
            }
            foreach (Row row in lineTable.Rows)
            {
                var code = row[0].ToString();
                if (!string.IsNullOrEmpty(code))
                {
                    var xmlRow = xmlDoc.CreateNode(XmlNodeType.Element, "Row", null);
                    var cells = xmlDoc.CreateNode(XmlNodeType.Element, "Cells", null);
                    if (columns[0] == "LineId")
                    {
                        var cell = xmlDoc.CreateNode(XmlNodeType.Element, "Cell", null);
                        var columnUid = xmlDoc.CreateNode(XmlNodeType.Element, "ColumnUid", null);
                        columnUid.InnerText = columns[0];
                        var value = xmlDoc.CreateNode(XmlNodeType.Element, "Value", null);
                        value.InnerText = (rowIndex + 1).ToString();
                        cell.AppendChild(columnUid);
                        cell.AppendChild(value);
                        cells.AppendChild(cell);
                    }
                    var flg = true;
                    for (int i = 1; i < columns.Length; i++)
                    {
                        var column = columns[i];
                        var cell = xmlDoc.CreateNode(XmlNodeType.Element, "Cell", null);
                        var columnUid = xmlDoc.CreateNode(XmlNodeType.Element, "ColumnUid", null);
                        columnUid.InnerText = column;
                        var value = xmlDoc.CreateNode(XmlNodeType.Element, "Value", null);
                        if (column.Equals("U_ItemName"))
                        {
                            flg = false;
                            var itemCode = row[i - 2].ToString();
                            if (itemInfos.Keys.Contains(itemCode))
                            {
                                var itemName = itemInfos[itemCode];
                                value.InnerText = itemName;
                            }
                        }
                        else
                        {
                            if (flg)
                            {
                                value.InnerText = row[i - 1].ToString();
                            }
                            else
                            {
                                value.InnerText = row[i - 2].ToString();
                            }
                        }
                        cell.AppendChild(columnUid);
                        cell.AppendChild(value);
                        cells.AppendChild(cell);
                    }
                    xmlRow.AppendChild(cells);
                    xmlRows.AppendChild(xmlRow);
                    rowIndex++;
                }
                progressBar.Value = pIndex;
                pIndex++;
            }
            var x = xmlDoc.InnerXml;
            DTL2.LoadSerializedXML(BoDataTableXmlSelect.dxs_DataOnly, xmlDoc.InnerXml);
            progressBar.Stop();
            try
            {
                MyForm.Freeze(true);
                mtx1.LoadFromDataSource();
                mtx2.LoadFromDataSource();
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

        private void loadbutton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = "C:\\",
                Filter = ".xls文件(*.xls)|*.xls",
                FilterIndex = 1,
                FileName = "Template1.xls",
                RestoreDirectory = true
            };
            saveFileDialog.FileSetEvent += saveFileDialog_FileSetEvent;
            saveFileDialog.ShowDialog();
        }

        private void saveFileDialog_FileSetEvent(string path)
        {
            var thisExe = Assembly.GetExecutingAssembly();
            var readStream = thisExe.GetManifestResourceStream("HuDongHeavyMachinery.File.Template1.xls");
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

        private void folder2_ClickAfter(object sboObject, SBOItemEventArg pVal)
        {
            MyForm.PaneLevel = 2;
        }

        private void folder1_ClickAfter(object sboObject, SBOItemEventArg pVal)
        {
            MyForm.PaneLevel = 1;
        }
        public override void ItemEventHandler(string formUid, ref SAPbouiCOM.ItemEvent pVal, ref bool bubbleEvent)
        {
            if (pVal.EventType == BoEventTypes.et_FORM_RESIZE && !pVal.BeforeAction)
            {
                Rec.Width = mtx1.Item.Width + 8;
                Rec.Height = mtx1.Item.Height + 8;
            }
        }
    }
}
