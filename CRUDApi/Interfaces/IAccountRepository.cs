using CRUDApi.DTOs.UserDTOs;

namespace CRUDApi.Interfaces
{
    public interface IAccountRepository
    {
        /// <summary>
        /// Проверка существования пользователя с указанным логином
        /// </summary>
        /// <param name="login">логин</param>
        /// <returns>
        /// true - пользователь существует;
        /// flase - полльзователь не существует
        /// </returns>
        public Task<bool> LoginIsExist(string login);

        /// <summary>
        /// Проверка корректности логина
        /// </summary>
        /// <param name="login">Логин</param>
        /// <returns>
        /// true - логин пользователя валиден;
        /// flase - логин пользователя не валиден;
        /// </returns>
        public bool LoginIsCorrect(string login);

        /// <summary>
        /// Проверка корректности пароля до хеширования
        /// </summary>
        /// <param name="password">пароль</param>
        /// <returns>
        /// true - пароль пользователя валиден;
        /// flase - пароль пользователя не валиден;
        /// </returns>
        public bool PasswordIsCorrect(string password);

        /// <summary>
        /// Проверка корректности имени пользователя
        /// </summary>
        /// <param name="name">имя пользователя</param>
        /// <returns>
        /// true - имя пользователя валидно;
        /// flase - имя пользователя не валидно;
        /// </returns>
        public bool NameIsCorrect(string name);

        /// <summary>
        /// Проверка корректности идентификатора гендера (0 - женщина, 1 - мужчина, 2 - неизвестно)
        /// </summary>
        /// <param name="genderId">id гендера</param>
        /// <returns>
        /// true - гендер пользователя валиден;
        /// flase - гендер пользователя не валиден;
        /// </returns>
        public bool GenderIsCorrect(int genderId);


        /// <summary>
        /// Создание пользователя в базе данных
        /// </summary>
        /// <param name="user">данные пользователя</param>
        /// <param name="adminLogin">логин администратора</param>
        /// <returns>
        /// true - пользователь записан в базу данных;
        /// flase - пользователь не записан в базу данных;
        /// </returns>
        public Task<bool> CreateUserAsync(RegistrationDTO user, string adminLogin);

        /// <summary>
        /// Проверка, активен ли пользователь по его логину
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <returns>
        /// true - пользователь активен;
        /// flase - пользователь не активен;
        /// </returns>
        public Task<bool> UserIsActiveAsync(string login);

        /// <summary>
        /// Проверка пароля пользователя по его логину
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <param name="password">пароль</param>
        /// <returns>
        /// Заполненная модель UserDataForClaims - пароль верный;
        /// null - пароль не верный
        /// </returns>
        public Task<UserDataForClaimsDTO> CheckUserPasswodByLoginAsync(string login, string password);

        /// <summary>
        /// Получение данных пользователя после его создания по логину
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <returns>Данные пользователя</returns>
        public Task<CreatedUserDTO> GetUserDataAfterCreateByLoginAsync(string login);

        /// <summary>
        /// Получние данных пользователя без его пароля по логину
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <returns>Модель данных пользователя UserWithoutPasswordDTO</returns>
        public Task<UserWithoutPasswordDTO> GetUserWithoutPasswordByLoginAsync(string login);

        /// <summary>
        /// Получение списка активных пользователей отсортированных по дате создания.
        /// </summary>
        /// <returns>Список моделей с данными UserWithoutPasswordDTO</returns>
        public Task<List<UserWithoutPasswordDTO>> GetActiveUsersOrderByCreatedOnAsync();

        /// <summary>
        /// Получение пользователей старше определённого возраста (если дата рождения не указана, то пользователь игнорируется)
        /// </summary>
        /// <param name="age">возраст</param>
        /// <returns>Список моделей с данными UserWithoutPasswordDTO</returns>
        public Task<List<UserWithoutPasswordDTO>> GetUsersOverAsync(int age);

        /// <summary>
        /// Получение данных пользователя в виде модели ShortUserDTO по логину
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <returns>Данные пользователя в виде модели ShortUserDTO</returns>
        public Task<ShortUserDTO> GetUserShortDataByLoginAsync(string login);

        /// <summary>
        /// Изменение личных данных пользователя
        /// </summary>
        /// <param name="updateUser">изменённые данные пользователя</param>
        /// <param name="loginAuthUser">логин авторизированного пользователя, который производит изменения</param>
        /// <returns>
        /// true - данные пользователя изменены успешно;
        /// flase - при изменении даных произошла ошибка;
        /// </returns>
        public Task<bool> UpdateUserAsync(UpdateUserDTO updateUser, string loginAuthUser);

        /// <summary>
        /// Обновление логина пользователя
        /// </summary>
        /// <param name="oldLogin">старый логин</param>
        /// <param name="newLogin">новый логин</param>
        /// <param name="loginAuthUser">логин авторизированного пользователя, который производит изменения</param>
        /// <returns>
        /// true - логин пользователя изменён успешно;
        /// flase - при изменении логина произошла ошибка;
        /// </returns>
        public Task<bool> UpdateUserLogin(string oldLogin, string newLogin, string loginAuthUser);

        /// <summary>
        /// Получение данных для создания JWT-токена по новому логину пользователя
        /// </summary>
        /// <param name="newLogin">новый логин пользователя</param>
        /// <returns>
        /// Данные для создания токена в виде модели UserDataForClaimsDTO
        /// </returns>
        public Task<UserDataForClaimsDTO> GetDataForClaimsByNewLogin(string newLogin);

        /// <summary>
        /// Обновление пароля пользователя по логину
        /// </summary>
        /// <param name="updateUser">моделаь с данными для обновления</param>
        /// <param name="loginAuthUser">логин авторизованого пользователя</param>
        /// <returns>
        /// true - пароль пользователя изменён успешно;
        /// flase - при изменении пароля произошла ошибка;
        /// </returns>
        public Task<bool> UpdatePasswordByLoginAsync(UpdateUserPasswordDTO updateUser, string loginAuthUser);

        /// <summary>
        /// Полное удаление пользователя
        /// </summary>
        /// <param name="login">логин польователя</param>
        /// <returns>
        /// true - пользователь удалён успешно;
        /// flase - при удалении пользователя произошла ошибка;
        /// </returns>
        public Task<bool> DeleteUserHardAsync(string login);

        /// <summary>
        /// Мягкое удаление пользователя
        /// </summary>
        /// <param name="deleteUserLogin">логин удаляемого пользователя</param>
        /// <param name="loginAuthUser">логин авторизованного пользователя</param>
        /// <returns>
        /// true - пользователь удалён успешно;
        /// flase - при удалении пользователя произошла ошибка;
        /// </returns>
        public Task<bool> DeleteUserSoftAsync(string deleteUserLogin, string loginAuthUser);

        /// <summary>
        /// Восстановление пользователя после мягкого удаления
        /// </summary>
        /// <param name="recoveryUserLogin">логин пользователя, которого необходимо восстановить</param>
        /// <param name="loginAuthUser">логин авторизированного пользователя</param>
        /// <returns>
        /// true - пользователь восстановлен успешно;
        /// flase - при восстановлении пользователя произошла ошибка;
        /// </returns>
        public Task<bool> UserRecovery(string recoveryUserLogin, string loginAuthUser);
    }
}
