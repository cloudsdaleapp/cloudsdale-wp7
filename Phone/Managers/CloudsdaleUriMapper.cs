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
            if (aUri.AbsolutePath == "/Protocol" || aUri.AbsolutePath == "/CloudTile") {
                Connection.LaunchedUri = GetCloudsdaleUri(aUri);
                return new Uri("/MainPage.xaml", UriKind.Relative);
            }
            return uri;
        }

        public static Uri GetCloudsdaleUri(Uri uri) {
            return uri.Query.IndexOf("=", StringComparison.Ordinal) != -1 ? new Uri(HttpUtility.UrlDecode(uri.Query.Substring(uri.Query.IndexOf("=", StringComparison.Ordinal) + 1))) : null;
        }
    }
}
