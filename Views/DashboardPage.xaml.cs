using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using EvacuationDashboard;

namespace EvacuationDashboard.Views
{
    public partial class DashboardPage : ContentPage
    {
        private string _username;
        private string _role;
        private Button _activeTabButton;
        private GeminiService _geminiService = new GeminiService();

        public DashboardPage(string username, string role)
        {
            InitializeComponent();
            _username = username ?? "User";
            _role = role ?? "Guest";

            UserLabel.Text = $"Welcome, {_username}!";
            UserRoleLabel.Text = $"Access Level: {_role}";

            _activeTabButton = TabTab1;

            if (_role == "Maintenance Engineer")
            {
                MaintenanceExtraPanel.IsVisible = true;
                ManagerExtraPanel.IsVisible = false;
            }
            else if (_role == "Building Manager")
            {
                MaintenanceExtraPanel.IsVisible = false;
                ManagerExtraPanel.IsVisible = true;
            }

            // Jalankan kondisi awal (Floor 1 aman secara fuzzy)
            EksekusiPerubahanDataLantai(26.8, 0.01, "Floor 1");
        }

        // KONTROL UTAMA: Mengatur perubahan angka telemetri sensor dan memicu kalkulasi Fuzzy Logic
        private void EksekusiPerubahanDataLantai(double dataSuhu, double dataAsap, string namaLantai)
        {
            ActiveFloorTitle.Text = $"Live Telemetry: {namaLantai}";

            // 1. HITUNG SECARA LIVE MENGGUNAKAN MATEMATIKA FUZZY LOGIC SUGENO 🧠
            var (riskPercentage, statusText, colorHex) = FuzzyEvaluator.JalankanAnalisisFuzzy(dataSuhu, dataAsap);

            // 2. UPDATE BANNER VISUAL UTAMA DASHBOARD
            FuzzyBanner.BackgroundColor = Color.FromHex(colorHex);
            FuzzyStatusLabel.Text = statusText;

            // 3. UPDATE TAMPILAN TEXT DATA SENSOR KASAR
            if (riskPercentage < 25.0)
            {
                SmokeLabel.Text = $"{dataAsap:F2} dB/m | Clean (🟢 SAFE)";
                SmokeLabel.TextColor = Color.FromHex("#2ECC71");
                TempLabel.Text = $"{dataSuhu:F1}°C (🟢 NORMAL)";
                TempLabel.TextColor = Color.FromHex("#2ECC71");
                IotStatusLabel.Text = "📶 48/48 Nodes Online (14ms Latency)";
                IotStatusLabel.TextColor = Color.FromHex("#2ECC71");
                HvacLabel.Text = "❄️ SYSTEM STANDBY (OFF)";
                HvacLabel.TextColor = Color.FromHex("#706573");
            }
            else if (riskPercentage >= 25.0 && riskPercentage < 75.0)
            {
                SmokeLabel.Text = $"{dataAsap:F2} dB/m | 95 ppm (⚠️ WARNING)";
                SmokeLabel.TextColor = Color.FromHex("#E67E22");
                TempLabel.Text = $"{dataSuhu:F1}°C (⚠️ HIGH TEMPERATURE)";
                TempLabel.TextColor = Color.FromHex("#E67E22");
                IotStatusLabel.Text = "⚠️ 47/48 Nodes Online (1 Node Burned Out)";
                IotStatusLabel.TextColor = Color.FromHex("#E67E22");
                HvacLabel.Text = "🌀 OVERPRESSURE FAN ACTIVE (🚨 ON)";
                HvacLabel.TextColor = Color.FromHex("#2ECC71");
            }
            else
            {
                SmokeLabel.Text = $"{dataAsap:F2} dB/m | CRITICAL SMOKE DENSITY (🚨 DANGER)";
                SmokeLabel.TextColor = Color.FromHex("#E74C3C");
                TempLabel.Text = $"{dataSuhu:F1}°C (🚨 EXTREME FIRE TEMPERATURE)";
                TempLabel.TextColor = Color.FromHex("#E74C3C");
                IotStatusLabel.Text = "❌ 42/48 Nodes Online (Mesh Network Degradation)";
                IotStatusLabel.TextColor = Color.FromHex("#E74C3C");
                HvacLabel.Text = "🌪️ EMERGENCY SMOKE EXTRACTION MODE (🚨 MAX)";
                HvacLabel.TextColor = Color.FromHex("#E74C3C");
            }
        }

