namespace IceCreamSystem.Models
{
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
        }

        [Key]
        public int IdPayment { get; set; }

        public int SaleId { get; set; }

        public int TypePayment { get; set; }

        public int? DebitCardId { get; set; }

        public int? CreditCardId { get; set; }

        public int CompanyId { get; set; }

        public int Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        public virtual CreditCard CreditCard { get; set; }

        public virtual DebitCard DebitCard { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        public virtual Sale Sale { get; set; }
    }
}
