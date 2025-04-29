using MySql.Data.MySqlClient;

namespace Infra.DataAccess
{
    public abstract class Repository
    {
        private readonly string connectionString;

        protected Repository()
        {
            // ★ DB 접속 정보 ‘직접’ 쓰기
            connectionString =
                "server=localhost;port=3306;" +
                "database=MyCompanyTest;" +
                "uid=root;pwd=root;";
        }

        protected MySqlConnection GetConnection()
            => new MySqlConnection(connectionString);
    }
}
