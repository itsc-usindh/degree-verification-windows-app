using AForge.Video.DirectShow;
using AForge.Video;
using SecuGen.SecuBSPPro.Windows;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

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
            InitializeFingerprintDevice();
            StartWebcam();
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
            await Task.Run(() =>
            {
                string status = "";
                BSPError err;
                BSPInitInfo initInfo = new BSPInitInfo();

                err = m_SecuBSP.GetInitInfo(initInfo);

                m_SecuBSP.DeviceID = 0x00FF;  // Auto-detect device
                if (!m_DeviceOpened) err = m_SecuBSP.OpenDevice();
                else err = BSPError.ERROR_NONE;

                if (err == BSPError.ERROR_NONE)
                {
                    m_DeviceOpened = true;

                    m_SecuBSP.EnableAuditData = true;
                    m_SecuBSP.SetFIRFormat(FIRFormat.STANDARDPRO);

                    m_SecuBSP.CaptureWindowOption.WindowStyle = 1;

                    err = m_SecuBSP.Capture(FIRPurpose.VERIFY);

                    if (err == BSPError.ERROR_NONE)
                    {
                        string capturedFingerprintData = m_SecuBSP.AuditFIRTextData;

                        status = "Fingerprint captured successfully!";

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
                    }
                    else
                    {
                        status = "Error capturing fingerprint: " + err.ToString();
                    }
                }
                else
                {
                    status = "Error opening device. Please make sure the device is conntect. Error#: " + err.ToString();
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusLabel.Content = status;
                });
            });
            this.scanStatus.Visibility = Visibility.Hidden;
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

        private void StartWebcam()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);
                videoSource.Start();
            }
            else
            {
                MessageBox.Show("No webcam detected.");
            }
        }

        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            BitmapImage bitmapImage = BitmapToImageSource(frame);

            Application.Current.Dispatcher.Invoke(() =>
            {
                WebcamImage.Source = bitmapImage;
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

                StoreImageInDatabase(imageBytes);
                MessageBox.Show("Image Captured!");
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

        private void StoreImageInDatabase(byte[] imageBytes)
        {
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
    }
}
