namespace IceCreamSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Employee")]
    public partial class Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee()
        {
            Log = new HashSet<Log>();
            Log1 = new HashSet<Log>();
            Phone = new HashSet<Phone>();
            Sale = new HashSet<Sale>();
        }

        [Key]
        public int IdEmployee { get; set; }

        [Required]
        [StringLength(50)]
        public string NameEmployee { get; set; }

        [Column(TypeName = "date")]
        public DateTime Birth { get; set; }

        [Column(TypeName = "date")]
        public DateTime Admission { get; set; }

        public decimal Salary { get; set; }

        public int AddressId { get; set; }

        public int OfficeId { get; set; }

        public int CompanyId { get; set; }

        public bool HaveLogin { get; set; }

        public int? Permission { get; set; }

        [StringLength(10)]
        public string LoginUser { get; set; }

        [StringLength(255)]
        public string PasswordUser { get; set; }

        public int Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Address Address { get; set; }

        public virtual Company Company { get; set; }

        public virtual Office Office { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Phone> Phone { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sale> Sale { get; set; }
    }
}
