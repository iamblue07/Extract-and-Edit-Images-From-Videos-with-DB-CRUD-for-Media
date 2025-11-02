using System;
using System.Collections.Generic;
using System.Web;
using BDM_P.Services;

namespace BDM_P.WebForms
{
    public partial class Search : System.Web.UI.Page
    {
        private const string VS_RESULTS_KEY = "SearchResults";
        private const string VS_INDEX_KEY = "SearchIndex";
        private const string VS_IS_PROCESSED = "SearchIsProcessed";
        private readonly ProcessedService _pSvc = new ProcessedService();
        private readonly UnprocessedService _uSvc = new UnprocessedService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState[VS_RESULTS_KEY] = null;
                ViewState[VS_INDEX_KEY] = 0;
                ViewState[VS_IS_PROCESSED] = false;
                lblMeta.Text = "";
                txtError.Text = "";
                imgSearch.ImageUrl = "";
            }
        }

        protected void btnEdited_Click(object sender, EventArgs e) => DoSearch(isProcessed: true);
        protected void btnRaw_Click(object sender, EventArgs e) => DoSearch(isProcessed: false);

        private void DoSearch(bool isProcessed)
        {
            txtError.Text = "";
            lblMeta.Text = "";
            imgSearch.ImageUrl = "";

            if (string.IsNullOrWhiteSpace(TextBox1.Text))
            {
                txtError.Text = "Introdu ID-ul videoclipului (număr).";
                return;
            }

            if (!int.TryParse(TextBox1.Text.Trim(), out int vidId))
            {
                txtError.Text = "ID invalid — folosește un număr întreg.";
                return;
            }

            try
            {
                List<int> ids = isProcessed ? _pSvc.GetIdsByVideoId(vidId) : _uSvc.GetIdsByVideoId(vidId);

                if (ids == null || ids.Count == 0)
                {
                    txtError.Text = $"Nu s-au găsit imagini {(isProcessed ? "procesate" : "neprocesate")} pentru videoclipul cu id={vidId}.";
                    ViewState[VS_RESULTS_KEY] = null;
                    ViewState[VS_INDEX_KEY] = 0;
                    ViewState[VS_IS_PROCESSED] = isProcessed;
                    lblMeta.Text = "";
                    return;
                }

                ViewState[VS_RESULTS_KEY] = ids;
                ViewState[VS_INDEX_KEY] = 0;
                ViewState[VS_IS_PROCESSED] = isProcessed;

                ShowCurrent();
            }
            catch (Exception ex)
            {
                txtError.Text = "Eroare la interogare: " + ex.Message;
            }
        }

        protected void btnPrevSearch_Click(object sender, EventArgs e) => AdjustIndex(-1);
        protected void btnNextSearch_Click(object sender, EventArgs e) => AdjustIndex(+1);

        protected void btnBack_Click(object sender, EventArgs e)
        {
            // redirect către Main.aspx
            Response.Redirect("Main.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            txtError.Text = "";

            var ids = ViewState[VS_RESULTS_KEY] as List<int>;
            if (ids == null || ids.Count == 0)
            {
                txtError.Text = "Nu există rezultate pentru ștergere.";
                return;
            }

            int idx = (int)(ViewState[VS_INDEX_KEY] ?? 0);
            if (idx < 0) idx = 0;
            if (idx >= ids.Count) idx = ids.Count - 1;

            int idToDelete = ids[idx];
            bool isProcessed = (bool)(ViewState[VS_IS_PROCESSED] ?? false);

            try
            {
                if (isProcessed)
                    _pSvc.DeleteById(idToDelete);
                else
                    _uSvc.DeleteById(idToDelete);

                // elimină din listă și actualizează ViewState/index
                ids.RemoveAt(idx);

                if (ids.Count == 0)
                {
                    ViewState[VS_RESULTS_KEY] = null;
                    ViewState[VS_INDEX_KEY] = 0;
                    imgSearch.ImageUrl = "";
                    lblMeta.Text = "";
                    txtError.Text = "Imaginea a fost ștearsă.";
                    return;
                }

                // ajustează indexul dacă era la final
                if (idx >= ids.Count) idx = ids.Count - 1;
                ViewState[VS_INDEX_KEY] = idx;
                ViewState[VS_RESULTS_KEY] = ids;

                txtError.Text = "Imaginea a fost ștearsă.";
                ShowCurrent();
            }
            catch (Exception ex)
            {
                txtError.Text = "Eroare la ștergere: " + ex.Message;
            }
        }


        private void AdjustIndex(int delta)
        {
            var ids = ViewState[VS_RESULTS_KEY] as List<int>;
            if (ids == null || ids.Count == 0)
            {
                txtError.Text = "Nu există rezultate.";
                return;
            }

            int idx = (int)(ViewState[VS_INDEX_KEY] ?? 0);
            idx += delta;
            if (idx < 0) idx = 0;
            if (idx >= ids.Count) idx = ids.Count - 1;
            ViewState[VS_INDEX_KEY] = idx;
            ShowCurrent();
        }

        // === REPLACED ShowCurrent() ===
        private void ShowCurrent()
        {
            var ids = ViewState[VS_RESULTS_KEY] as List<int>;
            if (ids == null || ids.Count == 0)
            {
                txtError.Text = "Nu există rezultate.";
                imgSearch.ImageUrl = "";
                lblMeta.Text = "";
                return;
            }

            int idx = (int)(ViewState[VS_INDEX_KEY] ?? 0);
            if (idx < 0) idx = 0;
            if (idx >= ids.Count) idx = ids.Count - 1;

            int id = ids[idx];
            bool isProcessed = (bool)(ViewState[VS_IS_PROCESSED] ?? false);

            try
            {
                byte[] data = isProcessed ? _pSvc.GetImageById(id) : _uSvc.GetImageById(id);

                if (data == null || data.Length == 0)
                {
                    txtError.Text = $"Imaginea cu id={id} nu există sau este goală.";
                    imgSearch.ImageUrl = "";
                    lblMeta.Text = $"Rezultat {idx + 1} din {ids.Count} — id imagine: {id} — {(isProcessed ? "procesată" : "neprocesată")}";
                    return;
                }

                // Construiește data URL (presupunem image/jpeg)
                string base64 = Convert.ToBase64String(data);
                string dataUrl = "data:image/jpeg;base64," + base64;

                imgSearch.ImageUrl = dataUrl;
                lblMeta.Text = $"Rezultat {idx + 1} din {ids.Count} — id imagine: {id} — {(isProcessed ? "procesată" : "neprocesată")}";
                txtError.Text = "";
            }
            catch (Exception ex)
            {
                txtError.Text = "Eroare la obținerea imaginii: " + ex.Message;
                imgSearch.ImageUrl = "";
                lblMeta.Text = "";
            }
        }


    }
}
