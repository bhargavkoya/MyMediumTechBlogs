using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace AsyncWPFExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private readonly HttpClient _httpClient = new HttpClient();

        private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Show loading indicator
                LoadingProgressBar.Visibility = Visibility.Visible;
                LoadDataButton.IsEnabled = false;

                //Asynchronous file operation
                var localData = await LoadLocalDataAsync();

                //Asynchronous web request
                var remoteData = await LoadRemoteDataAsync();

                //Update UI on main thread
                DataListBox.ItemsSource = localData.Concat(remoteData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
            finally
            {
                //Hide loading indicator
                LoadingProgressBar.Visibility = Visibility.Hidden;
                LoadDataButton.IsEnabled = true;
            }
        }

        private async Task<List<string>> LoadLocalDataAsync()
        {
            return await Task.Run(async () =>
            {
                //ConfigureAwait(false) since we're not updating UI here
                var content = await File.ReadAllTextAsync("data.txt")
                    .ConfigureAwait(false);
                return content.Split('\n').ToList();
            });
        }

        private async Task<List<string>> LoadRemoteDataAsync()
        {
            var response = await _httpClient.GetAsync("https://api.example.com/data")
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            return JsonSerializer.Deserialize<List<string>>(content);
        }
    }
}