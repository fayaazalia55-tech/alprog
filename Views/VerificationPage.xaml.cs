using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using EvacuationDashboard.Models;
using EvacuationDashboard.Services;

namespace EvacuationDashboard.Views
{
    public partial class VerificationPage : ContentPage
    {
        private string _username;
        private string _role;
        private FaceMatcher _faceMatcher;

        public VerificationPage(string username, string role)
        {
            InitializeComponent();
            _username = username;
            _role = role;

            _faceMatcher = new FaceMatcher();
            Task.Run(() => _faceMatcher.TrainSystem());
        }

        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        string localFilePath = photo.FullPath;

                        await Task.Delay(1000);

                        // Ambil prediksi dari FaceMatcher baru kita yang sudah distandardisasi
                        var (label, confidence) = _faceMatcher.PeriksaWajahSecaraPresisi(localFilePath);

                        // Angka batas aman standar setelah ukuran gambar distandardisasi
                        double batasKetatKita = 80.0;

                        if (label == 1 && confidence < batasKetatKita)
                        {
                            await DisplayAlert("Verification Success", "🟢 Face Verified! Access Granted. Welcome to the Command Center.", "OK");
                            await Navigation.PushAsync(new DashboardPage(_username, _role));
                        }
                        else
                        {
                            // MEMUNCULKAN DIAGNOSTIK KEMIRIPAN BIAR LU BISA LIHAT KINERJA OPENCV LU
                            string infoDiagnostik = $"Label: {label} | Score: {confidence:F1}";
                            await DisplayAlert("Access Denied", $"❌ Face Unrecognized!\n({infoDiagnostik})\nYou do not have permission to access this system.", "Try Again");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Kamera Error", "Kamera tidak didukung di perangkat ini.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Gagal mengambil gambar: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}