namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Employee")]
    public partial class Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee()
        {
            Log = new HashSet<Log>();
            Phone = new HashSet<Phone>();
            Sale = new HashSet<Sale>();

            Status = (StatusGeneral)1;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdEmployee { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Employee")]
        public string NameEmployee { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime Birth { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime Admission { get; set; }

        public decimal Salary { get; set; }

        public int AddressId { get; set; }

        public int OfficeId { get; set; }

        public int CompanyId { get; set; }

        public bool HaveLogin { get; set; } = false;

        public Permission? Permission { get; set; }

        [StringLength(10)]
        [Display(Name = "Login")]
        public string LoginUser { get; set; }

        [StringLength(255)]
        public string PasswordUser { get; set; }

        public StatusGeneral Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Address Address { get; set; }

        public virtual Company Company { get; set; }

        public virtual Office Office { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Phone> Phone { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sale> Sale { get; set; }
        #endregion

        #region METHODS
        public void DeactivateEmployee()
        {
            Status = 0;
        }

        public void ReactivateEmployee()
        {
            Status = (StatusGeneral)1;
        }

        public override bool Equals(object obj)
        {
            Employee employee = (Employee)obj;
            return employee.IdEmployee == IdEmployee
                && employee.CompanyId == CompanyId
                && employee.HaveLogin == HaveLogin
                && employee.Permission == Permission
                && employee.Salary == Salary
                && employee.OfficeId == OfficeId
                && employee.NameEmployee.Equals(NameEmployee)
                && employee.Birth.ToString("dd/MM/yyyy").Equals(Birth.ToString("dd/MM/yyyy"))
                && employee.Admission.ToString("dd/MM/yyyy").Equals(Admission.ToString("dd/MM/yyyy"));
        }
        #endregion
    }
}
