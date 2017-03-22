﻿using System;
using SAPbouiCOM;
using StatusBar = SwissAddonFramework.Messaging.StatusBar;

namespace HuDongHeavyMachinery.Code.Event
{
    internal class SwApplicationEventHandler
    {
        public static void ApplicationEventHandler(BoAppEventTypes eventtype)
        {
            try
            {
                foreach (var entry in Globle.SwFormsList)
                {
                    var swForm = entry.Value;
                    swForm.ApplicationEventHandler(eventtype);
                    break;
                }
            }
            catch (Exception ex)
            {
                StatusBar.WriteError("SwApplicationEventHandler:" + ex.Message, StatusBar.MessageTime.Short);
            }
        }
    }
}