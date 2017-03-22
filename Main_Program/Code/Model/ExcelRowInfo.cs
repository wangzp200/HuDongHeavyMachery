namespace HuDongHeavyMachinery.Code.Model
{
    public class ExcelRowInfo
    {
        public string MachineryNo { get; set; }
        public string PurchaseNo { get; set; }
        public string InstallEquipmentNo { get; set; }
        public string ExchangeNo { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }

        public string FrgnName { get; set; }
        public double PriceBeforeVat { get; set; }
        public double LeadTime { get; set; }
        public double Price { get; set; }

        public double Rate { get; set; }

        public string Memo { get; set; }
    }
}