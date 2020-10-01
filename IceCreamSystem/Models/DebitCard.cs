namespace IceCreamSystem.Models
{
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
        }

        [Key]
        public int IdDebitCard { get; set; }

        [Required]
        [StringLength(50)]
        public string NameDebitCard { get; set; }

        [StringLength(255)]
        public string DescriptionDebitCard { get; set; }

        public decimal Rate { get; set; }

        public int CompanyId { get; set; }

        public int Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payment> Payment { get; set; }
    }
}
