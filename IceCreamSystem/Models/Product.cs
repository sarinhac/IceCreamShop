namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Product")]
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            EntryStock = new HashSet<EntryStock>();
            Log = new HashSet<Log>();
            SaleProduct = new HashSet<SaleProduct>();

            Status = (StatusGeneral)1;
            Created = DateTime.Now;
            AmountStock = 0;
        }

        #region ATTRIBUTES
        [Key]
        public int IdProduct { get; set; }

        [Required]
        [StringLength(50)]
        public string NameProduct { get; set; }

        [StringLength(255)]
        public string DescriptionProduct { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalePrice { get; set; }

        public int MinStock { get; set; }

        public bool SellNegative { get; set; }

        public int AmountStock { get; set; }

        public int CategoryId { get; set; }

        public int UnitMeasureId { get; set; }

        public int CompanyId { get; set; }

        public StatusGeneral Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Category Category { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EntryStock> EntryStock { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        public virtual UnitMeasure UnitMeasure { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SaleProduct> SaleProduct { get; set; }
        #endregion

        #region METHODS
        public void ReactivateProduct()
        {
            Status = (StatusGeneral)1;
        }

        public void DeactivateProduct()
        {
            Status = 0;
        }

        public override bool Equals(object obj)
        {
            Product product = (Product)obj;
            return product.IdProduct == IdProduct && product.NameProduct.Equals(NameProduct) && product.DescriptionProduct.Equals(DescriptionProduct)
                && product.CostPrice == CostPrice && product.SalePrice == SalePrice && product.MinStock == MinStock && product.UnitMeasureId == UnitMeasureId
                && product.CategoryId == CategoryId && product.SellNegative == SellNegative && product.CompanyId == CompanyId;
        }
        #endregion //METHODS
    }
}
