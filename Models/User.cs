using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.Models
{
    /// <summary>
    /// Model User — bind cho các màn hình Hồ sơ, Header Dashboard, v.v.
    /// </summary>
    public class User : ViewModelBase
    {
        private string _id = string.Empty;
        private string _userName = string.Empty;
        private string _email = string.Empty;
        private string _displayName = string.Empty;
        private string _avatarUrl = string.Empty;
        private string _plan = "Miễn phí";

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public string AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }
        public string Plan { get => _plan; set => SetProperty(ref _plan, value); }
    }
}
