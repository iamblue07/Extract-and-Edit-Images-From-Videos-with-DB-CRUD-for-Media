using BDM_P.Services;
using System;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.UI;

namespace BDM_P.WebForms
{
    public partial class ViewImages : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string videoRelPath = Session["CurrentVideoRelativePath"] as string;
                // get the db video id from session (if any)
                var vidObj = Session["CurrentVideoDbId"];
                if (vidObj != null)
                    hfVideoId.Value = vidObj.ToString();
                else
                    hfVideoId.Value = "";

                if (string.IsNullOrEmpty(videoRelPath))
                {
                    hfVideoUrl.Value = "";
                    return;
                }

                hfVideoUrl.Value = ResolveUrl(videoRelPath);
            }
        }

        protected void btnInapoi_Click(object sender, EventArgs e)
        {
            Response.Redirect("Main.aspx");
        }

        // Salvează dataURI în tabela de imagini neprelucrate (BLOB) și returnează URL-ul /Unprocessed/Image?id=...
        // EnableSession = true pentru a citi session
        [WebMethod(EnableSession = true)]
        public static string SaveFrame(string dataUri, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dataUri))
                    throw new ArgumentException("dataUri este gol");

                var comma = dataUri.IndexOf(',');
                if (comma < 0)
                    throw new ArgumentException("dataUri invalid");

                var base64 = dataUri.Substring(comma + 1);
                var bytes = Convert.FromBase64String(base64);

                var svc = new UnprocessedService();
                int id = svc.GetNextId();

                // Obține vid_id din sesiune (nullable)
                var vidObj = HttpContext.Current.Session["CurrentVideoDbId"];
                int? vidId = vidObj == null ? (int?)null : Convert.ToInt32(vidObj);

                svc.Insert(id, bytes, vidId);

                var rel = "~/Unprocessed/Image?id=" + id;
                return VirtualPathUtility.ToAbsolute(rel);
            }
            catch (Exception ex)
            {
                throw new Exception("SaveFrame failed: " + ex.Message);
            }
        }

        // Salvează și setează sesiunea SelectedImage pentru pagina de prelucrare
        [WebMethod(EnableSession = true)]
        public static string SaveAndSelectFrame(string dataUri, string fileName)
        {
            try
            {
                var relPath = SaveFrame(dataUri, fileName);

                HttpContext.Current.Session["SelectedImage"] = relPath;
                HttpContext.Current.Session["SelectedImageData"] = dataUri;

                return relPath;
            }
            catch (Exception ex)
            {
                throw new Exception("SaveAndSelectFrame failed: " + ex.Message);
            }
        }
    }
}
