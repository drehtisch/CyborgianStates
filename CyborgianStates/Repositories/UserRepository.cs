using CyborgianStates.Interfaces;
using CyborgianStates.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        readonly string IsUserInDbSql;
        readonly string GetUserByExternalIdSql;
        readonly string RolePermissionsSql;
        readonly string UserPermissionsSql;
        #endregion

        IDbConnection _dbConnection;
        ISqlProvider _sql;
        AppSettings appSettings;
        public UserRepository(IDbConnection dbConnection, ISqlProvider sql, IOptions<AppSettings> options)
        {
            if (dbConnection is null) throw new ArgumentNullException(nameof(dbConnection));
            if (sql is null) throw new ArgumentNullException(nameof(sql));
            if (options is null) throw new ArgumentNullException(nameof(options));
            _dbConnection = dbConnection;
            _sql = sql;
            appSettings = options.Value;
            _dbConnection.ConnectionString = appSettings.DbConnection;
            IsUserInDbSql = _sql.GetSql("User.IsInDb");
            GetUserByExternalIdSql = _sql.GetSql("User.GetByExternalId");
            RolePermissionsSql = _sql.GetSql("User.RolePermissions");
            UserPermissionsSql = _sql.GetSql("User.Permissions");
        }

        public async Task AddUserToDbAsync(ulong userId)
        {
            await _dbConnection.InsertAsync(new User() { ExternalUserId = (long)userId }).ConfigureAwait(false);
        }

        public async Task<bool> IsAllowedAsync(string permissionType, ulong userId)
        {
            if (string.IsNullOrWhiteSpace(permissionType))
            {
                throw new ArgumentNullException(nameof(permissionType), "The permissionType can't be empty.");
            }

            if (appSettings.ExternalAdminUserId == userId) return true;

            IEnumerable<dynamic> res1 = await _dbConnection.QueryAsync(RolePermissionsSql, new { ExternalUserId = userId }).ConfigureAwait(false);
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
            return await _dbConnection.QueryFirstOrDefaultAsync(IsUserInDbSql, new { ExternalUserId = userId }).ConfigureAwait(false) != null;
        }

        public async Task RemoveUserFromDbAsync(User user)
        {
            await _dbConnection.DeleteAsync(user).ConfigureAwait(false);
        }

        public async Task<User> GetUserByExternalUserIdAsync(ulong externalUserId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(GetUserByExternalIdSql, new { ExternalUserId = externalUserId }).ConfigureAwait(false);
        }

        public async Task<User> GetUserByIdAsync(ulong userId)
        {
            return await _dbConnection.GetAsync<User>(userId).ConfigureAwait(false);
        }
    }
}
