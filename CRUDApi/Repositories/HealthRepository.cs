using CRUDApi.Interfaces;
using CRUDApi.Models;

namespace CRUDApi.Repositories
{
    public class HealthRepository : IHealthRepository
    {
        private readonly CrudApiDbContext _context;

        public HealthRepository(CrudApiDbContext context) => _context = context;

        public async Task<bool> CheckDatabaseConnectionAsync()
        {
            try
            {
                var connect = await _context.Database.CanConnectAsync();
                return connect;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
