namespace Annstore.Query.Entities.Catalog
{
    public sealed class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }
    }
}
