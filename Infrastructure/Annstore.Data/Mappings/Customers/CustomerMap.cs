using Annstore.Core.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Annstore.Data.Mappings.Customers
{
    public class CustomerMap : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customer", "dm");
            builder.HasKey(p => p.Id);
        }
    }
}
