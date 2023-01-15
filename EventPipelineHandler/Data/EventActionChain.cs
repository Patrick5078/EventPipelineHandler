using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EventPipelineHandler.Data
{
    public class EventActionChain: IEntityWithIdAndMetaData, IValidatableObject
    {
        public required string Name { get; set; }
        public required ICollection<EventAction> EventActions { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure one to many
            modelBuilder.Entity<EventActionChain>()
                .HasMany(e => e.EventActions)
                .WithOne(e => e.EventActionChain)
                .HasForeignKey(e => e.EventActionChainId);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var steps = EventActions.Select(e => e.Step).Distinct();
            var expectedSteps = Enumerable.Range(1, steps.Count());
            
            if (!steps.SequenceEqual(expectedSteps))
                yield return new ValidationResult("Steps must be sequential and start at 1");
        }
    }
}
