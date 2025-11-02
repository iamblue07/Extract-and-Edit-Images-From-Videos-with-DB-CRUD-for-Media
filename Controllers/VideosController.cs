using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BDM_P.Services;

namespace BDM_P.Controllers
{
    public class VideosController : Controller
    {
        private readonly VideoService videoService = new VideoService();

        public ActionResult Index()
        {
            var items = videoService.GetAll();
            return View(items);
        }

        // Streams the video bytes back to the client.
        // Attempts to set a reasonable MIME type based on the stored file name extension.
        public ActionResult Video(int id)
        {
            var data = videoService.GetVideoById(id);
            if (data == null)
            {
                System.Diagnostics.Trace.TraceWarning($"Video not found id={id}");
                return HttpNotFound();
            }

            try
            {
                Response.AppendHeader("X-Video-Size", data.Length.ToString());
            }
            catch { /* ignore header set errors */ }

            // Try to resolve a content-type from the stored name
            var name = videoService.GetVideoNameById(id) ?? string.Empty;
            var mime = GetMimeTypeFromName(name) ?? "application/octet-stream";

            try
            {
                Response.ContentType = mime;
            }
            catch { /* ignore */ }

            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            return File(data, mime);
        }

        [HttpPost]
        public ActionResult Upload(int videoId, string videoName)
        {
            var f = Request.Files.Count > 0 ? Request.Files[0] : null;
            if (f == null || f.ContentLength == 0)
                return new HttpStatusCodeResult(400, "No file uploaded");

            using (var ms = new MemoryStream())
            {
                f.InputStream.CopyTo(ms);
                videoService.Insert(videoId, ms.ToArray(), videoName);
            }

            return Content("OK");
        }

        // rudimentary mapping by extension -> mime
        private string GetMimeTypeFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var ext = Path.GetExtension(name).ToLowerInvariant();
            switch (ext)
            {
                case ".mp4": return "video/mp4";
                case ".mov": return "video/quicktime";
                case ".wmv": return "video/x-ms-wmv";
                case ".flv": return "video/x-flv";
                case ".mkv": return "video/x-matroska";
                case ".webm": return "video/webm";
                case ".avi": return "video/x-msvideo";
                default: return null;
            }
        }
    }
}
