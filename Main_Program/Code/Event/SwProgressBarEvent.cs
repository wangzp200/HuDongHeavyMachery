﻿using System;
using SAPbouiCOM;
using StatusBar = SwissAddonFramework.Messaging.StatusBar;

namespace HuDongHeavyMachinery.Code.Event
{
    internal class SwProgressBarEvent
    {
        public static void ProgressBarEventHandler(ref ProgressBarEvent pval, ref bool bubbleevent)
        {
            try
            {
                foreach (var entry in Globle.SwFormsList)
                {
                    var key = entry.Key;
                    var swForm = entry.Value;
                    swForm.ProgressBarEventHandler(ref pval, ref bubbleevent);
                }
            }
            catch (Exception exception)
            {
                StatusBar.WriteError("SwProgressBarEvent" + exception.Message, StatusBar.MessageTime.Short);
            }
        }
    }
}