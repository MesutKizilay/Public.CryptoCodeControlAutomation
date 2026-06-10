using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CryptoCodeControlAutomation.App
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        readonly string AllowedHost = new Uri("http://192.168.150.81:6868").Host;
        //readonly string AllowedHost = new Uri("http://10.11.2.154:5000").Host;
        //readonly string AllowedHost = new Uri("http://10.11.2.154:5000").Host;

        const string LoginPath = "/Auth/Login";
        //const string DashboardPath = "/Warehouses/Stocktaking";
        const string DashboardPath = "/";

        public MainPage()
        {
            InitializeComponent();
        }

        //private void OnCounterClicked(object? sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}

        void Browser_Navigating(object sender, WebNavigatingEventArgs e)
        {
            Loading.IsVisible = Loading.IsRunning = true;

            // Domain dışı linkleri sistem tarayıcısında aç (opsiyonel)
            if (Uri.TryCreate(e.Url, UriKind.Absolute, out var uri) && uri.Host != AllowedHost)
            {
                e.Cancel = true;
                Launcher.OpenAsync(uri);
            }
        }

        void Browser_Navigated(object sender, WebNavigatedEventArgs e)
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
            Refresher.IsRefreshing = false;

            //Console.WriteLine("e.url", e.Url);
            Debug.WriteLine("e.url: " + e.Url);

            if (IsDashboardUrl(e.Url) || IsLoginUrl(e.Url))
                ClearWebViewHistory();
        }

        void Refresher_Refreshing(object sender, EventArgs e) => Browser.Reload();

        // Android geri tuşu: WebView içinde geri
        protected override bool OnBackButtonPressed()
        {
            // Dashboard'dayken geri = uygulamayı kapat
            if (IsDashboardUrl(Browser?.Source?.ToString() ?? string.Empty))
            {
                Application.Current?.Quit();  // Android/Windows/Mac'te çalışır
                return true;
            }

            // Aksi halde WebView içinde geri git (varsa)
            if (Browser?.CanGoBack == true)
            {
                Browser.GoBack();
                return true;
            }

            return base.OnBackButtonPressed();
        }

        bool IsLoginUrl(string url)
        {
            var x = Uri.TryCreate(url, UriKind.Absolute, out var u) && u.AbsolutePath.StartsWith(LoginPath, StringComparison.OrdinalIgnoreCase);
            return x;
        }

        bool IsDashboardUrl(string url)
        {
            var x = Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.AbsolutePath.Equals(DashboardPath, StringComparison.OrdinalIgnoreCase) || u.AbsolutePath == "/Home/Homepage");
            return x;
        }

        void ClearWebViewHistory()
        {
#if ANDROID
            if (Browser?.Handler?.PlatformView is Android.Webkit.WebView androidWebView)
            {
                androidWebView.ClearHistory();
            }
#endif
        }
    }
}
