using BDM_P.Data;
using BDM_P.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;

namespace BDM_P.Services
{
    public class VideoService
    {
        public void Insert(int id, byte[] data, string videoName)
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("psInsVid", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("p_blob", OracleDbType.Blob).Value = data;
                cmd.Parameters.Add("p_vid_name", OracleDbType.Varchar2).Value = videoName ?? string.Empty;
                cmd.ExecuteNonQuery();
            }
        }

        public List<VideoItem> GetAll()
        {
            var list = new List<VideoItem>();
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT id, vid_name FROM BDM_P_VIDEOS ORDER BY id", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new VideoItem
                    {
                        Id = r.GetInt32(0),
                        VideoName = r.IsDBNull(1) ? string.Empty : r.GetString(1)
                    });
                }
            }
            return list;
        }

        public byte[] GetVideoById(int id)
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT v.vid.getContent() FROM BDM_P_VIDEOS v WHERE id = :id", conn))
            {
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read() && !r.IsDBNull(0))
                    {
                        var blob = r.GetOracleBlob(0);
                        return blob.Value;
                    }
                }
            }
            return null;
        }
        public string GetVideoNameById(int id)
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT vid_name FROM BDM_P_VIDEOS WHERE id = :id", conn))
            {
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                var obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? string.Empty : Convert.ToString(obj);
            }
        }

        public int GetNextId()
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT NVL(MAX(id),0)+1 FROM BDM_P_VIDEOS", conn))
            {
                var obj = cmd.ExecuteScalar();
                return Convert.ToInt32(obj);
            }
        }

        public void DeleteById(int id)
        {
            using (var conn = Db.GetConn())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    using (var cmd = new OracleCommand("DELETE FROM BDM_P_PROCESSED_IMAGES WHERE vid_id = :id", conn))
                    {
                        cmd.Transaction = tran;
                        cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    using (var cmd = new OracleCommand("DELETE FROM BDM_P_UNPROCESSED_IMAGES WHERE vid_id = :id", conn))
                    {
                        cmd.Transaction = tran;
                        cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    using (var cmd = new OracleCommand("DELETE FROM BDM_P_VIDEOS WHERE id = :id", conn))
                    {
                        cmd.Transaction = tran;
                        cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                        int affected = cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }
                catch
                {
                    try { tran.Rollback(); } catch { 
                    
                    }
                    throw;
                }
            }
        }

    }
}
