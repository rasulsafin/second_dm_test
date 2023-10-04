using System.Net;
using System.Threading.Tasks;

namespace Brio.Docs.External
{
    public static class WebFeatures
    {
        // https://stackoverflow.com/a/3808841/16047481
        public static async Task<bool> RemoteUrlExistsAsync(string url)
        {
            try
            {
                // Creating the HttpWebRequest
                var request = WebRequest.Create(url);

                // Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";

                // Getting the Web Response.
                var response = (HttpWebResponse)await request.GetResponseAsync();

                var urlExist = response.StatusCode == HttpStatusCode.OK;

                // Returns TRUE if the Status code == 200
                response.Close();
                return urlExist;
            }
            catch
            {
                // Any exception will returns false.
                return false;
            }
        }
    }
}
