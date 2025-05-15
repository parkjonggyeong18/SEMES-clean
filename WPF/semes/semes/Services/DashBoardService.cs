using MySqlConnector;
using System;
using System.Collections.Generic;
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
                    DefectCount = reader.GetInt32("defect_count"),
                    IsGood = reader.GetInt32("defect_count") == 0 // 🆕 IsGood 속성 추가
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

        // 🆕 챗봇용 추가 메서드들

        /// <summary>
        /// 최근 검사 기록 요약 정보 (챗봇용)
        /// </summary>
        public async Task<List<InspectionRecord>> GetRecentInspectionRecordsAsync(int limit = 10)
        {
            var result = new List<InspectionRecord>();

            string query = @"
                SELECT pcb_id, inspection_date, serial_number, defect_count
                FROM pcb
                ORDER BY inspection_date DESC
                LIMIT @limit;";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@limit", limit);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new InspectionRecord
                {
                    PcbId = reader.GetInt32("pcb_id"),
                    InspectionDate = reader.GetDateTime("inspection_date"),
                    SerialNumber = reader.GetString("serial_number"),
                    DefectCount = reader.GetInt32("defect_count"),
                    IsGood = reader.GetInt32("defect_count") == 0
                });
            }

            return result;
        }

        /// <summary>
        /// 일별 불량률 추이 (최근 30일)
        /// </summary>
        public async Task<List<DailyDefectRate>> GetDailyDefectRateTrendAsync(int days = 30)
        {
            var result = new List<DailyDefectRate>();

            string query = @"
                SELECT 
                    DATE(inspection_date) as inspection_date,
                    COUNT(*) as total_count,
                    SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) as defect_count,
                    SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) / COUNT(*) * 100.0 AS defect_rate
                FROM pcb
                WHERE inspection_date >= CURDATE() - INTERVAL @days DAY
                GROUP BY DATE(inspection_date)
                ORDER BY inspection_date DESC;";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@days", days);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new DailyDefectRate
                {
                    Date = reader.GetDateTime("inspection_date"),
                    TotalCount = reader.GetInt32("total_count"),
                    DefectCount = reader.GetInt32("defect_count"),
                    DefectRate = reader.IsDBNull("defect_rate") ? 0 : reader.GetDouble("defect_rate")
                });
            }

            return result;
        }

        /// <summary>
        /// 불량 PCB 상위 리스트
        /// </summary>
        public async Task<List<InspectionRecord>> GetTopDefectPCBsAsync(int limit = 10)
        {
            var result = new List<InspectionRecord>();

            string query = @"
                SELECT pcb_id, inspection_date, serial_number, defect_count
                FROM pcb
                WHERE defect_count > 0
                ORDER BY defect_count DESC, inspection_date DESC
                LIMIT @limit;";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@limit", limit);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new InspectionRecord
                {
                    PcbId = reader.GetInt32("pcb_id"),
                    InspectionDate = reader.GetDateTime("inspection_date"),
                    SerialNumber = reader.GetString("serial_number"),
                    DefectCount = reader.GetInt32("defect_count"),
                    IsGood = false
                });
            }

            return result;
        }

        /// <summary>
        /// 시간별 검사 통계 (챗봇용 요약)
        /// </summary>
        public async Task<QualityStatistics> GetQualityStatisticsAsync()
        {
            var stats = new QualityStatistics();

            string query = @"
                SELECT 
                    COUNT(*) as total_inspections,
                    SUM(CASE WHEN defect_count = 0 THEN 1 ELSE 0 END) as good_count,
                    SUM(CASE WHEN defect_count > 0 THEN 1 ELSE 0 END) as defect_count,
                    AVG(defect_count) as avg_defect_count,
                    MAX(defect_count) as max_defect_count,
                    MIN(inspection_date) as earliest_inspection,
                    MAX(inspection_date) as latest_inspection
                FROM pcb;";

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(query, connection);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                stats.TotalInspections = reader.GetInt32("total_inspections");
                stats.GoodCount = reader.GetInt32("good_count");
                stats.DefectCount = reader.GetInt32("defect_count");
                stats.AverageDefectCount = reader.IsDBNull("avg_defect_count") ? 0 : reader.GetDouble("avg_defect_count");
                stats.MaxDefectCount = reader.IsDBNull("max_defect_count") ? 0 : reader.GetInt32("max_defect_count");
                stats.EarliestInspection = reader.IsDBNull("earliest_inspection") ? DateTime.MinValue : reader.GetDateTime("earliest_inspection");
                stats.LatestInspection = reader.IsDBNull("latest_inspection") ? DateTime.MinValue : reader.GetDateTime("latest_inspection");

                // 불량률 계산
                if (stats.TotalInspections > 0)
                {
                    stats.DefectRate = (double)stats.DefectCount / stats.TotalInspections * 100.0;
                }
            }

            return stats;
        }
    }

    // 🆕 챗봇용 데이터 클래스들
    public class DailyDefectRate
    {
        public DateTime Date { get; set; }
        public int TotalCount { get; set; }
        public int DefectCount { get; set; }
        public double DefectRate { get; set; }
    }

    public class QualityStatistics
    {
        public int TotalInspections { get; set; }
        public int GoodCount { get; set; }
        public int DefectCount { get; set; }
        public double DefectRate { get; set; }
        public double AverageDefectCount { get; set; }
        public int MaxDefectCount { get; set; }
        public DateTime EarliestInspection { get; set; }
        public DateTime LatestInspection { get; set; }
    }

    // InspectionRecord 클래스 (기존에 없다면 추가)
    public class InspectionRecord
    {
        public int PcbId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string SerialNumber { get; set; }
        public int DefectCount { get; set; }
        public bool IsGood { get; set; }
    }
}