using CyborgianStates.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyborgianStates.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> IsUserInDbAsync(ulong userId);
        Task AddUserToDbAsync(ulong userId);
        Task RemoveUserFromDbAsync(User user);
        Task<bool> IsAllowedAsync(string permissionType, ulong userId);
        Task<User> GetUserByIdAsync(ulong userId);
        Task<User> GetUserByExternalUserIdAsync(ulong externalUserId);
    }
}
