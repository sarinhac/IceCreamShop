namespace IceCreamSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SaleProduct")]
    public partial class SaleProduct
    {
        [Key]
        public int IdSaleProduct { get; set; }

        public int SaleId { get; set; }

        public int ProductId { get; set; }

        public int Amount { get; set; }

        public virtual Product Product { get; set; }

        public virtual Sale Sale { get; set; }
    }
}
