using Microsoft.EntityFrameworkCore;

namespace CTFServer.Providers;

public class EntityConfigurationSource : IConfigurationSource
{
    public Action<DbContextOptionsBuilder> OptionsAction { get; set; }
    public int PollingInterval { get; private set; }

    public EntityConfigurationSource(Action<DbContextOptionsBuilder> _optionsAction, int _pollingInterval = 180000)
    {
        OptionsAction = _optionsAction;
        PollingInterval = _pollingInterval; // Defaults to 3min
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new EntityConfigurationProvider(this);
}