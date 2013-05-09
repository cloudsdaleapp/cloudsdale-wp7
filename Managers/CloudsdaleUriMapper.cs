using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Windows.Foundation;

namespace Cloudsdale.Managers {
    class CloudsdaleUriMapper : UriMapperBase {
        public override Uri MapUri(Uri uri) {
            var aUri = uri.IsAbsoluteUri ? uri : new Uri("cloudsdale://cloudsdale" + uri.OriginalString);
            if (aUri.AbsolutePath == "/Protocol") {
                var cloudUri = new Uri(HttpUtility.UrlDecode(aUri.Query.Substring(aUri.Query.IndexOf("=", StringComparison.Ordinal) + 1)));
                Connection.LaunchedUri = cloudUri;
                return new Uri("/MainPage.xaml", UriKind.Relative);
            }
            return uri;
        }
    }
}
