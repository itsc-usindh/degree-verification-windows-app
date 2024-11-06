using System.IO;
using System.Net.Http;
using System.Net;

namespace FingerPrintManagerApp.Service
{
    public static class APIService
    {
        #region API Service
        private static readonly HttpClient client = new HttpClient();

        public static string PostWebRequest(string url, string jsonPayload)
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
                throw new Exception($"WebRequest error: {ex.Message}");
            }
        }
        #endregion
    }
}
