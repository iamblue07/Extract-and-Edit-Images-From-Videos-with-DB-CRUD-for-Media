using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;

namespace BDM_P.Data
{
    public static class Db
    {
       
        public static OracleConnection GetConn()
        {
            var cs = ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString;
            cs = Environment.ExpandEnvironmentVariables(cs);

            var c = new OracleConnection(cs);
            c.Open();
            return c;
        }
    }
}
