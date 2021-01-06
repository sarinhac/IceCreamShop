namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DebitCard")]
    public partial class DebitCard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DebitCard()
        {
            Log = new HashSet<Log>();
            Payment = new HashSet<Payment>();

            Status = (StatusGeneral)1;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdDebitCard { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Debit Card")]
        public string NameDebitCard { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string DescriptionDebitCard { get; set; }

        public decimal Rate { get; set; }

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
        public void DeactivateDebitCard()
        {
            Status = 0;
        }

        public void ReactivateDebitCard()
        {
            Status = (StatusGeneral)1;
        }

        public override bool Equals(object obj)
        {
            DebitCard cc = (DebitCard)obj;
            return cc.IdDebitCard == IdDebitCard
                && cc.NameDebitCard == NameDebitCard
                && cc.DescriptionDebitCard == DescriptionDebitCard
                && cc.CompanyId == CompanyId
                && cc.Rate == Rate;
        }
        #endregion
    }
}
