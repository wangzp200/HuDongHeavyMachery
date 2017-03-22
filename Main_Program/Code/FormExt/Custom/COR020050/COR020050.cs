using SAPbouiCOM;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020050
{
    
    public class COR020050 : SwBaseForm
    {
        private Button okButton, cancleButton;
        private UserDataSource changeCode;
        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            changeCode = (UserDataSource) MyForm.DataSources.UserDataSources.Item("4");
            okButton = (Button) MyForm.Items.Item("11").Specific;
            okButton.PressedAfter+=okButton_PressedAfter;
            cancleButton = (Button) MyForm.Items.Item("2").Specific;
        }

        private void okButton_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            Globle.SwFormsList[MyFatherUid].SonFormCloseEventHandler(changeCode.Value,this);
        }
        public override void ItemEventHandler(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            if (pVal.BeforeAction && pVal.EventType == BoEventTypes.et_FORM_CLOSE)
            {
                Globle.SwFormsList[MyFatherUid].MySonUid = null;
            }
        }
    }
}