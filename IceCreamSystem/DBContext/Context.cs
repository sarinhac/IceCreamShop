namespace IceCreamSystem.DBContext
{
    using System.Data.Entity;
    using IceCreamSystem.Models;

    public partial class Context : DbContext
    {
        public Context()
            : base("name=Model1")
        {
        }

        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<CreditCard> CreditCard { get; set; }
        public virtual DbSet<DebitCard> DebitCard { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<EntryStock> EntryStock { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Office> Office { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<Phone> Phone { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Sale> Sale { get; set; }
        public virtual DbSet<SaleProduct> SaleProduct { get; set; }
        public virtual DbSet<UnitMeasure> UnitMeasure { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>()
                .Property(e => e.Cep)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Address>()
                .Property(e => e.Logradouro)
                .IsUnicode(false);

            modelBuilder.Entity<Address>()
                .Property(e => e.Numero)
                .IsUnicode(false);

            modelBuilder.Entity<Address>()
                .Property(e => e.Complemento)
                .IsUnicode(false);

            modelBuilder.Entity<Address>()
                .Property(e => e.Bairro)
                .IsUnicode(false);

            modelBuilder.Entity<Address>()
                .Property(e => e.Cidade)
                .IsUnicode(false);

            modelBuilder.Entity<Address>()
                .Property(e => e.Uf)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Address>()
                .HasMany(e => e.Employee)
                .WithRequired(e => e.Address)
                .HasForeignKey(e => e.AddressId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Category>()
                .Property(e => e.NameCategory)
                .IsUnicode(false);

            modelBuilder.Entity<Category>()
                .Property(e => e.DescriptionCategory)
                .IsUnicode(false);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.Category)
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Product)
                .WithRequired(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .Property(e => e.NameCompany)
                .IsUnicode(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Category)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.CreditCard)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.DebitCard)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Employee)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.Company)
                .HasForeignKey(e => e.CompanyId);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Office)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Payment)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Product)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Sale)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.EntryStock)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.UnitMeasure)
                .WithRequired(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CreditCard>()
                .Property(e => e.NameCreditCard)
                .IsUnicode(false);

            modelBuilder.Entity<CreditCard>()
                .Property(e => e.DescriptionCreditCard)
                .IsUnicode(false);

            modelBuilder.Entity<CreditCard>()
                .Property(e => e.RateCreditCard)
                .HasPrecision(5, 2);

            modelBuilder.Entity<CreditCard>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.CreditCard)
                .HasForeignKey(e => e.CreditCardId);

            modelBuilder.Entity<CreditCard>()
                .HasMany(e => e.Payment)
                .WithOptional(e => e.CreditCard)
                .HasForeignKey(e => e.CreditCardId);

            modelBuilder.Entity<DebitCard>()
                .Property(e => e.NameDebitCard)
                .IsUnicode(false);

            modelBuilder.Entity<DebitCard>()
                .Property(e => e.DescriptionDebitCard)
                .IsUnicode(false);

            modelBuilder.Entity<DebitCard>()
                .Property(e => e.Rate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<DebitCard>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.DebitCard)
                .HasForeignKey(e => e.DebitCardId);

            modelBuilder.Entity<DebitCard>()
                .HasMany(e => e.Payment)
                .WithOptional(e => e.DebitCard)
                .HasForeignKey(e => e.DebitCardId);

            modelBuilder.Entity<Employee>()
                .Property(e => e.NameEmployee)
                .IsUnicode(false);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(7, 2);

            modelBuilder.Entity<Employee>()
                .Property(e => e.LoginUser)
                .IsUnicode(false);

            modelBuilder.Entity<Employee>()
                .Property(e => e.PasswordUser)
                .IsUnicode(false);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.Employee)
                .HasForeignKey(e => e.EmployeeId);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Payment)
                .WithRequired(e => e.Employee)
                .HasForeignKey(e => e.EmployeeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Phone)
                .WithRequired(e => e.Employee)
                .HasForeignKey(e => e.EmployeeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Sale)
                .WithRequired(e => e.Employee)
                .HasForeignKey(e => e.EmployeeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EntryStock>()
                .Property(e => e.ProductBatch)
                .IsUnicode(false);

            modelBuilder.Entity<EntryStock>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.EntryStock)
                .HasForeignKey(e => e.EntryStockId);

            modelBuilder.Entity<Log>()
                .Property(e => e.Old)
                .IsUnicode(false);

            modelBuilder.Entity<Log>()
                .Property(e => e.New)
                .IsUnicode(false);

            modelBuilder.Entity<Office>()
                .Property(e => e.NameOffice)
                .IsUnicode(false);

            modelBuilder.Entity<Office>()
                .Property(e => e.DescriptionOffice)
                .IsUnicode(false);

            modelBuilder.Entity<Office>()
                .Property(e => e.Discount)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Office>()
                .HasMany(e => e.Employee)
                .WithRequired(e => e.Office)
                .HasForeignKey(e => e.OfficeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Office>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.Office)
                .HasForeignKey(e => e.OfficeId);

            modelBuilder.Entity<Payment>()
                .Property(e => e.CodePaymentCard)
                .IsUnicode(false);

            modelBuilder.Entity<Payment>()
                .Property(e => e.TotalPrice)
                .HasPrecision(7, 2);

            modelBuilder.Entity<Payment>()
               .Property(e => e.InstallmentPrice)
               .HasPrecision(7, 2);

            modelBuilder.Entity<Payment>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.Payment)
                .HasForeignKey(e => e.PaymentId);

            modelBuilder.Entity<Payment>()
                .Property(e => e.DiscontApply)
                .HasPrecision(7, 2);

            modelBuilder.Entity<Phone>()
                .Property(e => e.DDD)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Phone>()
                .Property(e => e.Number)
                .IsUnicode(false);

            modelBuilder.Entity<Product>()
                .Property(e => e.NameProduct)
                .IsUnicode(false);

            modelBuilder.Entity<Product>()
                .Property(e => e.DescriptionProduct)
                .IsUnicode(false);

            modelBuilder.Entity<Product>()
                .Property(e => e.CostPrice)
                .HasPrecision(7, 2);

            modelBuilder.Entity<Product>()
                .Property(e => e.SalePrice)
                .HasPrecision(7, 2);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.EntryStock)
                .WithRequired(e => e.Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.Product)
                .HasForeignKey(e => e.ProductId);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.SaleProduct)
                .WithRequired(e => e.Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Sale>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.Sale)
                .HasForeignKey(e => e.SaleId);

            modelBuilder.Entity<Sale>()
                .HasMany(e => e.Payment)
                .WithRequired(e => e.Sale)
                .HasForeignKey(e => e.SaleId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Sale>()
                .HasMany(e => e.SaleProduct)
                .WithRequired(e => e.Sale)
                .HasForeignKey(e => e.SaleId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Sale>()
               .Property(e => e.TotalPrice)
               .HasPrecision(7, 2);

            modelBuilder.Entity<UnitMeasure>()
                .Property(e => e.NameUnitMeasure)
                .IsUnicode(false);

            modelBuilder.Entity<UnitMeasure>()
                .Property(e => e.DescriptionUnitMeasure)
                .IsUnicode(false);

            modelBuilder.Entity<UnitMeasure>()
                .HasMany(e => e.Log)
                .WithOptional(e => e.UnitMeasure)
                .HasForeignKey(e => e.UnitMeasureId);

            modelBuilder.Entity<UnitMeasure>()
                .HasMany(e => e.Product)
                .WithRequired(e => e.UnitMeasure)
                .HasForeignKey(e => e.UnitMeasureId)
                .WillCascadeOnDelete(false);
        }
    }
}
