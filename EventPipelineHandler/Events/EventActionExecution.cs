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
                return new();

            eventAction.LastExecutedAt = DateTime.Now;
            eventAction.EventActionState = EventActionState.InProgress;

            eventAction.UpdatedAt = DateTime.Now;
            eventAction.UpdatedBy = "System";
            eventAction.ExecutionLog = "Executing event...";

            await _applicationDbContext.EventActions.InsertOrUpdate(eventAction);
            await _applicationDbContext.SaveChangesAsync();
            await _eventActionHub.Clients.All.SendAsync(HubChannels.EventActionStateUpdated, eventAction.Id, eventAction.EventActionState);

            var updatedEvent = await RunEvent(eventAction);

            eventAction.UpdatedAt = DateTime.Now;
            eventAction.UpdatedBy = "System";
            eventAction.CompletedAt = DateTime.Now;
            await _applicationDbContext.SaveChangesAsync();

            await _eventActionHub.Clients.All.SendAsync(HubChannels.EventActionStateUpdated, eventAction.Id, eventAction.EventActionState);

            if (updatedEvent.EventActionState == EventActionState.Failed)
                return new();

            var unfinishedParallelEventActions = await _applicationDbContext.EventActions
                   .Where(x => x.ParentEventActionId != null && x.ParentEventActionId == updatedEvent.ParentEventActionId
                       && x.EventActionState != EventActionState.Done && x.Id != updatedEvent.Id)
                   .ToListAsync();

            if (!unfinishedParallelEventActions.Any())
            {
                var nextEventActions = await _applicationDbContext.EventActions
                    .Where(x => x.ParentEventActionId == updatedEvent.Id)
                    .ToListAsync();

                return nextEventActions;
            }

            return new();
        }
    }
}
