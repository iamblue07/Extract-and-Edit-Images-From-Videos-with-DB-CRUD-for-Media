using BDM_P.Data;
using BDM_P.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace BDM_P.Services
{
    public class ProcessedService
    {
        public void Insert(int id, byte[] data, int? vidId)
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("psInsProc", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("p_blob", OracleDbType.Blob).Value = data;
                var pVid = cmd.Parameters.Add("p_vid_id", OracleDbType.Int32);
                pVid.Value = vidId.HasValue ? (object)vidId.Value : DBNull.Value;
                cmd.ExecuteNonQuery();
            }
        }

        public List<ProcessedImage> GetAll()
        {
            var list = new List<ProcessedImage>();
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT id, vid_id FROM BDM_P_PROCESSED_IMAGES ORDER BY id", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new ProcessedImage
                    {
                        Id = r.GetInt32(0),
                        VideoId = r.IsDBNull(1) ? (int?)null : r.GetInt32(1)
                    });
                }
            }
            return list;
        }

        public byte[] GetImageById(int id)
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT v.img.getContent() FROM BDM_P_PROCESSED_IMAGES v WHERE id = :id", conn))
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

        public int GetNextId()
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT NVL(MAX(id),0)+1 FROM BDM_P_PROCESSED_IMAGES", conn))
            {
                var obj = cmd.ExecuteScalar();
                return Convert.ToInt32(obj);
            }
        }

        public List<int> GetIdsByVideoId(int videoId)
        {
            var list = new List<int>();
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("SELECT id FROM BDM_P_PROCESSED_IMAGES WHERE vid_id = :vidId ORDER BY id", conn))
            {
                cmd.Parameters.Add("vidId", OracleDbType.Int32).Value = videoId;
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(r.GetInt32(0));
                    }
                }
            }
            return list;
        }
        public void DeleteById(int id)
        {
            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("DELETE FROM BDM_P_PROCESSED_IMAGES WHERE id = :id", conn))
            {
                cmd.Parameters.Add("id", OracleDbType.Int32).Value = id;
                cmd.ExecuteNonQuery();
            }
        }

    }
}
