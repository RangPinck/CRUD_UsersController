using CRUDApi.DTOs.UserDTOs;

namespace CRUDApi.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(UserDataForClaimsDTO data);
    }
}
