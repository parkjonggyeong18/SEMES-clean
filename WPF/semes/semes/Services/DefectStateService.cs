using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace semes.Services
{
    public class DefectStatsService
    {
        private readonly string _connectionString =
            "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;" +
            "Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";

        public async Task<List<(string date, double defectRate)>> GetDefectRateTrendAsync(int days)
        {
            var result = new List<(string, double)>();

            string query = @"
                SELECT DATE(inspection_date) AS date,
                       SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) / COUNT(*) * 100.0 AS defect_rate
                FROM pcb
                WHERE inspection_date >= CURDATE() - INTERVAL @days DAY
                GROUP BY DATE(inspection_date)
                ORDER BY DATE(inspection_date);";

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@days", days);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string date = Convert.ToDateTime(reader["date"]).ToString("MM/dd");
                double rate = Convert.ToDouble(reader["defect_rate"]);
                result.Add((date, rate));
            }

            return result;
        }

        public async Task<List<(string date, int defectCount)>> GetDefectCountTrendAsync(int days)
        {
            var result = new List<(string, int)>();

            string query = @"
                SELECT DATE(pd.inspection_date) AS date, COUNT(d.pcb_defect_id) AS defect_count
                FROM pcb pd
                LEFT JOIN pcb_defect d ON pd.pcb_id = d.pcb_id
                WHERE pd.inspection_date >= CURDATE() - INTERVAL @days DAY
                GROUP BY DATE(pd.inspection_date)
                ORDER BY DATE(pd.inspection_date);";

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@days", days);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string date = Convert.ToDateTime(reader["date"]).ToString("MM/dd");
                int count = Convert.ToInt32(reader["defect_count"]);
                result.Add((date, count));
            }

            return result;
        }

        public async Task<List<(int x, int y)>> GetDefectPositionsAsync(DateTime date)
        {
            var result = new List<(int, int)>();

            string query = @"
                SELECT d.x_position, d.y_position
                FROM pcb p
                JOIN pcb_defect d ON p.pcb_id = d.pcb_id
                WHERE DATE(p.inspection_date) = @date;";

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int x = reader.GetInt32("x_position");
                int y = reader.GetInt32("y_position");
                result.Add((x, y));
            }

            return result;
        }
        public async Task<(int total, int good, int defect)> GetDefectSummaryAsync(DateTime date)
        {
            string query = @"
        SELECT 
            COUNT(*) AS total,
            SUM(CASE WHEN defect_count = 0 THEN 1 ELSE 0 END) AS good,
            SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) AS defect
        FROM pcb
        WHERE DATE(inspection_date) = @date;";

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (
                    reader.IsDBNull("total") ? 0 : reader.GetInt32("total"),
                    reader.IsDBNull("good") ? 0 : reader.GetInt32("good"),
                    reader.IsDBNull("defect") ? 0 : reader.GetInt32("defect")
                );
            }

            return (0, 0, 0);
        }

    }
}
