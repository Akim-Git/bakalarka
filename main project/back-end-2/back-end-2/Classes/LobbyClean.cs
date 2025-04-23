using Microsoft.EntityFrameworkCore;


public class LobbyClean : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(980); // každých 980 minut

    public LobbyClean(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanUpLobbiesAsync();
            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanUpLobbiesAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var expirationTimeMinutes = 1;

            var lobbies = await context.Lobbies.ToListAsync();

            

            var lobbiesToDelete = lobbies
                .Where(l =>
                (!l.IsActive && (now - l.CreatedAt).TotalMinutes > 5000000) // neaktivní lobby starší než 50000000 minut
                )
                .ToList();

            if (lobbiesToDelete.Any())
            {
                context.Lobbies.RemoveRange(lobbiesToDelete);
                await context.SaveChangesAsync();
                Console.WriteLine($"[LobbyCleanup] Smazáno {lobbiesToDelete.Count} lobby.");
            }
        }
    }
}
