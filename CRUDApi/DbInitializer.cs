using CRUDApi.Models;
using Microsoft.AspNetCore.Identity;

namespace CRUDApi
{
    /// <summary>
    /// Класс инициализации базы данных при запуске API
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Метод инциализации базы данных при запуске API с созданием пользователя системы с статусом "Администратор"
        /// </summary>
        /// <param name="context">контекст базы данных</param>
        /// <param name="admin">кортеж данных для создания администратора</param>
        /// <returns></returns>
        /// <exception cref="Exception">Администратор не был создан</exception>
        public static async Task InitializeAsync(CrudApiDbContext context, (string adminLogin, string adminName, string adminPassword, int adminGender) admin)
        {
            await context.Database.EnsureCreatedAsync();

            if (context.Users.Any(user => user.Admin == true))
            {
                return;
            }

            User newAdmin = new User()
            {
                Admin = true,
                Guid = Guid.NewGuid(),
                Name = admin.adminName,
                Login = admin.adminLogin,
                Gender = admin.adminGender,
                Birthday = null,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "DbInitializer",
                ModifiedOn = DateTime.MinValue,
                ModifiedBy = String.Empty,
                RevokedOn = DateTime.MinValue,
                RevokedBy = String.Empty,
            };

            newAdmin.Password = new PasswordHasher<User>().HashPassword(new User(), admin.adminPassword);

            await context.Users.AddAsync(newAdmin);

            if(await context.SaveChangesAsync() == 0)
            {
                throw new Exception("Administrator doesn't create");
            }
        }
    }
}
