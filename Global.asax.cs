using dotenv.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BDM_P
{
    public class Global: System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            DotEnv.Load();            
        }
    }
}
