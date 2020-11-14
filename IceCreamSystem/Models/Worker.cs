namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Worker")]
    public partial class Worker
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Worker()
        {
            Log = new HashSet<Log>();
            Log1 = new HashSet<Log>();
            Phone = new HashSet<Phone>();
            Sale = new HashSet<Sale>();

            Status = 0;
            Created = DateTime.Now;
        }
        #region ATTRIBUTES
        [Key]
        public int IdWorker { get; set; }

        [Required]
        [StringLength(50)]
        public string NameWorker { get; set; }

        [Column(TypeName = "date")]
        public DateTime Birth { get; set; }

        [Column(TypeName = "date")]
        public DateTime Admission { get; set; }

        public decimal Salary { get; set; }

        public int AddressId { get; set; }

        public int OfficeId { get; set; }

        public int CompanyId { get; set; }

        public bool HaveLogin { get; set; }

        public Permission? Permission { get; set; }

        [StringLength(10)]
        public string LoginUser { get; set; }

        [StringLength(255)]
        public string PasswordUser { get; set; }

        public StatusGeneral Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Address Address { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log1 { get; set; }

        public virtual Office Office { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Phone> Phone { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sale> Sale { get; set; }
#endregion

        #region METHODS
        public void ReactivateWorker()
        {
            Status = 0;
        }

        public void DeactivateWorker()
        {
            Status = (StatusGeneral)1;
        }

        public override bool Equals(object obj)
        {
            Worker employee = (Worker)obj;
            return employee.IdWorker == IdWorker
                && employee.CompanyId == CompanyId
                && employee.HaveLogin == HaveLogin
                && employee.Permission == Permission
                && employee.Salary == Salary
                && employee.OfficeId == OfficeId
                && employee.NameWorker.Equals(NameWorker)
                && employee.Birth.ToString("dd/MM/yyyy").Equals(Birth.ToString("dd/MM/yyyy"))
                && employee.Admission.ToString("dd/MM/yyyy").Equals(Admission.ToString("dd/MM/yyyy"));
        }
        #endregion
    }
}
