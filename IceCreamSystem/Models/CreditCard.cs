namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CreditCard")]
    public partial class CreditCard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CreditCard()
        {
            Log = new HashSet<Log>();
            Payment = new HashSet<Payment>();

            Status = (StatusGeneral)1;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdCreditCard { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Credit Card")]
        public string NameCreditCard { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string DescriptionCreditCard { get; set; }

        [Display(Name = "Rate")]
        public decimal RateCreditCard { get; set; }

        public int CompanyId { get; set; }

        public StatusGeneral Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payment> Payment { get; set; }

        #endregion

        #region METHODS
        public void DeactivateCreditCard()
        {
            Status = 0;
        }

        public void ReactivateCreditCard()
        {
            Status = (StatusGeneral)1;
        }

        public override bool Equals(object obj)
        {
            CreditCard cc = (CreditCard)obj;
            return cc.IdCreditCard == IdCreditCard
                && cc.NameCreditCard == NameCreditCard
                && cc.DescriptionCreditCard == DescriptionCreditCard
                && cc.CompanyId == CompanyId
                && cc.RateCreditCard == RateCreditCard;
        }
        #endregion
    }
}
