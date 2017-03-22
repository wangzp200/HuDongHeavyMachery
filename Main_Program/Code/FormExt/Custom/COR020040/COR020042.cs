using System;
using System.Collections.Generic;
using System.Text;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020040
{
    public class COR020042 : SwBaseForm
    {
        private Matrix matrix1, matrix2;
        private DataTable DTL1, DTL2, TmpDt;
        private StaticText laStaticText;
        private Column column1, column2;
        private DBDataSource dbDataCor020041, dbDataCor020042;
        private UserDataSource showBatchNum, showQty;
        private Item rectangle1, rectangle2;
        private List<Dtl2RowInfo> dtl2RowInfos = new List<Dtl2RowInfo>();
        private int currentRow;
        private Button okButton;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            matrix1 = (Matrix) MyForm.Items.Item("10").Specific;
            matrix1.SelectionMode = BoMatrixSelect.ms_Single;
            column1 = matrix1.Columns.Item("C0");
            column1.ClickAfter += column1_ClickAfter;
            column1.ClickBefore += column1_PressedBefore;
            matrix2 = (Matrix) MyForm.Items.Item("4").Specific;
            column2 = matrix2.Columns.Item("C0");

            laStaticText = (StaticText) MyForm.Items.Item("3").Specific;
            DTL1 = MyForm.DataSources.DataTables.Item("DTL1");
            DTL2 = MyForm.DataSources.DataTables.Item("DTL2");

            TmpDt = MyForm.DataSources.DataTables.Add("TMP");

            showBatchNum = MyForm.DataSources.UserDataSources.Item("1000001");
            showQty = MyForm.DataSources.UserDataSources.Item("1000002");
            okButton = (Button) MyForm.Items.Item("1").Specific;
            okButton.PressedBefore += okButton_PressedBefore;
            rectangle1 = MyForm.Items.Item("12");
            rectangle2 = MyForm.Items.Item("13");
            FormResize();
        }

        private void okButton_PressedBefore(object sboObject, SBOItemEventArg pVal, out bool bubbleEvent)
        {
            if (MyForm.Mode == BoFormMode.fm_UPDATE_MODE)
            {
                dbDataCor020042.Clear();
                foreach (var dtl2RowInfo in dtl2RowInfos)
                {
                    var rowIndex = dbDataCor020042.Size;
                    dbDataCor020042.InsertRecord(rowIndex);
                    dbDataCor020042.SetValue("LineId", rowIndex, (rowIndex + 1) + "");
                    dbDataCor020042.SetValue("U_DistNumber", rowIndex, dtl2RowInfo.DistNumber);
                    dbDataCor020042.SetValue("U_WhseCode", rowIndex, dtl2RowInfo.WhseCode);
                    dbDataCor020042.SetValue("U_Quantity", rowIndex, dtl2RowInfo.Quantity);
                    dbDataCor020042.SetValue("U_MnfSerial", rowIndex, dtl2RowInfo.MnfSerial);
                    dbDataCor020042.SetValue("U_LotNumber", rowIndex, dtl2RowInfo.LotNumber);
                    dbDataCor020042.SetValue("U_ExpDate", rowIndex, dtl2RowInfo.ExpDate);
                    dbDataCor020042.SetValue("U_MnfDate", rowIndex, dtl2RowInfo.MnfDate);
                    dbDataCor020042.SetValue("U_InDate", rowIndex, dtl2RowInfo.InDate);
                    dbDataCor020042.SetValue("U_Location", rowIndex, dtl2RowInfo.Location);
                    dbDataCor020042.SetValue("U_Notes", rowIndex, dtl2RowInfo.Notes);
                    dbDataCor020042.SetValue("U_BaseLine", rowIndex, dtl2RowInfo.BaseLine);
                    dbDataCor020042.SetValue("U_SumVar", rowIndex, dtl2RowInfo.SumVar);
                }
            }
            bubbleEvent = true;
        }

        private void column1_PressedBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            if (currentRow > 0)
            {
                var bsRow = ((EditText) matrix1.Columns.Item("C0").Cells.Item(currentRow).Specific).Value.Trim();
                for (var i = dtl2RowInfos.Count - 1; i >= 0; i--)
                {
                    var dtl2RowInfo = dtl2RowInfos[i];
                    if (dtl2RowInfo.BaseLine == bsRow)
                    {
                        dtl2RowInfos.Remove(dtl2RowInfo);
                    }
                }
                for (var i = 1; i <= matrix2.VisualRowCount; i++)
                {
                    var distNumber = ((EditText) matrix2.Columns.Item("C1").Cells.Item(i).Specific).Value.Trim();
                    if (!string.IsNullOrEmpty(distNumber))
                    {
                        var dtl2RowInfo = new Dtl2RowInfo();
                        dtl2RowInfo.DistNumber =
                            ((EditText) matrix2.Columns.Item("C1").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.WhseCode =
                            ((EditText) matrix2.Columns.Item("C11").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.Quantity =
                            ((EditText) matrix2.Columns.Item("C2").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.SumVar = ((EditText) matrix2.Columns.Item("C3").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.MnfSerial =
                            ((EditText) matrix2.Columns.Item("C4").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.LotNumber =
                            ((EditText) matrix2.Columns.Item("C5").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.ExpDate =
                            ((EditText) matrix2.Columns.Item("C6").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.MnfDate =
                            ((EditText) matrix2.Columns.Item("C7").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.InDate = ((EditText) matrix2.Columns.Item("C8").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.Location =
                            ((EditText) matrix2.Columns.Item("C9").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.Notes = ((EditText) matrix2.Columns.Item("C10").Cells.Item(i).Specific).Value.Trim();
                        dtl2RowInfo.BaseLine = bsRow;
                        dtl2RowInfos.Add(dtl2RowInfo);
                    }
                }
            }

            currentRow = 0;
            BubbleEvent = true;
        }

        private void column1_ClickAfter(object sboObject, SBOItemEventArg pVal)
        {
            currentRow = pVal.Row;
            DTL2.Rows.Clear();
            var bsRow = DTL1.GetValue("LineId", pVal.Row - 1).ToString().Trim();
            var docQuan = double.Parse(DTL1.GetValue("DocQuan", pVal.Row - 1).ToString().Trim());
            var dtl2DocQuan = 0.0;
            var count = 0;

            foreach (var dtl2RowInfo in dtl2RowInfos)
            {
                if (bsRow == dtl2RowInfo.BaseLine)
                {
                    DTL2.Rows.Add();
                    var row = DTL2.Rows.Count - 1;
                    DTL2.SetValue("LineId", row, DTL2.Rows.Count);
                    DTL2.SetValue("DistNumber", row, dtl2RowInfo.DistNumber);
                    DTL2.SetValue("WhseCode", row, dtl2RowInfo.WhseCode);
                    DTL2.SetValue("Quantity", row, dtl2RowInfo.Quantity);
                    DTL2.SetValue("MnfSerial", row, dtl2RowInfo.MnfSerial);
                    DTL2.SetValue("LotNumber", row, dtl2RowInfo.LotNumber);
                    DTL2.SetValue("ExpDate", row, dtl2RowInfo.ExpDate);
                    DTL2.SetValue("MnfDate", row, dtl2RowInfo.MnfDate);
                    DTL2.SetValue("InDate", row, dtl2RowInfo.InDate);
                    DTL2.SetValue("Location", row, dtl2RowInfo.Location);
                    DTL2.SetValue("Notes", row, dtl2RowInfo.Notes);
                    DTL2.SetValue("BsRow", row, dtl2RowInfo.BaseLine);
                    DTL2.SetValue("SumVar", row, dtl2RowInfo.SumVar);
                    dtl2DocQuan = dtl2DocQuan + double.Parse(dtl2RowInfo.Quantity);
                    count = count + 1;
                }
            }

            try
            {
                MyForm.Freeze(true);
                if (docQuan > dtl2DocQuan && !string.IsNullOrEmpty(bsRow))
                {
                    DTL2.Rows.Add();
                    DTL2.SetValue("LineId", DTL2.Rows.Count - 1, DTL2.Rows.Count);
                    DTL2.SetValue("BsRow", DTL2.Rows.Count - 1, DTL2.Rows.Count);
                    DTL2.SetValue("DistNumber", DTL2.Rows.Count - 1, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    DTL2.SetValue("Quantity", DTL2.Rows.Count - 1, docQuan - dtl2DocQuan);
                    if (MyForm.Mode == BoFormMode.fm_OK_MODE)
                    {
                        MyForm.Mode = BoFormMode.fm_UPDATE_MODE;
                    }
                }
                matrix2.LoadFromDataSource();
                showQty.Value = dtl2DocQuan + "";
                showBatchNum.Value = count + "";
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

        private void noButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            Globle.SwFormsList[MyFatherUid].MySonUid = null;
            MyForm.Close();
        }


        public override void ItemEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_RESIZE)
            {
                FormResize();
            }
        }

        private void FormResize()
        {
            try
            {
                MyForm.Freeze(true);
                var h = MyForm.Height - 461;
                var a = h*113/279;
                matrix1.Item.Height = 113 + a;
                matrix2.Item.Height = 165 + h - a;
                laStaticText.Item.Top = matrix1.Item.Top + matrix1.Item.Height + 20;
                laStaticText.Item.Left = 6;
                matrix2.Item.Top = matrix1.Item.Height + matrix1.Item.Top + 39;

                matrix1.Item.Width = MyForm.Width - 30;
                matrix2.Item.Width = MyForm.Width - 30;

                rectangle1.Height = matrix1.Item.Height + 8;
                rectangle1.Width = matrix1.Item.Width + 8;

                rectangle2.Height = matrix2.Item.Height + 8;
                rectangle2.Width = matrix2.Item.Width + 8;

                rectangle2.Top = matrix2.Item.Top - 4;
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                MyForm.Freeze(false);
                MyForm.Refresh();
            }
        }

        /// <summary>
        ///     初始化本窗体的参数
        /// </summary>
        public void SetInformation(DBDataSource dbDataCor020041, DBDataSource dbDataCor020042, int row)
        {
            var sqlBuilder =
                new StringBuilder(
                    "select distinct \"ItemCode\" from OITM where \"ManBtchNum\"='Y' and \"MngMethod\"='A' and \"ItemCode\" in (");

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
            TmpDt.ExecuteQuery(sql);

            var tmpList = new List<string>();
            for (var i = 0; i < TmpDt.Rows.Count; i++)
            {
                tmpList.Add(TmpDt.GetValue("ItemCode", i).ToString());
            }

            for (var i = 0; i < dbDataCor020041.Size; i++)
            {
                var value = dbDataCor020041.GetValue("U_ItemCode", i).Trim();
                if (tmpList.Contains(value))
                {
                    DTL1.Rows.Add();
                    var rowIndex = DTL1.Rows.Count - 1;
                    DTL1.SetValue("LineId", rowIndex, DTL1.Rows.Count);
                    DTL1.SetValue("DocRef", rowIndex, dbDataCor020041.GetValue("U_BaseRef", i).Trim());
                    DTL1.SetValue("ItemCode", rowIndex, dbDataCor020041.GetValue("U_ItemCode", i).Trim());
                    DTL1.SetValue("ItemDesc", rowIndex, dbDataCor020041.GetValue("U_Dscription", i).Trim());
                    DTL1.SetValue("WhseCode", rowIndex, dbDataCor020041.GetValue("U_WhseCode", i).Trim());
                    DTL1.SetValue("DocQuan", rowIndex, dbDataCor020041.GetValue("U_CheckQty", i).Trim());
                    DTL1.SetValue("TotalCreat", rowIndex, 0.0);
                    DTL1.SetValue("BsLine", rowIndex, dbDataCor020041.GetValue("LineId", i).Trim());
                }
            }
            try
            {
                MyForm.Freeze(true);
                matrix1.LoadFromDataSource();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                MyForm.Freeze(false);
            }

            this.dbDataCor020041 = dbDataCor020041;
            this.dbDataCor020042 = dbDataCor020042;

            for (var i = 0; i < dbDataCor020042.Size; i++)
            {
                var distNumber = dbDataCor020042.GetValue("U_DistNumber", i).Trim();
                if (!string.IsNullOrEmpty(distNumber))
                {
                    var dtl2RowInfo = new Dtl2RowInfo();
                    dtl2RowInfo.LineId = dbDataCor020042.GetValue("LineId", i).Trim();
                    dtl2RowInfo.DistNumber = dbDataCor020042.GetValue("U_DistNumber", i).Trim();
                    dtl2RowInfo.WhseCode = dbDataCor020042.GetValue("U_WhseCode", i).Trim();
                    dtl2RowInfo.Quantity = dbDataCor020042.GetValue("U_Quantity", i).Trim();
                    dtl2RowInfo.MnfSerial = dbDataCor020042.GetValue("U_MnfSerial", i).Trim();
                    dtl2RowInfo.LotNumber = dbDataCor020042.GetValue("U_LotNumber", i).Trim();
                    dtl2RowInfo.ExpDate = dbDataCor020042.GetValue("U_ExpDate", i).Trim();
                    dtl2RowInfo.MnfDate = dbDataCor020042.GetValue("U_MnfDate", i).Trim();
                    dtl2RowInfo.InDate = dbDataCor020042.GetValue("U_InDate", i).Trim();
                    dtl2RowInfo.Location = dbDataCor020042.GetValue("U_Location", i).Trim();
                    dtl2RowInfo.Notes = dbDataCor020042.GetValue("U_Notes", i).Trim();
                    dtl2RowInfo.BaseLine = dbDataCor020042.GetValue("U_BaseLine", i).Trim();
                    dtl2RowInfo.SumVar = dbDataCor020042.GetValue("U_SumVar", i).Trim();
                    dtl2RowInfos.Add(dtl2RowInfo);
                }
            }
        }

        private class Dtl2RowInfo
        {
            public string LineId { get; set; }

            public string DistNumber { get; set; }

            public string WhseCode { get; set; }

            public string Quantity { get; set; }

            public string MnfSerial { get; set; }

            public string LotNumber { get; set; }

            public string ExpDate { get; set; }

            public string MnfDate { get; set; }

            public string InDate { get; set; }

            public string Location { get; set; }

            public string Notes { get; set; }

            public string BaseLine { get; set; }
            public string SumVar { get; set; }
        }
    }
}