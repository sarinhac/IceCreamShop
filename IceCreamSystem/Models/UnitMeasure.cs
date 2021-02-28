namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UnitMeasure")]
    public partial class UnitMeasure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UnitMeasure()
        {
            Log = new HashSet<Log>();
            Product = new HashSet<Product>();

            Status = (StatusGeneral)1;
            Created = DateTime.Now;
        }

        #region ATTRIBUTES
        [Key]
        public int IdUnitMeasure { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "UnitMeasure")]
        public string NameUnitMeasure { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string DescriptionUnitMeasure { get; set; }

        public int CompanyId { get; set; }

        public StatusGeneral Status { get; set; }

        public DateTime Created { get; set; }

        public virtual Company Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Product { get; set; }
        #endregion

        #region METHODS
        public void ReactivateUnitMeasure()
        {
            Status = (StatusGeneral)1;
        }

        public void DeactivateUnitMeasure()
        {
            Status = 0;
        }

        public override bool Equals(object obj)
        {
            UnitMeasure uM = (UnitMeasure)obj;
            return uM.IdUnitMeasure == IdUnitMeasure && uM.NameUnitMeasure.Equals(NameUnitMeasure) && (uM.DescriptionUnitMeasure == DescriptionUnitMeasure)
                && uM.CompanyId == CompanyId;
        }
        #endregion //METHODS
    }
}
