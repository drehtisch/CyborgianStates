using Dapper.Contrib.Extensions;

namespace CyborgianStates.Models
{
    [Table("RolePermission")]
    public class RolePermission
    {
        public long PermissionId { get; set; }
        public long RoleId { get; set; }
    }
}