
namespace ServiceHubMVC.ViewModels;

public sealed class DashboardViewModel
{
    public bool IsAdmin { get; set; }

    public int BusinessCount { get; set; }
    public int ServiceCount { get; set; }
    public int BookingCount { get; set; }

    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int RejectedCount { get; set; }

    public List<DashboardBusinessItemViewModel> Businesses
        { get; set; } = new();
}

public sealed class DashboardBusinessItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int ServiceCount { get; set; }
    public int BookingCount { get; set; }
}
