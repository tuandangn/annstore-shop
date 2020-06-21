using System.Threading.Tasks;

namespace Annstore.Core.Events
{
    public interface IEventHandler<in TMessage> where TMessage : Message
    {
        Task HandleAsync(TMessage eventArgs);
    }
}
