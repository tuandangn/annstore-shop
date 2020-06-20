using System;

namespace Annstore.Core
{
    [Serializable]
    public class BaseEntity
    {
        private readonly int _id;

        protected BaseEntity(int id)
        {
            _id = id;
        }

        public int Id => _id;
    }
}