        private void OnFloorTabClicked(object sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                _activeTabButton.BackgroundColor = Colors.White;
                _activeTabButton.TextColor = Color.FromArgb("#FF1493");
                _activeTabButton.BorderWidth = 1;

                clickedButton.BackgroundColor = Color.FromArgb("#FF1493");
                clickedButton.TextColor = Colors.White;
                clickedButton.BorderWidth = 0;
                _activeTabButton = clickedButton;

                string floorText = clickedButton.Text;

                switch (floorText)
                {
                    case "Floor 3":
                        // Skenario Bahaya Kebakaran Sedang (Memicu Status Standby secara Fuzzy)
                        EksekusiPerubahanDataLantai(54.2, 0.45, "Floor 3");
                        SprinklerLabel.Text = "3.8 BAR (💧 VALVE ENGAGED)";
                        SprinklerLabel.TextColor = Color.FromHex("#3498DB");
                        PowerLabel.Text = "100% Charged (🔋 AC Power Lost)";
                        PowerLabel.TextColor = Color.FromHex("#E67E22");
                        Log1Label.Text = "• [16:32] CRITICAL: Smoke density limit exceeded on Zone B Floor 3.";
                        Log1Label.TextColor = Color.FromHex("#E74C3C");
                        Log2Label.Text = "• [16:32] SYSTEM: Stairwell Pressurized Fan activated automatically.";
                        Log2Label.TextColor = Color.FromHex("#2ECC71");
                        OccupantsLabel.Text = "45 People";
                        GateLabel.Text = "🔒 LOCKED (Awaiting Evacuation Command)";
                        GateLabel.TextColor = Color.FromHex("#706573");
                        break;

                    case "Floor 5":
                        // Skenario Normal bawaan aslimu tapi ada masalah baterai
                        EksekusiPerubahanDataLantai(26.8, 0.01, "Floor 5");
                        SprinklerLabel.Text = "4.2 BAR (💧 READY)";
                        SprinklerLabel.TextColor = Color.FromHex("#3498DB");
                        BatteryLowTelemetry();
                        break;

                    default: // Floor 1, 2, dan 4 normal murni
                        EksekusiPerubahanDataLantai(26.8, 0.01, floorText);
                        SprinklerLabel.Text = "4.2 BAR (💧 READY)";
                        SprinklerLabel.TextColor = Color.FromHex("#3498DB");
                        PowerLabel.Text = "100% Charged (🔋 STANDBY)";
                        PowerLabel.TextColor = Color.FromHex("#2ECC71");
                        Log1Label.Text = "• [16:30] Info: Routine Battery & UPS Automation Test Completed.";
                        Log1Label.TextColor = Colors.Black;
                        Log2Label.Text = "• [14:15] Info: IoT Nodes Signal Calibration Drift Checked - Status Clear.";
                        Log2Label.TextColor = Colors.Gray;
                        OccupantsLabel.Text = "12 People";
                        GateLabel.Text = "🔒 LOCKED (Secured)";
                        GateLabel.TextColor = Color.FromHex("#706573");
                        break;
                }
            }
        }

        private void BatteryLowTelemetry()
        {
            PowerLabel.Text = "15% Capacity (⚠️ LOW BATTERY)";
            PowerLabel.TextColor = Color.FromHex("#E67E22");
            Log1Label.Text = "• [16:02] WARNING: UPS Node 41 Battery capacity dropped below 20%.";
            Log1Label.TextColor = Color.FromHex("#E67E22");
            OccupantsLabel.Text = "0 People";
        }

        private void OnSendChatClicked(object sender, EventArgs e)
        {
            string userText = ChatInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(userText)) return;

            AddChatBubble(userText, Colors.White, Color.FromArgb("#FF1493"), LayoutOptions.End);
            ChatInput.Text = string.Empty;

            Task.Run(async () =>
            {
                string botResponse;
                try
                {
                    botResponse = await _geminiService.TanyaCopilot(userText);
                }
                catch (Exception ex)
                {
                    botResponse = $"❌ Sistem Eror di Dalam: {ex.Message}";
                }

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    AddChatBubble(botResponse, Color.FromArgb("#FFF5F7"), Colors.Black, LayoutOptions.Start);
                    await Task.Delay(150);
                    await ChatScrollView.ScrollToAsync(0, ChatHistoryLayout.Height, true);
                });
            });
        }

        private void OnChatInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue) && (e.NewTextValue.EndsWith("\n") || e.NewTextValue.EndsWith("\r")))
            {
                OnSendChatClicked(sender, EventArgs.Empty);
            }
        }

        private void AddChatBubble(string text, Color bgColor, Color textColor, LayoutOptions alignment)
        {
            var bubble = new Border
            {
                BackgroundColor = bgColor,
                StrokeThickness = 0,
                Padding = new Thickness(10),
                HorizontalOptions = alignment,
                WidthRequest = 220,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) }
            };

            bubble.Content = new Label
            {
                Text = text,
                FontFamily = "GoogleSansRegular",
                FontSize = 12,
                TextColor = textColor
            };

            ChatHistoryLayout.Children.Add(bubble);
        }

        private async void OnTriggerAlarmClicked(object sender, EventArgs e)
        {
            // SKENARIO EKSTREM: Memaksa angka sensor naik ke tingkat katastrofik untuk memicu output Fuzzy Bahaya Akut (99.9%)
            EksekusiPerubahanDataLantai(78.5, 0.85, "ALL FLOORS (EVACUATION)");

            GateLabel.Text = "🔓 UNLOCKED (Evacuation Open)";
            GateLabel.TextColor = Color.FromHex("#E74C3C");

            await DisplayAlert("🚨 EMERGENCY ACTIVATED 🚨", "Main evacuation alarm has been triggered! All electronic access gates across all floors have been forced UNLOCKED.", "OK");
        }

        private async void OnSignOutClicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        private async void OnMaintenanceClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Hardware Calibration", "Initiating global IoT sensor mesh re-calibration. All telemetry data stream is successfully balanced!", "OK");
        }
    }
}