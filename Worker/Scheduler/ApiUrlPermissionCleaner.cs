
using App.Repositories;
using Coravel.Invocable;

namespace App.Worker.Scheduler;


public class ApiUrlPermissionCleaner(
    IApiUrlPermissionRepository apiUrlPermissionRepository,
    ILogger<ApiUrlPermissionCleaner> logger) : IInvocable
{
    private readonly IApiUrlPermissionRepository apiUrlPermissionRepository = apiUrlPermissionRepository;

    public Task Invoke()
    {
        var dt = DateTime.UtcNow;
        using (var cts = new CancellationTokenSource())
        {
            // Simulate a cancellation after 5 seconds
            cts.CancelAfter(5000);

            try
            {
                apiUrlPermissionRepository.HardDeleteClearTrash(cancellationToken: cts.Token).Wait();
                logger.LogInformation($"{dt.AddHours(7)} Worker-Scheduler-ApiUrlPermissionCleaner clean database");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{dt.AddHours(7)} Worker-Scheduler-ApiUrlPermissionCleaner clean database error");
            }
        }

        return Task.CompletedTask;
    }

}

