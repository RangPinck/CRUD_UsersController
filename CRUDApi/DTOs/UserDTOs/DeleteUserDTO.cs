using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CRUDApi.DTOs.UserDTOs
{
    public class DeleteUserDTO
    {
        [Required(ErrorMessage = "Login is not specified")]
        [Display(Name = "Login")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "The deletion type is not specified")]
        [Display(Name = "SoftDelete")]
        [DefaultValue(true)]
        public bool SoftDelete { get; set; } = true;
    }
}
