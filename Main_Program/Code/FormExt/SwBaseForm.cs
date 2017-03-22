using System.Collections.Generic;
using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt
{
    public class SwBaseForm
    {
        public bool Active;

        public SwBaseForm()
        {
            KeyFieldList = new SortedList<string, string>();
            Active = false;
        }

        public EventForm EventForm;

        public string MyFormUid { get; set; }
        public Form MyForm { get; set; }
        public string MyFatherUid { set; get; }
        public string MySonUid { set; get; }
        public SortedList<string, string> KeyFieldList { get; set; }

        public virtual void ApplicationEventHandler(BoAppEventTypes eventType)
        {
        }

        public virtual void SonFormCloseEventHandler(object obj, SwBaseForm sonSwBaseForm)
        {
        }

        public virtual void SetDataEventHandler(object obj, SwBaseForm swBaseForm)
        {
        }

        public virtual void FormDataEventHandler(ref BusinessObjectInfo businessObjectInfo, ref bool bubbleEvent)
        {
        }

        public virtual void FormLoadedEventHandler(string formUid, string formTypeEx, object pVal, ref bool bubbleEvent)
        {
        }

        public virtual void ItemEventHandler(string formUid, ref ItemEvent pVal, ref bool bubbleEvent)
        {
        }

        public virtual void MenuEventHandler(ref MenuEvent pVal, ref bool bubbleEvent)
        {
        }

        public virtual void PrintEventHandler(ref PrintEventInfo eventInfo, ref bool bubbleEvent)
        {
        }

        public virtual void ProgressBarEventHandler(ref ProgressBarEvent pVal, ref bool bubbleEvent)
        {
        }

        public virtual void ReportDataEventHandler(ref ReportDataInfo eventinfo, ref bool bubbleEvent)
        {
        }

        public virtual void RightClickHandler(ref ContextMenuInfo eventInfo, ref bool bubbleEvent)
        {
        }

        public virtual void StatusBarEventHandler(string text, BoStatusBarMessageType messageType)
        {
        }

        public virtual void FormDataAdd(ref BusinessObjectInfo businessobjectinfo, ref bool bubbleevent)
        {
        }

        public virtual void FormDataUpdate(ref BusinessObjectInfo businessobjectinfo, ref bool bubbleevent)
        {
        }

        public virtual void FormDataDelete(ref BusinessObjectInfo businessobjectinfo, ref bool bubbleevent)
        {
        }

        public virtual void FormDataLoad(ref BusinessObjectInfo businessobjectinfo, ref bool bubbleevent)
        {
        }

        public virtual void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
        }

        public virtual void LayoutKeyEventHandler(ref LayoutKeyInfo eventinfo, ref bool bubbleevent)
        {
        }
    }
}