using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CRUDApi.DTOs.UserDTOs
{
    public class RegistrationDTO
    {
        [Required(ErrorMessage = "Login is not specified")]
        [Display(Name = "Login")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Password is not specified")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Name is not specified")]
        [Display(Name = "Name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Gender is not specified")]
        [Display(Name = "Gender")]
        [DefaultValue(2)]
        public int Gender { get; set; }

        [Display(Name = "Birthday")]
        [DefaultValue(null)]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "It is not specified whether the user is an administrator.")]
        [Display(Name = "Admin")]
        [DefaultValue(false)]
        public bool Admin { get; set; }
    }
}
