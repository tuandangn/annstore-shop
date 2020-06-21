using System;

namespace Annstore.Core.Events
{
    [Serializable]
    public sealed class EntityCreatedEvent<TEntity> : EventBase where TEntity : class
    {
        private readonly TEntity _entity;

        public EntityCreatedEvent(TEntity entity)
        {
            _entity = entity;
        }

        public TEntity Entity => _entity;
    }
}
