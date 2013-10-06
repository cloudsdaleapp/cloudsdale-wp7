using System.Collections.Generic;
using System.Threading.Tasks;
using CloudsdaleWin7.lib.Models;
using CloudsdaleWin7.lib.Providers;

namespace CloudsdaleWin7.lib.Controllers
{
    public class ModelController : IUserProvider, ICloudProvider
    {
        private readonly Dictionary<string, User> _users = new Dictionary<string, User>();
        private readonly Dictionary<string, Cloud> _clouds = new Dictionary<string, Cloud>();

        public async Task<User> GetUserAsync(string id)
        {
            if (!_users.ContainsKey(id))
            {
                var user = new User(id);
                await user.ForceValidate();
                _users[id] = user;
            }
            else
            {
                await _users[id].Validate();
            }
            return _users[id];
        }

        public User GetUser(string id)
        {
            if (!_users.ContainsKey(id))
            {
                var user = new User(id);
                user.ForceValidate();
                _users[id] = user;
            }
            else
            {
                _users[id].Validate();
            }
            return _users[id];
        }

        public async Task<User> UpdateDataAsync(User user)
        {
            if (!_users.ContainsKey(user.Id))
            {
                await user.ForceValidate();
                _users[user.Id] = user;
            }
            else
            {
                user.CopyTo(_users[user.Id]);
            }
            return _users[user.Id];
        }

        public async Task<Cloud> UpdateCloudAsync(Cloud cloud)
        {
            if (!_clouds.ContainsKey(cloud.Id))
            {
                await cloud.ForceValidate();
                _clouds[cloud.Id] = cloud;
            }
            else
            {
                cloud.CopyTo(_clouds[cloud.Id]);
            }

            return _clouds[cloud.Id];
        }

        public Cloud UpdateCloud(Cloud cloud)
        {
            if (!_clouds.ContainsKey(cloud.Id))
            {
                cloud.ForceValidate();
                _clouds[cloud.Id] = cloud;
            }
            else
            {
                var cacheCloud = _clouds[cloud.Id];
                cloud.CopyTo(cacheCloud);
            }

            return _clouds[cloud.Id];
        }

        public Cloud GetCloud(string cloudId)
        {
            if (!_clouds.ContainsKey(cloudId))
            {
                var cloud = new Cloud(cloudId);
                cloud.ForceValidate();
                _clouds[cloud.Id] = cloud;
            }
            else
            {
                _clouds[cloudId].Validate();
            }

            return _clouds[cloudId];
        }
    }
}
