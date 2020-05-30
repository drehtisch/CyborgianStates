using CyborgianStates.Interfaces;
using CyborgianStates.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Repositories
{
    public class UserRepository : IUserRepository
    {
        #region SQLs
        const string IsUserInDbSql = "SELECT 1 FROM User as u WHERE u.DiscordUserId = @DiscordUserId";
        const string GetUserByDiscordIdSql = "SELECT * FROM User WHERE DiscordUserId = @DiscordUserId";
        const string RolePermissionsPerUserSql =
            @"SELECT 
	            DISTINCT p.Name
            FROM
	            Permission p
	        JOIN RolePermission rp ON rp.PermissionId = p.Id
	        JOIN UserRole ur ON ur.RoleId = rp.RoleId
	        JOIN User u on u.Id = ur.UserId
            WHERE u.ExternalUserId = @ExternalUserId";
        const string UserPermissionsSql =
            @"SELECT 
	            DISTINCT p.Name
              FROM
	            Permission p
	          JOIN UserPermission up ON up.PermissionId = p.Id
	          JOIN User u on u.Id = up.UserId
              WHERE u.ExternalUserId = @ExternalUserId";
        #endregion

        IDbConnection _dbConnection;
        public UserRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task AddUserToDbAsync(ulong userId)
        {
            await _dbConnection.InsertAsync(new User() { ExternalUserId = (long)userId }).ConfigureAwait(false);
        }

        public async Task<bool> IsAllowedAsync(string permissionType, ulong userId)
        {
            if (permissionType is null || !string.IsNullOrWhiteSpace(permissionType))
            {
                throw new ArgumentNullException(nameof(permissionType), "The permissionType can't be empty.");
            }
            IEnumerable<dynamic> res1 = await _dbConnection.QueryAsync(RolePermissionsPerUserSql, new { ExternalUserId = userId }).ConfigureAwait(false);
            IEnumerable<dynamic> res2 = await _dbConnection.QueryAsync(UserPermissionsSql, new { ExternalUserId = userId }).ConfigureAwait(false);
            var perms = res1.Select<dynamic, string>(r => r.Name).ToHashSet();
            perms.UnionWith(res2.Select<dynamic, string>(r => r.Name));
            perms = perms.Distinct().ToHashSet();
            if (!perms.Contains(permissionType))
            {
                if (perms.Any(p => p.EndsWith("*", StringComparison.InvariantCulture)))
                {
                    string rootString = permissionType.Substring(0, permissionType.LastIndexOf(".", StringComparison.InvariantCulture));
                    var listParts = new List<string>
                    {
                        "*",
                        permissionType.Substring(0, permissionType.IndexOf(".", StringComparison.InvariantCulture))
                    };
                    while (rootString.Contains('.', StringComparison.InvariantCulture))
                    {
                        listParts.Add(rootString);
                        rootString = rootString.Substring(0, rootString.LastIndexOf(".", StringComparison.InvariantCulture));
                    }
                    HashSet<string> rootPerms = listParts.Select(p => $"{p}.*").ToHashSet();
                    return perms.Overlaps(rootPerms);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> IsUserInDbAsync(ulong userId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync(IsUserInDbSql, new { DiscordUserId = userId }).ConfigureAwait(false) != null;
        }

        public async Task RemoveUserFromDbAsync(User user)
        {
            await _dbConnection.DeleteAsync(user).ConfigureAwait(false);
        }

        public async Task<User> GetUserByExternalUserIdAsync(ulong externalUserId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(GetUserByDiscordIdSql, new { ExternalUserId = externalUserId }).ConfigureAwait(false);
        }

        public async Task<User> GetUserByIdAsync(ulong userId)
        {
            return await _dbConnection.GetAsync<User>(userId).ConfigureAwait(false);
        }
    }
}
