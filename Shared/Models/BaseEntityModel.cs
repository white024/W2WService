namespace Shared.Models;

public class BaseEntityModel
{
    public bool IsActive { get; set; } = true;
    public DateTime? ModifiedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
}
