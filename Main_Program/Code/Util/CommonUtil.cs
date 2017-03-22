using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SAPbouiCOM;
using Company = SAPbobsCOM.Company;

namespace HuDongHeavyMachinery.Code.Util
{
    public class CommonUtil
    {
        public static string GetSpace(int num)
        {
            var spaces = "";
            for (var i = 0; i < num; i++)
            {
                spaces += " ";
            }
            return spaces;
        }

        public static string GetString(object obj)
        {
            var result =
                obj.ToString()
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("&", "&amp;")
                    .Replace("\'", "&apos;")
                    .Replace("\"", "&quot;");
            var va = result;
            if (va.StartsWith("-."))
            {
                va = result.Replace("-.", "-0.");
            }
            else if (va.StartsWith("."))
            {
                va = result.Replace(".", "0.");
            }
            var regex = new Regex(@"^[-]?[1-9]{1}\d*$|^[0]{1}$");
            var match = regex.IsMatch(va);
            if (!match)
            {
                return result;
            }
            return va;
        }

        public static int GetMaxLineId(DBDataSource lsDbs, string lsEntry)
        {
            if (string.IsNullOrEmpty(lsEntry))
            {
                lsEntry = "-1";
            }
            var lssql = "SELECT ISNULL(MAX(t10.\"LineId\"), 0) AS lineid  FROM ( SELECT * FROM [@A" +
                        lsDbs.TableName.Remove(0, 1) + "]" +
                        " WHERE \"DocEntry\" = " + lsEntry +
                        " UNION ALL " +
                        " SELECT * FROM [@" + lsDbs.TableName.Remove(0, 1) + "]  WHERE \"DocEntry\" = " + lsEntry +
                        " ) t10";
            Globle.ORecordSet.DoQuery(lssql);
            var liMaxLine = 0;
            while (Globle.ORecordSet.EoF == false)
            {
                liMaxLine = (int) Globle.ORecordSet.Fields.Item(0).Value;
                for (var i = 0; i < lsDbs.Size; i++)
                {
                    if (!string.IsNullOrEmpty(lsDbs.GetValue("LineId", i)))
                    {
                        if (liMaxLine < int.Parse(lsDbs.GetValue("LineId", i)))
                        {
                            liMaxLine = int.Parse(lsDbs.GetValue("LineId", i));
                        }
                    }
                }
                Globle.ORecordSet.MoveNext();
            }
            return liMaxLine;
        }

        public static void MtxAddRow(Matrix mtx, DBDataSource db, bool flg)
        {
            mtx.FlushToDataSource();
            if (!flg)
                db.InsertRecord(db.Size);
            db.SetValue("LineId", db.Size - 1, (GetMaxLineId(db, db.GetValue("DocEntry", 0)) + 1).ToString());
            mtx.LoadFromDataSource();
        }

        public static void SeriesValidValues(ValidValues validValues, Form oForm)
        {
            while (validValues.Count > 0)
            {
                validValues.Remove(0, BoSearchKey.psk_Index);
            }
            var lsObjectCode = oForm.TypeEx;
            var sql =
                "SELECT CAST(\"Series\" AS NVARCHAR(10)),\"SeriesName\" FROM NNM1 WHERE  \"Locked\"='N' AND \"ObjectCode\"='" +
                lsObjectCode + "'";
            Globle.ORecordSet.DoQuery(sql);
            while (Globle.ORecordSet.EoF == false)
            {
                var key = Globle.ORecordSet.Fields.Item(0).Value as string;
                var value = Globle.ORecordSet.Fields.Item(1).Value as string;
                validValues.Add(key, value);
                Globle.ORecordSet.MoveNext();
            }
        }

        public static void DeleteFolder(string dir)
        {
            foreach (var entry in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(entry))
                {
                    var fi = new FileInfo(entry);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly", StringComparison.Ordinal) != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(entry);
                }
                else
                {
                    var directoryInfo = new DirectoryInfo(entry);
                    if (directoryInfo.GetFiles().Length != 0)
                    {
                        DeleteFolder(directoryInfo.FullName);
                    }
                    Directory.Delete(entry);
                }
            }
        }

        public static void SaveAsFile(string content, string path)
        {
            var fs1 = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(fs1);
            sw.WriteLine(content);
            sw.Close();
            fs1.Close();
        }

