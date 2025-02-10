
using App.Repositories;
using Coravel.Invocable;

namespace App.Worker.Scheduler;


public class ApiUrlCleaner(
    IApiUrlRepository ApiUrlRepository,
    ILogger<ApiUrlCleaner> logger) : IInvocable
{
    private readonly IApiUrlRepository ApiUrlRepository = ApiUrlRepository;

    public Task Invoke()
    {
        var dt = DateTime.UtcNow;

        using (var cts = new CancellationTokenSource())
        {
            // Simulate a cancellation after 5 seconds
            cts.CancelAfter(5000);

            try
            {
                ApiUrlRepository.HardDeleteClearTrash(cancellationToken: cts.Token).Wait();
                logger.LogInformation($"{dt.AddHours(7)} Worker-Scheduler-ApiUrlCleaner clean database");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{dt.AddHours(7)} Worker-Scheduler-ApiUrlCleaner clean database error");
            }
        }

        return Task.CompletedTask;
    }

}

