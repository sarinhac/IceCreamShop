namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("EntryStock")]
    public partial class EntryStock
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntryStock()
        {
            Log = new HashSet<Log>();
            Status = (StatusStockSaleProduct)1;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdStock { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Fabication")]
        public DateTime FabicationDate { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "Expiration")]
        public DateTime ExpirationDate { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Batch")]
        public string ProductBatch { get; set; }

        public int Amount { get; set; }

        public int ProductId { get; set; }

        public int CompanyId { get; set; }

        public StatusStockSaleProduct Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        public virtual Product Product { get; set; }
        #endregion

        #region METHODS

        public void DeactivateStock()
        {
            Status = (StatusStockSaleProduct)3;
        }

        public override bool Equals(object obj)
        {
            EntryStock eS = (EntryStock)obj;
            return eS.IdStock == IdStock  && eS.CompanyId == CompanyId && eS.ProductId == ProductId
                 && eS.Amount == Amount
                && eS.ExpirationDate == ExpirationDate &&  eS.FabicationDate == FabicationDate 
                && eS.ProductBatch.Equals(ProductBatch);
        }
        #endregion //METHODS
    }
}
