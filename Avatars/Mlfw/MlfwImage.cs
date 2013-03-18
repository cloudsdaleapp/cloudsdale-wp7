using System;
using Newtonsoft.Json;

namespace Cloudsdale.Avatars.Mlfw {
    public class MlfwImage {
        private static readonly Uri BaseUri = new Uri("http://mylittlefacewhen.com/");

        // Identification
        public int Id { get; set; }

        // Meta
        public double Hotness { get; set; }
        public bool Accepted { get; set; }
        public bool Removed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime Added { get; set; }
        public Tag[] Tags { get; set; }
        public int Views { get; set; }
        public bool Processed { get; set; }

        // Information
        public string Comment { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Md5 { get; set; }

        // Resource endpoints
        [JsonProperty("resource_uri")]
        public string ResourceUri { get; set; }
        public string Image { get; set; }
        public ImageResizes Resizes { get; set; }
        public ImageThumbnails Thumbnails { get; set; }

        public Uri ImageUri {
            get { return new Uri(BaseUri, Image); }
        }

        public class ImageResizes {
            public string Large { get; set; }
            public string Medium { get; set; }
            public string Small { get; set; }

            public Uri MediumUri { get { return new Uri(BaseUri, Medium); } }
            public Uri SmallUri { get { return new Uri(BaseUri, Small); } }
            public Uri LargeUri { get { return new Uri(BaseUri, Large); } }
        }

        public class Tag {
            public string Name { get; set; }
        }

        public class ImageThumbnails {
            public string Jpg { get; set; }
            public string Png { get; set; }
            public string WebP { get; set; }

            public Uri JpgUri { get { return new Uri(BaseUri, Jpg); } }
            public Uri PngUri { get { return new Uri(BaseUri, Png); } }
            public Uri WebPUri { get { return new Uri(BaseUri, WebP); } }
        }

        public Uri Preview {
            get {
                if (Thumbnails.Png != null) return Thumbnails.PngUri;
                if (Thumbnails.Jpg != null) return Thumbnails.JpgUri;
                if (Resizes.Large != null) return Resizes.LargeUri;
                if (Resizes.Medium != null) return Resizes.MediumUri;
                if (Resizes.Small != null) return Resizes.SmallUri;
                return ImageUri;
            }
        }
    }
}
