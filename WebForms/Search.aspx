<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="BDM_P.WebForms.Search" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Caută</title>
    <meta charset="utf-8" />
    <style>
        :root{
            --blue:#007acc;
            --blue-strong:#005fa3;
            --green:#28a745;
            --red:#e74c3c;
            --bg:#f6f8fb;
            --card:#ffffff;
            --muted:#333;
            --radius:10px;
            --gap:12px;
            --shadow:0 6px 14px rgba(0,0,0,0.05);
        }

        html,body{height:100%;margin:0;padding:0;background:var(--bg);font-family:Inter, "Segoe UI", Tahoma, Arial, sans-serif;color:#222}
        .wrap{max-width:920px;margin:28px auto;background:var(--card);padding:20px;border-radius:var(--radius);box-shadow:var(--shadow);box-sizing:border-box}
        h2{margin:0 0 14px 0;font-weight:600;font-size:20px}
        .search-row{display:flex;gap:var(--gap);align-items:center;margin-bottom:14px;flex-wrap:wrap}
        .search-row .txt-ctrl{flex:1;min-width:200px;padding:10px 12px;border-radius:8px;border:1px solid #dcdfe6;font-size:14px}
        .search-row .btn{padding:9px 14px;border-radius:8px;border:0;cursor:pointer;font-weight:600;color:#fff;box-shadow:0 2px 6px rgba(0,0,0,0.04)}
        .btn-blue{background:var(--blue)}
        .btn-blue:hover{background:var(--blue-strong)}
        .btn-green{background:var(--green)}
        .btn-green:hover{background:#1e7e34}
        .btn-red{background:var(--red)}
        .btn-red:hover{background:#c0392b}
        .results{margin-top:16px;display:flex;gap:12px;align-items:center;justify-content:center;flex-wrap:wrap}
        .results .nav{display:flex;gap:8px;align-items:center}
        .img-frame{border-radius:8px;border:1px solid #e9f2fb;max-width:460px;max-height:360px;overflow:hidden;display:flex;align-items:center;justify-content:center;background:#fafcff}
        .img-frame img{max-width:100%;max-height:100%;display:block;border-radius:6px}
        .meta{text-align:center;margin-top:12px;color:var(--muted);font-size:14px}
        .txt-error{width:100%;border-radius:8px;border:1px solid #f5c6cb;background:#fff3f3;padding:10px;color:#a71d2a;box-sizing:border-box}
        @media(max-width:640px){
            .search-row{flex-direction:column;align-items:stretch}
            .search-row .btn{width:100%}
            .results{flex-direction:column}
            .img-frame{width:100%;max-width:100%}
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
        <div class="wrap">
            <h2>Căutare după ID-ul videoclipului</h2>

            <div class="search-row">
                <asp:TextBox ID="TextBox1" runat="server" CssClass="txt-ctrl" Placeholder="Introdu ID-ul videoclipului..." />
                <asp:Button ID="btnEdited" runat="server" Text="Vezi procesate" CssClass="btn btn-blue" OnClick="btnEdited_Click" />
                <asp:Button ID="btnRaw" runat="server" Text="Vezi neprocesate" CssClass="btn btn-blue" OnClick="btnRaw_Click" />
                <asp:Button ID="btnBack" runat="server" Text="⟵ Înapoi" CssClass="btn btn-blue" OnClick="btnBack_Click" />
                <asp:Button ID="btnDelete" runat="server" Text="Șterge" CssClass="btn btn-red" OnClick="btnDelete_Click" />
            </div>
            <h2>Căutare după imagine (semantic)</h2>
            <div class="search-row">
                <asp:FileUpload ID="fileQuery" runat="server" CssClass="txt-ctrl" />

                <div style="display:flex;gap:8px;align-items:center;">
                    <label for="rngThreshold" style="font-size:13px;color:var(--muted);margin-right:6px;">Nivel similitudine: mai mic = mai putin similar</label>
                    <input id="rngThreshold" type="range" min="1" max="100" value="10" oninput="document.getElementById('rngVal').innerText = this.value;" />
                    <span id="rngVal" style="min-width:30px;text-align:center;display:inline-block">10</span>
                    <asp:TextBox ID="txtThreshold" runat="server" Text="10" Style="display:none" />

                    <asp:Button ID="btnSearchByImage" runat="server" Text="Caută imagini similare"
                                CssClass="btn btn-green" OnClick="btnSearchByImage_Click"
                                OnClientClick="return syncThreshold();" />
                    <asp:Button ID="btnClearSimilar" runat="server" Text="Curăță" CssClass="btn btn-blue" OnClick="btnClearSimilar_Click" />
                </div>
            </div>

            <script type="text/javascript">
                function syncThreshold() {
                    try {
                        var tb = document.getElementById('<%= txtThreshold.ClientID %>');
                        var rng = document.getElementById('rngThreshold');
                        if (tb && rng) tb.value = rng.value;
                    } catch (e) {
                    }
                    return true;
                }
            </script>


            <div class="results">
                <div class="nav">
                    <asp:Button ID="btnPrevSearch" runat="server" Text="❮ Prev" CssClass="btn btn-blue" OnClick="btnPrevSearch_Click" />
                </div>

                <div class="img-frame">
                    <asp:Image ID="imgSearch" runat="server" Height="320px" Width="460px" />
                </div>

                <div class="nav">
                    <asp:Button ID="btnNextSearch" runat="server" Text="Next ❯" CssClass="btn btn-blue" OnClick="btnNextSearch_Click" />
                </div>
            </div>

            <div style="margin-top:10px; display:flex; gap:12px; justify-content:center; align-items:center;">
                <asp:Button ID="btnDownload" runat="server" Text="Descarcă" CssClass="btn btn-blue"
            OnClientClick="return downloadCurrentImage();" />
            </div>

            <div class="meta">
                <asp:Label ID="lblMeta" runat="server" Text=""></asp:Label>
            </div>

            <div style="margin-top:12px;">
                <asp:TextBox ID="txtError" runat="server" TextMode="MultiLine" Rows="2" CssClass="txt-error" ReadOnly="true" />
            </div>
        </div>
    </form>
            <script type="text/javascript">
          function downloadCurrentImage() {
            try {
              var img = document.getElementById('<%= imgSearch.ClientID %>');
              if (!img || !img.src) {
                alert('Nu este încărcată nicio imagine.');
                return false;
              }

              var src = img.src;

              if (src.indexOf('data:') === 0) {
                var mime = src.substring(5, src.indexOf(';'));
                var ext = '.jpg';
                if (mime === 'image/png') ext = '.png';
                else if (mime === 'image/gif') ext = '.gif';
                else if (mime === 'image/webp') ext = '.webp';
                else if (mime === 'image/tiff') ext = '.tif';
                else if (mime === 'image/bmp') ext = '.bmp';

                var a = document.createElement('a');
                a.href = src;
                a.download = 'image_' + Date.now() + ext;
                // Firefox requires the link to be in the DOM
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                return false; // prevent postback
              }

              // Else: src is a URL (likely /Processed/Image?id=123). Fetch it and download.
              // Use credentials so same-session cookies are sent.
              fetch(src, { credentials: 'include' })
                .then(function (resp) {
                  if (!resp.ok) throw new Error('Răspuns nereușit: ' + resp.status);
                  return resp.blob();
                })
                .then(function (blob) {
                  var url = URL.createObjectURL(blob);
                  var a = document.createElement('a');
                  a.href = url;

                  // try infer extension from blob.type
                  var ext = '.jpg';
                  if (blob.type) {
                    if (blob.type === 'image/png') ext = '.png';
                    else if (blob.type === 'image/gif') ext = '.gif';
                    else if (blob.type === 'image/webp') ext = '.webp';
                    else if (blob.type === 'image/tiff') ext = '.tif';
                    else if (blob.type === 'image/bmp') ext = '.bmp';
                    else if (blob.type === 'image/jpeg') ext = '.jpg';
                  }

                  a.download = 'image_' + Date.now() + ext;
                  document.body.appendChild(a);
                  a.click();

                  // cleanup
                  setTimeout(function () {
                    URL.revokeObjectURL(url);
                    try { document.body.removeChild(a); } catch (e) { /* ignore */ }
                  }, 150);

                })
                .catch(function (err) {
                  alert('Descărcare eșuată: ' + err.message);
                });

              return false; // prevent postback
            } catch (ex) {
              alert('Eroare la descărcare: ' + ex.message);
              return false;
            }
          }
        </script>

</body>
</html>
