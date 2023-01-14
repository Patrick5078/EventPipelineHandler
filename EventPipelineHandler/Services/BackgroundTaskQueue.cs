namespace EventPipelineHandler.Services
{
    using EventPipelineHandler.Data;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class BackgroundTaskQueue
    {
        private ConcurrentQueue<EventAction> _workItems =
            new ConcurrentQueue<EventAction>();

        public void QueueEvent(EventAction workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
        }

        public EventAction? Dequeue(
            CancellationToken cancellationToken)
        {
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
