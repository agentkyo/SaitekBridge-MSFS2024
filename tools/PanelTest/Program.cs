// Panel diagnostic, no simulator needed: lights every display with a test pattern and prints
// the raw input reports while you move knobs and press buttons. Close the Logitech plugin and
// SaitekBridge first so nothing else writes to the panels.
using HidSharp;

const int Vid = 0x06A3, RadioPid = 0x0D05, MultiPid = 0x0D06;
const int Seconds = 30;

var devices = DeviceList.Local.GetHidDevices(Vid).ToList();
if (devices.Count == 0)
{
    Console.WriteLine("No Saitek/Logitech panel found (vendor 06A3).");
    return;
}
foreach (var d in devices)
    Console.WriteLine($"PID {d.ProductID:X4}  feature={d.GetMaxFeatureReportLength()}  input={d.GetMaxInputReportLength()}");

var streams = new List<(string Name, HidStream Stream)>();

var radio = devices.FirstOrDefault(d => d.ProductID == RadioPid);
if (radio != null && radio.TryOpen(out var rs))
{
    // 1 2 3 4 5 | 6 7 8 9 0 in each stack
    var report = new byte[radio.GetMaxFeatureReportLength()];
    for (int i = 0; i < 20; i++) report[1 + i] = (byte)((i % 10 + 1) % 10);
    rs.SetFeature(report);
    streams.Add(("RADIO", rs));
    Console.WriteLine("Radio Panel: should show 12345 / 67890 in both stacks");
}

var multi = devices.FirstOrDefault(d => d.ProductID == MultiPid);
if (multi != null && multi.TryOpen(out var ms))
{
    // 12345 on top, 67890 below, every button LED on
    var report = new byte[multi.GetMaxFeatureReportLength()];
    for (int i = 0; i < 10; i++) report[1 + i] = (byte)((i + 1) % 10);
    report[11] = 0xFF;
    ms.SetFeature(report);
    streams.Add(("MULTI", ms));
    Console.WriteLine("Multi Panel: should show 12345 / 67890 with all LEDs on");
}

Console.WriteLine($"""

    If LEDs light up but digits stay dark, plug the panel straight into a motherboard USB port:
    USB 3.x hubs are known to leave the 7-segment displays off.

    Move knobs and press buttons for {Seconds} seconds...
    """);

var until = DateTime.Now.AddSeconds(Seconds);
var readers = streams.Select(s => Task.Run(() =>
{
    s.Stream.ReadTimeout = 500;
    var buffer = new byte[64];
    while (DateTime.Now < until)
    {
        try
        {
            int n = s.Stream.Read(buffer, 0, buffer.Length);
            Console.WriteLine($"{s.Name}: {Convert.ToHexString(buffer, 0, n)}");
        }
        catch (TimeoutException) { }
    }
})).ToArray();
Task.WaitAll(readers);
foreach (var s in streams) s.Stream.Dispose();
