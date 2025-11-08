using System;
using System.IO;
using System.Linq;
using System.Web;

namespace BDM_P
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            LoadDotEnvIfPresent();
            // ... any other startup logic
        }

        private void LoadDotEnvIfPresent()
        {
            var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (!File.Exists(envPath)) return;

            foreach (var raw in File.ReadAllLines(envPath))
            {
                var line = raw?.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var idx = line.IndexOf('=');
                if (idx <= 0) continue;

                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();

                // remove surrounding quotes if present
                if (val.Length >= 2 && ((val.StartsWith("\"") && val.EndsWith("\"")) ||
                                         (val.StartsWith("'") && val.EndsWith("'"))))
                {
                    val = val.Substring(1, val.Length - 2);
                }

                // DO NOT overwrite host-provided env vars (Render will set these)
                if (Environment.GetEnvironmentVariable(key) == null)
                {
                    Environment.SetEnvironmentVariable(key, val);
                }
            }
        }
    }
}
