using System.Text;
using System.Xml.Linq;

namespace SaitekBridge;

/// <summary>
/// Adds or removes SaitekBridge in MSFS 2024's exe.xml so the sim starts it automatically.
/// While installed, the legacy Logitech plugin entry is disabled so the two never fight over the panels.
/// </summary>
public static class ExeXmlInstaller
{
    private const string AddonName = "SaitekBridge";
    private const string LogitechExe = "LogiMicrosoftFlightSimulator.exe";

    private static IEnumerable<(string Edition, string Folder)> SimFolders()
    {
        yield return ("Steam", Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft Flight Simulator 2024"));
        yield return ("Microsoft Store / Xbox", Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Packages", "Microsoft.Limitless_8wekyb3d8bbwe", "LocalCache"));
    }

    /// <returns>Process exit code: 0 when at least one MSFS 2024 installation was updated.</returns>
    public static int Run(bool install)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // exe.xml is Windows-1252
        int updated = 0;

        foreach (var (edition, folder) in SimFolders().Where(f => Directory.Exists(f.Folder)))
        {
            var path = Path.Combine(folder, "exe.xml");
            XDocument doc;
            if (File.Exists(path))
            {
                File.Copy(path, path + ".bak", overwrite: true);
                doc = XDocument.Load(path);
            }
            else
            {
                if (!install) continue;
                doc = NewDocument();
            }

            var root = doc.Root!;
            root.Elements("Launch.Addon").Where(a => (string?)a.Element("Name") == AddonName).Remove();

            foreach (var logitech in root.Elements("Launch.Addon")
                         .Where(a => ((string?)a.Element("Path"))?.EndsWith(LogitechExe, StringComparison.OrdinalIgnoreCase) == true))
                SetChild(logitech, "Disabled", install ? "True" : "False");

            if (install)
                root.Add(new XElement("Launch.Addon",
                    new XElement("Name", AddonName),
                    new XElement("Disabled", "False"),
                    new XElement("Path", Environment.ProcessPath),
                    new XElement("CommandLine", "--exit-with-sim")));

            doc.Save(path);
            updated++;
            Log.Info($"{(install ? "Installed in" : "Removed from")} {edition}: {path}");
        }

        if (updated == 0)
            Log.Info(install ? "MSFS 2024 user folder not found. Is the simulator installed and started at least once?"
                             : "Nothing to remove.");
        return updated > 0 || !install ? 0 : 1;
    }

    private static XDocument NewDocument() => new(
        new XDeclaration("1.0", "Windows-1252", null),
        new XElement("SimBase.Document",
            new XAttribute("Type", "SimConnect"),
            new XAttribute("version", "1,0"),
            new XElement("Descr", "Auto launch external applications on MSFS start"),
            new XElement("Filename", "exe.xml"),
            new XElement("Disabled", "False"),
            new XElement("Launch.ManualLoad", "False")));

    private static void SetChild(XElement parent, string name, string value)
    {
        if (parent.Element(name) is { } child) child.Value = value;
        else parent.Add(new XElement(name, value));
    }
}
