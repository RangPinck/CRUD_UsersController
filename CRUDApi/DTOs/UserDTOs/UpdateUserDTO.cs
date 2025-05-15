using System.ComponentModel.DataAnnotations;

namespace CRUDApi.DTOs.UserDTOs
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Login is not specified")]
        [Display(Name = "Login")]
        public string Login { get; set; } = null!;

        public string? Name { get; set; }

        public int? Gender { get; set; }

        public DateTime? Birthday { get; set; }
    }
}
