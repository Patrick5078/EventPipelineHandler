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

        public void QueueFirstStepsOfEventActionChain(EventActionChain workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var lowestStep = workItem.EventActions.Min(e => e.Step);
            var lowestStepActions = workItem.EventActions.Where(e => e.Step == lowestStep);

            foreach (var eventAction in lowestStepActions)
                _workItems.Enqueue(eventAction);
        }

        public void QueueEventAction(EventAction workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
        }

        public EventAction? DequeueEventAction(
            CancellationToken cancellationToken)
        {
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
