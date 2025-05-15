using System.ComponentModel.DataAnnotations;

namespace CRUDApi.DTOs.UserDTOs
{
    public class UpdateUserPasswordDTO
    {
        [Required(ErrorMessage = "Login is not specified")]
        [Display(Name = "Login")]
        public string Login { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match")]
        [Display(Name = "ConfirmPassword")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
