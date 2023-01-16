﻿using CTFServer.Repositories.Interface;
using CTFServer.Services.Interface;
using CTFServer.Utils;

namespace CTFServer.Services;

public class ContainerChecker : IHostedService, IDisposable
{
    private readonly ILogger<ContainerChecker> logger;
    private readonly IServiceScopeFactory serviceProvider;
    private Timer? timer;

    public ContainerChecker(IServiceScopeFactory provider, ILogger<ContainerChecker> logger)
    {
        serviceProvider = provider;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new Timer(Execute, null, TimeSpan.Zero, TimeSpan.FromMinutes(3));
        logger.SystemLog("Container lifecycle check started", TaskStatus.Success, LogLevel.Debug);
        return Task.CompletedTask;
    }

    private async void Execute(object? state)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var containerRepo = scope.ServiceProvider.GetRequiredService<IContainerRepository>();
        var containerService = scope.ServiceProvider.GetRequiredService<IContainerService>();

        foreach (var container in await containerRepo.GetDyingContainers())
        {
            await containerService.DestroyContainerAsync(container);
            await containerRepo.RemoveContainer(container);
            logger.SystemLog($"Removed expired container [{container.ContainerId}]");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        logger.SystemLog("Container lifecycle check stopped", TaskStatus.Exit, LogLevel.Debug);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
