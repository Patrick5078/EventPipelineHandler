using EventPipelineHandler.Data;

namespace EventPipelineHandler.EventManager
{
    public interface IEventRunner
    {
        Task<List<EventAction>> ExecuteEventAction(EventAction eventAction);
    }
}