namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Payment")]
    public partial class Payment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Payment()
        {
            Log = new HashSet<Log>();
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdPayment { get; set; }

        public int SaleId { get; set; }

        [Display(Name = "Type Payment")]
        public TypePayment TypePayment { get; set; }

        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Installment Price")]
        public decimal InstallmentPrice { get; set; }

        public int? DebitCardId { get; set; }

        public int? CreditCardId { get; set; }

        public int CompanyId { get; set; }

        public int EmployeeId { get; set; }

        [StringLength(50)]
        [Display(Name = "Code Payment")]
        public string CodePaymentCard { get; set; }

        [Display(Name = "Nº Installment")]
        public int InstallmentNumber { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Date Payment")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime forecastDatePayment { get; set; }

        public StatusPayment Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        public virtual CreditCard CreditCard { get; set; }

        public virtual DebitCard DebitCard { get; set; }

        public virtual Employee Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        public virtual Sale Sale { get; set; }
        #endregion

        #region METHODS
        public void MarkPaid()
        {
            Status = (StatusPayment)1;
        }
        #endregion //METHODS
    }
}
