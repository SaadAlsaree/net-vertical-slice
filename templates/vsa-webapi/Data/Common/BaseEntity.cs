using System.ComponentModel.DataAnnotations;

namespace VsaTemplate.Data.Common;

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateAt { get; set; }
    public Guid? LastUpdateBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
