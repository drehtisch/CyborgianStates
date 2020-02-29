using CyborgianStates.Interfaces;
using CyborgianStates.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CyborgianStates.Repositories
{
    public class MongoDBUserRepository: IUserRepository
    {
        readonly IMongoCollection<User> users;
        readonly AppSettings _config;
        public MongoDBUserRepository(IMongoDatabase database, IOptions<AppSettings> config)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            users = database.GetCollection<User>("user");
            _config = config.Value;
        }

        public async Task AddUserToDbAsync(ulong userId)
        {
            var user = new User() { DiscordUserId = userId };
            user.Permissions.Add(new Permission() { Name = "ExecuteCommands", CreatedAt = DateTime.UtcNow });
            await users.InsertOneAsync(user).ConfigureAwait(false);
        }

        public async Task<bool> IsUserInDbAsync(ulong userId)
        {
            var res = await users.FindAsync(u => u.DiscordUserId == userId).ConfigureAwait(false);
            var user = await res.FirstOrDefaultAsync().ConfigureAwait(false);
            return user != null;
        }

        public async Task RemoveUserFromDbAsync(ulong userId)
        {
            await users.FindOneAndDeleteAsync(u => u.DiscordUserId == userId).ConfigureAwait(false);
        }

        public async Task<bool> IsAllowedAsync(string permissionType, ulong userId)
        {
            var res = await users.FindAsync(u => u.DiscordUserId == userId).ConfigureAwait(false);
            var user = await res.FirstOrDefaultAsync().ConfigureAwait(false);
            if (user != null)
            {
                return user.Permissions.Any(p => p.Name == permissionType);
            }
            else
            {
                return false;
            }
        }

        public Task<bool> IsBotAdminAsync(ulong userId)
        {
            return Task.FromResult(_config.DiscordBotAdminUser == userId);
        }
    }
}
