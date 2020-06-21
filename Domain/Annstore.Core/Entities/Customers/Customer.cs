namespace Annstore.Core.Entities.Customers
{
    public sealed class Customer : BaseEntity, IAggregateRoot, IHideableEntity
    {
        private bool _deleted;

        public Customer() : this(0, false) { }

        private Customer(int id) : this(id, false) { }

        private Customer(int id, bool deleted) : base(id)
        {
            _deleted = deleted;
        }

        public string FullName { get; set; }

        public bool Deleted
        {
            get => _deleted;
            private set => _deleted = value;
        }

        public static Customer CreateWithId(int id) => new Customer(id);

        public void IsDeleted(bool deleted) => Deleted = deleted;
    }
}
