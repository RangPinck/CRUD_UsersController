namespace CRUDApi.DTOs.UserDTOs
{
    public class ShortUserDTO
    {
        public string Name { get; set; } = null!;

        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public bool ActiveStatus { get; set; }
    }
}
