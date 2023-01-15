using EventPipelineHandler.Data;
using EventPipelineHandler.Events.CustomEvents;
using EventPipelineHandler.Hubs;
using EventPipelineHandler.Hubs.BlazorServerSignalRApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventPipelineHandler.EventManager
{
    public class EventRunner : IEventRunner
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<EventActionType, EventActionExecution> _cachedActionExecutions = new();

        public EventRunner(ApplicationDbContext applicationDbContext, IServiceProvider serviceProvider)
        {
            _applicationDbContext = applicationDbContext;
            _serviceProvider = serviceProvider;
        }

        public async Task<List<EventAction>> ExecuteEventAction(EventAction eventAction)
        {
            eventAction = (await _applicationDbContext.EventActions.FindAsync(eventAction.Id))!;

            if (eventAction is null)
                throw new Exception();

            var eventActionExecution = GetEventActionExecution(eventAction.EventActionType);
            return await eventActionExecution.Execute(eventAction);
        }

        private EventActionExecution GetEventActionExecution(EventActionType eventType)
        {
            if (_cachedActionExecutions.ContainsKey(eventType))
                return _cachedActionExecutions[eventType];

            EventActionExecution eventActionExecution = eventType switch
            {
                EventActionType.CreateCustomerInDb => _serviceProvider.GetRequiredService<CreateCustomerInDbEvent>(),
                EventActionType.TransferToSharepoint => _serviceProvider.GetRequiredService<TransferToSharepointEventAction>(),
                _ => throw new ArgumentException($"EventActionType {eventType} is not supported")
            };

            _cachedActionExecutions.Add(eventType, eventActionExecution);
            return eventActionExecution;
        }
    }
}
