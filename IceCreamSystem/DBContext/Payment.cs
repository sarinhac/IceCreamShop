//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IceCreamSystem.DBContext
{
    using System;
    using System.Collections.Generic;
    
    public partial class Payment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Payment()
        {
            this.Log = new HashSet<Log>();
        }
    
        public int IdPayment { get; set; }
        public int SaleId { get; set; }
        public int TypePayment { get; set; }
        public Nullable<int> DebitCardId { get; set; }
        public Nullable<int> CreditCardId { get; set; }
        public int CompanyId { get; set; }
        public int Status { get; set; }
        public System.DateTime Created { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual CreditCard CreditCard { get; set; }
        public virtual DebitCard DebitCard { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Log { get; set; }
        public virtual Sale Sale { get; set; }
    }
}
