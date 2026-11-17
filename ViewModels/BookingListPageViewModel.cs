
using ServiceHubMVC.Models;

namespace ServiceHubMVC.ViewModels;

public class BookingListPageViewModel
{
    public List<BookingListItemViewModel> Items { get; set; } = new();

    public DateOnly? Date { get; set; }

    public BookingStatus? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public bool IsOwner { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalItems / PageSize);
}
