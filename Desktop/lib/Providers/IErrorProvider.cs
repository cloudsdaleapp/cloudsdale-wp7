using System;
using System.Threading.Tasks;
using CloudsdaleWin7.lib.Helpers;

namespace CloudsdaleWin7.lib.Providers
{public interface IModelErrorProvider
        {
            Task OnError<T>(WebResponse<T> response);
        }

    internal class DefaultModelErrorProvider : IModelErrorProvider
    {
        public Task OnError<T>(WebResponse<T> response)
        {
            throw new NotImplementedException("Model error handler not implemented");
        }
    }
}