using System.Threading.Tasks;

namespace Annstore.Core.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync<TMessage>(TMessage eventArgs) where TMessage : Message;
    }
}
