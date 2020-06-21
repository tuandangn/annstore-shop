namespace Annstore.Core
{
    public interface IHideableEntity
    {
        bool Deleted { get; }

        void IsDeleted(bool deleted);
    }
}
