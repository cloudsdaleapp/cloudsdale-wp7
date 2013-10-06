using System.IO;
using System.Threading.Tasks;

namespace CloudsdaleWin7.lib.Models
{
    public interface IAvatarUploadTarget
    {
        Avatar Avatar { get; }
        Task UploadAvatar(Stream pictureStream, string mimeType);
    }
}
