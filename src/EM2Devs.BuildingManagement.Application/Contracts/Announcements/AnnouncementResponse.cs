namespace EM2Devs.BuildingManagement.Application.Contracts.Announcements;

public sealed record AnnouncementResponse(
    Guid AnnouncementId,
    Guid AuthoredByManagerId,
    string Title,
    string? Body,
    string Status,
    AudienceSpecificationDto Audience,
    DateTime? PublishedAt,
    DateTime? ArchivedAt,
    DateTime CreatedAt
);
