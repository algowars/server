using Algowars.Application.Dtos.Users;

namespace Algowars.Application.Queries.Users.GetProfileSettings;

public sealed record GetProfileSettingsQuery(string Sub) : IQuery<ProfileSettingsDto>;
