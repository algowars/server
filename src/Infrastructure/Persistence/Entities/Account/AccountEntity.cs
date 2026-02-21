using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities.Account;

[Table("accounts")]
public sealed class AccountEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required, MaxLength(36)]
    [Column("username")]
    public required string Username { get; set; }

    [Required, MaxLength(255)]
    [Column("sub")]
    public required string Sub { get; set; }

    [MaxLength(300)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("last_modified_on")]
    public DateTime? LastModifiedOn { get; set; }

    [Column("last_modified_by_id")]
    public Guid? LastModifiedById { get; set; }

    [ForeignKey(nameof(LastModifiedById))]
    public AccountEntity? LastModifiedByAccount { get; set; }

    [Column("deleted_on")]
    public DateTime? DeletedOn { get; set; }
}