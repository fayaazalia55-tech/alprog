using EvacuationDashboard.Views;

namespace EvacuationDashboard
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Membungkus LoginPage dalam NavigationPage agar bisa pindah antar halaman
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}