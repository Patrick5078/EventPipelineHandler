namespace EventPipelineHandler.Hubs
{
    using EventPipelineHandler.Data;
    using Microsoft.AspNetCore.SignalR;

    namespace BlazorServerSignalRApp.Server.Hubs
    {
        public class EventActionHub : Hub
        {
            public async Task SendUpdatedEventActionState(Guid eventId, EventActionState eventActionState)
            {
                await Clients.All.SendAsync(HubChannels.EventActionStateUpdated, eventId, eventActionState);
            }
        }
    }
}
