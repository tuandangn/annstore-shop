using System.Threading.Tasks;

namespace Annstore.Core.Events
{
    public interface IEventHandler<in TEvent> where TEvent : EventBase
    {
        Task Handle(TEvent targetEvent);
    }
}
