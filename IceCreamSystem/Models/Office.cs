namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Office")]
    public partial class Office
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Office()
        {
            Employee = new HashSet<Employee>();
            Log = new HashSet<Log>();

            Status = (StatusGeneral)1;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdOffice { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Office")]
        public string NameOffice { get; set; }

        [StringLength(255)]
        [Display(Name = "Description Office")]
        public string DescriptionOffice { get; set; }

        public decimal? Discount { get; set; }

        public int CompanyId { get; set; }

        public StatusGeneral Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }
        #endregion

        #region METHODS
        public void ReactivateOffice()
        {
            Status = (StatusGeneral)1;
        }

        public void DeactivateOffice()
        {
            Status = 0;
        }

        public override bool Equals(object obj)
        {
            Office office = (Office)obj;
            return office.IdOffice == IdOffice && office.NameOffice.Equals(NameOffice) && office.DescriptionOffice.Equals(DescriptionOffice)
                && office.Discount == Discount && office.CompanyId == CompanyId;
        }
        #endregion //METHODS
    }
}
