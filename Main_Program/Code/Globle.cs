using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HuDongHeavyMachinery.Code.FormExt;
using HuDongHeavyMachinery.Code.Model;
using HuDongHeavyMachinery.Code.Util;
using SAPbobsCOM;
using SAPbouiCOM;
using Company = SAPbobsCOM.Company;
using DataTable = System.Data.DataTable;

namespace HuDongHeavyMachinery.Code
{
    public class Globle
    {
        public static Application Application;
        public static Company DiCompany;
        public static Form CurrentForm;
        public static Recordset ORecordSet;
        public static readonly SortedList<string, SwBaseForm> SwFormsList = new SortedList<string, SwBaseForm>();
        public static readonly Random Random = new Random();
        public static DataTable FormSizeInfo;
        public static readonly string DllPath = Assembly.GetExecutingAssembly().Location;
        public static readonly string MyFormTmp = DllPath.Replace("\\HuDongHeavyMachinery.dll", "") + "\\HuDongHeavyMachineryTmp";
        public static readonly string FormSizeInfoPath = MyFormTmp + "\\formSizeInfo.xml";
        public static readonly string VersionPath = MyFormTmp + "\\version.txt";
        public static Company ScCompany;

        public static void GlobalIntial()
        {
            var menuItemList = new ArrayList();
             string topMenuItemId = "2304";
            //var oMenuItem = new OMenuItem(BoMenuType.mt_POPUP, "COR00", "测试", true, "", 19, topMenuItemId);
            //menuItemList.Add(oMenuItem);
            //oMenuItem = new OMenuItem(BoMenuType.mt_STRING, "COR020030", "测试物料", true, "", 19, "COR00");
            //menuItemList.Add(oMenuItem);
            //var oMenuItem = new OMenuItem(BoMenuType.mt_STRING, "COR020090", "匹配信息更新/添加", true, "", 1, "3072");
            //menuItemList.Add(oMenuItem);
             var  oMenuItem = new OMenuItem(BoMenuType.mt_STRING, "COR020070", "采购到货送检单", true, "", 4, topMenuItemId);
            menuItemList.Add(oMenuItem);
            MenuItemsUtil.AddMenuItems(menuItemList, topMenuItemId);

            menuItemList.Clear();
            topMenuItemId = "3072";
            oMenuItem = new OMenuItem(BoMenuType.mt_STRING, "COR020090", "匹配信息更新/添加", true, "", 1, topMenuItemId);
            menuItemList.Add(oMenuItem);
            MenuItemsUtil.AddMenuItems(menuItemList, topMenuItemId);
            for (var i = 0; i < Application.Forms.Count; i++)
            {
                var formCmdCenter = Application.Forms.Item(i);
                if (formCmdCenter.Type == 169)
                {
                    formCmdCenter.Update();
                    formCmdCenter.Refresh();
                }
            }
            if (!Directory.Exists(MyFormTmp))
            {
                Directory.CreateDirectory(MyFormTmp);
            }
            var newVersion = Assembly.LoadFile(DllPath).GetName().Version;

            if (File.Exists(VersionPath))
            {
                var oldVersion = new Version(CommonUtil.ReadText(VersionPath));
                if (oldVersion < newVersion)
                {
                    if (Directory.Exists(MyFormTmp))
                    {
                        CommonUtil.DeleteFolder(MyFormTmp);
                    }
                    CommonUtil.SaveAsFile(newVersion.ToString(), VersionPath);
                }
            }
            else
            {
                if (Directory.Exists(MyFormTmp))
                {
                    CommonUtil.DeleteFolder(MyFormTmp);
                }
                CommonUtil.SaveAsFile(newVersion.ToString(), VersionPath);
            }


            if (File.Exists(FormSizeInfoPath))
            {
                FormSizeInfo = XmlAndTdHelper.GetInstance().XmlToDataTable(FormSizeInfoPath);
            }
            else
            {
                FormSizeInfo = new DataTable("formSizeInfo");
                FormSizeInfo.Columns.Add("FormTypeEx", typeof(string));
                FormSizeInfo.Columns.Add("Left", typeof(int));
                FormSizeInfo.Columns.Add("Top", typeof(int));
                FormSizeInfo.Columns.Add("Width", typeof(int));
                FormSizeInfo.Columns.Add("Height", typeof(int));
            }
        }

        public static void MenusAdd()
        {
        }
    }
}