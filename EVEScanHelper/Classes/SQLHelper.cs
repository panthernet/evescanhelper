using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace EVEScanHelper.Classes
{
    public static class SQLHelper
    {
        private static readonly string DBFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "esh.db");

        public static void InsertOrUpdate(string table, Dictionary<string, object> values)
        {
            if (values == null)
            {
                LogHelper.Log($"[SQLiteDataInsertOrUpdate]: {table} query has empty values!");
                return;
            }
            var fromText = string.Empty;
            var valuesText = string.Empty;
            int count = 1;
            var last = values.Keys.Last();
            foreach (var pair in values)
            {
                fromText += $"{pair.Key}{(pair.Key == last ? null : ",")}";
                valuesText += $"@var{count++}{(pair.Key == last ? null : ",")}";
            }

            var query = $"insert or replace into {table} ({fromText}) values({valuesText})";
            using (var con = new SQLiteConnection($"Data Source = {DBFile};"))
            using (var querySQL = new SQLiteCommand(query, con))
            {
                con.Open();

                count = 1;
                foreach (var pair in values)
                {
                    querySQL.Parameters.Add(new SQLiteParameter($"@var{count++}", pair.Value ?? DBNull.Value));
                }
                try
                {
                    querySQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    LogHelper.LogEx($"[SQLiteDataInsertOrUpdate]: {query} - {string.Join("|", values.Select(a=> $"{a.Key}:{a.Value} "))}", ex);
                }
            }
        }

        public static T SQLiteDataQuery<T>(string table, string field, string whereField, object whereData)
        {
            return SQLiteDataQuery<T>(table, field, new Dictionary<string, object> {{whereField, whereData}});
        }

        public static T SQLiteDataQuery<T>(string table, string field, Dictionary<string, object> where)
        {
            if (where == null)
            {
                LogHelper.Log($"[SQLiteDataQuery]: {table}-{field} query has empty values!");
                return default;
            }

            var whereText = string.Empty;
            int count = 1;
            var last = where.Keys.Last();
            foreach (var pair in where)
            {
                whereText += $"{pair.Key}=@var{count++}{(pair.Key == last? null : " and ")}";
            }

            var query = $"SELECT {field} FROM {table} WHERE {whereText}";
            using (var con = new SQLiteConnection($"Data Source = {DBFile};"))
            using (var querySQL = new SQLiteCommand(query, con))
            {
                con.Open();

                count = 1;
                foreach (var pair in where)
                {
                    querySQL.Parameters.Add(new SQLiteParameter($"@var{count++}", pair.Value));
                }
                try
                {
                    using (var r = querySQL.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            r.Read();
                            if (r.IsDBNull(0))
                                return default;
                            var type = typeof(T);
                            if(type == typeof(string))
                                return (T)(object)(r.IsDBNull(0) ? "" : r.GetString(0));
                            if (type == typeof(int))
                                return (T) (object) (r.IsDBNull(0) ? 0 : r.GetInt32(0));
                            if (type == typeof(ulong))
                                return (T) (object) (r.IsDBNull(0) ? 0 : (ulong)r.GetInt64(0));
                            if (type == typeof(long))
                                return (T) (object) (r.IsDBNull(0) ? 0 : r.GetInt64(0));
                            if (type == typeof(DateTime))
                                return (T) (object) (r.IsDBNull(0) ? DateTime.MinValue : r.GetDateTime(0));
                        }
                        return default;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogEx($"[SQLiteDataQuery]: {query} - {string.Join("|", where.Select(a=> $"{a.Key}:{a.Value} "))}", ex);
                    return default;
                }
            }
        }

        public static void RunCommand(string query2, bool silent = false)
        {
            try
            {
                using (var con = new SQLiteConnection($"Data Source = {DBFile};"))
                using (var insertSQL = new SQLiteCommand(query2, con))
                {
                    con.Open();
                    insertSQL.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (!silent)
                    LogHelper.LogEx($"[RunCommand]: {query2}", ex);
            }
        }

        public static bool Prepare()
        {
            if (File.Exists(DBFile)) return true;
            RunCommand("ALTER COLUMN `lastUpdate` SET  NOT NULL DEFAULT CURRENT_TIMESTAMP");
            try
            {
                SQLiteConnection.CreateFile(DBFile);
                RunCommand("CREATE TABLE `sigs` ( `system` text UNIQUE PRIMARY KEY NOT NULL, `value` text NOT NULL, `lastUpdate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP)");

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.LogEx(ex.Message, ex);
                return false;
            }
        }
    }
}
