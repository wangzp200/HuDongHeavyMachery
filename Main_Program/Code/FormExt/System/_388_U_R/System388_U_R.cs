using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using HuDongHeavyMachinery.Code.Util;
using SAPbouiCOM;
using SwissAddonFramework.Utils.Windows;
using DataTable = System.Data.DataTable;
using Row = System.Data.DataRow;

namespace HuDongHeavyMachinery.Code.FormExt.Other.SystemQueryForm
{
    public class System388_U_R : SwBaseForm
    {
        private Button copyData;
        //private DataTable dataTable;
        private DBDataSource dbData;
        private Matrix matrix;
        private Button saveXML;

        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            saveXML = MyForm.Items.Add("saveXML", BoFormItemTypes.it_BUTTON).Specific as Button;
            if (saveXML != null)
            {
                saveXML.Caption = "另存为XML";
                copyData = MyForm.Items.Item("231000001").Specific as Button;
                if (copyData != null)
                {
                    saveXML.Item.Top = copyData.Item.Top;
                    saveXML.Item.Width = copyData.Item.Width;
                    saveXML.Item.Height = copyData.Item.Height;
                    saveXML.Item.Left = copyData.Item.Left - copyData.Item.Width - 10;
                }
                saveXML.ClickAfter += saveXML_ClickAfter;
            }
            for (var i = 0; i < MyForm.Items.Count; i++)
            {
                var item = MyForm.Items.Item(i);
                if (item.Type == BoFormItemTypes.it_MATRIX)
                {
                    matrix = item.Specific as Matrix;
                    break;
                }
            }

            dbData = MyForm.DataSources.DBDataSources.Item(0);

            //dataTable = new DataTable(MyForm.Title.Trim());


            //for (var i = 0; i < matrix.Columns.Count; i++)
            //{
            //    var column = matrix.Columns.Item(i);
            //    dataTable.Columns.Add(column.Title, typeof (string));
            //}

            //for (int i = 0; i < dbData.Fields.Count; i++)
            //{
            //    var column = dbData.Fields.Item(i);

            //    dataTable.Columns.Add(column.Name, CommonUtil.getType(column.Type));
            //}

            EventForm.ResizeAfter += eventForm_ResizeAfter;
        }

        public void eventForm_ResizeAfter(SBOItemEventArg pVal)
        {
        }

        public void saveXML_ClickAfter(object sboObject, SBOItemEventArg pVal)
        {
            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = "C:\\",
                Filter = ".XML文件(*.XML)|*.XML",
                FilterIndex = 1,
                FileName = MyForm.Title.Trim() + ".XML",
                RestoreDirectory = true
            };

