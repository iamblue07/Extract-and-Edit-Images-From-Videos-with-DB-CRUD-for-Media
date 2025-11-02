<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="BDM_P.Main" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Main - BDM Project</title>
    <meta charset="utf-8" />
    <style>
        :root{
            --blue:#0078D7;
            --blue-strong:#005ea2;
            --green:#28a745;
            --green-strong:#1e7e34;
            --red:#e74c3c;
            --red-strong:#c0392b;
            --bg:#f5f7fa;
            --card:#ffffff;
            --muted:#666;
            --radius:10px;
            --gap:12px;
            --shadow:0 6px 18px rgba(15,23,42,0.08);
            --banner-start:#055a3d;
            --banner-end:#0b8a54;
            --banner-text:#ffffff;
        }

        html,body{height:100%;margin:0;padding:0;background:var(--bg);font-family:Inter,Segoe UI,Roboto,Arial,sans-serif;color:#222}
        .page{min-height:100%;display:flex;flex-direction:column;align-items:center;justify-content:flex-start;padding:28px 20px 40px}
        .banner{width:100%;max-width:980px;border-radius:12px;background:linear-gradient(90deg,var(--banner-start),var(--banner-end));color:var(--banner-text);padding:16px 20px;box-shadow:0 8px 20px rgba(3,41,19,0.12);display:flex;gap:16px;align-items:center;box-sizing:border-box}
        .banner h1{margin:0;font-size:18px;font-weight:700;letter-spacing:0.2px}
        .banner p{margin:0;font-size:13px;opacity:0.95}
        .container{width:100%;max-width:760px;background:var(--card);border-radius:var(--radius);box-shadow:var(--shadow);padding:22px;box-sizing:border-box;margin-top:18px}
        h2{margin:0 0 12px 0;font-weight:600;font-size:20px}
        .upload-row{display:flex;gap:var(--gap);align-items:center;flex-wrap:wrap}
        .file-input{flex:1;display:flex;align-items:center;gap:8px}
        input[type=file]{display:block}
        .buttons{margin-top:18px;display:flex;gap:10px;flex-wrap:wrap;justify-content:flex-start}
        .btn{display:inline-block;padding:10px 14px;border-radius:8px;border:0;font-weight:600;color:#fff;cursor:pointer;box-shadow:0 2px 6px rgba(0,0,0,0.04)}
        .btn:active{transform:translateY(1px)}
        .btn-blue{background:var(--blue)}
        .btn-blue:hover{background:var(--blue-strong)}
        .btn-green{background:var(--green)}
        .btn-green:hover{background:var(--green-strong)}
        .btn-red{background:var(--red)}
        .btn-red:hover{background:var(--red-strong)}
        .btn-muted{background:#6c757d}
        .status{margin-top:14px;color:var(--muted);font-size:14px;min-height:18px}
        .db-load{margin-top:14px;display:flex;gap:8px;align-items:center;flex-wrap:wrap}
        .db-load input[type="text"]{padding:8px;border-radius:8px;border:1px solid #e2e6ea;width:140px}
        #videoPlayer{width:100%;border-radius:8px;margin-top:18px;background:#000;display:block}
        .helper{font-size:13px;color:#666;margin-top:8px}
        @media(max-width:640px){
            .container{padding:16px}
            .buttons{justify-content:stretch}
            .btn{flex:1;text-align:center}
            .banner{padding:12px}
            .banner h1{font-size:16px}
            .banner p{font-size:12px}
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page">
            <div class="banner" role="banner" aria-label="Banner aplicație">
                <div style="flex:0 0 auto;width:44px;height:44px;border-radius:8px;background:rgba(255,255,255,0.08);display:flex;align-items:center;justify-content:center;font-weight:700;font-size:18px">
                    S
                </div>
                <div style="flex:1;min-width:0">
                    <h1>Software pentru Supraveghere și monitorizare a faunei</h1>
                    <p>Aplicație destinată colectării, procesării și analizării cadrelor extrase din camerele live de monitorizare a faunei.</p>
                </div>
            </div>

            <div class="container">
                <h2>Încărcare videoclip</h2>

                <div class="upload-row">
                    <div class="file-input">
                        <asp:FileUpload ID="videoFileUpload" runat="server" />
                    </div>
                    <div class="helper">Tipuri: mp4 / mov / avi / mkv / webm. Max 200MB.</div>
                </div>

                <div class="buttons">
                    <asp:Button ID="btnUpload" runat="server" Text="Încarcă videoclipul" OnClick="btnUpload_Click" CssClass="btn btn-blue" />
                    <asp:Button ID="btnVeziImagini" runat="server" Text="Prelucrează imagini din videoclip" OnClick="btnVeziImagini_Click" CssClass="btn btn-blue" />
                    <asp:Button ID="btnSterge" runat="server" Text="Șterge videoclipul" OnClick="btnSterge_Click" CssClass="btn btn-red" />
                    <asp:Button ID="btnCauta" runat="server" Text="Caută în baza de date" OnClick="btnCauta_Click" CssClass="btn btn-blue" />
                </div>

                <div class="db-load">
                    <asp:TextBox ID="txtVideoId" runat="server" placeholder="ID video"></asp:TextBox>
                    <asp:Button ID="btnLoadFromDb" runat="server" Text="Încarcă din baza de date" OnClick="btnLoadFromDb_Click" CssClass="btn btn-blue" />
                </div>

                <asp:Label ID="lblStatus" runat="server" CssClass="status" />

                <asp:Panel ID="pnlVideoContainer" runat="server" Visible="false">
                    <video id="videoPlayer" runat="server" controls></video>
                </asp:Panel>
            </div>
        </div>
    </form>
</body>
</html>
