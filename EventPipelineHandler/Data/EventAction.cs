using Microsoft.EntityFrameworkCore;

namespace EventPipelineHandler.Data
{
    public class EventAction : IEntityWithIdAndMetaData
    {
        public EventActionState EventActionState { get; set; }
        public EventActionType EventActionType { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ExecutionLog { get; set; }
        public string? Data { get; set; } = string.Empty;
        public DateTime LastExecutedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public Guid? ParentEventActionId { get; set; }
        public EventAction? ParentEventAction { get; set; }
        public List<EventAction> ChildEventActions { get; set; } = new List<EventAction>();

        // setup entity framework foreign key .HasOne
        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventAction>()
                .HasOne(x => x.ParentEventAction)
                .WithMany(x => x.ChildEventActions)
                .HasForeignKey(x => x.ParentEventActionId);
        }

        public void AddLineToExecutionLog(string line)
        {
            ExecutionLog = ExecutionLog + "\n" + line;
        }
    }

    public enum EventActionState
    {
        Pending,
        InProgress,
        Done,
        Failed
    }

    public enum EventActionType
    {
        CreateCustomerInDb,
        TransferToSharepoint,
    }
}
