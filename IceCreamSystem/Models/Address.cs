namespace IceCreamSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Address")]
    public partial class Address
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Address()
        {
            Employee = new HashSet<Employee>();
        }

        [Key]
        public int IdAddress { get; set; }

        [Required]
        [StringLength(8)]
        public string Cep { get; set; }

        [Required]
        [StringLength(255)]
        public string Logradouro { get; set; }

        [Required]
        [StringLength(5)]
        public string Numero { get; set; }

        [StringLength(255)]
        public string Complemento { get; set; }

        [Required]
        [StringLength(50)]
        public string Bairro { get; set; }

        [Required]
        [StringLength(50)]
        public string Cidade { get; set; }

        [Required]
        [StringLength(2)]
        public string Uf { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employee { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null || obj.Equals(""))
                return false;

            Address address = (Address)obj;

            if (address.Complemento == null || Complemento == null)
                return address.Cep.Equals(Cep)
                    && address.Logradouro.Equals(Logradouro)
                    && address.Numero.Equals(Numero)
                    && address.Bairro.Equals(Bairro)
                    && address.Cidade.Equals(Cidade)
                    && address.Uf.Equals(Uf);
            else if ((address.Complemento != null && Complemento == null) || (address.Complemento == null && Complemento != null))
                return false;
            else
                return address.Cep.Equals(Cep)
                       && address.Logradouro.Equals(Logradouro)
                       && address.Numero.Equals(Numero)
                       && address.Complemento.Equals(Complemento)
                       && address.Bairro.Equals(Bairro)
                       && address.Cidade.Equals(Cidade)
                       && address.Uf.Equals(Uf);
        }
    }
}
