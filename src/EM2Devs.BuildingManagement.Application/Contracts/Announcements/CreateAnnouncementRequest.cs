namespace EM2Devs.BuildingManagement.Application.Contracts.Announcements;

public sealed record CreateAnnouncementRequest(
    string Title,
    string? Body,
    AudienceSpecificationDto Audience
);
