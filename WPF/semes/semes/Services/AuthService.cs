using MySqlConnector;

namespace semes.Services
{
    public class AuthService
    {
        private string _currentUser;
        private readonly string _connectionString;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_currentUser);

        public string CurrentUser => _currentUser;

        public string UserRole { get; private set; } = "USER";

        public AuthService()
        {
            _currentUser = string.Empty;
            _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            // 실제 애플리케이션에서는 DB나 API를 통해 인증해야 함
            // 여기서는 간단하게 데모 구현
            //await Task.Delay(500); // 서버 통신 시뮬레이션

            // DB 연결
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT role FROM users WHERE username = @username AND password = @password";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        var result = await command.ExecuteScalarAsync();
                        //int resultCnt = Convert.ToInt32(result);

                        //if (resultCnt == 1)
                        //{
                        //    _currentUser = username;
                        //    return true;
                        //}
                        //else if (resultCnt == 0)
                        //{
                        //    return false;
                        //}
                        //else
                        //{
                        //    Console.WriteLine("DB 입출력 중 오류 발생.");
                        //    throw new Exception("회원 조회 오류");
                        //}
                        if (result != null)
                        {
                            _currentUser = username;
                            UserRole = result.ToString();  // "USER" 또는 "ADMIN"
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // 로그 기록 등 예외 처리
                Console.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }

            

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
