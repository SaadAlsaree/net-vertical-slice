using __RootNamespace__.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __RootNamespace__.Data.Entities.__Category__;

[Table("__resource__")]
public class __Entity__ : BaseEntity
{
    // TODO(__Entity__): replace this placeholder with the actual columns of your table.
    // Example:
    //   [MaxLength(10)]
    //   public string Code { get; set; } = string.Empty;
    //   [MaxLength(50)]
    //   public string NameAr { get; set; } = string.Empty;
    //   [MaxLength(50)]
    //   public string? NameEn { get; set; }
    //   public bool IsActive { get; set; } = true;
    public string Name { get; set; } = string.Empty;
}
