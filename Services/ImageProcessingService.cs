using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using BDM_P.Data;

namespace BDM_P.Services
{
    public class ImageProcessingService
    {
        public byte[] ProcessImage(byte[] srcImage, string op, string param)
        {
            if (srcImage == null || srcImage.Length == 0) return null;

            using (var conn = Db.GetConn())
            using (var cmd = new OracleCommand("psProcessImageOp", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var pBlob = cmd.Parameters.Add("p_blob", OracleDbType.Blob);
                pBlob.Direction = ParameterDirection.Input;
                pBlob.Value = srcImage;

                cmd.Parameters.Add("p_op", OracleDbType.Varchar2).Value = (op ?? "").ToLowerInvariant();
                cmd.Parameters.Add("p_param", OracleDbType.Varchar2).Value = (param ?? "");

                var outParam = cmd.Parameters.Add("o_blob", OracleDbType.Blob);
                outParam.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                var oracleBlob = outParam.Value as Oracle.ManagedDataAccess.Types.OracleBlob;
                if (oracleBlob == null || oracleBlob.IsNull) return null;

                return oracleBlob.Value;
            }
        }
    }
}
