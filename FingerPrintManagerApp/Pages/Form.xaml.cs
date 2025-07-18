using AForge.Video.DirectShow;
using AForge.Video;
using FingerPrintManagerApp.Service;
using Newtonsoft.Json;
using SecuGen.SecuBSPPro.Windows;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using System.Net.Http;
using System.Text;

namespace FingerPrintManagerApp.Pages
{
    /// <summary>
    /// Interaction logic for Form.xaml
    /// </summary>
    public partial class Form : Page
    {
        private SecuBSPMx m_SecuBSP;
        private bool m_DeviceOpened;
        public VideoCaptureDevice videoSource;
        private ChallanModel queryModel;
        public Form()
        {
            InitializeComponent();
            UserName.Text = UserSession.Instance.CurrentUser.Profile.FirstName + " " + UserSession.Instance.CurrentUser.Profile.LastName;
            //this.Unloaded += Form_Unloaded;
        }

        private void Form_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (videoSource == null)
                    {
                        videoSource.SignalToStop();
                        videoSource.NewFrame -= Video_NewFrame;
                    }
                });
            }
            catch (Exception ex) { }
        }

        private void ShowStatusBlock(bool isSuccess, string statusMsg)
        {
            string color = isSuccess ? "#79ba55" : "#d06a6a";

            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusBlock.Visibility = Visibility.Visible;
                StatusBlock.BorderBrush = (Brush)new BrushConverter().ConvertFromString(color);
                StatusLabel.Text = statusMsg;
            });

            Task.Run(() =>
            {
                Thread.Sleep(3000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusBlock.Visibility = Visibility.Collapsed;
                });
            });
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
        private void EnterPress_Handler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SearchBtn_Click(sender, e);
        }
        private void SelfRadioBtn_Checked(object s, EventArgs e)
        {
            if (AuthField1 != null)
                AuthField1.Visibility = Visibility.Collapsed;
            if (AuthField2 != null)
                AuthField2.Visibility = Visibility.Collapsed;
            if (AuthField3 != null)
                AuthField3.Visibility = Visibility.Collapsed;
            if (AuthField4 != null)
                AuthField4.Visibility = Visibility.Collapsed;
        }
        private void AuthRadioBtn_Checked(object s, EventArgs e)
        {
            AuthField1.Visibility = Visibility.Visible;
            AuthField2.Visibility = Visibility.Visible;
            AuthField3.Visibility = Visibility.Visible;
            AuthField4.Visibility = Visibility.Visible;
        }
        private void SearchBtn_Click(object o, EventArgs e)
        {
            try
            {
                ProgressBarVisibilty(true);
                if (String.IsNullOrWhiteSpace(SearchText.Text)) throw new Exception("Enter a valid number in search field");
                string searchType = "CHALLAN_NO";
                string searchText = SearchText.Text;

                if ((bool)CnicNoRadio.IsChecked) searchType = CnicNoRadio.Content.ToString();
                if ((bool)BookingIdRadio.IsChecked) searchType = BookingIdRadio.Content.ToString();

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string apiResponse = APIService.PostWebRequest("https://annual.usindh.edu.pk/api/enquiry/getChallan", JsonConvert.SerializeObject(new { challan_no = searchText, search_by = searchType }));

                        if (apiResponse == null) throw new Exception("No records found");

                        ChallanModel model = JsonConvert.DeserializeObject<ChallanModel>(apiResponse);

                        if (model == null) throw new Exception("No records found");

                        if (model.BookingDetail == null) throw new Exception("No booking details found");

                        if (!model.BookingDetail.CertificateDesc.Contains("DEGREE CERTIFICATE")) throw new Exception("Invalid document: "+model.BookingDetail.CertificateDesc);

                        if (model.BookingDetail.DeliveryDate > DateTime.Now) throw new Exception("Please wait till: " + model.BookingDetail.DeliveryDate.ToShortDateString());

                        queryModel = model;
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ChallanInformation.Text = "Name:\t\t"+model.ChallanRec.Name  +
                                "\r\nS/o:\t\t" + model.ChallanRec.Fname +
                                "\r\nNIC:\t\t" + model.ChallanRec.CnicNo +
                                "\r\nDelivery Date:\t" +model.BookingDetail.DeliveryDate.ToLongDateString() +
                                "\r\nBooking Date:\t" +model.BookingDetail.BookingDate.ToLongDateString() +
                                "\r\nSeat No:\t\t" +model.BookingDetail.SeatNo +
                                "\r\nYear:\t\t" +model.ChallanRec.ExamYear +
                                "\r\nProgram:\t" +model.BookingDetail.ProgramTitle;
                        }));
                        ConnectDevices();
                        ProgressBarVisibilty(false);
                    }
                    catch (Exception ex)
                    {
                        ShowStatusBlock(false, ex.Message);
                        ProgressBarVisibilty(false);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowStatusBlock(false, ex.Message);
                ProgressBarVisibilty(false);
            }
        }
        private void ConnectDevices()
        {
            InitializeFingerprintDevice();
            StartWebcam();
        }
        #region Fingerprint Code
        private void InitializeFingerprintDevice()
        {
            try
            {
                m_SecuBSP = new SecuBSPMx();
                m_DeviceOpened = false;
            }
            catch { }
        }
        private async void CaptureBtn_Click(object sender, RoutedEventArgs e)
        {
            this.scanStatus.Visibility = Visibility.Visible;
            await CaptureFingerprint();
        }
        async private Task CaptureFingerprint()
        {
            string statusMsg = "";
            bool isSuccess = true;
            await Task.Run(() =>
            {
                BSPError err;
                BSPInitInfo initInfo = new BSPInitInfo();
                InitializeFingerprintDevice();
                err = m_SecuBSP.GetInitInfo(initInfo);

                m_SecuBSP.DeviceID = 0x00FF;  // Auto-detect device
                if (!m_DeviceOpened) err = m_SecuBSP.OpenDevice();
                else err = BSPError.ERROR_NONE;

                if (err == BSPError.ERROR_NONE)
                {
                    m_DeviceOpened = true;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CaptureBtn.Text = "Connected";
                        ConnectBubble.Background = (Brush)new BrushConverter().ConvertFromString("#79ba55");
                    });
                    m_SecuBSP.EnableAuditData = true;
                    m_SecuBSP.SetFIRFormat(FIRFormat.STANDARDPRO);

                    m_SecuBSP.CaptureWindowOption.WindowStyle = 1;

                    err = m_SecuBSP.Capture(FIRPurpose.VERIFY);

                    if (err == BSPError.ERROR_NONE)
                    {
                        string capturedFingerprintData = m_SecuBSP.AuditFIRTextData;

                        statusMsg = "Fingerprint captured successfully!";

                        err = m_SecuBSP.ExportAuditData(capturedFingerprintData);
                        string firForDb = m_SecuBSP.FIRTextData;
                        if (err == BSPError.ERROR_NONE)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DisplayFingerprintImage(ConvertHexStringToBytes(m_SecuBSP.FIRImageData.ImageData), m_SecuBSP.FIRImageData.ImageWidth, m_SecuBSP.FIRImageData.ImageHeight);
                            });
                        }

                        // Process or store capturedFingerprintData as needed
                        Console.WriteLine(capturedFingerprintData);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CaptureBtn.Text = "  Connect";
                            ConnectBubble.Background = (Brush)new BrushConverter().ConvertFromString("#CCCCCC");
                        });
                    }
                    else
                    {
                        statusMsg = "Error capturing fingerprint: " + err.ToString();
                        isSuccess = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CaptureBtn.Text = "  Connect";
                            ConnectBubble.Background = (Brush)new BrushConverter().ConvertFromString("#CCCCCC");
                        });
                    }
                }
                else
                {
                    statusMsg = "Error opening device. Please make sure the device is conntect. Error#: " + err.ToString();
                    isSuccess = false;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowStatusBlock(isSuccess, statusMsg);
                });
            });
            this.scanStatus.Visibility = Visibility.Hidden;
            ShowStatusBlock(isSuccess, statusMsg);
        }
        private byte[] ConvertHexStringToBytes(FINGER_DATA_STRUCT[] data)
        {
            int length = data[0].Sample1.Length;
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i += 2)
            {
                bytes[i] = data[0].Sample1[i];
            }

            return bytes;
        }
        private void DisplayFingerprintImage(byte[] fingerprintImage, int width, int height)
        {
            var bitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Gray8, null);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), fingerprintImage, width, 0);
            FingerprintImage.Source = bitmap;

        }
        #endregion

        #region Webcam code
        /// <summary>
        /// Web Cam
        /// </summary>
        /// 

        private void StartWebcam()
        {

            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);
                videoSource.Start();
                ShowStatusBlock(true, "Camera is connected successfully");

            }
            else
            {
                ShowStatusBlock(false, "No webcam detected.");
            }
        }


        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            BitmapImage bitmapImage = BitmapToImageSource(frame);
            try
            {
                if (Application.Current != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WebcamImage.Source = bitmapImage;
                    });
            }
            catch { }
        }


        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Freeze is needed for cross-thread updates
                return bitmapImage;
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchText.Text.Length == 0)
            {
                ShowStatusBlock(false, "Enter a search value.");
                return;
            }

            if (ReceiverPic.Source == null && ReceiverCnicPic.Source == null && FingerprintImage.Source == null)
            {
                ShowStatusBlock(false, "Fill required fields!");
                return;
            }

            try
            {
                // Convert image sources to base64 (for biometric API)
                string rightThumb = ImageSourceToBase64(AuthNicPic.Source);
                string rightIndex = ImageSourceToBase64(AuthLetterPic.Source);
                string rightMiddle = ImageSourceToBase64(ReceiverPic.Source);
                string leftThumb = ImageSourceToBase64(ReceiverCnicPic.Source);
                string leftIndex = ImageSourceToBase64(WebcamImage.Source);
                string leftMiddle = ImageSourceToBase64(FingerprintImage.Source);

                // Get user info
                var user = UserSession.Instance.CurrentUser.Profile;

                // ---------------------
                // 1️⃣ Call biometric API
                // ---------------------
                var biometricPayload = new
                {
                    QUERY_TYPE = "UPDATE",
                    NAME = queryModel.ChallanRec.Name,
                    FNAME = queryModel.ChallanRec.Fname,
                    CNIC_NO = queryModel.ChallanRec.CnicNo,
                    CNIC_ISSUE_DATE = queryModel.ChallanRec.Date, // Static or fetch from data source
                    ADDRESS = queryModel.ChallanRec.ChallanId,
                    RIGHT_THUMB = rightThumb,
                    RIGHT_INDEX = rightIndex,
                    RIGHT_MIDDLE = rightMiddle,
                    LEFT_THUMB = leftThumb,
                    LEFT_INDEX = leftIndex,
                    LEFT_MIDDLE = leftMiddle,
                    RECORD_DATE = DateTime.Now.ToString("dd-MM-yyyy")
                };

                // ---------------------
                // 2️nd Call challan upload API
                // ---------------------
                var challanUploadPayload = new
                {
                    CHALLAN_NO = SearchText.Text,
                    STATUS_ID = "1", // Adjust as needed
                    STATUS_DATE = DateTime.Now.ToString("yyyy-MM-dd"),
                    CERT_NO = "DEGREE-CERT-001", // Replace with actual cert number if available
                    RECEIVER_NAME = (AuthPersonNameText.Text),
                    RECEIVER_CNIC_NO = AuthPersonCNICText,
                    DELIVERED_BY = UserName.Text ?? "SYSTEM_USER" // Or fetch from session
                };

                using (var client = new HttpClient())
                {
                    // Send biometric data
                    var biometricContent = new StringContent(JsonConvert.SerializeObject(biometricPayload), Encoding.UTF8, "application/json");
                    var bioResponse = await client.PostAsync("https://itsc.usindh.edu.pk/sac/api/biometric_profile", biometricContent);

                    // Send challan delivery info
                    var challanContent = new StringContent(JsonConvert.SerializeObject(challanUploadPayload), Encoding.UTF8, "application/json");
                    var challanResponse = await client.PostAsync("https://annual.usindh.edu.pk/api/enquiry/uploadChallan", challanContent);

                    if (bioResponse.IsSuccessStatusCode && challanResponse.IsSuccessStatusCode)
                    {
                        ShowStatusBlock(true, "Data and delivery info uploaded successfully!");
                    }
                    else
                    {
                        ShowStatusBlock(false, "One or more API calls failed.");
                    }
                }

            }
            catch (Exception ex)
            {
                ShowStatusBlock(false, "Error: " + ex.Message);
            }
            finally
            {
                // Reset form
                ChallanInformation.Text = "";
                SearchText.Text = "";

                AuthNicPic.Source = null;
                AuthLetterPic.Source = null;
                ReceiverPic.Source = null;
                ReceiverCnicPic.Source = null;
                WebcamImage.Source = null;
                FingerprintImage.Source = null;

                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                }
            }
        }
        private string ImageSourceToBase64(ImageSource imageSource)
        {
            if (imageSource == null) return null;

            var bitmapSource = imageSource as BitmapSource;
            if (bitmapSource == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder(); // Use PNG or JPEG
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        //private void SaveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SearchText.Text.Length == 0) return;

        //    if (ReceiverPic.Source == null && ReceiverCnicPic.Source == null && FingerprintImage.Source == null)
        //    {
        //        ShowStatusBlock(false, "Fill required fields!");
        //        return;
        //    }

        //    ShowStatusBlock(true, "Data is saved successfully!");
        //    ChallanInformation.Text = "";
        //    SearchText.Text = "";

        //    AuthNicPic.Source = null;
        //    AuthLetterPic.Source =  null;
        //    ReceiverPic.Source =  null;
        //    ReceiverCnicPic.Source = null;
        //    WebcamImage.Source = null;
        //    FingerprintImage.Source = null;
        //    videoSource.SignalToStop();

        //}
        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebcamImage.Source != null)
            {
                //SaveImage((BitmapImage)WebcamImage.Source);
                byte[] imageBytes = SaveImageAsBytes();

                StoreImageInDatabase(imageBytes, "REVEIVER_PHOTO");
                ShowStatusBlock(true, "Image Captured!");
            }
        }
        private void CaptureCnicButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebcamImage.Source != null)
            {
                //SaveImage((BitmapImage)WebcamImage.Source);
                byte[] imageBytes = SaveImageAsBytes();

                StoreImageInDatabase(imageBytes, "REVEIVER_CNIC_PHOTO");
                ShowStatusBlock(true, "Image Captured!");
            }
        }
        private void CaptureAuthCnicButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebcamImage.Source != null)
            {
                byte[] imageBytes = SaveImageAsBytes();

                StoreImageInDatabase(imageBytes, "AUTH_CNIC_PIC");
                ShowStatusBlock(true, "CNIC Captured!");
            }
        }
        private void CaptureAuthLetterButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebcamImage.Source != null)
            {
                byte[] imageBytes = SaveImageAsBytes();

                StoreImageInDatabase(imageBytes, "AUTH_LATTER_PIC");
                ShowStatusBlock(true, "Auth letter Captured!");
            }
        }

        private void SaveImageToFiles(BitmapImage bitmapImage)
        {
            // Save the captured image to a file
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var fileStream = new FileStream("CapturedImage.png", FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        private byte[] SaveImageAsBytes()
        {
            // Capture the current frame from the webcam and convert it to byte array
            BitmapImage bitmapImage = (BitmapImage)WebcamImage.Source;

            if (bitmapImage != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // Use any format you prefer (PNG/JPEG)
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(memoryStream);
                    return memoryStream.ToArray(); // Return the byte array of the image
                }
            }

            return null;
        }

        private void StoreImageInDatabase(byte[] imageBytes, string imageFor) // 1.REVEIVER_PHOTO 2.AUTH_CNIC_PIC 3.AUTH_LATTER_PIC
        {
            if (imageFor == "AUTH_CNIC_PIC") AuthNicPic.Source = WebcamImage.Source.Clone();
            else if (imageFor == "AUTH_LATTER_PIC") AuthLetterPic.Source = WebcamImage.Source.Clone();
            else if (imageFor == "REVEIVER_PHOTO") ReceiverPic.Source = WebcamImage.Source.Clone();
            else if (imageFor == "REVEIVER_CNIC_PHOTO") ReceiverCnicPic.Source = WebcamImage.Source.Clone();
            // Example method to store image bytes in the database
            // You will need to adjust the connection string and the table/column names
        }
        #endregion

    }
}
