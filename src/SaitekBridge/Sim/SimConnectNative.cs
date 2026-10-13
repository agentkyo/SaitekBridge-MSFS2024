using System.Runtime.InteropServices;

namespace SaitekBridge.Sim;

/// <summary>
/// Minimal P/Invoke surface over the native SimConnect.dll (x64). Only what the bridge needs:
/// open/close, one FLOAT64 datum per data definition, client events and the dispatch queue.
/// </summary>
internal static class SimConnectNative
{
    private const string Dll = "SimConnect.dll";

    public const uint UserObject = 0;
    public const uint Unused = 0xFFFFFFFF;
    public const uint GroupPriorityHighest = 1;
    public const uint EventFlagGroupIdIsPriority = 0x10;
    public const uint DataTypeFloat64 = 4;
    public const uint PeriodSimFrame = 3;
    public const uint DataRequestFlagChanged = 1;

    // SIMCONNECT_RECV_ID
    public const uint RecvException = 1;
    public const uint RecvOpen = 2;
    public const uint RecvQuit = 3;
    public const uint RecvSimObjectData = 8;

    // SIMCONNECT_RECV (12 bytes) + 7 DWORDs of SIMCONNECT_RECV_SIMOBJECT_DATA before dwData
    public const int SimObjectDataRequestIdOffset = 12;
    public const int SimObjectDataValueOffset = 40;
    // SIMCONNECT_RECV_EXCEPTION: dwException, dwSendID, dwIndex after the header
    public const int ExceptionCodeOffset = 12;
    public const int ExceptionIndexOffset = 20;

    [DllImport(Dll, CharSet = CharSet.Ansi)]
    public static extern int SimConnect_Open(out IntPtr handle, string name, IntPtr hWnd, uint userEventWin32, IntPtr eventHandle, uint configIndex);

    [DllImport(Dll)]
    public static extern int SimConnect_Close(IntPtr handle);

    [DllImport(Dll, CharSet = CharSet.Ansi)]
    public static extern int SimConnect_AddToDataDefinition(IntPtr handle, uint defineId, string datumName, string unitsName, uint datumType, float epsilon, uint datumId);

    [DllImport(Dll)]
    public static extern int SimConnect_RequestDataOnSimObject(IntPtr handle, uint requestId, uint defineId, uint objectId, uint period, uint flags, uint origin, uint interval, uint limit);

    [DllImport(Dll, CharSet = CharSet.Ansi)]
    public static extern int SimConnect_MapClientEventToSimEvent(IntPtr handle, uint eventId, string eventName);

    [DllImport(Dll)]
    public static extern int SimConnect_TransmitClientEvent(IntPtr handle, uint objectId, uint eventId, uint data, uint groupId, uint flags);

    [DllImport(Dll)]
    public static extern int SimConnect_GetNextDispatch(IntPtr handle, out IntPtr data, out uint size);

    /// <summary>
    /// SimConnect.dll is not shipped with the game, so look for it next to the exe, in the MSFS SDKs
    /// and in the Logitech plugin folder (which bundles a compatible x64 copy).
    /// </summary>
    public static void RegisterResolver()
    {
        IntPtr loaded = IntPtr.Zero;
        NativeLibrary.SetDllImportResolver(typeof(SimConnectNative).Assembly, (name, _, _) =>
        {
            if (!name.Equals(Dll, StringComparison.OrdinalIgnoreCase)) return IntPtr.Zero;
            if (loaded != IntPtr.Zero) return loaded;
            foreach (var path in CandidatePaths())
                if (File.Exists(path) && NativeLibrary.TryLoad(path, out loaded))
                {
                    Log.Info($"Using {path}");
                    return loaded;
                }
            return IntPtr.Zero;
        });
    }

    private static IEnumerable<string> CandidatePaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, Dll);
        foreach (var variable in new[] { "MSFS2024_SDK", "MSFS_SDK" })
            if (Environment.GetEnvironmentVariable(variable) is { Length: > 0 } sdk)
                yield return Path.Combine(sdk, "SimConnect SDK", "lib", Dll);
        yield return @"C:\MSFS 2024 SDK\SimConnect SDK\lib\" + Dll;
        yield return @"C:\MSFS SDK\SimConnect SDK\lib\" + Dll;
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Logitech", "Microsoft Flight Simulator Plugin", Dll);
    }
}
