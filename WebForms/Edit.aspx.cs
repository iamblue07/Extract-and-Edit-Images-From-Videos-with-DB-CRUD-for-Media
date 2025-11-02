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
                var sel = Session["SelectedImage"] as string ?? "";
                var selData = Session["SelectedImageData"] as string ?? "";

                hfSelectedImageSession.Value = sel ?? "";

                if (!string.IsNullOrEmpty(selData))
                {
                    hfImageUrl.Value = selData;
                    imgToEdit.Src = selData;
                    return;
                }

                if (string.IsNullOrEmpty(sel))
                {
                    hfImageUrl.Value = "";
                    imgToEdit.Src = "";
                    return;
                }

                string abs;
                try
                {
                    abs = VirtualPathUtility.ToAbsolute(sel);
                }
                catch
                {
                    abs = sel;
                }

                hfImageUrl.Value = abs;
                imgToEdit.Src = abs;
            }
        }

        protected void btnInapoiEdit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ViewImages.aspx");
        }

        [WebMethod]
        public static string SaveProcessedImage(string imageData, string imageName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageData))
                    return "{\"success\":false,\"error\":\"No image data provided\"}";

                // Extract base64 data from data URI
                var base64Data = imageData.Split(',').Last();
                var imageBytes = Convert.FromBase64String(base64Data);

                // Get video id from session - this will link processed to unprocessed
                var vidObj = HttpContext.Current.Session["CurrentVideoDbId"];
                int? videoId = vidObj == null ? (int?)null : Convert.ToInt32(vidObj);

                // Get service
                var processedService = new Services.ProcessedService();

                // Save processed image with video id
                int processedId = processedService.GetNextId();
                processedService.Insert(processedId, imageBytes, videoId);

                return $"{{\"success\":true,\"processedId\":{processedId},\"message\":\"Imagine procesată salvată cu ID: {processedId}\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"success\":false,\"error\":\"{ex.Message}\"}}";
            }
        }

        // ----------------- Helper Methods (removed ExtractVideoNameFromSession) -----------------
    }
}
