using System.Windows;
using System.Windows.Media;
using FingerPrintManagerApp.Pages;
using FingerPrintManagerApp.Service;

namespace FingerPrintManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Form formDegreeVarificationPage;
        List listStudentsPage;
        private readonly string ACTIVE_COLOR = "#eeeeee";
        private readonly string INACTIVE_COLOR = "#ffffff";
        public MainWindow()
        {
            InitializeComponent();
            formDegreeVarificationPage = new Form();
            listStudentsPage = new List();
            PageArea.Content = formDegreeVarificationPage;
            FormPageBtn.Background = (Brush)new BrushConverter().ConvertFromString(ACTIVE_COLOR);
            this.Closing +=MainClosing_Handler;
        }

        private void MainClosing_Handler(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            formDegreeVarificationPage.videoSource.SignalToStop();
        }
        private void FormButn_Handler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PageArea.Content = formDegreeVarificationPage;
            FormPageBtn.Background = (Brush)new BrushConverter().ConvertFromString(ACTIVE_COLOR);
            ListPageBtn.Background = (Brush)new BrushConverter().ConvertFromString(INACTIVE_COLOR);
        }
        private void ListButn_Handler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PageArea.Content = listStudentsPage;
            ListPageBtn.Background = (Brush)new BrushConverter().ConvertFromString(ACTIVE_COLOR);
            FormPageBtn.Background = (Brush)new BrushConverter().ConvertFromString(INACTIVE_COLOR);
        }
        private void LogoutButn_Handler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UserSession.Instance.ClearUser();
            mainWindow.Close();
        }
    }
}
