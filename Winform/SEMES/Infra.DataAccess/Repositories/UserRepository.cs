using Infra.DataAccess.Contracts;
using Infra.DataAccess.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Infra.DataAccess.Repositories
{
    public class UserRepository : MasterRepository, IUserRepository
    {
        /*────────── LoginUser 저장 프로시저 호출 ──────────*/
        public User Login(string username, string password)
        {
            var p = new List<MySqlParameter>
            {
                new MySqlParameter("@p_user", username),
                new MySqlParameter("@p_password", password)
            };

            var tbl = ExecuteReader("LoginUser", p, CommandType.StoredProcedure);
            if (tbl.Rows.Count == 0) return null;

            var r = tbl.Rows[0];
            return new User
            {
                Id = Convert.ToInt32(r[0]),
                Username = r[1].ToString(),
                Password = r[2].ToString(),
                FirstName = r[3].ToString(),
                LastName = r[4].ToString(),
                Position = r[5].ToString(),
                Email = r[6].ToString(),
                Photo = r[7] == DBNull.Value ? null : (byte[])r[7]
            };
        }

        /*────────── AddUser 저장 프로시저 ──────────*/
        public int Add(User e)
        {
            var p = new List<MySqlParameter>
            {
                new MySqlParameter("@userName",  e.Username),
                new MySqlParameter("@password",  e.Password),
                new MySqlParameter("@firstName", e.FirstName),
                new MySqlParameter("@lastName",  e.LastName),
                new MySqlParameter("@position",  e.Position),
                new MySqlParameter("@email",     e.Email),
                new MySqlParameter("@photo",     e.Photo == null ? (object)DBNull.Value : e.Photo)
                    { MySqlDbType = MySqlDbType.Blob }
            };
            return ExecuteNonQuery("AddUser", p, CommandType.StoredProcedure);
        }

        /*────────── EditUser 저장 프로시저 ──────────*/
        public int Edit(User e)
        {
            var p = new List<MySqlParameter>
            {
                new MySqlParameter("@id",        e.Id),
                new MySqlParameter("@userName",  e.Username),
                new MySqlParameter("@password",  e.Password),
                new MySqlParameter("@firstName", e.FirstName),
                new MySqlParameter("@lastName",  e.LastName),
                new MySqlParameter("@position",  e.Position),
                new MySqlParameter("@email",     e.Email),
                new MySqlParameter("@photo",     e.Photo == null ? (object)DBNull.Value : e.Photo)
                    { MySqlDbType = MySqlDbType.Blob }
            };
            return ExecuteNonQuery("EditUser", p, CommandType.StoredProcedure);
        }

        /*────────── 단일 삭제 (텍스트 쿼리) ──────────*/
        public int Remove(User e)
        {
            return ExecuteNonQuery("DELETE FROM Users WHERE id=@id",
                                   new MySqlParameter("@id", e.Id),
                                   CommandType.Text);
        }

        /*────────── 대량 추가 ──────────*/
        public int AddRange(List<User> list)
        {
            var txs = new List<BulkTransaction>();
            foreach (var u in list)
            {
                var p = new List<MySqlParameter>
                {
                    new MySqlParameter("@userName",  u.Username),
                    new MySqlParameter("@password",  u.Password),
                    new MySqlParameter("@firstName", u.FirstName),
                    new MySqlParameter("@lastName",  u.LastName),
                    new MySqlParameter("@position",  u.Position),
                    new MySqlParameter("@email",     u.Email),
                    new MySqlParameter("@photo",     u.Photo == null ? (object)DBNull.Value : u.Photo)
                        { MySqlDbType = MySqlDbType.Blob }
                };
                txs.Add(new BulkTransaction { CommandText = "AddUser", Parameters = p });
            }
            return BulkExecuteNonQuery(txs, CommandType.StoredProcedure);
        }

        /*────────── 대량 삭제 ──────────*/
        public int RemoveRange(List<User> list)
        {
            var txs = new List<BulkTransaction>();
            foreach (var u in list)
            {
                txs.Add(new BulkTransaction
                {
                    CommandText = "DELETE FROM Users WHERE id=@id",
                    Parameters = new List<MySqlParameter>
                                  { new MySqlParameter("@id", u.Id) }
                });
            }
            return BulkExecuteNonQuery(txs, CommandType.Text);
        }

        /*────────── 검색 / 조회 ──────────*/
        public User GetSingle(string value)
        {
            string sql;
            List<MySqlParameter> p;
            int id;
            if (int.TryParse(value, out id))
            {
                sql = "SELECT * FROM Users WHERE id=@id";
                p = new List<MySqlParameter> { new MySqlParameter("@id", id) };
            }
            else
            {
                sql = "SELECT * FROM Users WHERE userName=@v OR email=@v";
                p = new List<MySqlParameter> { new MySqlParameter("@v", value) };
            }

            var tbl = ExecuteReader(sql, p, CommandType.Text);
            if (tbl.Rows.Count == 0) return null;

            var r = tbl.Rows[0];
            return new User
            {
                Id = Convert.ToInt32(r[0]),
                Username = r[1].ToString(),
                Password = r[2].ToString(),
                FirstName = r[3].ToString(),
                LastName = r[4].ToString(),
                Position = r[5].ToString(),
                Email = r[6].ToString(),
                Photo = r[7] == DBNull.Value ? null : (byte[])r[7]
            };
        }

        public IEnumerable<User> GetAll()
        {
            var tbl = ExecuteReader("SelectAllUsers", CommandType.StoredProcedure);
            var list = new List<User>();
            foreach (DataRow r in tbl.Rows)
            {
                list.Add(new User
                {
                    Id = Convert.ToInt32(r[0]),
                    Username = r[1].ToString(),
                    Password = r[2].ToString(),
                    FirstName = r[3].ToString(),
                    LastName = r[4].ToString(),
                    Position = r[5].ToString(),
                    Email = r[6].ToString(),
                    Photo = r[7] == DBNull.Value ? null : (byte[])r[7]
                });
            }
            return list;
        }

        public IEnumerable<User> GetByValue(string value)
        {
            var tbl = ExecuteReader("SelectUser",
                                    new MySqlParameter("@findValue", value),
                                    CommandType.StoredProcedure);
            var list = new List<User>();
            foreach (DataRow r in tbl.Rows)
            {
                list.Add(new User
                {
                    Id = Convert.ToInt32(r[0]),
                    Username = r[1].ToString(),
                    Password = r[2].ToString(),
                    FirstName = r[3].ToString(),
                    LastName = r[4].ToString(),
                    Position = r[5].ToString(),
                    Email = r[6].ToString(),
                    Photo = r[7] == DBNull.Value ? null : (byte[])r[7]
                });
            }
            return list;
        }
    }
}
