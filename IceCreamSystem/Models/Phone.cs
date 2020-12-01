namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Phone")]
    public partial class Phone : IEqualityComparer<Phone>
    {
        public Phone()
        {
            Status = 0;
        }


        [Key]
        public int IdPhone { get; set; }

        public TypePhone TypePhone { get; set; }

        [Required]
        [StringLength(2)]
        public string DDD { get; set; }

        [Required]
        [StringLength(9)]
        public string Number { get; set; }

        public StatusGeneral Status { get; set; }

        public int EmployeeId { get; set; }

        public virtual Employee Employee { get; set; }

       
        public bool Equals(Phone x, Phone y)
        {
            return x.IdPhone == y.IdPhone && x.TypePhone == y.TypePhone && x.DDD.Equals(y.DDD) && x.Number.Equals(y.Number);
        }

        public int GetHashCode(Phone obj)
        {
            throw new NotImplementedException();
        }
    }
}
