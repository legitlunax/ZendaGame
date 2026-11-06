using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// Unity's built-in WebGL template ships a separate favicon, outside PlayerSettings.
public sealed class GenesisWebIcon : IPostprocessBuildWithReport
{
    public int callbackOrder => 100;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL) Apply(report.summary.outputPath);
    }

    public static void Apply(string outputPath)
    {
        string indexPath = Path.Combine(outputPath, "index.html");
        if (!File.Exists(indexPath)) throw new FileNotFoundException("WebGL index is missing.", indexPath);
        string templateData = Path.Combine(outputPath, "TemplateData");
        Directory.CreateDirectory(templateData);
        foreach (string path in Directory.GetFiles(GenesisBuild.IconKitPath + "Web"))
            if (!path.EndsWith(".meta")) File.Copy(path, Path.Combine(templateData, Path.GetFileName(path)), true);
        File.Copy("Assets/Genesis/Resources/Genesis/Art/Branding/ZendaGenesisLogo.png", Path.Combine(templateData, "ZendaGenesisLogo.png"), true);
        string html = File.ReadAllText(indexPath);
        const string favicon = "<link rel=\"icon\" href=\"TemplateData/favicon.ico\">\n"
            + "<link rel=\"apple-touch-icon\" href=\"TemplateData/apple-touch-icon.png\">\n"
            + "<link rel=\"manifest\" href=\"TemplateData/site.webmanifest\">\n"
            + "<style>#unity-logo{background-image:url('TemplateData/ZendaGenesisLogo.png');background-size:contain;background-repeat:no-repeat;background-position:center}#unity-webgl-logo{background-image:url('TemplateData/ZendaGenesisLogo.png');background-size:contain;background-repeat:no-repeat;background-position:center}</style>";
        File.WriteAllText(Path.Combine(templateData, "site.webmanifest"),
            "{\"name\":\"ZendaGenesis\",\"short_name\":\"ZendaGenesis\",\"icons\":[{\"src\":\"ZendaGenesis_192.png\",\"sizes\":\"192x192\",\"type\":\"image/png\"},{\"src\":\"ZendaGenesis_512.png\",\"sizes\":\"512x512\",\"type\":\"image/png\"}],\"display\":\"standalone\",\"theme_color\":\"#321846\",\"background_color\":\"#321846\"}");
        // Remove template-provided favicons so browsers cannot choose the Unity icon.
        html = System.Text.RegularExpressions.Regex.Replace(html,
            "<link\\b[^>]*\\brel\\s*=\\s*[\"'](?:shortcut\\s+)?icon[\"'][^>]*>", "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        int headEnd = html.IndexOf("</head>", System.StringComparison.OrdinalIgnoreCase);
        if (headEnd < 0) throw new InvalidDataException("WebGL template has no head element.");
        File.WriteAllText(indexPath, html.Insert(headEnd, favicon + "\n"));
    }
}