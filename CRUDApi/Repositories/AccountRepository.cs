using CRUDApi.DTOs.UserDTOs;
using CRUDApi.Interfaces;
using CRUDApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CRUDApi.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly CrudApiDbContext _context;

        public AccountRepository(CrudApiDbContext context) => _context = context;

        public async Task<UserDataForClaimsDTO> CheckUserPasswodByLoginAsync(string login, string password)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == login);

            var pass = new PasswordHasher<User>().HashPassword(new User(), password);

            if (new PasswordHasher<User>().VerifyHashedPassword(new User(), user.Password, password) == PasswordVerificationResult.Success)
            {
                return new UserDataForClaimsDTO()
                {
                    Login = user.Login,
                    Admin = user.Admin,
                };
            }

            return null;
        }

        public async Task<bool> CreateUserAsync(RegistrationDTO user, string adminLogin)
        {
            User newUser = new User()
            {
                Guid = Guid.NewGuid(),
                Name = user.Name,
                Login = user.Login,
                Password = user.Password,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                CreatedBy = adminLogin,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.MinValue,
                ModifiedBy = String.Empty,
                RevokedOn = DateTime.MinValue,
                RevokedBy = String.Empty
            };

            await _context.Users.AddAsync(newUser);

            return await SaveChangesAsync();
        }

        public async Task<bool> DeleteUserHardAsync(string login)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == login);

            _context.Users.Remove(user);

            return await SaveChangesAsync();
        }

        public async Task<bool> DeleteUserSoftAsync(string deleteUserLogin, string loginAuthUser)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == deleteUserLogin);

            user.RevokedBy = loginAuthUser;
            user.RevokedOn = DateTime.UtcNow;

            _context.Users.Update(user);

            return await SaveChangesAsync();
        }

        public bool GenderIsCorrect(int genderId) => genderId >= 0 && genderId <= 2;

        public async Task<List<UserWithoutPasswordDTO>> GetActiveUsersOrderByCreatedOnAsync()
        {
            return await _context.Users.AsNoTracking().Where(x => x.RevokedOn == DateTime.MinValue).OrderBy(x => x.CreatedOn).Select(x => new UserWithoutPasswordDTO()
            {
                Guid = x.Guid,
                Login = x.Login,
                Name = x.Name,
                Gender = x.Gender,
                Birthday = x.Birthday,
                Admin = x.Admin,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn,
                ModifiedBy = x.ModifiedBy,
                ModifiedOn = x.ModifiedOn,
                RevokedBy = x.RevokedBy,
                RevokedOn = x.RevokedOn
            }).ToListAsync();
        }

        public async Task<UserDataForClaimsDTO> GetDataForClaimsByNewLogin(string newLogin)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == newLogin);

            return new UserDataForClaimsDTO()
            {
                Login = user.Login,
                Admin = user.Admin,
            };
        }

        public async Task<CreatedUserDTO> GetUserDataAfterCreateByLoginAsync(string login)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == login);

            return new CreatedUserDTO()
            {
                Guid = user.Guid,
                Login = login,
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                CreatedBy = user.CreatedBy,
                CreatedOn = user.CreatedOn
            };
        }

        public async Task<ShortUserDTO> GetUserShortDataByLoginAsync(string login)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == login);

            return new ShortUserDTO()
            {
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                ActiveStatus = user.RevokedOn == DateTime.MinValue
            };
        }

        public async Task<List<UserWithoutPasswordDTO>> GetUsersOverAsync(int age)
        {
            return await _context.Users.AsNoTracking().Where(x => x.Birthday != null &&
            (DateTime.UtcNow.Year - ((DateTime)(x.Birthday)).Year) > age)
                .OrderBy(x => x.Birthday)
                .Select(x => new UserWithoutPasswordDTO()
                {
                    Guid = x.Guid,
                    Login = x.Login,
                    Name = x.Name,
                    Gender = x.Gender,
                    Birthday = x.Birthday,
                    Admin = x.Admin,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedOn = x.ModifiedOn,
                    RevokedBy = x.RevokedBy,
                    RevokedOn = x.RevokedOn
                }).ToListAsync();
        }

        public async Task<UserWithoutPasswordDTO> GetUserWithoutPasswordByLoginAsync(string login)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == login);

            return new UserWithoutPasswordDTO()
            {
                Guid = user.Guid,
                Login = login,
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                CreatedBy = user.CreatedBy,
                CreatedOn = user.CreatedOn,
                ModifiedBy = user.ModifiedBy,
                ModifiedOn = user.ModifiedOn,
                RevokedBy = user.RevokedBy,
                RevokedOn = user.RevokedOn
            };
        }

        public bool LoginIsCorrect(string login) => new Regex(@"^[a-zA-Z0-9]+$").IsMatch(login);

        public async Task<bool> LoginIsExist(string login) => await _context.Users.AsNoTracking().AnyAsync(x => x.Login == login);

        public bool NameIsCorrect(string name) => new Regex(@"^[a-zA-Zа-яА-Я]+$").IsMatch(name);

        public bool PasswordIsCorrect(string password) => new Regex(@"^[a-zA-Z0-9]+$").IsMatch(password);

        public async Task<bool> UpdatePasswordByLoginAsync(UpdateUserPasswordDTO updateUser, string loginAuthUser)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == updateUser.Login);

            user.Password = new PasswordHasher<User>().HashPassword(new User(), updateUser.Password);
            user.ModifiedBy = loginAuthUser;
            user.ModifiedOn = DateTime.UtcNow;

            _context.Users.Update(user);

            return await SaveChangesAsync();
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDTO updateUser, string loginAuthUser)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == updateUser.Login);

            if (!string.IsNullOrEmpty(updateUser.Name))
            {
                user.Name = updateUser.Name;
            }

            if (updateUser.Birthday != null)
            {
                user.Birthday = updateUser.Birthday;
            }

            if (updateUser.Gender != null)
            {
                user.Gender = (int)updateUser.Gender;
            }

            if (!string.IsNullOrEmpty(updateUser.Name) || updateUser.Birthday != null || updateUser.Gender != null)
            {
                user.ModifiedBy = loginAuthUser;
                user.ModifiedOn = DateTime.UtcNow;
            }

            _context.Users.Update(user);

            return await SaveChangesAsync();
        }

        public async Task<bool> UpdateUserLogin(string oldLogin, string newLogin, string loginAuthUser)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == oldLogin);

            user.Login = newLogin;
            user.ModifiedBy = loginAuthUser == oldLogin ? newLogin : loginAuthUser;
            user.ModifiedOn = DateTime.UtcNow;

            _context.Users.Update(user);

            return await SaveChangesAsync();
        }

        public async Task<bool> UserIsActiveAsync(string login)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == login);

            return user.RevokedOn == DateTime.MinValue;
        }

        public async Task<bool> UserRecovery(string recoveryUserLogin, string loginAuthUser)
        {
            var user = await _context.Users.AsNoTracking().FirstAsync(x => x.Login == recoveryUserLogin);

            user.ModifiedBy = loginAuthUser;
            user.ModifiedOn = DateTime.UtcNow;
            user.RevokedBy = String.Empty;
            user.RevokedOn = DateTime.MinValue;

            _context.Users.Update(user);

            return await SaveChangesAsync();
        }

        /// <summary>
        /// Сохранение изменений в базе данных
        /// </summary>
        /// <returns>
        /// true - изменения в базе данных сохранены;
        /// flase - изменения в базе данных не сохранены;
        /// </returns>
        private async Task<bool> SaveChangesAsync()
        {
            var save = await _context.SaveChangesAsync();
            return save > 0;
        }
    }
}