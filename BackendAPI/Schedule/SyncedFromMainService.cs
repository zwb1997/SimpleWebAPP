namespace BackendAPI.Schedule;

using BackendAPI.Models.Tables;
using BackendAPI.Services;
using System.Security.Cryptography;

public class SyncedFromMainService : IHostedService, IDisposable
{
    /// <summary>
    /// Create a number generator
    /// </summary>
    private static RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

    /// <summary>
    /// Mock case severity only in ["A", "B", "C"]
    /// </summary>
    private static readonly string[] SEV = ["A", "B", "C"];

    private readonly ILogger<SyncedFromMainService> _logger;
    private Timer _timer;
    private readonly IServiceProvider _serviceProvider;

    public SyncedFromMainService(ILogger<SyncedFromMainService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mock Schedule Synced from Main Case Table Task Hosted Service running every 20 seconds");

        _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20)); // Adjust interval as needed

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mock Schedule Synced from Main Case Table Task Hosted Service  is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        _logger.LogInformation("Mock for syncing data from main Case table.");

        _logger.LogInformation(">>>Try fetch IFollowedCaseService from Scope ServiceProvider.");
        using (var scope = _serviceProvider.CreateScope())
        {
            var _followedCaseService = scope.ServiceProvider.GetRequiredService<IFollowedCaseService>();

            var queryResult = _followedCaseService.QueryCases(new CaseFollowDTO());

            // Ensure we do not use .Result, which can block. Use await instead.
            List<CaseFollowModel> currentValues = queryResult.Result.Value.Value;

            _logger.LogInformation("Mock for change every followed cases [CurrentSyncedTime] to DateTime.Now");


            currentValues.ForEach(val =>
            {
                _logger.LogInformation($"[CaseFollowModel]FollowedCase:[{val}]");

                val.CurrentSyncedTime = DateTime.Now;
                val.CaseSev = GetRandomCaseSev(val.CaseSev ?? SEV[2], 0);
                val.IsClosed = !(val.IsClosed ?? true);

                _logger.LogInformation($"[CaseFollowModel] Updated => FollowedCase:[{val}]");
            });

            _logger.LogInformation("Waiting for Batch update the followed cases after change their [CurrentSyncedTime]");
            _followedCaseService.BatchUpdate(currentValues);
        }
    }

    private string GetRandomCaseSev(string currentSev, int deep)
    {
        if (deep == 3)
        {
            return currentSev;
        }
        string newSev = SEV[GetRandomLimitNumber(3)];
        if (newSev.Equals(currentSev))
        {
            return GetRandomCaseSev(currentSev, deep + 1);
        }

        return newSev;
    }

    // Ensure result is uniform distributed
    private int GetRandomLimitNumber(int maxValue)
    {
        byte[] data = new byte[4];
        int result;

        do
        {
            _randomNumberGenerator.GetBytes(data);
            // result can be generated between [0,int.MaxValue]
            result = BitConverter.ToInt32(data, 0) & int.MaxValue;

            //this line ensure result < int.MaxValue - 1
        } while (result >= maxValue * (int.MaxValue / maxValue));

        return result % maxValue;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}