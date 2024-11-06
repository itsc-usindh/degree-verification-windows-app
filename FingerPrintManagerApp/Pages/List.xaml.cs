using FingerPrintManagerApp.Model;
using FingerPrintManagerApp.Service;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FingerPrintManagerApp.Pages
{
    /// <summary>
    /// Interaction logic for List.xaml
    /// </summary>
    public partial class List : Page
    {
        public ObservableCollection<UserListModel> UserRecords { get; set; }
        public List()
        {
            InitializeComponent();
            UserRecords = new ObservableCollection<UserListModel>
            {
                new UserListModel { Name = "Aimal Jan", FatherName = "Qureshi", CNIC = "12345-6789012-3", ChallanNo = "CH1234", Date = DateTime.Now, Status = "Pending" },
                new UserListModel { Name = "Ali Ahmed", FatherName = "Hassan", CNIC = "98765-4321098-7", ChallanNo = "CH5678", Date = DateTime.Now.AddDays(-1), Status = "Approved" },
                new UserListModel { Name = "Sara Khan", FatherName = "Akbar", CNIC = "65432-1098765-4", ChallanNo = "CH9101", Date = DateTime.Now.AddDays(-2), Status = "Pending" },
                new UserListModel { Name = "Zainab Shah", FatherName = "Khalid", CNIC = "56789-0123456-8", ChallanNo = "CH1122", Date = DateTime.Now.AddDays(-3), Status = "Rejected" }
            };

            UserRecordsGrid.ItemsSource = UserRecords;

            UserName.Text = UserSession.Instance.CurrentUser.Profile.FirstName + " " + UserSession.Instance.CurrentUser.Profile.LastName;
        }
        private void EnterPress_Handler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SearchBtn_Click(sender, e);
        }
        private void SearchBtn_Click(object o, EventArgs e)
        {
            try
            {
                ProgressBarVisibilty(true);
                if (String.IsNullOrWhiteSpace(SearchText.Text)) throw new Exception("Enter a valid number in search field");
                string searchType = "CHALLAN_NO";
                string searchText = SearchText.Text;

                if ((bool)CnicNoRadio_List.IsChecked) searchType = CnicNoRadio_List.Content.ToString();
                if ((bool)BookingIdRadio_List.IsChecked) searchType = BookingIdRadio_List.Content.ToString();

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string apiResponse = APIService.PostWebRequest("https://annual.usindh.edu.pk/api/enquiry/getChallan", JsonConvert.SerializeObject(new { challan_no = searchText, search_by = searchType }));

                        if (apiResponse == null) throw new Exception("No records found");

                        ChallanModel model = JsonConvert.DeserializeObject<ChallanModel>(apiResponse);

                        if (model == null) throw new Exception("No records found");

                        if (model.BookingDetail == null) throw new Exception("No booking details found");

                        if (model.BookingDetail.CertificateDesc != "DEGREE CERTIFICATE") throw new Exception("Invalid document: "+model.BookingDetail.CertificateDesc);

                        if (model.BookingDetail.DeliveryDate > DateTime.Now) throw new Exception("Please wait till: " + model.BookingDetail.DeliveryDate.ToShortDateString());

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            // TABLE LOGIC HERE.....

                            //ChallanInformation.Text = "Name:\t\t"+model.ChallanRec.Name  +
                            //    "\r\nS/o:\t\t" + model.ChallanRec.Fname +
                            //    "\r\nNIC:\t\t" + model.ChallanRec.CnicNo +
                            //    "\r\nDelivery Date:\t" +model.BookingDetail.DeliveryDate.ToLongDateString() +
                            //    "\r\nBooking Date:\t" +model.BookingDetail.BookingDate.ToLongDateString() +
                            //    "\r\nSeat No:\t\t" +model.BookingDetail.SeatNo +
                            //    "\r\nProgram:\t" +model.BookingDetail.ProgramTitle;
                        }));
                        ProgressBarVisibilty(false);
                    }
                    catch (Exception ex)
                    {
                        ProgressBarVisibilty(false);
                    }
                });
            }
            catch (Exception ex)
            {
                ProgressBarVisibilty(false);
            }
        }
        private void ProgressBarVisibilty(bool visible = true)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressBar.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
                });
            });
        }
        private void OnViewCommandMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is UserListModel userRecord)
            {
                userRecord.ViewCommand?.Execute(null);
            }
        }
    }
}
