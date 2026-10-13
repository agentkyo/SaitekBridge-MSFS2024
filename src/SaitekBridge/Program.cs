using SaitekBridge;
using SaitekBridge.Panels;
using SaitekBridge.Sim;

const string Usage = """
    Usage: SaitekBridge [options]
      --install        start SaitekBridge automatically with MSFS 2024 (edits exe.xml)
      --uninstall      remove it from exe.xml and re-enable the Logitech plugin entry
      --debug          log every raw panel report and every event sent to the sim
      --exit-with-sim  quit when the simulator closes (used by --install)
    """;

if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine(Usage);
    return 0;
}
if (args.Contains("--install")) return ExeXmlInstaller.Run(install: true);
if (args.Contains("--uninstall")) return ExeXmlInstaller.Run(install: false);

Log.Verbose = args.Contains("--debug");
bool exitWithSim = args.Contains("--exit-with-sim");

using var singleInstance = new Mutex(true, "SaitekBridge.SingleInstance", out bool firstInstance);
if (!firstInstance)
{
    Log.Info("SaitekBridge is already running.");
    return 1;
}

var version = typeof(SimClient).Assembly.GetName().Version!;
Log.Info($"SaitekBridge {version.ToString(3)} - press Ctrl+C to quit.");
SimConnectNative.RegisterResolver();

using var sim = new SimClient("SaitekBridge");
HidPanel[] panels = { new RadioPanel(), new MultiPanel() };

var stop = new ManualResetEventSlim();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    stop.Set();
};

while (!stop.IsSet)
{
    sim.Pump();
    foreach (var panel in panels) panel.Update(sim);
    if (exitWithSim && sim.SimQuit) break;
    stop.Wait(15);
}

foreach (var panel in panels)
{
    panel.Blank();
    panel.Dispose();
}
Log.Info("Bye!");
return 0;
