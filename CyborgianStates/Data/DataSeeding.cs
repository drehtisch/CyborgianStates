using CyborgianStates.Interfaces;
using CyborgianStates.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Data
{
    public class DataSeeding
    {
        IDbConnection _dbConnection;
        ILogger<DataSeeding> _logger;
        ISqlProvider _sql;
        const int DbInteration = 1;
        const int DBInteration = 1;
        readonly List<string> Tables = new List<string>() { "RolePermission", "UserPermission", "Nation", "Permission", "Status", "StatusNames", "CodeType", "CodePurpose", "Codes", "Stats", "Timers", "User", "UserRole" };
        public DataSeeding(IDbConnection dbConnection, ISqlProvider sql)
        {
            if (dbConnection is null) throw new ArgumentNullException(nameof(dbConnection));
            if (sql is null) throw new ArgumentNullException(nameof(sql));
            _dbConnection = dbConnection;
            _logger = ApplicationLogging.CreateLogger<DataSeeding>();
            _sql = sql;
            CheckRoleSql = _sql.GetSql("Seeding.CheckRole");
            CheckPermissionSql = _sql.GetSql("Seeding.CheckPermission");
            CheckTablesSql = _sql.GetSql("Seeding.CheckTables");
            CheckStatusNameCountSql = _sql.GetSql("Seeding.CheckStatusName");
        }

        #region SQLs
        readonly string CheckRoleSql;
        readonly string CheckPermissionSql;
        readonly string CheckStatusNameCountSql;
        readonly string CheckTablesSql;
        #endregion

        public async Task SeedAsync()
        {
            bool abort = false;
            var tables = await _dbConnection.QueryAsync(CheckTablesSql).ConfigureAwait(false);
            var existingTables = tables.Select<dynamic, string>(t => t.Name.ToString());
            var missingTables = Tables.Except(existingTables);
            var statusNames = await _dbConnection.QueryFirstOrDefaultAsync(CheckStatusNameCountSql).ConfigureAwait(false);
            if(missingTables.Any())
            {
                foreach(string table in missingTables)
                {
                    _logger.LogError($"Missing table '{table}' in Database. Please run CreateDb.sql for your database or check the documentation for migration instructions between versions.");
                    abort = true;
                }
            }
            var version = await _dbConnection.QueryFirstOrDefaultAsync("SELECT * from DbInfo Order By DbInfo.Timestamp DESC Limit 1;").ConfigureAwait(false);
            if (version.Id != DBInteration)
            {
                _logger.LogError($"Expected database to be on interation {1} but was {version.Id}. Check documentation for migration instructions between versions.");
                abort = true;
            }
            if (abort)
            {
                throw new InvalidOperationException("Database didn't satisfy seeding requirements. Check Logs for details.");
            }
            if (await _dbConnection.QueryFirstOrDefaultAsync(CheckRoleSql, new { RoleName = "BotAdmin" }).ConfigureAwait(false) is null)
            {
                await _dbConnection.InsertAsync(new Role() { Name = "BotAdmin" }).ConfigureAwait(false);
            }
            if (await _dbConnection.QueryFirstOrDefaultAsync(CheckPermissionSql, new { PermName = "*.*" }).ConfigureAwait(false) is null)
            {
                await _dbConnection.InsertAsync(new Permission() { Name = "*.*" }).ConfigureAwait(false);
                var perm = _dbConnection.QueryFirstAsync("SELECT Id FROM Permission WHERE Name = \"*.*\"");
                var role = _dbConnection.QueryFirstAsync("SELECT Id FROM Role WHERE Name = \"BotAdmin\"");
                await _dbConnection.InsertAsync(new RolePermission() { PermissionId = perm.Id, RoleId = role.Id }).ConfigureAwait(false);
            }
        }
    }
}
