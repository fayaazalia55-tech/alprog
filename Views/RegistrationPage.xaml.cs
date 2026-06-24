using System;
using System.IO;
using Microsoft.Maui.Controls;
using SQLite;
using OpenCvSharp;
using EvacuationDashboard.Models;
using EvacuationDashboard.Services;

namespace EvacuationDashboard.Views
{
    public partial class RegistrationPage : ContentPage
    {
        private SQLiteAsyncConnection _dbConnection;

        public RegistrationPage()
        {
            InitializeComponent();

            var dbService = new DatabaseService();
            _dbConnection = dbService.GetConnection();

            RolePicker.SelectedIndex = 0;
        }

        private async void OnRegisterAndCaptureClicked(object sender, EventArgs e)
        {
            string? selectedRole = RolePicker.SelectedItem?.ToString();
            string? username = RegUsernameEntry.Text?.Trim();
            string? password = RegPasswordEntry.Text;

            if (string.IsNullOrEmpty(selectedRole) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Registration Failed", "Please fill in all fields to register.", "OK");
                return;
            }

            if (username.Length < 5)
            {
                await DisplayAlert("Registration Failed", "Username must be at least 5 characters long!", "OK");
                return;
            }

            var newAccount = new UserAccount
            {
                Username = username,
                Password = password,
                Role = selectedRole,
                PhotoPath = "PENDING"
            };

            try
            {
                await _dbConnection.InsertAsync(newAccount);

                string folderDatabasePath = Path.Combine(FileSystem.AppDataDirectory, "FaceDatabase");
                if (!Directory.Exists(folderDatabasePath))
                {
                    Directory.CreateDirectory(folderDatabasePath);
                }

                // Menghitung jumlah file riil yang berawalan 1_ untuk menentukan urutan berikutnya
                int totalFileWajah = Directory.GetFiles(folderDatabasePath, "1_*.jpg").Length;
                int nomorFotoBerikutnya = totalFileWajah + 1;

                string fileName = $"1_{nomorFotoBerikutnya}.jpg";
                string fullPhotoPath = Path.Combine(folderDatabasePath, fileName);

                bool isCaptureSuccess = CaptureFaceInteraktifOpenCV(fullPhotoPath);

                if (isCaptureSuccess)
                {
                    newAccount.PhotoPath = fullPhotoPath;
                    await _dbConnection.UpdateAsync(newAccount);

                    await DisplayAlert("Registration Success", $"Account created successfully! Filename: {fileName}", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await _dbConnection.DeleteAsync(newAccount);
                    await DisplayAlert("Registration Canceled", "Camera closed without capturing. Registration aborted.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE constraint failed"))
                {
                    await DisplayAlert("Registration Failed", "This username is already taken. Please choose another username!", "OK");
                }
                else
                {
                    await DisplayAlert("Database Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
        }

        private bool CaptureFaceInteraktifOpenCV(string targetPath)
        {
            try
            {
                using (var videoCapture = new VideoCapture(0))
                using (var imageFrame = new Mat())
                {
                    if (!videoCapture.IsOpened()) return false;

                    string windowTitle = "Camera Preview - [SPACE] to Capture | [ESC] to Cancel";
                    Cv2.NamedWindow(windowTitle, WindowFlags.AutoSize);

                    bool hasCapturedAndSaved = false;

                    while (true)
                    {
                        videoCapture.Read(imageFrame);
                        if (imageFrame.Empty()) break;

                        Cv2.ImShow(windowTitle, imageFrame);

                        int keyPressed = Cv2.WaitKey(30);

                        if (keyPressed == 32) // SPACEBAR
                        {
                            Cv2.ImWrite(targetPath, imageFrame);
                            hasCapturedAndSaved = true;
                            break;
                        }

                        if (keyPressed == 27 || Cv2.GetWindowProperty(windowTitle, WindowPropertyFlags.Visible) < 1) // ESC
                        {
                            break;
                        }
                    }

                    Cv2.DestroyWindow(windowTitle);
                    return hasCapturedAndSaved;
                }
            }
            catch
            {
                return false;
            }
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            RegPasswordEntry.IsPassword = !RegPasswordEntry.IsPassword;
            ToggleRegPasswordBtn.Text = RegPasswordEntry.IsPassword ? "🙈" : "🐵";
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}