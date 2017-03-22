using coresuiteFramework.Loader.Module;
using HuDongHeavyMachinery.Code;
using HuDongHeavyMachinery.Code.Event;
using HuDongHeavyMachinery.Code.Util;
using SAPbobsCOM;
using SAPbouiCOM;
using SwissAddonFramework;
using Company = SAPbobsCOM.Company;
using MenuItem = SwissAddonFramework.UI.Components.MenuItem;
using StatusBar = SwissAddonFramework.Messaging.StatusBar;

namespace HuDongHeavyMachinery
{
    public class ProgramIModule : IModule
    {
        public void CompanyChanged()
        {
            if (Globle.FormSizeInfo != null)
            {
                if (Globle.FormSizeInfo.Rows.Count > 0)
                {
                    XmlAndTdHelper.GetInstance().DataTableToXml(Globle.FormSizeInfo, Globle.FormSizeInfoPath);
                }
            }
            Global.ItemEvent -= SwItemEventHandler.ItemEventHandler;
            Global.ApplicationEvent -= SwApplicationEventHandler.ApplicationEventHandler;
            Global.FormDataEvent -= SwFormDataEventHandler.FormDataEventHandler;
            Global.FormLoadedEvent -= SwFormLoadedEventHandler.FormLoadedEventHandler;
            Global.MenuEvent -= SwMenuEventHandler.MenuEventHandler;
            Global.PrintEvent -= SwPrintEventHandler.PrintEventHandler;
            Global.ProgressBarEvent -= SwProgressBarEvent.ProgressBarEventHandler;
            Global.ReportDataEvent -= SwReportDataEventHandler.ReportDataEventHandler;
            Global.RightClickEvent -= SwRightClickHandler.RightClickHandler;
            Global.StatusBarEvent -= SwStatusBarEventHandler.StatusBarEventHandler;
            Globle.Application.LayoutKeyEvent -= SwLayoutKeyEventHandler.LayoutKeyEventEventHandler;
        }

        public void CreateMenu(MenuItem menuItemConfiguration)
        {
        }

        public void Install()
        {
            //throw new NotImplementedException();
        }

        public void LanguageChanged()
        {
            //throw new NotImplementedException();
        }

        public string ModuleGuid
        {
            get { return "HuDongHeavyMachinery"; }
        }

        public string ModuleInfoLink
        {
            get { return "http://www.coresystems.ch"; }
        }

        public string ModuleName
        {
            get { return "HuDong Heavy Machinery"; }
        }

        public string ModuleVersion
        {
            get { return "1.31.00075"; }
        }

        public bool PreInstall()
        {
            //throw new NotImplementedException();
            return false;
        }

        public void Run()
        {
            StatusBar.WriteSucess("Running " + ModuleName);

            Global.ItemEvent += SwItemEventHandler.ItemEventHandler;
            Global.ApplicationEvent += SwApplicationEventHandler.ApplicationEventHandler;
            Global.FormDataEvent += SwFormDataEventHandler.FormDataEventHandler;
            Global.FormLoadedEvent += SwFormLoadedEventHandler.FormLoadedEventHandler;
            Global.MenuEvent += SwMenuEventHandler.MenuEventHandler;
            Global.PrintEvent += SwPrintEventHandler.PrintEventHandler;
            Global.ProgressBarEvent += SwProgressBarEvent.ProgressBarEventHandler;
            Global.ReportDataEvent += SwReportDataEventHandler.ReportDataEventHandler;
            Global.RightClickEvent += SwRightClickHandler.RightClickHandler;
            Global.StatusBarEvent += SwStatusBarEventHandler.StatusBarEventHandler;

            Globle.Application = B1Connector.GetB1Connector().Application;
            Globle.Application.LayoutKeyEvent += SwLayoutKeyEventHandler.LayoutKeyEventEventHandler;


            Globle.Application.AppEvent += Application_AppEvent;
            Globle.DiCompany = Globle.Application.Company.GetDICompany() as Company;
            if (Globle.DiCompany != null)
                Globle.ORecordSet = Globle.DiCompany.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;
            Globle.GlobalIntial();
        }

        public void Terminate()
        {
        }

        private void Application_AppEvent(BoAppEventTypes eventType)
        {
            if (eventType == BoAppEventTypes.aet_ShutDown)
            {
                if (Globle.FormSizeInfo != null)
                {
                    if (Globle.FormSizeInfo.Rows.Count > 0)
                    {
                        XmlAndTdHelper.GetInstance().DataTableToXml(Globle.FormSizeInfo, Globle.FormSizeInfoPath);
                    }
                }
                Global.ItemEvent -= SwItemEventHandler.ItemEventHandler;
                Global.ApplicationEvent -= SwApplicationEventHandler.ApplicationEventHandler;
                Global.FormDataEvent -= SwFormDataEventHandler.FormDataEventHandler;
                Global.FormLoadedEvent -= SwFormLoadedEventHandler.FormLoadedEventHandler;
                Global.MenuEvent -= SwMenuEventHandler.MenuEventHandler;
                Global.PrintEvent -= SwPrintEventHandler.PrintEventHandler;
                Global.ProgressBarEvent -= SwProgressBarEvent.ProgressBarEventHandler;
                Global.ReportDataEvent -= SwReportDataEventHandler.ReportDataEventHandler;
                Global.RightClickEvent -= SwRightClickHandler.RightClickHandler;
                Global.StatusBarEvent -= SwStatusBarEventHandler.StatusBarEventHandler;
            }
        }
    }
}