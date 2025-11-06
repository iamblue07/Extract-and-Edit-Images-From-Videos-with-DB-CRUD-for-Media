using BDM_P.Services;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;

namespace BDM_P.WebForms
{
    public partial class Edit : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var selData = Session["SelectedImageData"] as string ?? "";
                if (!string.IsNullOrEmpty(selData))
                {
                    hfOriginalDataUri.Value = selData;
                    hfImageUrl.Value = selData;
                    imgToEdit.Src = selData;
                }
            }
        }

        protected void btnInapoiEdit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ViewImages.aspx");
        }

        [WebMethod(EnableSession = true)]
        public static string ApplyEditServer(string imageDataUri, string op, string param)
        {
            try
            {
                if (string.IsNullOrEmpty(imageDataUri))
                    return "{\"success\":false,\"error\":\"No image data provided\"}";

                var parts = imageDataUri.Split(',');
                if (parts.Length != 2)
                    return "{\"success\":false,\"error\":\"Invalid data URI\"}";

                string base64 = parts[1];
                byte[] imageBytes = Convert.FromBase64String(base64);

                var procSvc = new ImageProcessingService();
                byte[] processed = procSvc.ProcessImage(imageBytes, op, param);

                if (processed == null || processed.Length == 0)
                    return "{\"success\":false,\"error\":\"Processing returned empty result\"}";

                string outBase64 = Convert.ToBase64String(processed);
                string dataUri = "data:image/jpeg;base64," + outBase64;

                return $"{{\"success\":true,\"dataUri\":\"{dataUri}\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"success\":false,\"error\":\"{HttpUtility.JavaScriptStringEncode(ex.Message)}\"}}";
            }
        }

        [WebMethod(EnableSession = true)]
        public static string SaveProcessedImage(string imageData, string imageName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageData))
                    return "{\"success\":false,\"error\":\"No image data provided\"}";

                var base64Data = imageData.Split(',').Last();
                var imageBytes = Convert.FromBase64String(base64Data);

                var vidObj = HttpContext.Current.Session["CurrentVideoDbId"];
                int? videoId = vidObj == null ? (int?)null : Convert.ToInt32(vidObj);

                var processedService = new Services.ProcessedService();
                int processedId = processedService.GetNextId();
                processedService.Insert(processedId, imageBytes, videoId);

                return $"{{\"success\":true,\"processedId\":{processedId},\"message\":\"Imagine procesată salvată cu ID: {processedId}\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"success\":false,\"error\":\"{HttpUtility.JavaScriptStringEncode(ex.Message)}\"}}";
            }
        }
    }
}
