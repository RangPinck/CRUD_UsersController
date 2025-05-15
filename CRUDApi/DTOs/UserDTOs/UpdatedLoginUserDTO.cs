namespace CRUDApi.DTOs.UserDTOs
{
    public class UpdatedLoginUserDTO
    {
        public UserWithoutPasswordDTO Metadata { get; set; } = null!;

        public string? Token { get; set; }
    }
}
