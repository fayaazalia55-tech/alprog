using System;
using Microsoft.Maui.Controls;
using EvacuationDashboard.Models;
using EvacuationDashboard.Services;

namespace EvacuationDashboard.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            // Mengunci otomatis agar default-nya memilih item pertama
            RolePicker.SelectedIndex = 0;
        }

        // Triggered oleh Tombol LOGIN
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string? selectedRole = RolePicker.SelectedItem?.ToString();
            string? username = UsernameEntry.Text?.Trim();
            string? password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(selectedRole) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Access Denied", "Please fill in all login fields.", "OK");
                return;
            }

            // 1. Cek Akun Hardcoded Lama Milikmu Terlebih Dahulu
            bool isHardcodedUser = false;

            if (selectedRole == "Maintenance Engineer" && username == "FayaTeknisi" && password == "Teknisi77")
            {
                isHardcodedUser = true;
            }
            else if (selectedRole == "Building Manager" && username == "FayaManager" && password == "Manager77")
            {
                isHardcodedUser = true;
            }

            if (isHardcodedUser)
            {
                // Teruskan ke Halaman Verifikasi Wajah
                await Navigation.PushAsync(new VerificationPage(username, selectedRole));
                return;
            }

            // 2. Cadangan: Cek ke SQLite jika yang masuk adalah Akun Baru
            var dbService = new DatabaseService();
            var dbConnection = dbService.GetConnection();

            var matchedUser = await dbConnection.Table<UserAccount>()
                .Where(u => u.Username == username && u.Password == password && u.Role == selectedRole)
                .FirstOrDefaultAsync();

            if (matchedUser != null)
            {
                await Navigation.PushAsync(new VerificationPage(matchedUser.Username, matchedUser.Role));
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid username, password, or role selection.", "Try Again");
            }
        }

        // Triggered oleh Tombol SIGN IN (Pindah ke Registrasi)
        private async void OnSignInClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistrationPage());
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            TogglePasswordBtn.Text = PasswordEntry.IsPassword ? "🙈" : "🐵";
        }
    }
}