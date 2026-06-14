using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Algowars.Infrastructure.Persistence.Models.Users;

[Table("users")]
internal sealed class UserDataModel
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("sub")]
    [Required]
    public required string Sub { get; set; }

    [Column("username")]
    [Required]
    [MaxLength(20)]
    public required string Username { get; set; }

    [Column("bio")]
    [MaxLength(500)]
    public string? Bio { get; set; }

    [Column("image_url")]
    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    [Column("username_last_changed_at")]
    public DateTime? UsernameLastChangedAt { get; set; }
}
