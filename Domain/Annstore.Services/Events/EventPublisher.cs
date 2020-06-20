using Annstore.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Annstore.Services.Events
{
    public sealed class EventPublisher : IEventPublisher
    {
        private static IEnumerable<Type> _handlerTypes;
        private readonly IServiceProvider _serviceProvider;

        public EventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void RegisterEventHandlers(IEnumerable<Assembly> assemblies)
        {
            var targetOpenGenericType = typeof(IEventHandler<>);
            var handlerTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var eventHandlerTypeQuery = from type in assembly.GetTypes()
                                            from implementedInterface in type.GetInterfaces()
                                            where !type.IsAbstract && !type.IsInterface &&
                                                implementedInterface.IsGenericType &&
                                                targetOpenGenericType.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition())
                                            select type;

                var resultTypes = eventHandlerTypeQuery.Distinct().ToList();
                if (resultTypes.Count > 0)
                {
                    handlerTypes.AddRange(resultTypes);
                }
            }
            _handlerTypes = handlerTypes;
        }

        public Task Publish<TMessage>(TMessage message) where TMessage : Message
        {
            throw new NotImplementedException();
        }
    }
}
