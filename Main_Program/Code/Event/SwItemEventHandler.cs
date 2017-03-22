using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using HuDongHeavyMachinery.Code.FormExt;
using HuDongHeavyMachinery.Code.Util;
using SAPbouiCOM;
using StatusBar = SwissAddonFramework.Messaging.StatusBar;

namespace HuDongHeavyMachinery.Code.Event
{
    internal class SwItemEventHandler
    {
        public static void ItemEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            try
            {
                switch (pVal.BeforeAction)
                {
                    case true:
                        ItemBeforeActionEventHandler(formUId, ref pVal, ref bubbleEvent);
                        break;
                    case false:
                        ItemNotBeforeActionEventHandler(formUId, ref pVal, ref bubbleEvent);
                        break;
                }
                if (Globle.SwFormsList.ContainsKey(formUId))
                {
                    var swBaseForm = Globle.SwFormsList[formUId];
                    if (swBaseForm.MyForm != null)
                    {
                        if (swBaseForm.MyForm.Items.Count > 0 && !swBaseForm.Active)
                        {
                            swBaseForm.Active = true;
                            swBaseForm.FormCreate(formUId, ref pVal, ref bubbleEvent);
                        }
                        if (swBaseForm.Active)
                            swBaseForm.ItemEventHandler(formUId, ref pVal, ref bubbleEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusBar.WriteError("SwItemEventHandler:" + ex.Message + ex.Source, StatusBar.MessageTime.Short);
            }
        }

        /// <summary>
        ///     Item事件发生之后
        /// </summary>
        /// <param name="formUId"></param>
        /// <param name="pVal"></param>
        /// <param name="bubbleEvent"></param>
        private static void ItemNotBeforeActionEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            switch (pVal.EventType)
            {
                case BoEventTypes.et_FORM_RESIZE:
                    if (Globle.SwFormsList.ContainsKey(formUId))
                    {
                        var swForm = Globle.SwFormsList[formUId];
                        var form = (swForm.MyForm == null ? Globle.Application.Forms.Item(formUId) : swForm.MyForm);
                        if (!form.IsSystem)
                        {
                            var update = false;
                            foreach (DataRow entry in Globle.FormSizeInfo.Rows)
                            {
                                if (entry["FormTypeEx"].ToString() == pVal.FormTypeEx)
                                {
                                    entry["Left"] = form.Left;
                                    entry["Top"] = form.Top;
                                    entry["Width"] = form.Width;
                                    entry["Height"] = form.Height;
                                    update = true;
                                    break;
                                }
                            }
                            if (!update)
                            {
                                var newRow = Globle.FormSizeInfo.NewRow();
                                newRow["FormTypeEx"] = pVal.FormTypeEx;
                                newRow["Left"] = form.Left;
                                newRow["Top"] = form.Top;
                                newRow["Width"] = form.Width;
                                newRow["Height"] = form.Height;
                                Globle.FormSizeInfo.Rows.Add(newRow);
                            }
                        }
                    }
                    break;


                case BoEventTypes.et_DATASOURCE_LOAD:
                    if (Globle.SwFormsList.ContainsKey(formUId))
                    {
                        var swBaseForm = Globle.SwFormsList[formUId];
                        var oForm = swBaseForm.MyForm;
                        if (oForm.DataSources.DBDataSources.Count > 0 && Globle.SwFormsList.ContainsKey(formUId))
                        {
                            var keyFieldList = swBaseForm.KeyFieldList;
                            foreach (var entry in keyFieldList)
                            {
                                var db = oForm.DataSources.DBDataSources.Item(entry.Key);
                                var field = entry.Value;
                                for (var k = db.Size - 1; k >= 0; k--)
                                {
                                    if (string.IsNullOrEmpty(db.GetValue(field, k)))
                                    {
                                        db.RemoveRecord(k);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case BoEventTypes.et_FORM_LOAD:
                    if (Globle.SwFormsList.ContainsKey(formUId))
                    {
                        var swBaseForm = Globle.SwFormsList[formUId];
                        if (swBaseForm.MyForm == null)
                        {
                            var oForm = Globle.Application.Forms.Item(formUId);
                            Globle.CurrentForm = oForm;
                            swBaseForm.MyForm = oForm;
                        }
                    }
                    break;


                case BoEventTypes.et_GOT_FOCUS:
                    if (Globle.SwFormsList.ContainsKey(formUId))
                    {
                        var swBaseForm = Globle.SwFormsList[formUId];
                        SaveFormXml(swBaseForm);
                    }
                    break;
            }
        }

        private static void SaveFormXml(SwBaseForm swBaseForm)
        {
            if (!swBaseForm.Active && !swBaseForm.MyForm.IsSystem)
            {
                if (!File.Exists(Globle.MyFormTmp + "\\" + swBaseForm.MyForm.TypeEx + ".ftxt"))
                {
                    Globle.Application.SetStatusBarMessage("Initialize the form....", BoMessageTime.bmt_Short, false);
                    for (var i = 0; i < swBaseForm.MyForm.Items.Count; i++)
                    {
                        var loitem = swBaseForm.MyForm.Items.Item(i);
                        if (loitem.Type == BoFormItemTypes.it_MATRIX)
                        {
                            var loMtx = (Matrix) loitem.Specific;
                            for (var j = 0; j < loMtx.Columns.Count; j++)
                            {
                                var column = loMtx.Columns.Item(j);
                                if (column.Type == BoFormItemTypes.it_EDIT && column.DataBind.DataBound)
                                {
                                    var tableName = column.DataBind.TableName;
                                    for (var k = 0; k < swBaseForm.MyForm.DataSources.DBDataSources.Count; k++)
                                    {
                                        var db = swBaseForm.MyForm.DataSources.DBDataSources.Item(k);
                                        if (tableName == db.TableName)
                                        {
                                            for (var l = 0; l < db.Fields.Count; l++)
                                            {
                                                var field = db.Fields.Item(l);
                                                if (field.Name == column.DataBind.Alias)
                                                {
                                                    if (field.Type == BoFieldsType.ft_Percent ||
                                                        field.Type == BoFieldsType.ft_Price ||
                                                        field.Type == BoFieldsType.ft_Quantity ||
                                                        field.Type == BoFieldsType.ft_Rate ||
                                                        field.Type == BoFieldsType.ft_Sum ||
                                                        field.Type == BoFieldsType.ft_Float)
                                                    {
                                                        column.RightJustified = true;
                                                    }
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }

                                    for (var k = 0; k < swBaseForm.MyForm.DataSources.DataTables.Count; k++)
                                    {
                                        var table = swBaseForm.MyForm.DataSources.DataTables.Item(k);
                                        if (tableName == table.UniqueID)
                                        {
                                            for (var l = 0; l < table.Columns.Count; l++)
                                            {
                                                var field = table.Columns.Item(l);
                                                if (field.Name == column.DataBind.Alias)
                                                {
                                                    if (field.Type == BoFieldsType.ft_Percent ||
                                                        field.Type == BoFieldsType.ft_Price ||
                                                        field.Type == BoFieldsType.ft_Quantity ||
                                                        field.Type == BoFieldsType.ft_Rate ||
                                                        field.Type == BoFieldsType.ft_Sum ||
                                                        field.Type == BoFieldsType.ft_Float)
                                                    {
                                                        column.RightJustified = true;
                                                    }
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (loitem.Type == BoFormItemTypes.it_EDIT)
                        {
                            var edit = (EditText) loitem.Specific;
                            if (edit.DataBind.DataBound)
                            {
                                var alias = edit.DataBind.Alias;
                                var tableName = edit.DataBind.TableName;
                                if (!string.IsNullOrEmpty(tableName))
                                {
                                    for (var k = 0; k < swBaseForm.MyForm.DataSources.DBDataSources.Count; k++)
                                    {
                                        var db = swBaseForm.MyForm.DataSources.DBDataSources.Item(k);
                                        if (tableName == db.TableName)
                                        {
                                            for (var l = 0; l < db.Fields.Count; l++)
                                            {
                                                var field = db.Fields.Item(l);
                                                if (field.Name == alias && alias != "DocNum")
                                                {
                                                    if (field.Type == BoFieldsType.ft_Percent ||
                                                        field.Type == BoFieldsType.ft_Price ||
                                                        field.Type == BoFieldsType.ft_Quantity ||
                                                        field.Type == BoFieldsType.ft_Rate ||
                                                        field.Type == BoFieldsType.ft_Sum ||
                                                        field.Type == BoFieldsType.ft_Float)
                                                    {
                                                        loitem.RightJustified = true;
                                                    }
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    for (var j = 0; j < swBaseForm.MyForm.DataSources.UserDataSources.Count; j++)
                                    {
                                        var userdb = swBaseForm.MyForm.DataSources.UserDataSources.Item(j);
                                        if (userdb.UID == alias)
                                        {
                                            if (userdb.DataType == BoDataType.dt_PERCENT ||
                                                userdb.DataType == BoDataType.dt_PRICE ||
                                                userdb.DataType == BoDataType.dt_QUANTITY ||
                                                userdb.DataType == BoDataType.dt_RATE ||
                                                userdb.DataType == BoDataType.dt_SUM ||
                                                userdb.DataType == BoDataType.dt_LONG_NUMBER ||
                                                userdb.DataType == BoDataType.dt_SHORT_NUMBER
                                                )
                                            {
                                                loitem.RightJustified = true;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    var xml = swBaseForm.MyForm.GetAsXML();
                    xml = ZipFileHelper.CompressString(xml);
                    CommonUtil.SaveAsFile(xml, Globle.MyFormTmp + "\\" + swBaseForm.MyForm.TypeEx + ".ftxt");
                }
            }
        }

        /// <summary>
        ///     Item事件发生之前
        /// </summary>
        /// <param name="formUId"></param>
        /// <param name="pVal"></param>
        /// <param name="bubbleEvent"></param>
        private static void ItemBeforeActionEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            Form oForm = null;
            switch (pVal.EventType)
            {
                //窗体一旦被关闭，从对应的SwFormsList移除
                case BoEventTypes.et_FORM_UNLOAD:
                    if (Globle.SwFormsList.ContainsKey(formUId))
                    {
                        Globle.SwFormsList.Remove(formUId);
                    }
                    break;
                //出现窗体加载的时候实例化对应的SwForm，主要针对于系统窗体
                case BoEventTypes.et_FORM_LOAD:

                    if (!Globle.SwFormsList.ContainsKey(formUId))
                    {
                        for (var i = 0; i < Globle.Application.Forms.Count; i++)
                        {
                            oForm = Globle.Application.Forms.Item(i);
                            if (oForm.UniqueID == formUId)
                            {
                                if (oForm.IsSystem)
                                {
                                    var formType = pVal.FormTypeEx;
                                    if (formType.StartsWith("-"))
                                    {
                                        formType = formType.Remove(0, 1) + "UDF";
                                    }
                                    else if (formType.EndsWith("-U-R"))
                                    {
                                      formType=  formType.Replace("-U-R", "_U_R");
                                    }
                                    var formUid = pVal.FormUID;
                                    var thisExe = Assembly.GetExecutingAssembly();
                                    foreach (var type in thisExe.GetTypes())
                                    {
                                        var sArray = type.FullName.Split('.');
                                        if (sArray[sArray.Length - 1].Equals("System" + formType))
                                        {
                                            if (type.BaseType == typeof (SwBaseForm))
                                            {
                                                var swBaseForm = (SwBaseForm) Activator.CreateInstance(type);
                                                swBaseForm.MyForm = oForm;
                                                swBaseForm.MyFormUid = formUid;
                                                swBaseForm.EventForm = Globle.Application.Forms.GetEventForm(formUid);
                                                Globle.SwFormsList.Add(formUid, swBaseForm);
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    break;
                //当确定按钮点击时，判断数据是否已经被修改。保证数据的一致性。
                case BoEventTypes.et_ITEM_PRESSED:

                    if (pVal.ItemUID == "1")
                    {
                        oForm = Globle.Application.Forms.Item(formUId);
                        if (oForm.Mode == BoFormMode.fm_UPDATE_MODE)
                        {
                            if (oForm.DataSources.DBDataSources.Count > 0)
                            {
                                var db = oForm.DataSources.DBDataSources.Item(0);

                                for (var i = 0; i < db.Fields.Count; i++)
                                {
                                    if (db.Fields.Item(i).Name == "U_UpdateTS")
                                    {
                                        var dtFormat = new DateTimeFormatInfo
                                        {
                                            ShortDatePattern = "yyyy-MM-dd HH:mm:ss.fff"
                                        };
                                        var docentry = string.IsNullOrEmpty(db.GetValue("DocEntry", 0))
                                            ? "-1"
                                            : db.GetValue("DocEntry", 0);
                                        var sql =
                                            "select  isnull(\"U_UpdateTS\",CONVERT(varchar(100),GETDATE(),25))  from [" +
                                            db.TableName + "] where \"docentry\"=" + docentry + " limit 1";
                                        Globle.ORecordSet.DoQuery(sql);
                                        Globle.ORecordSet.MoveFirst();
                                        var updateTs =
                                            string.IsNullOrEmpty(Globle.ORecordSet.Fields.Item(0).Value as string)
                                                ? DateTime.MinValue
                                                : DateTime.Parse(Globle.ORecordSet.Fields.Item(0).Value as string,
                                                    dtFormat);

                                        if (db.GetValue("U_UpdateTS", 0) != null)
                                        {
                                            if (!string.IsNullOrEmpty(db.GetValue("U_UpdateTS", 0).Trim()))
                                            {
                                                var nowTime = string.IsNullOrEmpty(db.GetValue("U_UpdateTS", 0).Trim())
                                                    ? DateTime.Now
                                                    : DateTime.Parse(db.GetValue("U_UpdateTS", 0).Trim(), dtFormat);
                                                var span = updateTs - nowTime;
                                                if (span.Seconds > 20)
                                                {
                                                    const string errorMessage =
                                                        "This Form had been updated,Must be reload!";
                                                    Globle.Application.SetStatusBarMessage(errorMessage,
                                                        BoMessageTime.bmt_Short);
                                                    bubbleEvent = false;
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    break;
                //这里主要是处理LinkButton事件，链接对应的UDO窗体。
                case BoEventTypes.et_CLICK:
                case BoEventTypes.et_MATRIX_LINK_PRESSED:
                    var obj = string.Empty;
                    var key = string.Empty;

                    if (pVal.EventType == BoEventTypes.et_CLICK)
                    {
                        //------------------------------------------LinkButton 触发连接UDO----------------------------------------------------------
                        if (!string.IsNullOrEmpty(formUId))
                        {
                            for (var i = 0; i < Globle.Application.Forms.Count; i++)
                            {
                                if (Globle.Application.Forms.Item(i).UniqueID == pVal.ItemUID)
                                {
                                    if (Globle.Application.Forms.Item(i).Items != null)
                                    {
                                        oForm = Globle.Application.Forms.Item(i);
                                    }
                                    break;
                                }
                            }
                            if (oForm != null)
                            {
                                var item = oForm.Items.Item(pVal.ItemUID);
                                if (item.Type == BoFormItemTypes.it_LINKED_BUTTON)
                                {
                                    var link = (LinkedButton) item.Specific;
                                    obj = link.LinkedObjectType;
                                    var linkTo = item.LinkTo;
                                    if (oForm.Items.Item(linkTo).Type == BoFormItemTypes.it_EDIT)
                                    {
                                        key = ((EditText) oForm.Items.Item(linkTo).Specific).Value;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //------------------------------------------Matx或者Grid 触发连接UDO----------------------------------------------------------
                        oForm = Globle.Application.Forms.Item(formUId);
                        var oitm = oForm.Items.Item(pVal.ItemUID);
                        switch (oitm.Type)
                        {
                            case BoFormItemTypes.it_MATRIX:
                                var mtx = (Matrix) oitm.Specific;
                                var column = mtx.Columns.Item(pVal.ColUID);
                                obj = ((LinkedButton) column.ExtendedObject).LinkedObjectType;
                                key = ((EditText) column.Cells.Item(pVal.Row).Specific).Value;
                                break;
                            case BoFormItemTypes.it_GRID:
                                var grid = (Grid) oitm.Specific;
                                var dataTable = grid.DataTable;
                                var row = pVal.Row;
                                var editText = (EditTextColumn) grid.Columns.Item(pVal.ColUID);
                                if (editText != null)
                                    obj = editText.LinkedObjectType;
                                key = dataTable.GetValue(pVal.ColUID, row).ToString();
                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(obj) && !string.IsNullOrEmpty(key))
                    {
                        var sql = "SELECT \"Code\",\"TableName\",\"TYPE\" FROM OUDO WHERE \"Code\" ='" + obj + "'";
                        Globle.ORecordSet.DoQuery(sql);
                        if (Globle.ORecordSet.EoF) return;
                        var tableName = Globle.ORecordSet.Fields.Item(1).Value.ToString();
                        var type = Globle.ORecordSet.Fields.Item(2).Value.ToString();
                        if (type == "1")
                        {
                            sql = "select \"Code\" from \"@" + tableName + "\" where \"Code\"='" + key + "'";
                        }
                        else
                        {
                            sql = "select \"DocEntry\"  from \"@" + tableName + "\" where \"DocEntry\"='" + key + "'";
                        }
                        Globle.ORecordSet.DoQuery(sql);
                        if (Globle.ORecordSet.EoF) return;
                        Form linkForm = null;
                        try
                        {
                            linkForm = CreateNewFormUtil.CreateNewForm(obj, -1, -1);
                            if (linkForm == null) return;

                            linkForm.Freeze(true);
                            var keyType = type == "1" ? "Code" : "DocEntry";
                            for (var i = 0; i < linkForm.Items.Count; i++)
                            {
                                if (linkForm.Items.Item(i).Type == BoFormItemTypes.it_EDIT)
                                {
                                    var editTextKey = (EditText) linkForm.Items.Item(i).Specific;
                                    if (editTextKey.DataBind.TableName == "@" + tableName &&
                                        editTextKey.DataBind.Alias == keyType)
                                    {
                                        if (linkForm.Mode != BoFormMode.fm_FIND_MODE)
                                        {
                                            linkForm.Mode = BoFormMode.fm_FIND_MODE;
                                        }
                                        var btok = (Button) linkForm.Items.Item("1").Specific;
                                        var mAble = false;
                                        if (!editTextKey.Item.Enabled)
                                        {
                                            editTextKey.Item.Enabled = true;
                                            mAble = true;
                                        }
                                        editTextKey.Value = key;
                                        if (mAble)
                                        {
                                            editTextKey.Item.Enabled = false;
                                        }
                                        btok.Item.Click();
                                        bubbleEvent = false;
                                        return;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            StatusBar.WriteError("SwItemEventHandler:" + ex.Message + ex.Source,
                                StatusBar.MessageTime.Short);
                        }
                        finally
                        {
                            if (linkForm != null)
                            {
                                linkForm.Freeze(false);
                            }
                        }
                    }

                    break;
            }
        }
    }
}