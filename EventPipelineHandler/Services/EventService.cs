using EventPipelineHandler.Data;
using EventPipelineHandler.Events.CustomEvents;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Xml.Linq;

namespace EventPipelineHandler.Services
{
    public class EventService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly BackgroundTaskQueue _backgroundTaskQueue;

        public EventService(ApplicationDbContext applicationDbContext, BackgroundTaskQueue backgroundTaskQueue)
        {
            _applicationDbContext = applicationDbContext;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public async Task<List<EventActionChain>> GetEventActionChainsAsync()
        {
            var eventActions = await _applicationDbContext.EventActionChains
                .Include(e => e.EventActions)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return eventActions;
        }

        public async Task CreateTestEvents()
        {
            var eventActionChain = new EventActionChain
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedBy = "System",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "System",
                Name = "Test pipeline",
                EventActions = new List<EventAction>()
                {
                    new EventAction
                    {
                        CreatedAt = DateTime.Now,
                        Name = "Create customer in CRM",
                        EventActionType = EventActionType.CreateCustomerInDb,
                        CreatedBy = "System",
                        Data = JsonSerializer.Serialize(new CreateCustomerInDbData
                        {
                            CustomerEmail = "test@email.dk",
                            CustomerName = "Test Customer",
                            CustomerPhone = "12345678",

                        }),
                        Id = Guid.NewGuid(),
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "System",
                        Step = 1,
                    },
                    new EventAction
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        Name = "Create customer in SharePoint",
                        EventActionType = EventActionType.TransferToSharepoint,
                        CreatedBy = "System",
                        Data = JsonSerializer.Serialize(new TransferToSharepointEventActionData
                        {
                            CustomerAddress = "Test Address",
                            CustomerEmail = "s",
                            CustomerName = "Test Customer",
                            CustomerPhone = "1234"
                        }),
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "System",
                        Step = 2,
                    },
                    new EventAction
                    {
                        CreatedAt = DateTime.Now,
                        Name = "Create customer in Workzone",
                        EventActionType = EventActionType.CreateCustomerInDb,
                        CreatedBy = "System",
                        Data = JsonSerializer.Serialize(new CreateCustomerInDbData
                        {
                            CustomerEmail = "test@email.dk",
                            CustomerName = "Test Customer",
                            CustomerPhone = "12345678",

                        }),
                        Id = Guid.NewGuid(),
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "System",
                        Step = 2,
                    },
                    new EventAction
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        Name = "Transfer uploaded files to Sharepoint",
                        EventActionType = EventActionType.TransferToSharepoint,
                        CreatedBy = "System",
                        Data = JsonSerializer.Serialize(new TransferToSharepointEventActionData
                        {
                            CustomerAddress = "Test Address",
                            CustomerEmail = "s",
                            CustomerName = "Test Customer",
                            CustomerPhone = "1234"
                        }),
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "System",
                        Step = 3
                    },
                    new EventAction
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        Name = "Transfer uploaded files to Workzone",
                        EventActionType = EventActionType.TransferToSharepoint,
                        CreatedBy = "System",
                        Data = JsonSerializer.Serialize(new TransferToSharepointEventActionData
                        {
                            CustomerAddress = "Test Address",
                            CustomerEmail = "s",
                            CustomerName = "Test Customer",
                            CustomerPhone = "1234"
                        }),
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "System",
                        Step = 3
                    },
                    new EventAction
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        Name = "File server cleanup",
                        EventActionType = EventActionType.TransferToSharepoint,
                        CreatedBy = "System",
                        Data = JsonSerializer.Serialize(new TransferToSharepointEventActionData
                        {
                            CustomerAddress = "Test Address",
                            CustomerEmail = "s",
                            CustomerName = "Test Customer",
                            CustomerPhone = "1234"
                        }),
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "System",
                        Step = 4
                    },
                }
            };

            _applicationDbContext.EventActionChains.Add(eventActionChain);
            await _applicationDbContext.SaveChangesAsync();

            _backgroundTaskQueue.QueueFirstStepsOfEventActionChain(eventActionChain);
        }
    }
}
