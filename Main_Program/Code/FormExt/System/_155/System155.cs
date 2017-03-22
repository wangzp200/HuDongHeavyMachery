using HuDongHeavyMachinery.Code.FormExt.Custom.COR020080;
using HuDongHeavyMachinery.Code.Util;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuDongHeavyMachinery.Code.FormExt.System._155
{
    public class System155 : SwBaseForm
    {
        private Matrix matrix;
        public override void FormCreate(string formUId, ref SAPbouiCOM.ItemEvent pVal, ref bool bubbleEvent)
        {
            matrix = MyForm.Items.Item("3").Specific as Matrix;
            var menu = MyForm.Menu;
            menu.Add("COR080020", "自定义价格显示", SAPbouiCOM.BoMenuType.mt_STRING, 8);
        }
        public override void MenuEventHandler(ref SAPbouiCOM.MenuEvent pVal, ref bool bubbleEvent)
        {
            
            if (pVal.MenuUID == "COR080020" && !pVal.BeforeAction) {
                var selectRow = matrix.GetNextSelectedRow();
                if (selectRow>0)
                {
                    var priceList =((EditText) (matrix.Columns.Item("0").Cells.Item(selectRow).Specific)).Value.Trim();
                    const string formType = "COR020080";
                    var form = CreateNewFormUtil.CreateNewForm(formType, -1, -1);
                    var swBaseForm = Globle.SwFormsList[form.UniqueID] as COR020080;
                    if (swBaseForm != null) swBaseForm.setInfo(priceList);
                }
            }
        }
    }
}
