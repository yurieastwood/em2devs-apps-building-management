namespace EM2Devs.BuildingManagement.Application.Contracts.Announcements;

public sealed record UpdateAnnouncementRequest(
    string Title,
    string? Body,
    AudienceSpecificationDto Audience
);
