using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI
{
    public class WebClientEx : WebClient
    {
        private int _timeout = 300000;
        private Uri _responseUri;


        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        public CookieContainer CookieContainer { get; set; }

        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public WebClientEx()
        {

        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            r.Timeout = Timeout;

            HttpWebRequest request = r as HttpWebRequest;
            request.AllowAutoRedirect = false;

            if (request != null)
            {
                request.CookieContainer = CookieContainer;
            }

            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);

            _responseUri = response.ResponseUri;

            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);

            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            if (CookieContainer == null)
            {
                return;
            }

            HttpWebResponse response = r as HttpWebResponse;

            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                CookieContainer.Add(cookies);
            }
        }

        #region AsyncMethods

        public new async Task<string> DownloadStringTaskAsync(string address)
        {
            return await Task.Run(() => this.DownloadString(address)).ConfigureAwait(false);
        }

        public new async Task<string> DownloadStringTaskAsync(Uri address)
        {
            return await DownloadStringTaskAsync(address.ToString()).ConfigureAwait(false);
        }

        public new async Task<byte[]> DownloadDataTaskAsync(string address)
        {
            return await Task.Run(() => this.DownloadData(address)).ConfigureAwait(false);
        }

        public new async Task<byte[]> DownloadDataTaskAsync(Uri address)
        {
            return await DownloadDataTaskAsync(address.ToString()).ConfigureAwait(false);
        }

        public new async Task DownloadFileTaskAsync(string address, string fileName)
        {
            await Task.Run(() => this.DownloadFile(address, fileName)).ConfigureAwait(false);
        }

        public new async Task DownloadFileTaskAsync(Uri address, string fileName)
        {
            await DownloadFileTaskAsync(address.ToString(), fileName).ConfigureAwait(false);
        }

        #endregion
    }
}
