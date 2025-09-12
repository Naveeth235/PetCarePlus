using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCare.Application.Auth.RegisterOwner;
using PetCare.Domain.Pets;
using PetCare.Domain.Users;
using PetCare.Infrastructure.Auth;
using PetCare.Infrastructure.Persistence;
using Xunit;

namespace PetCare.Application.Auth.RegisterOwner.Tests
{
    public class RegisterOwnerCommandTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<PetCareDbContext> _dbMock;

        public RegisterOwnerCommandTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            var options = new DbContextOptionsBuilder<PetCareDbContext>()
                .Options;
            _dbMock = new Mock<PetCareDbContext>(options);
            var petsDbSet = new Mock<DbSet<Pet>>();
            _dbMock.Setup(db => db.Pets).Returns(petsDbSet.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsEmailInUse_WhenEmailExists()
        {
            var usersDbSet = new Mock<DbSet<ApplicationUser>>();
            usersDbSet.Setup(x => x.AsNoTracking()).Returns(usersDbSet.Object);
            usersDbSet.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.Users).Returns(usersDbSet.Object);

            var command = new RegisterOwnerCommand(_userManagerMock.Object, _roleManagerMock.Object, _dbMock.Object);
            var request = new RegisterOwnerRequest { Email = "test@test.com", FullName = "Test", Password = "pass" };

            var result = await command.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Equal("email_in_use", result.error);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsIdentityError_WhenUserCreationFails()
        {
            var usersDbSet = new Mock<DbSet<ApplicationUser>>();
            usersDbSet.Setup(x => x.AsNoTracking()).Returns(usersDbSet.Object);
            usersDbSet.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userManagerMock.Setup(x => x.Users).Returns(usersDbSet.Object);

            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult);

            var command = new RegisterOwnerCommand(_userManagerMock.Object, _roleManagerMock.Object, _dbMock.Object);
            var request = new RegisterOwnerRequest { Email = "test@test.com", FullName = "Test", Password = "pass" };

            var result = await command.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Contains("identity_error", result.error);
            Assert.Contains("Password too weak", result.error);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsRoleError_WhenRoleAssignmentFails()
        {
            var usersDbSet = new Mock<DbSet<ApplicationUser>>();
            usersDbSet.Setup(x => x.AsNoTracking()).Returns(usersDbSet.Object);
            usersDbSet.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userManagerMock.Setup(x => x.Users).Returns(usersDbSet.Object);

            var identityResultSuccess = IdentityResult.Success;
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResultSuccess);

            _roleManagerMock.Setup(x => x.RoleExistsAsync("Owner")).ReturnsAsync(true);

            var identityResultFailed = IdentityResult.Failed(new IdentityError { Description = "Role error" });
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Owner"))
                .ReturnsAsync(identityResultFailed);

            var command = new RegisterOwnerCommand(_userManagerMock.Object, _roleManagerMock.Object, _dbMock.Object);
            var request = new RegisterOwnerRequest { Email = "test@test.com", FullName = "Test", Password = "pass" };

            var result = await command.ExecuteAsync(request);

            Assert.False(result.ok);
            Assert.Contains("role_error", result.error);
            Assert.Contains("Role error", result.error);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsSuccess_WhenAllStepsSucceed_WithoutPet()
        {
            var usersDbSet = new Mock<DbSet<ApplicationUser>>();
            usersDbSet.Setup(x => x.AsNoTracking()).Returns(usersDbSet.Object);
            usersDbSet.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userManagerMock.Setup(x => x.Users).Returns(usersDbSet.Object);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock.Setup(x => x.RoleExistsAsync("Owner")).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Owner"))
                .ReturnsAsync(IdentityResult.Success);

            _dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new RegisterOwnerCommand(_userManagerMock.Object, _roleManagerMock.Object, _dbMock.Object);
            var request = new RegisterOwnerRequest { Email = "test@test.com", FullName = "Test", Password = "pass" };

            var result = await command.ExecuteAsync(request);

            Assert.True(result.ok);
            Assert.Null(result.error);
        }

        [Fact]
        public async Task ExecuteAsync_AddsPet_WhenPetProvided()
        {
            var usersDbSet = new Mock<DbSet<ApplicationUser>>();
            usersDbSet.Setup(x => x.AsNoTracking()).Returns(usersDbSet.Object);
            usersDbSet.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _userManagerMock.Setup(x => x.Users).Returns(usersDbSet.Object);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock.Setup(x => x.RoleExistsAsync("Owner")).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Owner"))
                .ReturnsAsync(IdentityResult.Success);

            var petsDbSet = new Mock<DbSet<Pet>>();
            petsDbSet.Setup(x => x.Add(It.IsAny<Pet>()));
            _dbMock.Setup(db => db.Pets).Returns(petsDbSet.Object);
            _dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new RegisterOwnerCommand(_userManagerMock.Object, _roleManagerMock.Object, _dbMock.Object);
            var petRequest = new RegisterOwnerPetRequest
            {
                Name = "Buddy",
                Species = "Dog",
                Breed = "Golden Retriever",
                Dob = DateTime.UtcNow.AddYears(-2)
            };
            var request = new RegisterOwnerRequest
            {
                Email = "test@test.com",
                FullName = "Test",
                Password = "pass",
                Pet = petRequest
            };

            var result = await command.ExecuteAsync(request);

            petsDbSet.Verify(x => x.Add(It.IsAny<Pet>()), Times.Once);
            Assert.True(result.ok);
            Assert.Null(result.error);
        }
    }
}