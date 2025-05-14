using MySqlConnector;
using System;
using System.Data;
using System.Threading.Tasks;

namespace semes.Services
{
    public class DashboardService
    {
        private readonly string _connectionString;

        public DashboardService()
        {
            _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";
        }



        public async Task<List<InspectionRecord>> GetInspectionRecordsByDateAsync(DateTime date)
        {
            var result = new List<InspectionRecord>();

            string query = @"
        SELECT pcb_id, inspection_date, serial_number, defect_count
        FROM pcb
        WHERE DATE(inspection_date) = @date
        ORDER BY inspection_date DESC;";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@date", date.Date);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new InspectionRecord
                {
                    PcbId = reader.GetInt32("pcb_id"),
                    InspectionDate = reader.GetDateTime("inspection_date"),
                    SerialNumber = reader.GetString("serial_number"),
                    DefectCount = reader.GetInt32("defect_count")
                });
            }

            return result;
        }
        public async Task<double> GetDefectRateByDateAsync(DateTime date)
        {
            string query = @"
        SELECT 
            SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) / COUNT(*) * 100.0 AS defect_rate
        FROM pcb
        WHERE DATE(inspection_date) = @date;";

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@date", date.Date);
            object result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        public async Task<double> Get7DayAverageDefectRateAsync()
        {
            string query = @"
        SELECT AVG(daily_defect_rate) FROM (
            SELECT 
                DATE(inspection_date) as day,
                SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) / COUNT(*) * 100.0 AS daily_defect_rate
            FROM pcb
            WHERE inspection_date >= CURDATE() - INTERVAL 7 DAY
            GROUP BY day
        ) sub;
    ";

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);
            object result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
        public async Task<double> GetOverallDefectRateAsync()
        {
            string query = @"
        SELECT 
            SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) / COUNT(*) * 100.0 AS defect_rate
        FROM pcb;";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            object result = await command.ExecuteScalarAsync();

            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        public async Task<(int total, int good, int defect)> GetDashboardStatsByDateAsync(DateTime date)
        {
            int total = 0, good = 0, defect = 0;

            string query = @"
                SELECT
                    COUNT(*) AS total,
                    SUM(CASE WHEN defect_count = 0 THEN 1 ELSE 0 END) AS good,
                    SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) AS defect
                FROM pcb
                WHERE DATE(inspection_date) = @date;";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@date", date.Date);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                total = reader.IsDBNull("total") ? 0 : reader.GetInt32("total");
                good = reader.IsDBNull("good") ? 0 : reader.GetInt32("good");
                defect = reader.IsDBNull("defect") ? 0 : reader.GetInt32("defect");
            }

            return (total, good, defect);
        }
    }
}