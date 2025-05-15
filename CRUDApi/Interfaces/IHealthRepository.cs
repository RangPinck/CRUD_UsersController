namespace CRUDApi.Interfaces
{
    public interface IHealthRepository
    {
        /// <summary>
        /// Метод проверки подключения API к базе данных
        /// </summary>
        /// /// <returns>
        /// true - подключение к базе данных есть;
        /// flase - подключения к базе данных нет;
        /// </returns>
        public Task<bool> CheckDatabaseConnectionAsync();
    }
}
