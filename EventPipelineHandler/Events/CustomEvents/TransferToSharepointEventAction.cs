using EventPipelineHandler.Data;
using EventPipelineHandler.Hubs.BlazorServerSignalRApp.Server.Hubs;
using EventPipelineHandler.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using EventPipelineHandler.EventManager;

namespace EventPipelineHandler.Events.CustomEvents
{
    public class TransferToSharepointEventActionData
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }

    public class TransferToSharepointEventAction : EventActionExecution
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public TransferToSharepointEventAction(ApplicationDbContext applicationDbContext,
            IHubContext<EventActionHub> eventActionHub) : base(applicationDbContext, eventActionHub)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async override Task<EventAction> RunEvent(EventAction eventAction)
        {
            eventAction = (await _applicationDbContext.EventActions.FindAsync(eventAction.Id))!;

            try
            {
                var data = JsonSerializer.Deserialize<TransferToSharepointEventActionData>(eventAction.Data!)!;

                await Task.Delay(5000);

                eventAction.EventActionState = EventActionState.Done;
            }
            catch (Exception ex)
            {
                eventAction.EventActionState = EventActionState.Failed;
                eventAction.AddLineToExecutionLog(ex.Message);
            }

            return eventAction;
        }
    }
}
