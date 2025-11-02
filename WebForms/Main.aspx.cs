using System;
using System.IO;
using System.Web;
using System.Web.UI;
using BDM_P.Services;

namespace BDM_P
{
    public partial class Main : System.Web.UI.Page
    {
        private readonly string[] AllowedExtensions = { ".mp4", ".mov", ".avi", ".mkv", ".webm" };
        private const int MaxFileSizeBytes = 200 * 1024 * 1024; // 200 MB

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlVideoContainer.Visible = false;
                lblStatus.Text = "";
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoFileUpload.HasFile && videoFileUpload.PostedFile.ContentLength > 0)
                {
                    var posted = videoFileUpload.PostedFile;
                    var ext = Path.GetExtension(posted.FileName).ToLowerInvariant();

                    if (Array.IndexOf(AllowedExtensions, ext) < 0)
                    {
                        lblStatus.Text = "Tip de fișier nepermis. Folosește mp4 / mov / avi / mkv / webm.";
                        return;
                    }

                    if (posted.ContentLength > MaxFileSizeBytes)
                    {
                        lblStatus.Text = "Fișier prea mare. Limita actuală este 200 MB.";
                        return;
                    }

                    var videosFolder = Server.MapPath("~/Videos");
                    if (!Directory.Exists(videosFolder))
                        Directory.CreateDirectory(videosFolder);

                    var fileName = Path.GetFileName(posted.FileName);
                    var savePath = Path.Combine(videosFolder, fileName);

                    if (File.Exists(savePath))
                        File.Delete(savePath);

                    posted.SaveAs(savePath);

                    var relativePath = "~/Videos/" + fileName;
                    Session["CurrentVideoRelativePath"] = relativePath;

                    try
                    {
                        var svc = new VideoService();
                        var nextId = svc.GetNextId();
                        byte[] bytes = File.ReadAllBytes(savePath);
                        svc.Insert(nextId, bytes, fileName);
                        Session["CurrentVideoDbId"] = nextId;
                        lblStatus.Text = $"Videoclip încărcat local și salvat în baza de date (id={nextId}): {fileName}";
                    }
                    catch (Exception dbEx)
                    {
                        lblStatus.Text = $"Videoclip salvat local, dar eroare la încărcarea în baza de date: {dbEx.Message}";
                    }

                    pnlVideoContainer.Visible = true;
                    SetVideoSource(relativePath);
                }
                else
                {
                    lblStatus.Text = "Selectează un fișier video înainte de a încărca.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Eroare la încărcare: " + ex.Message;
            }
        }

        protected void btnLoadFromDb_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtVideoId.Text))
                {
                    lblStatus.Text = "Introdu un ID video valid.";
                    return;
                }

                if (!int.TryParse(txtVideoId.Text.Trim(), out int id))
                {
                    lblStatus.Text = "ID invalid (trebuie număr).";
                    return;
                }

                var svc = new VideoService();
                var data = svc.GetVideoById(id);
                if (data == null || data.Length == 0)
                {
                    lblStatus.Text = $"Videoclipul cu id={id} nu a fost găsit în baza de date.";
                    return;
                }

                var origName = svc.GetVideoNameById(id);
                if (string.IsNullOrEmpty(origName))
                {
                    origName = $"video_{id}.mp4";
                }

                var videosFolder = Server.MapPath("~/Videos");
                if (!Directory.Exists(videosFolder))
                    Directory.CreateDirectory(videosFolder);

                var fileName = $"db_{id}_{Path.GetFileName(origName)}";
                var savePath = Path.Combine(videosFolder, fileName);

                if (File.Exists(savePath))
                    File.Delete(savePath);

                File.WriteAllBytes(savePath, data);

                var relativePath = "~/Videos/" + fileName;
                Session["CurrentVideoRelativePath"] = relativePath;
                Session["CurrentVideoDbId"] = id;

                pnlVideoContainer.Visible = true;
                SetVideoSource(relativePath);

                lblStatus.Text = $"Videoclip încărcat din baza de date: id={id}, fișier local: {fileName}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Eroare la încărcarea din baza de date: " + ex.Message;
            }
        }

        protected void btnVeziImagini_Click(object sender, EventArgs e)
        {
            var videoPath = Session["CurrentVideoRelativePath"] as string;

            if (!string.IsNullOrEmpty(videoPath))
            {
                Response.Redirect("ViewImages.aspx");
            }
            else
            {
                lblStatus.Text = "Trebuie mai întâi să încarci un videoclip.";
            }
        }

        protected void btnSterge_Click(object sender, EventArgs e)
        {
            try
            {
                int? dbId = null;
                if (Session["CurrentVideoDbId"] != null)
                {
                    if (int.TryParse(Session["CurrentVideoDbId"].ToString(), out int parsed))
                        dbId = parsed;
                }

                var videoPath = Session["CurrentVideoRelativePath"] as string;

                if (dbId.HasValue)
                {
                    try
                    {
                        var svc = new VideoService();
                        svc.DeleteById(dbId.Value);
                        Session.Remove("CurrentVideoDbId");
                    }
                    catch (Exception dbEx)
                    {
                        lblStatus.Text = "Eroare la ștergerea din baza de date: " + dbEx.Message;
                    }
                }

                if (!string.IsNullOrEmpty(videoPath))
                {
                    var fullPath = Server.MapPath(videoPath);
                    if (File.Exists(fullPath))
                    {
                        try
                        {
                            File.Delete(fullPath);
                        }
                        catch (Exception fEx)
                        {
                            lblStatus.Text = "Eroare la ștergerea fișierului local: " + fEx.Message;
                        }
                    }

                    Session.Remove("CurrentVideoRelativePath");
                }

                pnlVideoContainer.Visible = false;

                if (string.IsNullOrEmpty(lblStatus.Text))
                {
                    if (dbId.HasValue)
                        lblStatus.Text = $"Videoclipul a fost șters din baza de date (id={dbId.Value}) și copia locală a fost eliminată.";
                    else if (!string.IsNullOrEmpty(videoPath))
                        lblStatus.Text = "Copia locală a videoclipului a fost ștearsă.";
                    else
                        lblStatus.Text = "Nu exista videoclip de șters.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Eroare la ștergere: " + ex.Message;
            }
        }


        protected void btnCauta_Click(object sender, EventArgs e)
        {
            Response.Redirect("Search.aspx");
        }
                
        private void SetVideoSource(string relativePath)
        {
            videoPlayer.Attributes["src"] = ResolveUrl(relativePath);
        }
    }
}
