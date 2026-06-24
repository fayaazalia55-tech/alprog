using SQLite;

namespace EvacuationDashboard.Models
{
    [Table("UserAccount")]
    public class UserAccount
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Nomor urut otomatis, akan dipakai sebagai ID unik oleh OpenCV

        [Unique, NotNull]
        public string Username { get; set; } = string.Empty; // Input teks nama untuk login [cite: 125, 143]

        [NotNull]
        public string Password { get; set; } = string.Empty; // Kata sandi pengguna [cite: 125, 143]

        [NotNull]
        public string Role { get; set; } = string.Empty; // Berisi string "Maintenance" atau "Building Manager" [cite: 125, 143]

        [NotNull]
        public string PhotoPath { get; set; } = string.Empty; // Lokasi penyimpanan file gambar "1_ID_Nama.jpg" di laptop
    }
}