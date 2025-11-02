<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewImages.aspx.cs" Inherits="BDM_P.WebForms.ViewImages" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Vezi Imagini</title>
    <meta charset="utf-8" />
    <style>
        :root{
            --blue:#007acc;
            --blue-strong:#005fa3;
            --green:#28a745;
            --red:#e74c3c;
            --bg:#f6f8fa;
            --card:#fff;
            --muted:#333;
            --radius:10px;
            --shadow:0 4px 10px rgba(0,0,0,0.08);
        }

        html,body{height:100%;margin:0;padding:0;background:var(--bg);font-family:Inter,Segoe UI,Arial,sans-serif;color:#222}
        .container{max-width:920px;margin:28px auto;background:var(--card);padding:20px;border-radius:var(--radius);box-shadow:var(--shadow);text-align:center}
        h1{margin:0 0 14px 0;font-size:20px;font-weight:600}
        #preview{border-radius:8px;max-width:100%;height:auto;margin:12px 0;border:1px solid #ddd;display:block;margin-left:auto;margin-right:auto}
        .controls{display:flex;justify-content:center;flex-wrap:wrap;gap:8px;margin-top:12px}
        .btn{background:var(--blue);color:#fff;border:none;padding:8px 14px;border-radius:8px;cursor:pointer;font-weight:600}
        .btn:hover{background:var(--blue-strong)}
        .btn-green{background:var(--green)}
        .btn-green:hover{background:#1e7e34}
        .btn-red{background:var(--red)}
        .btn-red:hover{background:#c0392b}
        .status{margin-top:10px;color:var(--muted);font-size:14px;min-height:1.2em}
        #thumbs{display:flex;justify-content:center;gap:8px;flex-wrap:wrap;margin-top:12px}
        .thumb{width:120px;height:80px;object-fit:cover;border-radius:8px;cursor:pointer;border:2px solid transparent;transition:transform .12s,box-shadow .12s}
        .thumb:hover{transform:translateY(-4px);box-shadow:0 8px 20px rgba(0,0,0,0.08)}
        .thumb.selected{border-color:var(--blue)}
        @media(max-width:720px){.thumb{width:88px;height:60px}}
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        <div class="container">
            <h1>Imagini extrase din videoclip</h1>

            <asp:HiddenField ID="hfVideoUrl" runat="server" />
            <asp:HiddenField ID="hfVideoId" runat="server" />

            <video id="videoPlayer" style="display:none"></video>
            <canvas id="captureCanvas" style="display:none"></canvas>

            <img id="preview" alt="Cadru" src="" />

            <div id="thumbs"></div>

            <div class="controls">
                <button type="button" id="btnPrev" class="btn">◀ Anterior</button>
                <button type="button" id="btnNext" class="btn">Următor ▶</button>
                <button type="button" id="btnExtract" class="btn">Extrage cadre</button>
                <button type="button" id="btnSave" class="btn btn-green">Salvează cadrul curent</button>
                <button type="button" id="btnEdit" class="btn">Prelucrează cadrul curent</button>
                <asp:Button ID="btnInapoi" runat="server" Text="⟵ Înapoi" CssClass="btn" OnClick="btnInapoi_Click" />
            </div>

            <div class="status" id="status"></div>
        </div>
    </form>

    <script>
        const preview = document.getElementById('preview');
        const video = document.getElementById('videoPlayer');
        const canvas = document.getElementById('captureCanvas');
        const status = document.getElementById('status');
        const thumbsDiv = document.getElementById('thumbs');
        let frames = [];
        let currentIndex = 0;

        (function init() {
            const hf = document.getElementById('<%= hfVideoUrl.ClientID %>');
            const videoUrl = hf.value;
            if (!videoUrl) {
                status.textContent = "Niciun videoclip încărcat sau sesiunea a expirat.";
                disableControls(true);
                return;
            }

            video.src = videoUrl;
            video.preload = "auto";
            video.crossOrigin = "anonymous";

            document.getElementById('btnExtract').addEventListener('click', () => extractRandomFrames(6));
            document.getElementById('btnPrev').addEventListener('click', showPrev);
            document.getElementById('btnNext').addEventListener('click', showNext);
            document.getElementById('btnSave').addEventListener('click', saveCurrentFrame);
            document.getElementById('btnEdit').addEventListener('click', editCurrentFrame);
        })();

        function disableControls(disabled) {
            document.querySelectorAll('.btn').forEach(b => b.disabled = disabled);
        }

        function extractRandomFrames(count) {
            frames = [];
            thumbsDiv.innerHTML = '';
            preview.src = '';
            status.textContent = "Se încarcă videoclipul...";

            video.addEventListener('loadedmetadata', async function onMeta() {
                video.removeEventListener('loadedmetadata', onMeta);
                const duration = video.duration;
                if (!duration || isNaN(duration) || duration <= 0) {
                    status.textContent = "Durata videoclipului nu este disponibilă.";
                    return;
                }

                const times = [];
                for (let i = 0; i < count; i++) {
                    const t = 0.5 + Math.random() * Math.max(0, duration - 1.0);
                    times.push(t);
                }

                status.textContent = "Se extrag cadrele...";
                canvas.width = video.videoWidth || 640;
                canvas.height = video.videoHeight || 360;

                for (let i = 0; i < times.length; i++) {
                    try {
                        const d = await captureAt(times[i]);
                        frames.push(d);
                        addThumb(d, i);
                    } catch (err) {
                        console.warn('capture error', err);
                    }
                }

                if (frames.length === 0) {
                    status.textContent = "Nu s-au putut extrage cadre.";
                    disableControls(false);
                    return;
                }

                currentIndex = 0;
                showFrame(currentIndex);
                status.textContent = `S-au extras ${frames.length} cadre.`;
            });

            if (video.readyState >= 1) {
                const e = new Event('loadedmetadata');
                video.dispatchEvent(e);
            } else {
                video.load();
            }
        }

        function captureAt(time) {
            return new Promise((resolve, reject) => {
                let timeout = setTimeout(() => {
                    video.removeEventListener('seeked', onSeeked);
                    reject(new Error('Timeout seeked'));
                }, 5000);

                function onSeeked() {
                    clearTimeout(timeout);
                    video.removeEventListener('seeked', onSeeked);
                    try {
                        const ctx = canvas.getContext('2d');
                        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                        const dataUrl = canvas.toDataURL('image/jpeg', 0.85);
                        resolve(dataUrl);
                    } catch (err) {
                        reject(err);
                    }
                }

                video.addEventListener('seeked', onSeeked);

                try {
                    video.currentTime = Math.min(time, video.duration - 0.1);
                } catch (err) {
                    setTimeout(() => video.currentTime = Math.min(time, video.duration - 0.1), 50);
                }
            });
        }

        function addThumb(dataUrl, idx) {
            const img = document.createElement('img');
            img.src = dataUrl;
            img.className = 'thumb';
            img.addEventListener('click', () => {
                setSelectedThumb(idx);
                showFrame(idx);
            });
            thumbsDiv.appendChild(img);
        }

        function setSelectedThumb(idx) {
            Array.from(thumbsDiv.children).forEach((c, i) => {
                c.classList.toggle('selected', i === idx);
            });
        }

        function showFrame(idx) {
            if (!frames || frames.length === 0) return;
            if (idx < 0) idx = 0;
            if (idx >= frames.length) idx = frames.length - 1;
            currentIndex = idx;
            preview.src = frames[currentIndex];
            setSelectedThumb(currentIndex);
            status.textContent = `Imagine ${currentIndex + 1} din ${frames.length}`;
        }

        function showPrev() { showFrame(currentIndex - 1); }
        function showNext() { showFrame(currentIndex + 1); }

        function saveCurrentFrame() {
            if (!frames || frames.length === 0) {
                status.textContent = "Nu există cadre de salvat.";
                return;
            }
            const data = frames[currentIndex];
            status.textContent = "Se salvează imaginea...";
            const name = 'frame_' + (new Date()).getTime() + '.jpg';

            PageMethods.SaveFrame(data, name, function (relPath) {
                status.textContent = "Imagine salvată în baza de date: " + relPath;
            }, function (err) {
                status.textContent = "Eroare la salvare: " + (err.get_message ? err.get_message() : err);
            });
        }

        function editCurrentFrame() {
            if (!frames || frames.length === 0) {
                status.textContent = "Nu există cadre.";
                return;
            }
            const data = frames[currentIndex];
            const name = 'frame_' + (new Date()).getTime() + '.jpg';
            status.textContent = "Se salvează temporar pentru editare...";

            PageMethods.SaveAndSelectFrame(data, name, function (relPath) {
                status.textContent = "Imagine pregătită pentru editare.";
                window.location.href = 'Edit.aspx';
            }, function (err) {
                status.textContent = "Eroare la pregătirea editării: " + (err.get_message ? err.get_message() : err);
            });
        }
    </script>
</body>
</html>