        public static string ReadText(string path)
        {
            var content = string.Empty;
            if (File.Exists(path))
            {
                var sr = new StreamReader(path, Encoding.UTF8);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    content = content + line;
                }
                sr.Close();
            }
            return content;
        }

        public static void FlushToDataSource()
        {
        }

        public static bool IsFileInUse(string fileName)
        {
            var inUse = true;
            if (File.Exists(fileName))
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    inUse = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }
                return inUse; //true表示正在使用,false没有使用
            }
            return false; //文件不存在则一定没有被使用
        }

        public static Type getType(BoFieldsType boFieldsType)
        {
            var obj = typeof (string);
            switch (boFieldsType)
            {
                case BoFieldsType.ft_NotDefined:
                    obj = typeof (string);
                    break;
                case BoFieldsType.ft_AlphaNumeric:
                    obj = typeof (string);
                    break;
                case BoFieldsType.ft_Integer:
                    obj = typeof (int);
                    break;
                case BoFieldsType.ft_Text:
                    obj = typeof (string);
                    break;
                case BoFieldsType.ft_Date:
                    obj = typeof (DateTime);
                    break;
                case BoFieldsType.ft_Float:
                    obj = typeof (float);
                    break;
                case BoFieldsType.ft_ShortNumber:
                    obj = typeof (int);
                    break;
                case BoFieldsType.ft_Quantity:
                    obj = typeof (decimal);
                    break;
                case BoFieldsType.ft_Price:
                    obj = typeof (decimal);
                    break;
                case BoFieldsType.ft_Rate:
                    obj = typeof (decimal);
                    break;
                case BoFieldsType.ft_Measure:
                    obj = typeof (decimal);
                    break;
                case BoFieldsType.ft_Sum:
                    obj = typeof (decimal);
                    break;
                case BoFieldsType.ft_Percent:
                    obj = typeof (decimal);
                    break;
            }

            return obj;
        }

        public static Company getSCompany()
        {
            Globle.Application.StatusBar.SetSystemMessage("正在连接DI....", BoMessageTime.bmt_Medium,
                BoStatusBarMessageType.smt_Success);
            var company = (Company) Globle.Application.Company.GetDICompany();
            var ocompany = new Company
            {
                CompanyDB = company.CompanyDB,
                Server = company.Server,
                language = company.language,
                DbServerType = company.DbServerType,
                LicenseServer = company.LicenseServer,
                DbUserName = "SYSTEM",
                DbPassword = "Passw0rd",
                UserName = "manager",
                Password = "123456q."
            };

            var ret = ocompany.Connect();
            if (ret != 0)
            {
                var errMsg = "";
                var errCode = 0;
                ocompany.GetLastError(out errCode, out errMsg);
                Globle.Application.StatusBar.SetSystemMessage(
                    "使用DI连接服务器失败,信息:errCode:" + errCode + ",errMsg:" + errMsg, BoMessageTime.bmt_Short,
                    BoStatusBarMessageType.smt_Error);
                return null;
            }
            return ocompany;
        }

        internal static object getValue(string p, BoFieldsType boFieldsType)
        {
            if (string.IsNullOrEmpty(p))
            {
                return null;
            }
            object obj = p;
            switch (boFieldsType)
            {
                case BoFieldsType.ft_NotDefined:
                    obj = p;
                    break;
                case BoFieldsType.ft_AlphaNumeric:
                    obj = p;
                    break;
                case BoFieldsType.ft_Integer:
                    obj = int.Parse(p);
                    break;
                case BoFieldsType.ft_Text:
                    obj = p;
                    break;
                case BoFieldsType.ft_Date:
                    obj = DateTime.Parse(p);
                    break;
                case BoFieldsType.ft_Float:
                    obj = float.Parse(p);
                    break;
                case BoFieldsType.ft_ShortNumber:
                    obj = int.Parse(p);
                    break;
                case BoFieldsType.ft_Quantity:
                    obj = decimal.Parse(p);
                    break;
                case BoFieldsType.ft_Price:
                    obj = decimal.Parse(p);
                    break;
                case BoFieldsType.ft_Rate:
                    obj = decimal.Parse(p);
                    break;
                case BoFieldsType.ft_Measure:
                    obj = decimal.Parse(p);
                    break;
                case BoFieldsType.ft_Sum:
                    obj = decimal.Parse(p);
                    break;
                case BoFieldsType.ft_Percent:
                    obj = decimal.Parse(p);
                    break;
            }

            return obj;
        }
    }
}