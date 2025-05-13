using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace semes.Services
{
    public class PcbInspectionService
    {
        private readonly string _connectionString;

        public PcbInspectionService()
        {
            _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";
        }

        // PCB 검사 결과를 데이터베이스에 저장
        public async Task<bool> SaveInspectionResultAsync(string serialNumber, IEnumerable<DefectDetectionPage.DefectItem> defectItems)
        {
            if (string.IsNullOrEmpty(serialNumber))
            {
                throw new ArgumentException("유효한 시리얼 번호가 필요합니다.");
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            // 불량 항목 개수 계산
                            int defectCount = 0;
                            foreach (var _ in defectItems)
                            {
                                defectCount++;
                            }

                            // 1. PCB 테이블에 기본 정보 저장
                            string insertPcbQuery = @"
                                INSERT INTO pcb (serial_number, inspection_date, defect_count) 
                                VALUES (@serialNumber, @inspectionDate, @defectCount);
                                SELECT LAST_INSERT_ID();";

                            int pcbId;
                            using (var command = new MySqlCommand(insertPcbQuery, connection))
                            {
                                command.Transaction = transaction;
                                command.Parameters.AddWithValue("@serialNumber", serialNumber);
                                command.Parameters.AddWithValue("@inspectionDate", DateTime.Now);
                                command.Parameters.AddWithValue("@defectCount", defectCount);

                                var result = await command.ExecuteScalarAsync();
                                pcbId = Convert.ToInt32(result);
                            }

                            // 2. 이물질 정보 저장 (있는 경우에만)
                            if (defectCount > 0)
                            {
                                string insertDefectQuery = @"
                                    INSERT INTO pcb_defect (pcb_id, x_position, y_position, width, height) 
                                    VALUES (@pcbId, @xPosition, @yPosition, @width, @height);";

                                using (var command = new MySqlCommand(insertDefectQuery, connection))
                                {
                                    command.Transaction = transaction;

                                    command.Parameters.Add(new MySqlParameter("@pcbId", MySqlDbType.Int32));
                                    command.Parameters.Add(new MySqlParameter("@xPosition", MySqlDbType.Int32));
                                    command.Parameters.Add(new MySqlParameter("@yPosition", MySqlDbType.Int32));
                                    command.Parameters.Add(new MySqlParameter("@width", MySqlDbType.Double));
                                    command.Parameters.Add(new MySqlParameter("@height", MySqlDbType.Double));

                                    foreach (var defect in defectItems)
                                    {
                                        command.Parameters["@pcbId"].Value = pcbId;
                                        command.Parameters["@xPosition"].Value = defect.X;
                                        command.Parameters["@yPosition"].Value = defect.Y;
                                        command.Parameters["@width"].Value = defect.Width;
                                        command.Parameters["@height"].Value = defect.Height;

                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                            }

                            await transaction.CommitAsync();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"DB 오류: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB 연결 오류: {ex.Message}");
                return false;
            }
        }
    }
}
