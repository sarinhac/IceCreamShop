namespace IceCreamSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using IceCreamSystem.Models.Enum;

    [Table("Company")]
    public partial class Company
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Company()
        {
            Category = new HashSet<Category>();
            CreditCard = new HashSet<CreditCard>();
            DebitCard = new HashSet<DebitCard>();
            Employee = new HashSet<Employee>();
            Log = new HashSet<Log>();
            Office = new HashSet<Office>();
            Payment = new HashSet<Payment>();
            Product = new HashSet<Product>();
            Sale = new HashSet<Sale>();
            EntryStock = new HashSet<EntryStock>();
            UnitMeasure = new HashSet<UnitMeasure>();
            Status = 0;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES

        [Key]
        public int IdCompany { get; set; }

        [Required]
        [StringLength(50)]
        public string NameCompany { get; set; }

        public StatusGeneral Status { get; set; }

        public DateTime Created { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Category> Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CreditCard> CreditCard { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DebitCard> DebitCard { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Office> Office { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Payment> Payment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sale> Sale { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EntryStock> EntryStock { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UnitMeasure> UnitMeasure { get; set; }
        #endregion //ATTRIBUTES

        #region METHODS
        public void ReactivateCompany()
        {
            Status = 0;
        }

        public void DeactivateCompany()
        {
            Status = (StatusGeneral)1;
        }

        public override bool Equals(object obj)
        {
            Company company = (Company)obj;
            return company.IdCompany == IdCompany && company.NameCompany == NameCompany;
        }
        #endregion //METHODS
    }
}
