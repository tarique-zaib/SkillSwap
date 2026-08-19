using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateAsync(
        Guid userId,
        string type,
        string title,
        string message,
        Guid? relatedId = null,
        CancellationToken cancellationToken = default);

    Task<List<Notification>> GetMyNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}