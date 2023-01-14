using EventPipelineHandler.Data;
using Microsoft.EntityFrameworkCore;

namespace EventPipelineHandler.Services
{
    public class EventService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public EventService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<EventAction>> GetEventActionsAsync()
        {
            return await _applicationDbContext.EventActions
                .Include(e => e.ParentEventAction).AsNoTracking().ToListAsync();
        }
    }
}
