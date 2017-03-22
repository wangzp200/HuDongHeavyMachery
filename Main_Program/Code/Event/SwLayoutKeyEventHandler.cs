using System;
using SAPbouiCOM;
using StatusBar = SwissAddonFramework.Messaging.StatusBar;

namespace HuDongHeavyMachinery.Code.Event
{
    internal class SwLayoutKeyEventHandler
    {
        public static void LayoutKeyEventEventHandler(ref LayoutKeyInfo eventinfo, out bool bubbleevent)
        {
            var bubbleevents = true;
            try
            {
                if (Globle.SwFormsList.ContainsKey(eventinfo.FormUID))
                {
                    var swForm = Globle.SwFormsList[eventinfo.FormUID];

                    swForm.LayoutKeyEventHandler(ref eventinfo, ref bubbleevents);
                }
            }
            catch (Exception ex)
            {
                StatusBar.WriteError("SwMenuEventHandler" + ex.Message, StatusBar.MessageTime.Short);
            }
            finally
            {
                bubbleevent = bubbleevents;
            }
        }
    }
}