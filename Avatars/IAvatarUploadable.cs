﻿using System;
using System.IO;

namespace Cloudsdale.Avatars {
    public interface IAvatarUploadable {
        void UploadAvatar(Stream pictureStream, Action<Uri> callback);
        Uri CurrentAvatar { get; }
        Uri DefaultAvatar { get; }
    }
}
