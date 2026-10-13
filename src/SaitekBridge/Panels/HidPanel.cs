using System.Collections.Concurrent;
using HidSharp;
using SaitekBridge.Sim;

namespace SaitekBridge.Panels;

/// <summary>
/// Common plumbing for a Saitek panel: finds and (re)opens the HID device, reads its 3-byte input
/// reports on a background thread and writes the display feature report only when it changes.
/// </summary>
public abstract class HidPanel : IDisposable
{
    private const int SaitekVendorId = 0x06A3;
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(3);

    private readonly int productId;
    private readonly ConcurrentQueue<byte[]> inputs = new();
    private HidStream? stream;
    private volatile bool readFailed;
    private DateTime nextAttempt = DateTime.MinValue;
    private byte[]? lastFeature;
    private byte[] previousInput = new byte[4];

    protected HidPanel(int productId, string name)
    {
        this.productId = productId;
        Name = name;
    }

    public string Name { get; }

    /// <summary>Length of the display feature report, including the leading report id.</summary>
    protected int FeatureLength { get; private set; }

    /// <summary>Called once per loop on the main thread: handles input, then refreshes the displays.</summary>
    public void Update(SimClient sim)
    {
        if (!EnsureOpen()) return;

        while (inputs.TryDequeue(out var report))
        {
            Log.Debug($"{Name}: {Convert.ToHexString(report)}");
            OnInput(report, previousInput, sim);
            previousInput = report;
        }

        var feature = new byte[FeatureLength];
        Render(feature, sim);
        WriteFeature(feature);
    }

    /// <summary>Handles one input report; <paramref name="previous"/> allows edge detection.</summary>
    protected abstract void OnInput(byte[] report, byte[] previous, SimClient sim);

    /// <summary>Fills the display feature report (byte 0 is the report id and stays 0).</summary>
    protected abstract void Render(byte[] feature, SimClient sim);

    /// <summary>Bits that went from 0 to 1 between two reports, for byte <paramref name="index"/>.</summary>
    protected static int Pressed(byte[] report, byte[] previous, int index) => report[index] & ~previous[index];

    /// <summary>Blanks every display and LED, e.g. on exit.</summary>
    public void Blank()
    {
        if (stream == null) return;
        var feature = new byte[FeatureLength];
        BlankFeature(feature);
        WriteFeature(feature);
    }

    protected abstract void BlankFeature(byte[] feature);

    private bool EnsureOpen()
    {
        if (readFailed) Close();
        if (stream != null) return true;
        if (DateTime.Now < nextAttempt) return false;
        nextAttempt = DateTime.Now + RetryInterval;

        var device = DeviceList.Local.GetHidDevices(SaitekVendorId, productId).FirstOrDefault();
        if (device == null || !device.TryOpen(out var opened)) return false;

        stream = opened;
        FeatureLength = device.GetMaxFeatureReportLength();
        lastFeature = null;
        readFailed = false;
        new Thread(() => ReadLoop(opened)) { IsBackground = true, Name = $"{Name} reader" }.Start();
        Log.Info($"{Name} connected.");
        return true;
    }

    private void ReadLoop(HidStream s)
    {
        s.ReadTimeout = Timeout.Infinite;
        var buffer = new byte[s.Device.GetMaxInputReportLength()];
        try
        {
            while (true)
            {
                int n = s.Read(buffer, 0, buffer.Length);
                if (n >= 4) inputs.Enqueue(buffer[..4]);
            }
        }
        catch (Exception) when (!ReferenceEquals(s, stream))
        {
            // Closed on purpose.
        }
        catch (Exception e)
        {
            Log.Info($"{Name} disconnected ({e.GetType().Name}).");
            readFailed = true;
        }
    }

    private void WriteFeature(byte[] feature)
    {
        if (stream == null || (lastFeature != null && feature.AsSpan().SequenceEqual(lastFeature))) return;
        try
        {
            stream.SetFeature(feature);
            lastFeature = feature;
        }
        catch (Exception e)
        {
            Log.Info($"{Name}: failed to write the displays ({e.GetType().Name}).");
            readFailed = true;
        }
    }

    private void Close()
    {
        var s = stream;
        stream = null;
        readFailed = false;
        s?.Dispose();
    }

    public void Dispose() => Close();
}
