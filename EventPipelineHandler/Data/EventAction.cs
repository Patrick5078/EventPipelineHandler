using EventPipelineHandler.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
        
        /// <summary>
        /// At which step in the chain should the event be executed?
        /// </summary>
        public required int Step { get; set; }

        public Guid EventActionChainId { get; set; }
        public EventActionChain EventActionChain { get; set; }

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