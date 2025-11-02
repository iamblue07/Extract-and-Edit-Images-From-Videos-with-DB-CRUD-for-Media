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
    <form id="form1" runat="server">
        <div class="wrap">
            <h2>Căutare după ID-ul videoclipului</h2>

            <div class="search-row">
                <asp:TextBox ID="TextBox1" runat="server" CssClass="txt-ctrl" Placeholder="Introdu ID-ul videoclipului..." />
                <asp:Button ID="btnEdited" runat="server" Text="Vezi procesate" CssClass="btn btn-blue" OnClick="btnEdited_Click" />
                <asp:Button ID="btnRaw" runat="server" Text="Vezi neprocesate" CssClass="btn btn-blue" OnClick="btnRaw_Click" />
                <asp:Button ID="btnBack" runat="server" Text="⟵ Înapoi" CssClass="btn btn-blue" OnClick="btnBack_Click" />
                <asp:Button ID="btnDelete" runat="server" Text="Șterge" CssClass="btn btn-red" OnClick="btnDelete_Click" />
            </div>

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

            <div class="meta">
                <asp:Label ID="lblMeta" runat="server" Text=""></asp:Label>
            </div>

            <div style="margin-top:12px;">
                <asp:TextBox ID="txtError" runat="server" TextMode="MultiLine" Rows="2" CssClass="txt-error" ReadOnly="true" />
            </div>
        </div>
    </form>
</body>
</html>
