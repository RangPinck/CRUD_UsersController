namespace CRUDApi.DTOs.UserDTOs
{
    public class UserWithoutPasswordDTO
    {
        public Guid Guid { get; set; }

        public string Login { get; set; } = null!;

        public string Name { get; set; } = null!;

        public int Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public bool Admin { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime RevokedOn { get; set; }

        public string RevokedBy { get; set; }
    }
}
