using System.Diagnostics;
using System.Runtime.InteropServices;
using static SaitekBridge.Sim.SimConnectNative;

namespace SaitekBridge.Sim;

/// <summary>
/// Owns the SimConnect connection: (re)connects while the sim runs, keeps the latest value of every
/// <see cref="SimVar"/> and sends key events by name. Must be used from a single thread.
/// </summary>
public sealed class SimClient : IDisposable
{
    private static readonly string[] SimProcesses = { "FlightSimulator2024", "FlightSimulator" };
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(5);

    private readonly string appName;
    private readonly double[] values = new double[Enum.GetValues<SimVar>().Length];
    private readonly Dictionary<string, uint> eventIds = new();
    private IntPtr handle;
    private DateTime nextAttempt = DateTime.MinValue;
    private DateTime nextProcessCheck = DateTime.MinValue;
    private bool dllMissing;

    public SimClient(string appName) => this.appName = appName;

    public bool Connected { get; private set; }

    /// <summary>Set once the sim announces it is closing.</summary>
    public bool SimQuit { get; private set; }

    public double this[SimVar v] => values[(int)v];

    public bool IsOn(SimVar v) => values[(int)v] != 0;

    /// <summary>Call regularly: connects when possible and drains the dispatch queue.</summary>
    public void Pump()
    {
        if (handle == IntPtr.Zero)
        {
            if (!dllMissing && DateTime.Now >= nextAttempt) TryOpen();
            return;
        }

        // A crashed sim never sends QUIT, so also watch the process list.
        if (DateTime.Now >= nextProcessCheck)
        {
            nextProcessCheck = DateTime.Now + RetryInterval;
            if (!SimProcesses.Any(IsRunning))
            {
                Log.Info("Simulator is no longer running.");
                SimQuit = true;
                Disconnect();
                return;
            }
        }

        while (handle != IntPtr.Zero && SimConnect_GetNextDispatch(handle, out var data, out _) >= 0)
            Handle(data);
    }

    /// <summary>Sends a key event (e.g. "COM_STBY_RADIO_SWAP") to the user aircraft.</summary>
    public void Send(string eventName, uint data = 0)
    {
        if (!Connected) return;
        if (!eventIds.TryGetValue(eventName, out var id))
        {
            id = (uint)eventIds.Count;
            eventIds[eventName] = id;
            SimConnect_MapClientEventToSimEvent(handle, id, eventName);
        }
        Log.Debug($"-> {eventName}");
        if (SimConnect_TransmitClientEvent(handle, UserObject, id, data, GroupPriorityHighest, EventFlagGroupIdIsPriority) < 0)
        {
            Log.Info("Failed to send event; reconnecting.");
            Disconnect();
        }
    }

    private void TryOpen()
    {
        nextAttempt = DateTime.Now + RetryInterval;
        try
        {
            if (SimConnect_Open(out var h, appName, IntPtr.Zero, 0, IntPtr.Zero, 0) < 0) return;
            handle = h;
        }
        catch (DllNotFoundException)
        {
            dllMissing = true;
            Log.Info("SimConnect.dll not found. Copy it next to SaitekBridge.exe (see the README).");
            return;
        }

        // One definition per variable: an unknown variable then only loses itself, never shifts the others.
        foreach (var (simVar, (name, units)) in SimVars.Definitions)
        {
            var id = (uint)simVar;
            SimConnect_AddToDataDefinition(handle, id, name, units, DataTypeFloat64, 0, Unused);
            SimConnect_RequestDataOnSimObject(handle, id, id, UserObject, PeriodSimFrame, DataRequestFlagChanged, 0, 0, 0);
        }
    }

    private void Handle(IntPtr data)
    {
        switch ((uint)Marshal.ReadInt32(data, 8))
        {
            case RecvOpen:
                Connected = true;
                SimQuit = false;
                Log.Info("Connected to the simulator.");
                break;
            case RecvQuit:
                Log.Info("Simulator closed.");
                SimQuit = true;
                Disconnect();
                break;
            case RecvSimObjectData:
                var request = Marshal.ReadInt32(data, SimObjectDataRequestIdOffset);
                if (request >= 0 && request < values.Length)
                    values[request] = BitConverter.Int64BitsToDouble(Marshal.ReadInt64(data, SimObjectDataValueOffset));
                break;
            case RecvException:
                Log.Debug($"SimConnect exception {Marshal.ReadInt32(data, ExceptionCodeOffset)} (index {Marshal.ReadInt32(data, ExceptionIndexOffset)})");
                break;
        }
    }

    private static bool IsRunning(string processName)
    {
        var found = Process.GetProcessesByName(processName);
        foreach (var p in found) p.Dispose();
        return found.Length > 0;
    }

    private void Disconnect()
    {
        if (handle != IntPtr.Zero) SimConnect_Close(handle);
        handle = IntPtr.Zero;
        Connected = false;
        eventIds.Clear();
        Array.Clear(values);
        nextAttempt = DateTime.Now + RetryInterval;
    }

    public void Dispose() => Disconnect();
}
