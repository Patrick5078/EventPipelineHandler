using EventPipelineHandler.Data;
using EventPipelineHandler.EventManager;
using EventPipelineHandler.Services;
using System.Collections.Concurrent;

public class EventActionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly BackgroundTaskQueue _backgroundTaskQueue;
    private readonly SemaphoreSlim _semaphore;

    public EventActionBackgroundService(IServiceScopeFactory serviceScopeFactory,
        BackgroundTaskQueue backgroundTaskQueue, int maxConcurrentTasks = 20)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _backgroundTaskQueue = backgroundTaskQueue;
        _semaphore = new SemaphoreSlim(maxConcurrentTasks);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            
            while (_backgroundTaskQueue.Dequeue(stoppingToken) is EventAction eventAction)
            {
                await _semaphore.WaitAsync(stoppingToken);
                
                var task = new Task(async () => {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var eventRunner = scope.ServiceProvider.GetRequiredService<IEventRunner>();
                    var result = await eventRunner.ExecuteEventAction(eventAction);

                    foreach (var action in result)
                        _backgroundTaskQueue.QueueEvent(action);

                    _semaphore.Release();
                });

                task.Start();
            }

            await Task.Delay(1000);
        }
    }
}
