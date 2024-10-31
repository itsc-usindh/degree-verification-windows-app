using AForge.Video.DirectShow;
using AForge.Video;
using SecuGen.SecuBSPPro.Windows;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Brush = System.Windows.Media.Brush;
using System.Windows.Input;

namespace FingerPrintManagerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SecuBSPMx m_SecuBSP;
        private bool m_DeviceOpened;
        private VideoCaptureDevice videoSource;

        public MainWindow()
        {
            InitializeComponent();
            mainWindow.Closing+=MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (videoSource != null)
            {
                videoSource.NewFrame -= new NewFrameEventHandler(Video_NewFrame);
                videoSource.SignalToStop();
            }
        }

        private void ConnectDevices()
        {
            InitializeFingerprintDevice();
            StartWebcam();
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
                        string apiResponse = PostWithWebRequest("https://annual.usindh.edu.pk/api/enquiry/getChallan", JsonConvert.SerializeObject(new { challan_no = searchText, search_by = searchType }));

                        if (apiResponse == null) throw new Exception("No records found");

                        ChallanModel model = JsonConvert.DeserializeObject<ChallanModel>(apiResponse);

                        if (model == null) throw new Exception("No records found");

                        if (model.BookingDetail == null) throw new Exception("No booking details found");

                        if (model.BookingDetail.DeliveryDate > DateTime.Now) throw new Exception("Please wait till: " + model.BookingDetail.DeliveryDate.ToShortDateString());

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ChallanInformation.Text = "Name:\t\t"+model.ChallanRec.Name  +
                                "\r\nS/o:\t\t" + model.ChallanRec.Fname +
                                "\r\nNIC:\t\t" + model.ChallanRec.CnicNo +
                                "\r\nDelivery Date:\t" +model.BookingDetail.DeliveryDate.ToLongDateString();
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

        #region Fingerprint Code
        private void InitializeFingerprintDevice()
        {
            m_SecuBSP = new SecuBSPMx();
            m_DeviceOpened = false;
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    WebcamImage.Source = bitmapImage;
                }
                catch { }
            });
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
            // Example method to store image bytes in the database
            // You will need to adjust the connection string and the table/column names
        }



        protected override void OnClosed(EventArgs e)
        {
            // Stop the webcam when the window is closed
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
            base.OnClosed(e);
        }
        #endregion

        #region API Service
        private static readonly HttpClient client = new HttpClient();

        public string PostWithWebRequest(string url, string jsonPayload)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                // Write JSON data to request body
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonPayload);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                // Get response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    return result;
                }
            }
            catch (WebException ex)
            {
                // Capture and log any issues
                ShowStatusBlock(false, $"Unable to fetch data (Retry): {ex.Message}");
                Console.WriteLine($"WebRequest error: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