            saveFileDialog.FileSetEvent += saveFileDialog_FileSetEvent;
            saveFileDialog.ShowDialog();
        }

        public void saveFileDialog_FileSetEvent(string path)
        {
            var clnList = new List<Column>();

            var progressBar = Globle.Application.StatusBar.CreateProgressBar("整理数据中....", matrix.VisualRowCount, false);
            try
            {
                var xml = matrix.SerializeAsXML(BoMatrixXmlSelect.mxs_All);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                var headers = xmlDoc.SelectNodes("Matrix/ColumnsInfo/ColumnInfo/UniqueID");

                var dataTable = new DataTable();
                foreach (XmlNode header in headers)
                {
                    var s = header.InnerText;
                    dataTable.Columns.Add(s, typeof (string));
                }

                var rows = xmlDoc.SelectNodes("Matrix/Rows/Row");


                for (var i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];

                    var visible = row.SelectSingleNode("Visible");

                    if (visible.InnerText == "1")
                    {
                        var dataRow = dataTable.Rows.Add();

                        var lines = row.SelectNodes("Columns/Column/Value");

                        for (var j = 0; j < lines.Count; j++)
                        {
                            dataRow[j] = lines[j].InnerText;
                        }
                    }

                    progressBar.Value = i;
                }


                if (dataTable.Rows.Count > 0)
                {
                    var data = new StringBuilder();
                    data.Append("<?xml version=\"1.0\" encoding='UTF-8'?>").Append("\r\n");
                    data.Append(
                        "<ufinterface account=\"develop\" billtype=\"vouchergl\" businessunitcode=\"develop\" filename=\"\" groupcode=\"\" isexchange=\"\" orgcode=\"\" receiver=\"\" replace=\"\" roottag=\"\" sender=\"SAP\">")
                        .Append("\r\n");
                    var numbers = new List<string>();
                    for (var i = 0; i < dataTable.Rows.Count; i++)
                    {
                        var number = dataTable.Rows[i][59].ToString();
                        if (!numbers.Contains(number))
                        {
                            numbers.Add(number);
                        }
                    }
                    var rowList = new List<Row>();
                    foreach (var number in numbers)
                    {
                        rowList.Clear();
                        for (var i = 0; i < dataTable.Rows.Count; i++)
                        {
                            if (number == dataTable.Rows[i][59].ToString())
                            {
                                var row = dataTable.Rows[i];
                                rowList.Add(row);
                            }
                        }
                        if (rowList.Count > 0)
                        {
                            data.Append("<voucher id=\"")
                                .Append(CommonUtil.GetString(rowList[0][59]))
                                .Append("\">")
                                .Append("\r\n");
                            data.Append("<voucher_head>").Append("\r\n");
                            data.Append("<pk_voucher></pk_voucher>").Append("\r\n");
                            data.Append("<pk_vouchertype>")
                                .Append(CommonUtil.GetString(rowList[0][2]))
                                .Append("</pk_vouchertype>")
                                .Append("\r\n");
                            data.Append("<year>")
                                .Append(rowList[0][25].ToString().Substring(0, 4))
                                .Append("</year>")
                                .Append("\r\n");
                            data.Append("<pk_system>GL</pk_system>").Append("\r\n");
                            data.Append("<voucherkind>0</voucherkind>").Append("\r\n");
                            data.Append("<pk_accountingbook>")
                                .Append(CommonUtil.GetString(rowList[0][1]))
                                .Append("</pk_accountingbook>")
                                .Append("\r\n");
                            data.Append("<discardflag>N</discardflag>").Append("\r\n");
                            data.Append("<period>")
                                .Append(CommonUtil.GetString(rowList[0][58]))
                                .Append("</period>")
                                .Append("\r\n");
                            data.Append("<no>")
                                .Append(CommonUtil.GetString(rowList[0][3]))
                                .Append("</no>")
                                .Append("\r\n");
                            data.Append("<attachment>0</attachment>").Append("\r\n");
                            data.Append("<prepareddate>")
                                .Append(CommonUtil.GetString(rowList[0][6]))
                                .Append("</prepareddate>")
                                .Append("\r\n");
                            data.Append("<pk_prepared>")
                                .Append(CommonUtil.GetString(rowList[0][5]))
                                .Append("</pk_prepared>")
                                .Append("\r\n");
                            data.Append("<pk_casher></pk_casher>").Append("\r\n");
                            data.Append("<signflag>Y</signflag>").Append("\r\n");
                            data.Append("<pk_checked>")
                                .Append(CommonUtil.GetString(rowList[0][60]))
                                .Append("</pk_checked>").Append("\r\n");
                            data.Append("<tallydate></tallydate>").Append("\r\n");
                            data.Append("<pk_manager></pk_manager>").Append("\r\n");
                            data.Append("<memo1></memo1>").Append("\r\n");
                            data.Append("<memo2></memo2>").Append("\r\n");
                            data.Append("<reserve1></reserve1>").Append("\r\n");
                            data.Append("<reserve2>N</reserve2>").Append("\r\n");
                            data.Append("<pk_org>669442468</pk_org>").Append("\r\n");
                            data.Append("<pk_org_v>669442468</pk_org_v>").Append("\r\n");
                            data.Append("<pk_group>00</pk_group>").Append("\r\n");
                            data.Append("<details>").Append("\r\n");
                            var count = 0;
                            foreach (var row in rowList)
                            {
                                var debit = row[11].ToString();
                                if (Math.Abs(double.Parse(debit)) > 0.0)
                                {
                                    data.Append("<item>").Append("\r\n");
                                    data.Append("<detailindex>").Append(count).Append("</detailindex>").Append("\r\n");
                                    data.Append("<explanation>")
                                        .Append(CommonUtil.GetString(row[7]))
                                        .Append("</explanation>")
                                        .Append("\r\n");
                                    data.Append("<verifydate></verifydate>").Append("\r\n");
                                    data.Append("<price>0.00000000</price>").Append("\r\n");
                                    data.Append("<explanation>")
                                        .Append(CommonUtil.GetString(row[37]))
                                        .Append("</explanation>")
                                        .Append("\r\n");
                                    data.Append("<debitquantity>0.00000000</debitquantity>").Append("\r\n");
                                    data.Append("<debitamount>")
                                        .Append(CommonUtil.GetString(row[10]))
                                        .Append("</debitamount>")
                                        .Append("\r\n");
                                    data.Append("<localdebitamount>")
                                        .Append(CommonUtil.GetString(row[11]))
                                        .Append("</localdebitamount>")
                                        .Append("\r\n");
                                    data.Append("<groupdebitamount>")
                                        .Append(CommonUtil.GetString(row[11]))
                                        .Append("</groupdebitamount>")
                                        .Append("\r\n");
                                    data.Append("<globaldebitamount>")
                                        .Append(CommonUtil.GetString(row[11]))
                                        .Append("</globaldebitamount>")
                                        .Append("\r\n");
                                    data.Append("<pk_currtype>")
                                        .Append(CommonUtil.GetString(row[9]))
                                        .Append("</pk_currtype>")
                                        .Append("\r\n");
                                    data.Append("<pk_accasoa>")
                                        .Append(CommonUtil.GetString(row[8]))
                                        .Append("</pk_accasoa>")
                                        .Append("\r\n");
                                    data.Append("<ass>").Append("\r\n");

                                    for (var i = 40; i < 49; i++)
                                    {
                                        if (!string.IsNullOrEmpty(row[i].ToString()))
                                        {
                                            var vl = row[i].ToString().Split('-');
                                            if (vl.Length >= 2)
                                            {
                                                var v = "";
                                                for (var j = 1; j < vl.Length; j++)
                                                {
                                                    v = v + vl[j] + "-";
                                                }
                                                v = v.Substring(0, v.Length - 1);
                                                data.Append("<item>").Append("\r\n");
                                                data.Append("<pk_Checktype>")
                                                    .Append(CommonUtil.GetString(vl[0]))
                                                    .Append("</pk_Checktype>")
                                                    .Append("\r\n");
                                                data.Append("<pk_Checkvalue>")
                                                    .Append(CommonUtil.GetString(v))
                                                    .Append("</pk_Checkvalue>")
                                                    .Append("\r\n");
                                                data.Append("</item>").Append("\r\n");
                                            }
                                        }
                                    }
                                    data.Append("</ass>").Append("\r\n");
                                    data.Append("<cashFlow>").Append("\r\n");
                                    data.Append("<item>").Append("\r\n");
                                    data.Append("<m_pk_currtype>")
                                        .Append(CommonUtil.GetString(row[50]))
                                        .Append("</m_pk_currtype>")
                                        .Append("\r\n");
                                    data.Append("<money>")
                                        .Append(CommonUtil.GetString(row[51]))
                                        .Append("</money>")
                                        .Append("\r\n");
                                    data.Append("<moneyglobal>")
                                        .Append(CommonUtil.GetString(row[54]))
                                        .Append("</moneyglobal>")
                                        .Append("\r\n");
                                    data.Append("<moneygroup>")
                                        .Append(CommonUtil.GetString(row[53]))
                                        .Append("</moneygroup>")
                                        .Append("\r\n");
                                    data.Append("<moneymain>")
                                        .Append(CommonUtil.GetString(row[52]))
                                        .Append("</moneymain>")
                                        .Append("\r\n");
                                    data.Append("<pk_cashflow>")
                                        .Append(CommonUtil.GetString(row[57]))
                                        .Append("</pk_cashflow>")
                                        .Append("\r\n");
                                    data.Append("</item>").Append("\r\n");
                                    data.Append("</cashFlow>").Append("\r\n");
                                    data.Append("</item>").Append("\r\n");
                                }
                                else
                                {
                                    data.Append("<item>").Append("\r\n");
                                    data.Append("<creditquantity>0.00000000</creditquantity>").Append("\r\n");
                                    data.Append("<creditamount>")
                                        .Append(CommonUtil.GetString(row[18]))
                                        .Append("</creditamount>")
                                        .Append("\r\n");
                                    data.Append("<localcreditamount>")
                                        .Append(CommonUtil.GetString(row[19]))
                                        .Append("</localcreditamount>")
                                        .Append("\r\n");
                                    data.Append("<groupcreditamount>")
                                        .Append(CommonUtil.GetString(row[19]))
                                        .Append("</groupcreditamount>")
                                        .Append("\r\n");
                                    data.Append("<globalcreditamount>")
                                        .Append(CommonUtil.GetString(row[19]))
                                        .Append("</globalcreditamount>")
                                        .Append("\r\n");


                                    data.Append("<detailindex>").Append(count).Append("</detailindex>").Append("\r\n");
                                    data.Append("<explanation>")
                                        .Append(CommonUtil.GetString(row[7]))
                                        .Append("</explanation>")
                                        .Append("\r\n");
                                    data.Append("<verifydate></verifydate>").Append("\r\n");
                                    data.Append("<price>0.00000000</price>").Append("\r\n");
                                    data.Append("<excrate2></excrate2>").Append("\r\n");
                                    data.Append("<pk_currtype>")
                                        .Append(CommonUtil.GetString(row[9]))
                                        .Append("</pk_currtype>")
                                        .Append("\r\n");
                                    data.Append("<explanation>")
                                        .Append(CommonUtil.GetString(row[37]))
                                        .Append("</explanation>")
                                        .Append("\r\n");


                                    data.Append("<pk_accasoa>")
                                        .Append(CommonUtil.GetString(row[8]))
                                        .Append("</pk_accasoa>")
                                        .Append("\r\n");
                                    data.Append("<ass>").Append("\r\n");

                                    for (var i = 40; i < 49; i++)
                                    {
                                        if (!string.IsNullOrEmpty(row[i].ToString()))
                                        {
                                            var vl = row[i].ToString().Split('-');
                                            if (vl.Length >= 2)
                                            {
                                                var v = "";
                                                for (var j = 1; j < vl.Length; j++)
                                                {
                                                    v = v + vl[j] + "-";
                                                }
                                                v = v.Substring(0, v.Length - 1);

                                                data.Append("<item>").Append("\r\n");
                                                data.Append("<pk_Checktype>")
                                                    .Append(CommonUtil.GetString(vl[0]))
                                                    .Append("</pk_Checktype>")
                                                    .Append("\r\n");
                                                data.Append("<pk_Checkvalue>")
                                                    .Append(CommonUtil.GetString(v))
                                                    .Append("</pk_Checkvalue>")
                                                    .Append("\r\n");
                                                data.Append("</item>").Append("\r\n");
                                            }
                                        }
                                    }
                                    data.Append("</ass>").Append("\r\n");
                                    data.Append("<cashFlow>").Append("\r\n");
                                    data.Append("<item>").Append("\r\n");
                                    data.Append("<m_pk_currtype>")
                                        .Append(CommonUtil.GetString(row[50]))
                                        .Append("</m_pk_currtype>")
                                        .Append("\r\n");
                                    data.Append("<money>")
                                        .Append(CommonUtil.GetString(row[51]))
                                        .Append("</money>")
                                        .Append("\r\n");
                                    data.Append("<moneyglobal>")
                                        .Append(CommonUtil.GetString(row[54]))
                                        .Append("</moneyglobal>")
                                        .Append("\r\n");
                                    data.Append("<moneygroup>")
                                        .Append(CommonUtil.GetString(row[53]))
                                        .Append("</moneygroup>")
                                        .Append("\r\n");
                                    data.Append("<moneymain>")
                                        .Append(CommonUtil.GetString(row[52]))
                                        .Append("</moneymain>")
                                        .Append("\r\n");
                                    data.Append("<pk_cashflow>")
                                        .Append(CommonUtil.GetString(row[57]))
                                        .Append("</pk_cashflow>")
                                        .Append("\r\n");
                                    data.Append("</item>").Append("\r\n");
                                    data.Append("</cashFlow>").Append("\r\n");
                                    data.Append("</item>").Append("\r\n");
                                }
                                count = count + 1;
                            }
                            data.Append("</details>").Append("\r\n");
                            data.Append("</voucher_head>").Append("\r\n");
                            data.Append("</voucher>").Append("\r\n");
                        }
                    }
                    data.Append("</ufinterface>");
                    var writer = new StreamWriter(path);
                    try
                    {
                        writer.Write(data.ToString());
                        writer.Flush();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        writer.Close();
                    }


                    //XmlAndTdHelper.GetInstance().DataTableToXml(dataTable, path);
                    Globle.Application.StatusBar.SetSystemMessage("数据导出成功!", BoMessageTime.bmt_Short,
                        BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception exception)
            {
                Globle.Application.SetStatusBarMessage(exception.Message, BoMessageTime.bmt_Short);
            }
            finally
            {
                progressBar.Stop();
            }
        }
    }
}