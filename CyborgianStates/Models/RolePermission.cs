using Dapper.Contrib.Extensions;

namespace CyborgianStates.Models
{
    [Table("RolePermission")]
    public class RolePermission
    {
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
    }
}
