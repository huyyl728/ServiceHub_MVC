
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Services;

public class BookingStatusService
{
    private readonly ApplicationDbContext _context;

    public BookingStatusService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CancelAsync(
        int bookingId,
        string customerId)
    {
        if (bookingId <= 0 || string.IsNullOrWhiteSpace(customerId))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;

        var updated = await _context.Bookings
            .Where(b =>
                b.Id == bookingId &&
                b.CustomerId == customerId &&
                b.StartTime > now &&
                (b.Status == BookingStatus.Pending ||
                 b.Status == BookingStatus.Confirmed))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    b => b.Status,
                    BookingStatus.Cancelled));

        return updated == 1;
    }

    public Task<bool> ConfirmAsync(
        int bookingId,
        string ownerId)
    {
        return ChangeOwnerStatusAsync(
            bookingId,
            ownerId,
            BookingStatus.Confirmed);
    }

    public Task<bool> RejectAsync(
        int bookingId,
        string ownerId)
    {
        return ChangeOwnerStatusAsync(
            bookingId,
            ownerId,
            BookingStatus.Rejected);
    }

    private async Task<bool> ChangeOwnerStatusAsync(
        int bookingId,
        string ownerId,
        BookingStatus newStatus)
    {
        if (bookingId <= 0 || string.IsNullOrWhiteSpace(ownerId))
        {
            return false;
        }

        if (newStatus != BookingStatus.Confirmed &&
            newStatus != BookingStatus.Rejected)
        {
            return false;
        }

        var updated = await _context.Bookings
            .Where(b =>
                b.Id == bookingId &&
                b.Service.Business.OwnerId == ownerId &&
                b.Status == BookingStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    b => b.Status,
                    newStatus));

        return updated == 1;
    }

    public async Task<bool> CompleteAsync(
        int bookingId,
        string ownerId)
    {
        if (bookingId <= 0 || string.IsNullOrWhiteSpace(ownerId))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;

        var updated = await _context.Bookings
            .Where(b =>
                b.Id == bookingId &&
                b.Service.Business.OwnerId == ownerId &&
                b.Status == BookingStatus.Confirmed &&
                b.EndTime <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    b => b.Status,
                    BookingStatus.Completed));

        return updated == 1;
    }
}
