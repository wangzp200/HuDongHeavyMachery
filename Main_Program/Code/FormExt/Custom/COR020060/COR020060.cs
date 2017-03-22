using SAPbobsCOM;
using SAPbouiCOM;
using Items = SAPbobsCOM.Items;

namespace HuDongHeavyMachinery.Code.FormExt.Custom.COR020060
{
    public class COR020060 : SwBaseForm
    {
        public override void FormCreate(string formUId, ref ItemEvent pVal, ref bool bubbleEvent)
        {
            var vItem = (Items) Globle.DiCompany.GetBusinessObject(BoObjectTypes.oItems);
            
        }
    }
}