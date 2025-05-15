using CRUDApi.DTOs.UserDTOs;
using CRUDApi.Interfaces;
using CRUDApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace CRUDApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _repository;
        private readonly ITokenService _tokenService;

        public AccountController(IAccountRepository repository, ITokenService tokenService)
        {
            _repository = repository;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        /// <param name="request">данные для аутентификации</param>
        /// <returns>токен в виде строки</returns>
        [SwaggerOperation(Summary = "Аутентификация пользователя (Вход в систему)")]
        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login(LoginDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!await _repository.LoginIsExist(request.Login) || !await _repository.UserIsActiveAsync(request.Login))
                {
                    return BadRequest("User not found or deleted!");
                }

                var userDataForCliams = await _repository.CheckUserPasswodByLoginAsync(request.Login, request.Password);

                if (userDataForCliams is not null)
                {
                    return Ok(_tokenService.CreateToken(userDataForCliams));
                }
                else
                {
                    return BadRequest("Wrong password!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Создание пользователя по логину, паролю, имени, полу и дате рождения + указание будет ли пользователь админом (Доступно Админам)
        /// </summary>
        /// <param name="request">данные для создания пользователя</param>
        /// <returns>данные только что созданного пользователя в виде модели CreatedUserDTO</returns>
        [SwaggerOperation(Summary = "Создание пользователя")]
        [HttpPost("registration")]
        [ProducesResponseType(201, Type = typeof(CreatedUserDTO))]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreatedUserDTO>> Registration(RegistrationDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_repository.LoginIsCorrect(request.Login))
                {
                    return BadRequest("Login is not correct! All characters except Latin letters and numbers are prohibited!");
                }

                if (!_repository.PasswordIsCorrect(request.Password))
                {
                    return BadRequest("Password is not correct! All characters except Latin letters and numbers are prohibited!");
                }

                if (!_repository.NameIsCorrect(request.Name))
                {
                    return BadRequest("Name is not correct! All characters except Latin and Russian letters are prohibited!");
                }

                if (!_repository.GenderIsCorrect(request.Gender))
                {
                    return BadRequest("Name is not correct! 0 - female, 1 - male, 2 - unknown!");
                }

                if (await _repository.LoginIsExist(request.Login))
                {
                    return BadRequest("The user with this login already exists!");
                }

                request.Password = new PasswordHasher<User>().HashPassword(new User(), request.Password);

                var loginUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                if (!await _repository.CreateUserAsync(request, loginUser))
                {
                    return BadRequest("No correct data!");
                }

                var newUser = await _repository.GetUserDataAfterCreateByLoginAsync(request.Login);

                return Created($"api/account/{newUser.Guid}", newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Изменение имени, пола или даты рождения пользователя (Может менять Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <param name="userData">данные пользователя для обновления</param>
        /// <returns>Данные обновлённого пользователя в модели UserWithoutPasswordDTO</returns>
        [SwaggerOperation(Summary = "Изменение личных данных пользователя")]
        [HttpPut("update-user")]
        [ProducesResponseType(200, Type = typeof(UserWithoutPasswordDTO))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [Authorize]
        public async Task<ActionResult<UserWithoutPasswordDTO>> UpdateUser([FromBody] UpdateUserDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var loginUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var loginUserRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                if (!await _repository.UserIsActiveAsync(loginUser))
                {
                    return BadRequest("Loggined user is not active!");
                }

                if (!await _repository.LoginIsExist(request.Login))
                {
                    return BadRequest("User not found!");
                }

                if (loginUserRole == "Admin" && !await _repository.UserIsActiveAsync(request.Login))
                {
                    return BadRequest("User is not active!");
                }

                if (!string.IsNullOrEmpty(request.Name) && !_repository.NameIsCorrect(request.Name))
                {
                    return BadRequest("Name is not correct! All characters except Latin and Russian letters are prohibited!");
                }

                if (request.Gender != null && !_repository.GenderIsCorrect((int)request.Gender))
                {
                    return BadRequest("Name is not correct! 0 - female, 1 - male, 2 - unknown!");
                }

                if (loginUserRole == "Admin" || loginUser == request.Login)
                {
                    if (!await _repository.UpdateUserAsync(request, loginUser))
                    {
                        throw new Exception("The user could not be updated.");
                    }

                    return Ok(
                        await _repository.GetUserWithoutPasswordByLoginAsync(request.Login)
                        );
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Изменение логина (Логин может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn), логин должен оставаться уникальным)
        /// </summary>
        /// <param name="oldLogin">старый логин</param>
        /// <param name="newLogin">новый логин</param>
        /// <returns>
        /// Метаданные пользоватя и токен (если пользователь менял логин себе сам) в виде модели UpdatedLoginUserDTO
        /// </returns>
        [SwaggerOperation(Summary = "Изменение логина пользователя")]
        [HttpPut("update-login")]
        [ProducesResponseType(200, Type = typeof(UpdatedLoginUserDTO))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [Authorize]
        public async Task<ActionResult<UpdatedLoginUserDTO>> UpdateUserLogin(string oldLogin, string newLogin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var loginUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var loginUserRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                var loginUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!await _repository.UserIsActiveAsync(loginUser))
                {
                    return BadRequest("Loggined user is not active!");
                }

                if (loginUserRole == "Admin" && !await _repository.UserIsActiveAsync(oldLogin))
                {
                    return BadRequest("User is not active!");
                }

                if (!await _repository.LoginIsExist(oldLogin))
                {
                    return BadRequest("User not found!");
                }

                if (!_repository.LoginIsCorrect(newLogin))
                {
                    return BadRequest("New login is not correct! All characters except Latin letters and numbers are prohibited!");
                }

                if (await _repository.LoginIsExist(newLogin))
                {
                    return BadRequest("The user with youre new login already exists!");
                }

                if (loginUserRole == "Admin" || loginUser == oldLogin)
                {
                    if (oldLogin != newLogin)
                    {
                        if (!await _repository.UpdateUserLogin(oldLogin, newLogin, loginUser))
                        {
                            throw new Exception("The user could not be updated.");
                        }

                        if (loginUserRole == "Admin" && loginUser != oldLogin)
                        {
                            return Ok(
                           new UpdatedLoginUserDTO()
                           {
                               Metadata = await _repository.GetUserWithoutPasswordByLoginAsync(newLogin),
                               Token = null
                           });
                        }

                        var userDataForCliams = await _repository.GetDataForClaimsByNewLogin(newLogin);

                        return Ok(
                            new UpdatedLoginUserDTO()
                            {
                                Metadata = await _repository.GetUserWithoutPasswordByLoginAsync(newLogin),
                                Token = _tokenService.CreateToken(userDataForCliams)
                            });
                    }
                    else
                    {
                        return Ok();
                    }
                }
                else
                {
                    return Forbid();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Изменение пароля (Пароль может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <param name="request">данные для изменения пароля</param>
        /// <returns>строка с сообщением, что пароль обновлён</returns>
        [SwaggerOperation(Summary = "Изменение пароля пользователя")]
        [HttpPut("update-password")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [Authorize]
        public async Task<ActionResult<string>> UpdateUserPassword(UpdateUserPasswordDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var loginUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var loginUserRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                if (!await _repository.UserIsActiveAsync(loginUser))
                {
                    return BadRequest("Loggined user is not active!");
                }

                if (!await _repository.LoginIsExist(request.Login))
                {
                    return BadRequest("User not found!");
                }

                if (loginUserRole == "Admin" && !await _repository.UserIsActiveAsync(request.Login))
                {
                    return BadRequest("User is not active!");
                }

                if (!_repository.PasswordIsCorrect(request.Password))
                {
                    return BadRequest("Password is not correct! All characters except Latin letters and numbers are prohibited!");
                }

                if (!_repository.PasswordIsCorrect(request.ConfirmPassword))
                {
                    return BadRequest("ConfirmPassword is not correct! All characters except Latin letters and numbers are prohibited!");
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest("Passwords don't match");
                }

                if (loginUserRole == "Admin" || loginUser == request.Login)
                {
                    if (!await _repository.UpdatePasswordByLoginAsync(request, loginUser))
                    {
                        throw new Exception("The user password could not be updated.");
                    }

                    return Ok("Password update success!");
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Запрос списка всех активных (отсутствует RevokedOn) пользователей, список отсортирован по CreatedOn (Доступно Админам)
        /// </summary>
        /// <returns>Список активных пользователей в виде модели UserWithoutPasswordDTO</returns>
        [SwaggerOperation(Summary = "Получние списка активных пользователей пользователей")]
        [HttpGet("active-users")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UserWithoutPasswordDTO>))]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserWithoutPasswordDTO>>> GetActiveUsers()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(
                    await _repository.GetActiveUsersOrderByCreatedOnAsync()
                    );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Запрос пользователя по логину, в списке долны быть имя, пол и дата рождения статус активный или нет (Доступно Админам)
        /// </summary>
        /// <param name="login">логин пользователя</param>
        /// <returns>данные пользователя в виде модели ShortUserDTO</returns>
        [SwaggerOperation(Summary = "Получение данных пользователя по логину")]
        [HttpGet("user-short-data")]
        [ProducesResponseType(200, Type = typeof(ShortUserDTO))]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShortUserDTO>> GetUserShortDataByLogin([FromQuery] string login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_repository.LoginIsCorrect(login))
                {
                    return BadRequest("Login is not correct! All characters except Latin letters and numbers are prohibited!");
                }

                if (!await _repository.LoginIsExist(login))
                {
                    return BadRequest("User not found!");
                }

                return Ok(
                    await _repository.GetUserShortDataByLoginAsync(login)
                    );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Запрос пользователя по логину и паролю (Доступно только самому пользователю, если он активен(отсутствует RevokedOn))
        /// </summary>
        /// <param name="request">данные для аутентификации</param>
        /// <returns>Данные пользователя без пароля в виде модели UserWithoutPasswordDTO</returns>
        [SwaggerOperation(Summary = "Получение профиля пользователя")]
        [HttpGet("profile")]
        [ProducesResponseType(200, Type = typeof(UserWithoutPasswordDTO))]
        [ProducesResponseType(400)]
        [Authorize]
        public async Task<ActionResult<UserWithoutPasswordDTO>> GetProfile([FromQuery] LoginDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var loginUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                if (loginUser != request.Login)
                {
                    return BadRequest("The login of authorization and the provided login do not match!");
                }

                if (!await _repository.LoginIsExist(request.Login) || !await _repository.UserIsActiveAsync(request.Login))
                {
                    return BadRequest("User not found or deleted!");
                }

                if (await _repository.CheckUserPasswodByLoginAsync(request.Login, request.Password) is null)
                {
                    return BadRequest("Invalid password!");
                }

                return Ok(
                    await _repository.GetUserWithoutPasswordByLoginAsync(loginUser)
                    );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Запрос всех пользователей старше определённого возраста (Доступно Админам)
        /// </summary>
        /// <param name="age">возраст</param>
        /// <returns>Список пользователей старше указанного возраста в виде моделей UserWithoutPasswordDTO</returns>
        [SwaggerOperation(Summary = "Получение списка пользователей старше определённого возраста")]
        [HttpGet("user-oldes")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UserWithoutPasswordDTO>))]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserWithoutPasswordDTO>>> GetUserOverAge([FromQuery] int age = 10)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (age < 0 || age > 100)
                {
                    return BadRequest("No correct age!");
                }

                return Ok(
                    await _repository.GetUsersOverAsync(age)
                    );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Удаление пользователя по логину полное или мягкое (При мягком удалении должна происходить простановка RevokedOn и RevokedBy) (Доступно Админам)
        /// </summary>
        /// <param name="request">данные для удаления пользователя: логин и указание, что будет мягкое удаление</param>
        /// <returns>строка с типом и подтверждением удаления пользователя с указанным логином</returns>
        [SwaggerOperation(Summary = "Удаление пользователя")]
        [HttpDelete("delete")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> DeleteUser(DeleteUserDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var loginUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                if (!await _repository.LoginIsExist(request.Login))
                {
                    return BadRequest("User not found!");
                }

                bool result = false;
                string variant = "soft";
                string deletdLogin = request.Login;

                if (request.SoftDelete)
                {
                    if (!await _repository.UserIsActiveAsync(request.Login))
                    {
                        return Ok("Deleting user is not active!");
                    }

                    result = await _repository.DeleteUserSoftAsync(request.Login, loginUser);
                }
                else
                {
                    result = await _repository.DeleteUserHardAsync(request.Login);
                    variant = "hard";
                }

                if (!result)
                {
                    throw new Exception("The user has not been deleted.");
                }

                return Ok($"The {variant} removal user \"{deletdLogin}\" was successful!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Восстановление пользователя - Очистка полей(RevokedOn, RevokedBy) (Доступно Админам)
        /// </summary>
        /// <param name="login">Логин пользователя, которого надо восстановить</param>
        /// <returns>Строка с подтверждением восстановления пользователя из мягкого удаления</returns>
        [SwaggerOperation(Summary = "Восстановление пользователя из мягкого удаления")]
        [HttpPut("user-recovery")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UserRecovery(string login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_repository.LoginIsCorrect(login))
                {
                    return BadRequest("Login is not correct! All characters except Latin letters and numbers are prohibited!");
                }

                var loginUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

                if (!await _repository.LoginIsExist(login))
                {
                    return BadRequest("User not found!");
                }

                if (await _repository.UserIsActiveAsync(login))
                {
                    return Ok("User doesn't soft deleted.");
                }

                if (!await _repository.UserRecovery(login, loginUser))
                {
                    throw new Exception("The user has not been recovered.");
                }

                return Ok($"The user's recovery was successful!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
