using EvacuationDashboard.Models;
using EvacuationDashboard.Models;
using SQLite;

namespace EvacuationDashboard.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _connection;

        public DatabaseService()
        {
            // Membuat atau membuka file database bernama "EvakuasiGedung.db3" di folder aman lokal laptop
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "EvakuasiGedung.db3");
            _connection = new SQLiteAsyncConnection(dbPath);

            // Otomatis membuat tabel UserAccount jika tabel tersebut belum ada di komputer
            _connection.CreateTableAsync<UserAccount>().Wait();
        }

        public SQLiteAsyncConnection GetConnection()
        {
            return _connection;
        }
    }
}