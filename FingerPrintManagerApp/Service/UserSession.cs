using FingerPrintManagerApp.Model;

namespace FingerPrintManagerApp.Service
{
    public class UserSession
    {
        private static UserSession _instance;
        private static readonly object _lock = new object();

        public UserModel CurrentUser { get; private set; }

        private UserSession() { }

        public static UserSession Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new UserSession();
                    }
                    return _instance;
                }
            }
        }

        public void SetUser(UserModel user)
        {
            CurrentUser = user;
        }

        public void ClearUser()
        {
            CurrentUser = null;
        }
    }
}
