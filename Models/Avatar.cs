using System;

namespace Cloudsdale.Models {
    public struct Avatar {
        public Uri Normal { get; set; }
        public Uri Mini { get; set; }
        public Uri Thumb { get; set; }
        public Uri Preview { get; set; }
        public Uri Chat { get; set; }
    }
}
