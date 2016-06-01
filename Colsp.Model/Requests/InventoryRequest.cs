namespace Colsp.Model.Requests
{
    public class InventoryRequest : PaginatedRequest
    {
        public string Pid { get; set; }
        public string SearchText { get; set; }
        public int Quantity { get; set; }
        public int UpdateQuantity { get; set; }
        public int StockAdjustQty { get; set; }
        public int HoldAdjustQty { get; set; }
        public int DefectAdjustQty { get; set; }
        public int ReserveAdjustQty { get; set; }
        public string Remark { get; set; }

        public InventoryRequest()
        {
            Pid = string.Empty;
            SearchText = string.Empty;
            Quantity = 0;
            Remark = string.Empty;
        }

        public override void DefaultOnNull()
        {
            Pid = GetValueOrDefault(Pid, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "Quantity-Defect-OnHold-Reserve");
            base.DefaultOnNull();
        }
    }
}
