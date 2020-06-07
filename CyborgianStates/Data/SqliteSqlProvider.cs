using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Data
{
    public class SqliteSqlProvider : ISqlProvider // -> BaseSqlProvider with all SQLs that are the same for all Datbases
    {
        public SqliteSqlProvider()
        {
            InitDictionary();
        }
        Dictionary<string, string> sqlDict = new Dictionary<string, string>();
        private void InitDictionary()
        {
            sqlDict = new Dictionary<string, string>()
            {
                ["User.IsInDb"] = "SELECT 1 FROM User WHERE ExternalUserId = @ExternalUserId",
                ["User.GetByExternalId"] = "SELECT * FROM User WHERE ExternalUserId = @ExternalUserId",
                ["User.RolePermissions"] = @"SELECT 
	                    DISTINCT p.Name
                    FROM
	                    Permission p
	                JOIN RolePermission rp ON rp.PermissionId = p.Id
	                JOIN UserRole ur ON ur.RoleId = rp.RoleId
	                JOIN User u on u.Id = ur.UserId
                    WHERE u.ExternalUserId = @ExternalUserId",
                ["User.Permissions"] = @"SELECT 
	                   DISTINCT p.Name
                    FROM
	                   Permission p
	                JOIN UserPermission up ON up.PermissionId = p.Id
	                JOIN User u on u.Id = up.UserId
                    WHERE u.ExternalUserId = @ExternalUserId",
                ["Seeding.CheckRole"] = "SELECT 1 FROM Role as r WHERE r.Name = @RoleName",
                ["Seeding.CheckPermission"] = "SELECT 1 FROM Permission as p WHERE p.Name = @PermName",
                ["Seeding.CheckStatusName"] = "SELECT count(*) as Count FROM StatusNames",
                ["Seeding.CheckTables"] = "SELECT tbl_name as Name FROM sqlite_master WHERE type=\"table\"",
            };
        }
        public string GetSql(string key)
        {
            if (sqlDict.TryGetValue(key, out string sql))
            {
                return sql;
            }
            else
            {
                throw new KeyNotFoundException($"No sql for key '{key}' found.");
            }
        }
    }
}
