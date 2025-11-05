using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using BDM_P.Services;

namespace BDM_P.WebForms
{
    public partial class Search : System.Web.UI.Page
    {
        private const string VS_RESULTS_KEY = "SearchResults";
        private const string VS_INDEX_KEY = "SearchIndex";
            
        private readonly ProcessedService _pSvc = new ProcessedService();
        private readonly UnprocessedService _uSvc = new UnprocessedService();
        private readonly SimilarityService _simSvc = new SimilarityService();

        [Serializable]
        private class SearchResult
        {
            public bool IsProcessed { get; set; }
            public int Id { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState[VS_RESULTS_KEY] = null;
                ViewState[VS_INDEX_KEY] = 0;
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
                    lblMeta.Text = "";
                    return;
                }

                var results = new List<SearchResult>(ids.Count);
                foreach (var i in ids) results.Add(new SearchResult { Id = i, IsProcessed = isProcessed });

                ViewState[VS_RESULTS_KEY] = results;
                ViewState[VS_INDEX_KEY] = 0;

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
            Response.Redirect("Main.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            txtError.Text = "";

            var results = ViewState[VS_RESULTS_KEY] as List<SearchResult>;
            if (results == null || results.Count == 0)
            {
                txtError.Text = "Nu există rezultate pentru ștergere.";
                return;
            }

            int idx = (int)(ViewState[VS_INDEX_KEY] ?? 0);
            if (idx < 0) idx = 0;
            if (idx >= results.Count) idx = results.Count - 1;

            var toDelete = results[idx];

            try
            {
                if (toDelete.IsProcessed)
                    _pSvc.DeleteById(toDelete.Id);
                else
                    _uSvc.DeleteById(toDelete.Id);

                results.RemoveAt(idx);

                if (results.Count == 0)
                {
                    ViewState[VS_RESULTS_KEY] = null;
                    ViewState[VS_INDEX_KEY] = 0;
                    imgSearch.ImageUrl = "";
                    lblMeta.Text = "";
                    txtError.Text = "Imaginea a fost ștearsă.";
                    return;
                }

                if (idx >= results.Count) idx = results.Count - 1;
                ViewState[VS_INDEX_KEY] = idx;
                ViewState[VS_RESULTS_KEY] = results;

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
            var results = ViewState[VS_RESULTS_KEY] as List<SearchResult>;
            if (results == null || results.Count == 0)
            {
                txtError.Text = "Nu există rezultate.";
                return;
            }

            int idx = (int)(ViewState[VS_INDEX_KEY] ?? 0);
            idx += delta;
            if (idx < 0) idx = 0;
            if (idx >= results.Count) idx = results.Count - 1;
            ViewState[VS_INDEX_KEY] = idx;
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            var results = ViewState[VS_RESULTS_KEY] as List<SearchResult>;
            if (results == null || results.Count == 0)
            {
                txtError.Text = "Nu există rezultate.";
                imgSearch.ImageUrl = "";
                lblMeta.Text = "";
                return;
            }

            int idx = (int)(ViewState[VS_INDEX_KEY] ?? 0);
            if (idx < 0) idx = 0;
            if (idx >= results.Count) idx = results.Count - 1;

            var item = results[idx];

            try
            {
                byte[] data = item.IsProcessed ? _pSvc.GetImageById(item.Id) : _uSvc.GetImageById(item.Id);

                if (data == null || data.Length == 0)
                {
                    txtError.Text = $"Imaginea cu id={item.Id} nu există sau este goală.";
                    imgSearch.ImageUrl = "";
                    lblMeta.Text = $"Rezultat {idx + 1} din {results.Count} — id imagine: {item.Id} — {(item.IsProcessed ? "procesată" : "neprocesată")}";
                    return;
                }

                string base64 = Convert.ToBase64String(data);
                string dataUrl = "data:image/jpeg;base64," + base64;

                imgSearch.ImageUrl = dataUrl;
                lblMeta.Text = $"Rezultat {idx + 1} din {results.Count} — id imagine: {item.Id} — {(item.IsProcessed ? "procesată" : "neprocesată")}";
                txtError.Text = "";
            }
            catch (Exception ex)
            {
                txtError.Text = "Eroare la obținerea imaginii: " + ex.Message;
                imgSearch.ImageUrl = "";
                lblMeta.Text = "";
            }
        }

        protected void btnSearchByImage_Click(object sender, EventArgs e)
        {
            txtError.Text = "";
            lblMeta.Text = "";
            imgSearch.ImageUrl = "";

            if (!fileQuery.HasFile || fileQuery.PostedFile.ContentLength == 0)
            {
                txtError.Text = "Selectează o imagine pentru căutare.";
                return;
            }

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                fileQuery.PostedFile.InputStream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            int threshold = 10;
            if (!int.TryParse(txtThreshold.Text, out threshold))
            {
                threshold = 10;
            }
            if (threshold < 1) threshold = 1;
            if (threshold > 100) threshold = 100;

            try
            {
                var simResults = _simSvc.FindSimilar(bytes, threshold: threshold, attrWeights: "color=\"1.0\"");
                if (simResults == null || simResults.Count == 0)
                {
                    txtError.Text = "Nu s-au găsit imagini similare.";
                    ViewState[VS_RESULTS_KEY] = null;
                    ViewState[VS_INDEX_KEY] = 0;
                    lblMeta.Text = "";
                    return;
                }
                var results = new List<SearchResult>(simResults.Count);
                foreach (var r in simResults)
                {
                    results.Add(new SearchResult { Id = r.Id, IsProcessed = r.IsProcessed });
                }

                ViewState[VS_RESULTS_KEY] = results;
                ViewState[VS_INDEX_KEY] = 0;

                ShowCurrent();
                txtError.Text = $"Găsite {results.Count} imagini similare (threshold={threshold}).";
            }
            catch (Exception ex)
            {
                txtError.Text = "Eroare la căutarea semantică: " + ex.Message;
            }
        }


        protected void btnClearSimilar_Click(object sender, EventArgs e)
        {
            ViewState[VS_RESULTS_KEY] = null;
            ViewState[VS_INDEX_KEY] = 0;
            imgSearch.ImageUrl = "";
            lblMeta.Text = "";
            txtError.Text = "";
        }

    }
}
