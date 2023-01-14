using EventPipelineHandler.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public int Level { get; set; }
        [NotMapped]
        public char? TreeLetter { get; set; }


        /// <summary>
        /// A child event will run once all of its parent event actions have completed successfully
        /// </summary>
        public ICollection<EventAction> ParentEventActions { get; set; } = new List<EventAction>();
        public ICollection<EventAction> ChildEventActions { get; set; } = new List<EventAction>();

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            // configure many to many
            modelBuilder.Entity<EventAction>()
                .HasMany(e => e.ParentEventActions)
                .WithMany(e => e.ChildEventActions)
                .UsingEntity(j => j.ToTable("EventActionRelationships"));
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