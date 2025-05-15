using CRUDApi.DTOs.ErrorDTOs;
using CRUDApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CRUDApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IHealthRepository _repository;

        public HealthController(IHealthRepository repository) => _repository = repository;

        [HttpGet("health")]
        [SwaggerOperation(Summary = "Проверка состояния API (подключения к базе данных)")]
        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> CheckHealth()
        {
            var connection = await _repository.CheckDatabaseConnectionAsync();

            if (connection)
            {
                return Ok();
            }
            else
            {
                return StatusCode(503, new ErrorResponseDTO()
                {
                    Error = "Database Unavailable",
                    Message = "Database is currently unavailable. Please try again later."
                });
            }
        }
    }
}
