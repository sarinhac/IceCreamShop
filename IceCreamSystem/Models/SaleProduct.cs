namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SaleProduct")]
    public partial class SaleProduct
    {
        public SaleProduct()
        {
            Status = (StatusStockSaleProduct)1;
        }

        [Key]
        public int IdSaleProduct { get; set; }

        public int SaleId { get; set; }

        public int ProductId { get; set; }

        public int Amount { get; set; }

        public StatusStockSaleProduct Status { get; set; }

        public virtual Product Product { get; set; }

        public virtual Sale Sale { get; set; }
    }
}
