<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="BDM_P.WebForms.Edit" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Prelucrează imagine</title>
    <meta charset="utf-8" />
    <style>
        :root{--blue:#007acc;--green:#28a745;--card:#fff;--bg:#f7f9fb;--muted:#333;--radius:10px;--gap:12px;}
        body{background:var(--bg);font-family:Inter,Segoe UI,Arial,sans-serif}
        .wrap{max-width:960px;margin:30px auto;background:var(--card);padding:20px;border-radius:var(--radius)}
        .img-holder{background:#f8fbff;padding:12px;border-radius:8px;border:1px solid #eef6ff}
        .img-holder img{width:100%;height:auto;border-radius:6px;display:block}
        .toolbar{display:flex;gap:12px;align-items:center;margin-top:12px;flex-wrap:wrap}
        .small{padding:6px 10px;font-size:13px;border-radius:8px}
        .btn{padding:8px 12px;border-radius:8px;border:0;color:#fff;cursor:pointer}
        .btn-blue{background:var(--blue)}
        .btn-green{background:var(--green)}
        .status{margin-top:10px;color:var(--muted);min-height:1.2em}
        #controls-right{margin-left:auto;display:flex;gap:8px;align-items:center}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" ID="ScriptManager1" EnablePageMethods="true" />
        <div class="wrap">
            <h2>Prelucrare imagine (server-side)</h2>

            <asp:HiddenField ID="hfImageUrl" runat="server" />
            <asp:HiddenField ID="hfOriginalDataUri" runat="server" />

            <div class="img-holder" style="margin-top:8px;">
                <img id="imgToEdit" runat="server" src="" alt="Imagine pentru editare" />
            </div>

            <div class="toolbar">
                <label for="ddlEdit">Operație</label>
                <select id="ddlEdit" class="small">
                    <option value="rotate">Rotire (grade)</option>
                    <option value="brightness">Luminozitate (-100..100)</option>
                    <option value="contrast">Contrast (-100..100)</option>
                </select>
                <input id="tbEdit" type="text" placeholder="Valoare (ex: 90 sau 15)" class="small" style="width:160px" />
                <button id="btnApplyEdit" type="button" class="btn btn-blue small">Aplică (server)</button>
                <button id="btnReset" type="button" class="btn small" style="background:#6c757d;color:#fff">Reset</button>
                <div id="controls-right">
                    <button id="btnSaveDb" type="button" class="btn btn-green small">Salvează în baza de date</button>
                    <asp:Button ID="btnInapoiEdit" runat="server" Text="⟵ Înapoi" CssClass="btn btn-blue small" OnClick="btnInapoiEdit_Click" />
                </div>
            </div>

            <div class="status" id="status"> </div>
        </div>
    </form>

    <script>
        const imgEl = document.getElementById('<%= imgToEdit.ClientID %>');
        const hfOriginal = document.getElementById('<%= hfOriginalDataUri.ClientID %>');
        const ddl = document.getElementById('ddlEdit');
        const tb = document.getElementById('tbEdit');
        const status = document.getElementById('status');

        let originalDataUrl = null;
        let currentDataUrl = null;

        (function init() {
            originalDataUrl = '<%= (hfImageUrl.Value ?? "") %>';
            if (!originalDataUrl) {
                originalDataUrl = hfOriginal.value || '';
            }
            if (!originalDataUrl) {
                status.textContent = "Nicio imagine disponibilă pentru editare.";
                disableAll(true);
                return;
            }
            currentDataUrl = originalDataUrl;
            imgEl.src = currentDataUrl;
            status.textContent = "Imagine încărcată pentru editare.";
        })();

        function disableAll(disabled) {
            document.querySelectorAll('button, select, input').forEach(el => el.disabled = disabled);
        }

        document.getElementById('btnApplyEdit').addEventListener('click', () => {
            if (!currentDataUrl) { status.textContent = "Nu există imagine."; return; }
            const op = ddl.value;
            const param = tb.value || '';

            status.textContent = 'Se procesează imaginea pe server...';
            PageMethods.ApplyEditServer(currentDataUrl, op, param,
                function (response) {
                    try {
                        const r = JSON.parse(response);
                        if (!r.success) {
                            status.textContent = 'Eroare la procesare: ' + (r.error || 'unknown');
                            return;
                        }
                        currentDataUrl = r.dataUri;
                        imgEl.src = currentDataUrl;
                        status.textContent = 'Editare aplicată (server).';
                    } catch (ex) {
                        status.textContent = 'Răspuns invalid de la server.';
                    }
                },
                function (err) {
                    status.textContent = 'Eroare la apelarea serverului: ' + (err && err.get_message ? err.get_message() : err);
                }
            );
        });

        document.getElementById('btnReset').addEventListener('click', () => {
            if (!originalDataUrl) return;
            currentDataUrl = originalDataUrl;
            imgEl.src = currentDataUrl;
            status.textContent = 'Reset la original.';
        });

        document.getElementById('btnSaveDb').addEventListener('click', () => {
            if (!currentDataUrl) { status.textContent = 'Nu există imagine de salvat.'; return; }
            status.textContent = 'Se salvează imaginea procesată...';

            PageMethods.SaveProcessedImage(currentDataUrl, 'processed_' + (new Date()).getTime() + '.jpg',
                function (response) {
                    try {
                        const r = JSON.parse(response);
                        if (r.success) {
                            status.textContent = 'Imagine procesată salvată cu succes! ID: ' + r.processedId;
                        } else {
                            status.textContent = 'Eroare la salvare: ' + (r.error || 'unknown');
                        }
                    } catch (ex) { status.textContent = 'Răspuns invalid la salvare.'; }
                },
                function (err) { status.textContent = 'Eroare la salvare: ' + (err && err.get_message ? err.get_message() : err); }
            );
        });
    </script>
</body>
</html>
