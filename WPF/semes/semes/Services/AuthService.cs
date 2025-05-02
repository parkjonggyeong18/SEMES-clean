using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semes.Services
{
    public class AuthService
    {
        private string _currentUser;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_currentUser);

        public string CurrentUser => _currentUser;

        public AuthService()
        {
            _currentUser = string.Empty;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            // 실제 애플리케이션에서는 DB나 API를 통해 인증해야 함
            // 여기서는 간단하게 데모 구현
            await Task.Delay(500); // 서버 통신 시뮬레이션

            // 테스트용 계정: admin/admin 또는 user/user
            if ((username == "admin" && password == "admin") ||
                (username == "user" && password == "user"))
            {
                _currentUser = username;
                return true;
            }

            return false;
        }
    }
}
