namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Category")]
    public partial class Category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Category()
        {
            Log = new HashSet<Log>();
            Product = new HashSet<Product>();
            
            Status = (StatusGeneral)1;
            Created = DateTime.Now;
        }
        #region ATTRIBUTES
        [Key]
        public int IdCategory { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string NameCategory { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string DescriptionCategory { get; set; }

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
        public void ReactivateCategory()
        {
            Status = (StatusGeneral)1;
        }

        public void DeactivateCategory()
        {
            Status = 0;
        }

        public override bool Equals(object obj)
        {
            Category category = (Category)obj;
            return category.IdCategory == IdCategory && category.NameCategory.Equals(NameCategory) && category.DescriptionCategory == DescriptionCategory
                && category.CompanyId == CompanyId;
        }
        #endregion //METHODS
    }
}
