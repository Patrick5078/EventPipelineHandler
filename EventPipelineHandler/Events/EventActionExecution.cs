using EventPipelineHandler.Data;
using EventPipelineHandler.Hubs;
using EventPipelineHandler.Hubs.BlazorServerSignalRApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EventPipelineHandler.EventManager
{
    public abstract class EventActionExecution
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHubContext<EventActionHub> _eventActionHub;

        public EventActionExecution(ApplicationDbContext applicationDbContext, IHubContext<EventActionHub> eventActionHub)
        {
            _applicationDbContext = applicationDbContext;
            _eventActionHub = eventActionHub;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAction"></param>
        /// <returns>An updated event action</returns>
        public abstract Task<EventAction> RunEvent(EventAction eventAction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAction"></param>
        /// <returns>A list of the next event actions to be executed</returns>
        public async Task<List<EventAction>> Execute(EventAction eventAction)
        {
            if (eventAction.EventActionState != EventActionState.Pending && eventAction.EventActionState != EventActionState.Failed)
                return new List<EventAction>();

            eventAction.LastExecutedAt = DateTime.Now;
            eventAction.EventActionState = EventActionState.InProgress;

            eventAction.UpdatedAt = DateTime.Now;
            eventAction.UpdatedBy = "System";
            eventAction.ExecutionLog = "Executing event...";

            await _applicationDbContext.EventActions.InsertOrUpdate(eventAction);
            await _applicationDbContext.SaveChangesAsync();
            await _eventActionHub.Clients.All.SendAsync(HubChannels.EventActionStateUpdated, eventAction.Id, eventAction.EventActionState);

            var updatedEvent = await RunEvent(eventAction);

            updatedEvent.UpdatedAt = DateTime.Now;
            updatedEvent.UpdatedBy = "System";
            updatedEvent.CompletedAt = DateTime.Now;
            await _applicationDbContext.SaveChangesAsync();

            await _eventActionHub.Clients.All.SendAsync(HubChannels.EventActionStateUpdated, updatedEvent.Id, updatedEvent.EventActionState);

            if (updatedEvent.EventActionState == EventActionState.Failed)
                return new List<EventAction>();

            var eventsInChain = await _applicationDbContext.EventActions.Where(e => e.EventActionChainId == updatedEvent.EventActionChainId).ToListAsync();

            if (eventsInChain.Where(e => e.Step == updatedEvent.Step).All(e => e.EventActionState == EventActionState.Done))
            {
                var nextEvents = eventsInChain.Where(e => e.Step == updatedEvent.Step + 1).ToList();
                return nextEvents;
            }


            return new List<EventAction>();
        }
    }
}
