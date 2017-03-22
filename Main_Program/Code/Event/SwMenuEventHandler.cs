using System;
using System.Reflection;
using HuDongHeavyMachinery.Code.FormExt;
using HuDongHeavyMachinery.Code.Util;
using SAPbouiCOM;
using StatusBar = SwissAddonFramework.Messaging.StatusBar;

namespace HuDongHeavyMachinery.Code.Event
{
    internal class SwMenuEventHandler
    {
        public static void MenuEventHandler(ref MenuEvent pval, ref bool bubbleevent)
        {
            try
            {
                if (!pval.BeforeAction)
                {
                    var formType = pval.MenuUID;
                    var thisExe = Assembly.GetExecutingAssembly();
                    foreach (var type in thisExe.GetTypes())
                    {
                        var sArray = type.FullName.Split('.');
                        if (sArray[sArray.Length - 1].ToLower() == formType.ToLower())
                        {
                            if (type.BaseType == typeof(SwBaseForm))
                            {
                                Globle.CurrentForm = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
                                break;
                            }
                        }
                    }
                }
                if (pval.BeforeAction)
                {
                    Globle.CurrentForm = Globle.Application.Forms.ActiveForm;
                }

                foreach (var entry in Globle.SwFormsList)
                {
                    var swForm = entry.Value;
                    if (Globle.CurrentForm.UniqueID == swForm.MyFormUid)
                    {
                        if (swForm.MyForm == null)
                            swForm.MyForm = Globle.CurrentForm;
                        swForm.MenuEventHandler(ref pval, ref bubbleevent);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusBar.WriteError("SwMenuEventHandler" + ex.Message, StatusBar.MessageTime.Short);
            }
        }
    }
}