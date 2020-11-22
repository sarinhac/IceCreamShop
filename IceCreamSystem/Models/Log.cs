namespace IceCreamSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Log")]
    public partial class Log
    {
        public Log()
        {
            Created = DateTime.Now;
        }

        [Key]
        public int IdLog { get; set; }

        [StringLength(255)]
        public string Old { get; set; }

        [Required]
        [StringLength(255)]
        public string New { get; set; }

        public int Who { get; set; }

        public DateTime Created { get; set; }

        public int? CompanyId { get; set; }

        public int? EmployeeId { get; set; }

        public int? OfficeId { get; set; }

        public int? CategoryId { get; set; }

        public int? UnitMeasureId { get; set; }

        public int? ProductId { get; set; }

        public int? SaleId { get; set; }

        public int? PaymentId { get; set; }

        public int? CreditCardId { get; set; }

        public int? EntryStockId { get; set; }

        public int? DebitCardId { get; set; }

        public virtual Category Category { get; set; }

        public virtual Company Company { get; set; }

        public virtual CreditCard CreditCard { get; set; }

        public virtual DebitCard DebitCard { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual EntryStock EntryStock { get; set; }

        public virtual Office Office { get; set; }

        public virtual Payment Payment { get; set; }

        public virtual Product Product { get; set; }

        public virtual Sale Sale { get; set; }

        public virtual UnitMeasure UnitMeasure { get; set; }
    }
}
