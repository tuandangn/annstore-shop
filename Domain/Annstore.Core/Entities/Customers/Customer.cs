namespace Annstore.Core.Entities.Customers
{
    public sealed class Customer : BaseEntity
    {
        public string FullName { get; set; }

        public bool Deleted { get; set; }
    }
}
