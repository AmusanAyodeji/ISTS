namespace Ticketing.Infrastructure.BackgroundJobs;

public class BackgroundJobService
{
    public Task Enqueue(Func<Task> work)
    {
        _ = Task.Run(work);
        return Task.CompletedTask;
    }
}
