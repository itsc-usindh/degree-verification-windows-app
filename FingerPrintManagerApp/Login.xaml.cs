using FingerPrintManagerApp.Model;
using FingerPrintManagerApp.Service;
using Newtonsoft.Json;
using System.Windows;

namespace FingerPrintManagerApp
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void EnterPress_Handler(object sender, RoutedEventArgs e)
        {
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar(true);

            if (!ExecuteLogin(UsernameText.Text, PasswordText.Password))
            {
                ErrorTxt.Text = "Invalid user name or password";
                ErrorTxt.Visibility = Visibility.Visible;
                ProgressBar(false);
            }

        }
        private void ProceedLogin() =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
                ProgressBar(false);
            });

        private bool ExecuteLogin(string cnic, string password)
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    string endpoint = "https://itsc.usindh.edu.pk/sac/api/eservices/login";
                    string payload = JsonConvert.SerializeObject(new { cnic_no = cnic, password });
                    string res = APIService.PostWebRequest(endpoint, payload);

                    UserResponseModel model = JsonConvert.DeserializeObject<UserResponseModel>(res);
                    if (model.Success == null) throw new Exception("No data found");

                    UserSession.Instance.SetUser(model.Success);
                    ProceedLogin();
                });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void ProgressBar(bool show)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressBarLogin.Visibility = show ? Visibility.Visible : Visibility.Hidden;
                });
            });
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}
