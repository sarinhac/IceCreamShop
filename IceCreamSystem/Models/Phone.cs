namespace IceCreamSystem.Models
{
    using IceCreamSystem.Models.Enum;
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

        public TypePhone TypePhone { get; set; }

        [Required]
        [StringLength(2)]
        public string DDD { get; set; }

        [Required]
        [StringLength(9)]
        public string Number { get; set; }

        public int Status { get; set; }

        public int WorkerId { get; set; }

        public virtual Worker Worker { get; set; }
    }
}
