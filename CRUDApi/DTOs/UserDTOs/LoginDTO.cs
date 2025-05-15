using System.ComponentModel.DataAnnotations;

namespace CRUDApi.DTOs.UserDTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Login is not specified")]
        [Display(Name = "Login")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Password is not specified")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
