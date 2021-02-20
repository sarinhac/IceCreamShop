namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Sale")]
    public partial class Sale
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sale()
        {
            Log = new HashSet<Log>();
            Payment = new HashSet<Payment>();
            SaleProduct = new HashSet<SaleProduct>();

            Status = (SaleStatus) 1;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdSale { get; set; }

        public int CompanyId { get; set; }

        public int EmployeeId { get; set; }

        [Display(Name = "Total")]
        public decimal TotalPrice { get; set; }

        public SaleStatus Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        public virtual Employee Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payment> Payment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SaleProduct> SaleProduct { get; set; }
        #endregion

        #region METHODS
        public void CancelSale()
        {
            Status = (SaleStatus) 4;
        }
        #endregion //METHODS
    }
}
