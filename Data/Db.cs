using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BDM_P.Data
{
    public static class Db
    {
        public static OracleConnection GetConn()
        {
            var c = new OracleConnection(ConfigurationManager.ConnectionStrings["OracleDb"].ConnectionString);
            c.Open();
            return c;
        }
    }
}