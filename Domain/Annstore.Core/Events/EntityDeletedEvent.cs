using System;

namespace Annstore.Core.Events
{
    [Serializable]
    public sealed class EntityDeletedEvent<TEntity> : EventBase where TEntity : class
    {
        private readonly TEntity _entity;

        public EntityDeletedEvent(TEntity entity)
        {
            _entity = entity;
        }

        public TEntity Entity => _entity;
    }
}
