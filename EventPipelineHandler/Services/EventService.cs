using EventPipelineHandler.Data;
using EventPipelineHandler.Events.CustomEvents;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        public async Task<List<EventAction>> GetEventActionsAsync()
        {
            var eventActions =  await _applicationDbContext.EventActions
                .Include(e => e.ParentEventActions)
                .Include(e => e.ChildEventActions)
                .ToListAsync();

            MarkEventActionTrees(eventActions);

            return eventActions
                .OrderBy(e => e.TreeLetter)
                .ThenBy(e => e.Level)
                .ToList();
        }

        public async Task CreateTestEvents()
        {
            var customerEventAction = new EventAction
            {
                CreatedAt = DateTime.Now,
                Name = "Create customer in DB",
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
                UpdatedBy = "System"
            };

            var sharepointEventAction = new EventAction
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
                ParentEventActions = new List<EventAction>() { customerEventAction }
            };

            var customerEventAction2 = new EventAction
            {
                CreatedAt = DateTime.Now,
                Name = "Create customer in DB 2",
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
                ParentEventActions = new List<EventAction>() { customerEventAction }
            };


            var sharepointEvent2 = new EventAction
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
                ParentEventActions = new List<EventAction>() { customerEventAction2 }
            };

            _applicationDbContext.EventActions.Add(customerEventAction);
            _applicationDbContext.EventActions.Add(sharepointEventAction);
            _applicationDbContext.EventActions.Add(customerEventAction2);
            _applicationDbContext.EventActions.Add(sharepointEvent2);

            await _applicationDbContext.SaveChangesAsync();

            _backgroundTaskQueue.QueueEvent(customerEventAction);
        }

        public void MarkEventActionTrees(List<EventAction> eventActions)
        {
            
            char treeLetter = 'A';
            foreach (var eventAction in eventActions.Where(e => !e.ParentEventActions.Any()))
            {
                MarkEventActionTree(eventAction, 1, treeLetter);
                treeLetter++;
            }
        }

        private void MarkEventActionTree(EventAction eventAction, int level, char treeLetter)
        {
            eventAction.Level = level;
            eventAction.TreeLetter = treeLetter;
            foreach (var childEventAction in eventAction.ChildEventActions)
            {
                MarkEventActionTree(childEventAction, level + 1, treeLetter);
            }
        }
    }
}
