using EventPipelineHandler.Data;
using EventPipelineHandler.EventManager;
using EventPipelineHandler.Hubs.BlazorServerSignalRApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace EventPipelineHandler.Events.CustomEvents
{
    public class CreateCustomerInDbData
    {
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public required string CustomerPhone { get; set; }
    }

    public class CreateCustomerInDbEvent : EventActionExecution
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CreateCustomerInDbEvent(ApplicationDbContext applicationDbContext,
            IHubContext<EventActionHub> eventActionHub) : base(applicationDbContext, eventActionHub)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async override Task<EventAction> RunEvent(EventAction eventAction)
        {
            eventAction = (await _applicationDbContext.EventActions.FindAsync(eventAction.Id))!;

            try
            {
                var data = JsonSerializer.Deserialize<CreateCustomerInDbData>(eventAction.Data!)!;

                // Simulate an API call
                await Task.Delay(5000);

                if (new Random().Next(0, 2) == 0)
                    throw new Exception("Something went wrong");

                var customer = new Customer
                {
                    Name = data.CustomerName,
                    Email = data.CustomerEmail,
                    Phone = data.CustomerPhone,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = "System"
                };

                _applicationDbContext.Customers.Add(customer);

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
