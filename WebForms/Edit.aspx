<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="BDM_P.WebForms.Edit" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Prelucrează imagine</title>
    <meta charset="utf-8" />
    <style>
        :root{
            --blue:#007acc;
            --blue-strong:#005fa3;
            --green:#28a745;
            --green-strong:#1e7e34;
            --red:#e74c3c;
            --card:#ffffff;
            --bg:#f7f9fb;
            --muted:#333;
            --radius:10px;
            --gap:12px;
            --shadow:0 6px 16px rgba(0,0,0,0.06);
        }

        html,body{height:100%;margin:0;padding:0;background:var(--bg);font-family:Inter,Segoe UI,Arial,sans-serif;color:#222}
        .wrap{max-width:960px;margin:30px auto;background:var(--card);padding:20px;border-radius:var(--radius);box-shadow:var(--shadow)}
        h2{margin:0 0 12px;font-weight:600}
        .img-holder{background:#f8fbff;padding:12px;border-radius:8px;border:1px solid #eef6ff}
        .img-holder img{width:100%;height:auto;border-radius:6px;border:1px solid #e9f0fb;display:block;margin:0 auto}
        .toolbar{display:flex;gap:12px;align-items:center;margin-top:12px;flex-wrap:wrap}
        select,input[type=text]{padding:8px 10px;border-radius:8px;border:1px solid #ddd;font-size:14px}
        .btn{padding:8px 12px;border-radius:8px;border:0;font-weight:600;cursor:pointer;color:#fff}
        .btn-blue{background:var(--blue)}
        .btn-blue:hover{background:var(--blue-strong)}
        .btn-green{background:var(--green)}
        .btn-green:hover{background:var(--green-strong)}
        .btn-red{background:var(--red)}
        .btn-muted{background:#6c757d}
        .status{margin-top:10px;color:var(--muted);font-size:14px;min-height:1.2em}
        #controls-right{margin-left:auto;display:flex;gap:8px;align-items:center}
        .small{padding:6px 10px;font-size:13px;border-radius:8px}
        @media(max-width:680px){#controls-right{width:100%;justify-content:space-between}}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" ID="ScriptManager1" EnablePageMethods="true" />

        <div class="wrap">
            <h2>Prelucrare imagine</h2>

            <asp:HiddenField ID="hfImageUrl" runat="server" />
            <asp:HiddenField ID="hfSelectedImageSession" runat="server" />

            <div class="img-holder" style="margin-top:8px;">
                <img id="imgToEdit" runat="server" src="" alt="Imagine pentru editare" />
            </div>

            <canvas id="editCanvas" style="display:none;"></canvas>

            <div class="toolbar" style="margin-top:12px;">
                <label for="ddlEdit">Operație</label>
                <select id="ddlEdit" class="small">
                    <option value="rotate">Rotire (grade)</option>
                    <option value="brightness">Luminozitate (-100..100)</option>
                    <option value="contrast">Contrast (-100..100)</option>
                    <option value="saturation">Saturație (-100..100)</option>
                    <option value="grayscale">Alb-negru</option>
                    <option value="invert">Invertire</option>
                </select>

                <input id="tbEdit" type="text" placeholder="Valoare (ex: 90 sau 15)" class="small" style="width:160px" />

                <button id="btnApplyEdit" type="button" class="btn btn-blue small">Aplică (pe copie)</button>
                <button id="btnReset" type="button" class="btn btn-muted small">Reset</button>

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
        const hfImageUrl = document.getElementById('<%= hfImageUrl.ClientID %>');
        const hfSelectedImageSession = document.getElementById('<%= hfSelectedImageSession.ClientID %>');
        const canvas = document.getElementById('editCanvas');
        const ddl = document.getElementById('ddlEdit');
        const tb = document.getElementById('tbEdit');
        const status = document.getElementById('status');

        let originalDataUrl = null;
        let currentDataUrl = null;
        let workingImage = new Image();
        let lastObjectUrl = null;

        (function init() {
            const url = hfImageUrl.value;
            const selSession = hfSelectedImageSession.value || '';

            if (!url) {
                status.textContent = "Nicio imagine disponibilă pentru editare.";
                disableAll(true);
                return;
            }

            if (url.startsWith('data:')) {
                workingImage.onload = () => {
                    canvas.width = workingImage.naturalWidth;
                    canvas.height = workingImage.naturalHeight;
                    const ctx = canvas.getContext('2d');
                    ctx.drawImage(workingImage, 0, 0);
                    originalDataUrl = canvas.toDataURL('image/jpeg', 0.9);
                    currentDataUrl = originalDataUrl;
                    imgEl.src = originalDataUrl;
                    status.textContent = "Imagine încărcată pentru editare (din data URI).";
                };
                workingImage.onerror = () => {
                    status.textContent = "Eroare la încărcarea imaginii (data URI).";
                    disableAll(true);
                };
                workingImage.src = url;
                return;
            }

            (async function fetchAndLoadImage() {
                try {
                    const resp = await fetch(url, { method: 'GET', credentials: 'same-origin' });
                    if (!resp.ok) {
                        status.textContent = 'Eroare la cererea imaginii: HTTP ' + resp.status;
                        disableAll(true);
                        return;
                    }

                    const blob = await resp.blob();

                    if (lastObjectUrl) {
                        URL.revokeObjectURL(lastObjectUrl);
                        lastObjectUrl = null;
                    }
                    const objectUrl = URL.createObjectURL(blob);
                    lastObjectUrl = objectUrl;

                    workingImage.onload = () => {
                        canvas.width = workingImage.naturalWidth;
                        canvas.height = workingImage.naturalHeight;
                        const ctx = canvas.getContext('2d');
                        ctx.drawImage(workingImage, 0, 0);
                        originalDataUrl = canvas.toDataURL('image/jpeg', 0.9);
                        currentDataUrl = originalDataUrl;
                        imgEl.src = originalDataUrl;
                        status.textContent = "Imagine încărcată pentru editare.";
                    };

                    workingImage.onerror = () => {
                        status.textContent = "Eroare la încărcarea imaginii (onerror).";
                        try { imgEl.src = url; } catch (ex) { }
                        disableAll(true);
                    };

                    workingImage.src = objectUrl;

                } catch (err) {
                    status.textContent = 'Eroare la descărcare imagine: ' + (err && err.message ? err.message : String(err));
                    try {
                        workingImage.onload = () => {
                            canvas.width = workingImage.naturalWidth;
                            canvas.height = workingImage.naturalHeight;
                            const ctx = canvas.getContext('2d');
                            ctx.drawImage(workingImage, 0, 0);
                            originalDataUrl = canvas.toDataURL('image/jpeg', 0.9);
                            currentDataUrl = originalDataUrl;
                            imgEl.src = originalDataUrl;
                            status.textContent = "Imagine încărcată pentru editare (fallback).";
                        };
                        workingImage.onerror = () => {
                            status.textContent = "Eroare la încărcarea imaginii (fallback).";
                            disableAll(true);
                        };
                        workingImage.src = url;
                    } catch (e2) {
                        disableAll(true);
                    }
                }
            })();
        })();

        function disableAll(disabled) {
            document.querySelectorAll('button, select, input').forEach(el => el.disabled = disabled);
        }

        function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }

        function applyOperation(op, val) {
            const tmp = document.createElement('canvas');
            tmp.width = canvas.width;
            tmp.height = canvas.height;
            const tctx = tmp.getContext('2d');
            const img = new Image();
            return new Promise((resolve, reject) => {
                img.onload = () => {
                    tctx.drawImage(img, 0, 0, tmp.width, tmp.height);
                    try {
                        if (op === 'rotate') {
                            const deg = parseFloat(val) || 0;
                            const rad = deg * Math.PI / 180;
                            const w = tmp.width, h = tmp.height;
                            const cos = Math.abs(Math.cos(rad)), sin = Math.abs(Math.sin(rad));
                            const nw = Math.round(w * cos + h * sin);
                            const nh = Math.round(w * sin + h * cos);
                            const c2 = document.createElement('canvas');
                            c2.width = nw; c2.height = nh;
                            const c2ctx = c2.getContext('2d');
                            c2ctx.translate(nw / 2, nh / 2);
                            c2ctx.rotate(rad);
                            c2ctx.drawImage(tmp, -w / 2, -h / 2);
                            currentDataUrl = c2.toDataURL('image/jpeg', 0.9);
                            imgEl.src = currentDataUrl;
                            resolve();
                            return;
                        }

                        const ctx = tctx;
                        const id = ctx.getImageData(0, 0, tmp.width, tmp.height);
                        const d = id.data;
                        if (op === 'brightness') {
                            const b = parseFloat(val) || 0;
                            const shift = Math.round(255 * (b / 100));
                            for (let i = 0; i < d.length; i += 4) {
                                d[i] = clamp(d[i] + shift, 0, 255);
                                d[i + 1] = clamp(d[i + 1] + shift, 0, 255);
                                d[i + 2] = clamp(d[i + 2] + shift, 0, 255);
                            }
                        } else if (op === 'contrast') {
                            const c = parseFloat(val) || 0;
                            const factor = (259 * (c + 255)) / (255 * (259 - c));
                            for (let i = 0; i < d.length; i += 4) {
                                d[i] = clamp(factor * (d[i] - 128) + 128, 0, 255);
                                d[i + 1] = clamp(factor * (d[i + 1] - 128) + 128, 0, 255);
                                d[i + 2] = clamp(factor * (d[i + 2] - 128) + 128, 0, 255);
                            }
                        } else if (op === 'saturation') {
                            const s = parseFloat(val) || 0;
                            const sf = (s + 100) / 100;
                            for (let i = 0; i < d.length; i += 4) {
                                const r = d[i], g = d[i + 1], b = d[i + 2];
                                const lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;
                                d[i] = clamp(lum + (r - lum) * sf, 0, 255);
                                d[i + 1] = clamp(lum + (g - lum) * sf, 0, 255);
                                d[i + 2] = clamp(lum + (b - lum) * sf, 0, 255);
                            }
                        } else if (op === 'grayscale') {
                            for (let i = 0; i < d.length; i += 4) {
                                const v = Math.round(0.299 * d[i] + 0.587 * d[i + 1] + 0.114 * d[i + 2]);
                                d[i] = d[i + 1] = d[i + 2] = v;
                            }
                        } else if (op === 'invert') {
                            for (let i = 0; i < d.length; i += 4) {
                                d[i] = 255 - d[i];
                                d[i + 1] = 255 - d[i + 1];
                                d[i + 2] = 255 - d[i + 2];
                            }
                        }

                        ctx.putImageData(id, 0, 0);
                        currentDataUrl = tmp.toDataURL('image/jpeg', 0.9);
                        imgEl.src = currentDataUrl;
                        resolve();
                    } catch (err) { reject(err); }
                };
                img.onerror = () => reject(new Error('Imposibil de desenat imaginea pentru edit.'));
                img.src = currentDataUrl;
            });
        }

        document.getElementById('btnApplyEdit').addEventListener('click', async () => {
            const op = ddl.value;
            const val = tb.value;
            status.textContent = 'Se aplică editarea...';
            try {
                await applyOperation(op, val);
                status.textContent = 'Editare aplicată (pe copie).';
            } catch (err) {
                status.textContent = 'Eroare la aplicare.';
            }
        });

        document.getElementById('btnReset').addEventListener('click', () => {
            if (!originalDataUrl) return;
            currentDataUrl = originalDataUrl;
            imgEl.src = originalDataUrl;
            status.textContent = 'Reset la original.';
        });

        document.getElementById('btnSaveDb').addEventListener('click', async () => {
            if (!currentDataUrl) {
                status.textContent = "Nu există imagine de salvat.";
                return;
            }

            status.textContent = "Se salvează imaginea procesată...";

            try {
                const requestData = {
                    imageData: currentDataUrl,
                    imageName: 'processed_' + (new Date()).getTime() + '.jpg'
                };

                PageMethods.SaveProcessedImage(
                    requestData.imageData,
                    requestData.imageName,
                    function (response) {
                        const result = JSON.parse(response);
                        if (result.success) {
                            status.textContent = "Imagine procesată salvată cu succes! ID: " + result.processedId;
                            setTimeout(() => { }, 1200);
                        } else {
                            status.textContent = "Eroare la salvare: " + result.error;
                        }
                    },
                    function (err) {
                        status.textContent = "Eroare la salvare: " + (err.get_message ? err.get_message() : err);
                    }
                );
            } catch (err) {
                status.textContent = "Eroare la salvare: " + err.message;
            }
        });
    </script>
</body>
</html>
