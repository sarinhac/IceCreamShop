namespace IceCreamSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Phone")]
    public partial class Phone
    {
        [Key]
        public int IdPhone { get; set; }

        public int TypePhone { get; set; }

        [Required]
        [StringLength(2)]
        public string DDD { get; set; }

        [Required]
        [StringLength(9)]
        public string Number { get; set; }

        public int Status { get; set; }

        public int EmployeeId { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
