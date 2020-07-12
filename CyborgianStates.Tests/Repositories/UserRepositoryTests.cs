using CyborgianStates.Data;
using CyborgianStates.Interfaces;
using CyborgianStates.Models;
using CyborgianStates.Repositories;
using Dapper.Contrib.Extensions;
using DataAbstractions.Dapper;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.Repositories
{
    public class UserRepositoryTests
    {
        Mock<IDataAccessor> dataAccessorMock;
        ISqlProvider sqlProvider = new SqliteSqlProvider();
        Mock<IOptions<AppSettings>> appSettingsMock;
        UserRepository userRepo;
        public UserRepositoryTests()
        {
            dataAccessorMock = new Mock<IDataAccessor>(MockBehavior.Strict);
            dataAccessorMock.SetupGet<string>(db => db.ConnectionString).Returns(string.Empty);
            dataAccessorMock.SetupSet<string>(db => db.ConnectionString = It.IsAny<string>());
            dataAccessorMock.SetupGet<ConnectionState>(db => db.State).Returns(ConnectionState.Open);
            appSettingsMock = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            appSettingsMock
                .Setup(m => m.Value)
                .Returns(new AppSettings() { Contact = "" });
            userRepo = new UserRepository(dataAccessorMock.Object, sqlProvider, appSettingsMock.Object);
        }

        [Fact]
        public void TestCreateRepository()
        {
            Assert.Throws<ArgumentNullException>(() => new UserRepository(null, sqlProvider, appSettingsMock.Object));
            Assert.Throws<ArgumentNullException>(() => new UserRepository(dataAccessorMock.Object, null, appSettingsMock.Object));
            Assert.Throws<ArgumentNullException>(() => new UserRepository(dataAccessorMock.Object, sqlProvider, null));
            var userRepo = new UserRepository(dataAccessorMock.Object, sqlProvider, appSettingsMock.Object);
        }

        [Fact]
        public async Task TestAddUserToDb()
        {
            dataAccessorMock.Setup(db => db.InsertAsync(It.IsAny<User>(), null, null, null)).Returns(Task.FromResult(1));
            await userRepo.AddUserToDbAsync(1).ConfigureAwait(false);
            dataAccessorMock.Verify(db => db.InsertAsync(It.IsAny<User>(), null, null, null), Times.Once);
        }
        [Fact]
        public async Task TestRemoveUserFromDb()
        {
            dataAccessorMock.Setup(db => db.DeleteAsync(It.IsAny<User>(), null, null)).Returns(Task.FromResult(true));
            await userRepo.RemoveUserFromDbAsync(It.IsAny<User>()).ConfigureAwait(false);
            dataAccessorMock.Verify(db => db.DeleteAsync(It.IsAny<User>(), null, null), Times.Once);
        }
        [Fact]
        public async Task TestGetUserById()
        {
            dataAccessorMock.Setup(db => db.GetAsync<User>(It.IsAny<ulong>(), null, null)).Returns(Task.FromResult(new User()));
            await userRepo.GetUserByIdAsync(1).ConfigureAwait(false);
            dataAccessorMock.Setup(db => db.QueryFirstOrDefaultAsync<User>(It.IsAny<string>(),It.IsAny<object>(), null, null, null)).Returns(Task.FromResult(new User()));
            await userRepo.GetUserByExternalUserIdAsync(1).ConfigureAwait(false);
            dataAccessorMock.Verify(db => db.GetAsync<User>(It.IsAny<ulong>(), null, null), Times.Once);
        }

    }
}
