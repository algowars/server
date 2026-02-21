using ApplicationCore.Dtos.Accounts;

namespace ApplicationCore.Queries.Accounts.GetProfileSettings;

public sealed record GetProfileSettingsQuery(string Sub) : IQuery<ProfileSettingsDto>;