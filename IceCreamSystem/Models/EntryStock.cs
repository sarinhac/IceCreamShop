namespace IceCreamSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EntryStock")]
    public partial class EntryStock
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntryStock()
        {
            Log = new HashSet<Log>();
        }

        [Key]
        public int IdStock { get; set; }

        [Column(TypeName = "date")]
        public DateTime FabicationDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime ExpirationDate { get; set; }

        [Required]
        [StringLength(10)]
        public string ProductBatch { get; set; }

        public int Amount { get; set; }

        public int ProductId { get; set; }

        public int CompanyId { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        public virtual Product Product { get; set; }
    }
}
