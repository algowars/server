using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Entities.Users;

[Table("users")]
public sealed class UserDataModel : AuditableEntity
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Required, MaxLength(36), Column("username")]
    public required string Username { get; set; }

    [Required, MaxLength(128), Column("sub")]
    public required string Sub { get; set; }

    [MaxLength(2048), Column("image_url")]
    public string? ImageUrl { get; set; }

    [MaxLength(500), Column("bio")]
    public string? Bio { get; set; }

    [Column("username_last_changed_at")]
    public DateTime? UsernameLastChangedAt { get; set; }
}
