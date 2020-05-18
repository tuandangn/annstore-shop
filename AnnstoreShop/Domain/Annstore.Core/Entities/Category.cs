namespace Annstore.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayOrder { get; set; }

        public int ParentId { get; set; }
    }
}
