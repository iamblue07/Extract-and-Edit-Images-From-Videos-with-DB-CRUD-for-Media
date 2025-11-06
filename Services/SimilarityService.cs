using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using BDM_P.Data;

namespace BDM_P.Services
{
    public class SimilarityService
    {
        public class SimilarItem
        {
            public bool IsProcessed { get; set; }
            public int Id { get; set; }
            public string ThumbUrl => IsProcessed ? $"/Processed/Image?id={Id}" : $"/Unprocessed/Image?id={Id}";
            public string Key => $"{(IsProcessed ? "P" : "U")}:{Id}";
        }

        public List<SimilarItem> FindSimilar(byte[] imageBlob, int threshold = 10, string attrWeights = "color=\"1.0\"")
        {
            var outList = new List<SimilarItem>();

            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("psFindSimilarImages", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("p_blob", OracleDbType.Blob).Value = imageBlob ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_attrWeights", OracleDbType.Varchar2).Value = attrWeights ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_threshold", OracleDbType.Decimal).Value = threshold;

                var pCursor = cmd.Parameters.Add("o_cursor", OracleDbType.RefCursor);
                pCursor.Direction = ParameterDirection.Output;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var src = reader.GetString(0); // 'P' or 'U'
                        var id = reader.GetInt32(1);
                        outList.Add(new SimilarItem
                        {
                            IsProcessed = (src == "P"),
                            Id = id
                        });
                    }
                }
            }

            return outList;
        }
    }
}
