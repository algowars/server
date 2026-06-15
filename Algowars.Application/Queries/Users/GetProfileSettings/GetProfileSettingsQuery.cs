using Algowars.Application.Dtos.Users;

namespace Algowars.Application.Queries.Users.GetProfileSettings;

internal sealed record GetProfileSettingsQuery(string Sub) : IQuery<ProfileSettingsDto>;
