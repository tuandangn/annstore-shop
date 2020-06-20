using System.Threading.Tasks;

namespace Annstore.Core.Events
{
    public interface IEventPublisher
    {
        Task Publish<TMessage>(TMessage message) where TMessage : Message;
    }
}
