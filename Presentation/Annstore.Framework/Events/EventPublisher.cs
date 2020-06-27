using Annstore.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Annstore.Framework.Events
{
    public sealed class EventPublisher : IEventPublisher
    {
        private static IEnumerable<Type> _handlerTypes;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, object> _savedHandlerObjects;

        public EventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _savedHandlerObjects = new ConcurrentDictionary<Type, object>();
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

        public async Task PublishAsync<TMessage>(TMessage message) where TMessage : Message
        {
            var targetHandlerType = typeof(IEventHandler<TMessage>);
            foreach (var handlerType in _handlerTypes)
            {
                if (!targetHandlerType.IsAssignableFrom(handlerType))
                    continue;
                var handler = _savedHandlerObjects.GetOrAdd(handlerType, type =>
                {
                    return _serviceProvider.GetRequiredService(handlerType);
                });
                await ((IEventHandler<TMessage>)handler).HandleAsync(message)
                    .ConfigureAwait(false);
            }
        }
    }
}
