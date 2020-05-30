using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class DataSeeding
    {
        IDbConnection _dbConnection;
        public DataSeeding(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        #region SQLs
        const string CheckRoleSql = "SELECT 1 FROM Role as r WHERE r.Name = @RoleName";
        const string CheckPermissionSql = "SELECT 1 FROM Permission as p WHERE p.Name = @PermName";
        const string CheckStatusNameCountSql = "SELECT count(*) as Count FROM StatusNames";
        #endregion

        public async Task<bool> IsSeedingRequiredAsync()
        {
            var roleCheck = await _dbConnection.QueryFirstOrDefaultAsync(CheckRoleSql, new { RoleName = "BotAdmin" }).ConfigureAwait(false) != null;
            var permCheck = await _dbConnection.QueryFirstOrDefaultAsync(CheckPermissionSql, new { RoleName = "*.*" }).ConfigureAwait(false) != null;
            var statusNames = await _dbConnection.QueryFirstOrDefaultAsync(CheckStatusNameCountSql, new { RoleName = "*.*" }).ConfigureAwait(false);
            return !(roleCheck && permCheck && statusNames.Count >= 8);
        }

        public async Task SeedAsync()
        {

        }

    }
}
