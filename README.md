# SaitekBridge

**English** · [Português](README.pt-BR.md) · [Español](README.es.md)

Free, open-source driver that brings the **Saitek / Logitech Pro Flight Radio Panel and Multi Panel**
back to life in **Microsoft Flight Simulator 2024**.

The official Logitech plugin was last updated in 2020 and was built for MSFS 2020. In MSFS 2024 the panels
often stay dark: the Radio Panel shows nothing, the Multi Panel lights a few LEDs but no digits. The only
alternatives are paid tools. SaitekBridge is a small program that talks to the panels over USB and to the
simulator through SimConnect, and does just that job.

## Features

**Radio Panel**, both stacks:

| Mode | Displays (active / standby)      | Outer knob        | Inner knob        | ACT/STBY                     |
|------|----------------------------------|-------------------|-------------------|------------------------------|
| COM1 | active / standby MHz             | MHz               | kHz               | swap                         |
| COM2 | active / standby MHz             | MHz               | kHz               | swap                         |
| NAV1 | active / standby MHz             | MHz               | kHz               | swap                         |
| NAV2 | active / standby MHz             | MHz               | kHz               | swap                         |
| ADF  | active / standby kHz             | 100 kHz           | 10 kHz            | swap                         |
| DME  | NAV1 distance (nm) / speed (kt)  | —                 | —                 | —                            |
| XPDR | squawk code                      | 1st or 3rd digit  | 2nd or 4th digit  | toggles first / last pair    |

**Multi Panel**:

| Selector | Top display        | Bottom display     | Knob                 |
|----------|--------------------|--------------------|----------------------|
| ALT      | autopilot altitude | vertical speed     | altitude             |
| VS       | autopilot altitude | vertical speed     | vertical speed       |
| IAS      | autopilot airspeed | —                  | airspeed             |
| HDG      | heading bug        | —                  | heading bug          |
| CRS      | NAV1 course (OBS)  | —                  | course               |

- AP, HDG, NAV, IAS, ALT, VS, APR and REV buttons toggle the autopilot modes, and their LEDs follow the
  aircraft's real autopilot state.
- AUTO THROTTLE switch arms / disarms the autothrottle.
- Flaps lever and pitch trim wheel work.

Buttons do what the aircraft allows: a Cessna 172 has no autothrottle or IAS mode, and NAV only engages
with a flight plan or a tuned VOR, exactly like in the real aircraft.

## Installation

1. **Plug the panels straight into a USB port on the motherboard.** Do not use a USB 3.x hub: the panels
   keep sending buttons and may even light LEDs there, but the digit displays stay off.
2. Download the latest `SaitekBridge-vX.Y.Z-win-x64.zip` from
   [Releases](../../releases) and extract it to a permanent folder, e.g. `C:\SaitekBridge`.
3. SaitekBridge needs `SimConnect.dll`:
   - if the Logitech flight panels plugin is installed, it is found automatically;
   - otherwise, install the MSFS 2024 SDK (Dev Mode → SDK) and copy
     `SimConnect SDK\lib\SimConnect.dll` next to `SaitekBridge.exe`.
4. Double-click **`install.bat`**. MSFS 2024 will now start SaitekBridge on every launch.
   The Logitech plugin entry is disabled so both don't fight over the panels.
5. Start the simulator and fly.

To undo, run `uninstall.bat`: SaitekBridge is removed and the Logitech plugin entry is re-enabled.
A backup of your previous configuration is kept as `exe.xml.bak`.

Windows SmartScreen may warn that the program is unrecognized because it is not code-signed.
Click "More info" → "Run anyway", or build it yourself from the source.

### Running by hand

You can also just double-click `SaitekBridge.exe` before or after starting the sim. It waits for the
panels and the simulator and reconnects if either goes away. Command-line options:

```
--install        start SaitekBridge automatically with MSFS 2024 (edits exe.xml)
--uninstall      remove it from exe.xml and re-enable the Logitech plugin entry
--debug          log every raw panel report and every event sent to the sim
--exit-with-sim  quit when the simulator closes (used by --install)
```

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Buttons work, LEDs light, but no digits | Move the panel off the USB hub, straight into the motherboard. |
| "SimConnect.dll not found" | See step 3 of the installation. |
| Displays flicker or show wrong values | Make sure the Logitech plugin isn't running too (Task Manager → `LogiMicrosoftFlightSimulator.exe`). |
| A knob does nothing in some aircraft | Some complex add-ons (PMDG, Fenix, …) ignore standard sim events. Default and most GA aircraft work. |
| Panel not detected at all | Run `PanelTest` (see below) to check the panel without the simulator. |

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build SaitekBridge.slnx
dotnet run --project src/SaitekBridge -- --debug
```

`tools/PanelTest` lights every display with a test pattern and prints the raw USB reports, without the
simulator. The USB protocol of both panels is documented in [docs/PROTOCOL.md](docs/PROTOCOL.md).

## Contributing

Issues and pull requests are welcome, especially support for the Switch Panel and the BIP / FIP,
and per-aircraft event mappings.

## Acknowledgements

Panel protocol details were cross-checked against the open-source
[xsaitekpanels](https://github.com/sparker256/xsaitekpanels) (X-Plane) and
[DCSFlightpanels](https://github.com/DCS-Skunkworks/DCSFlightpanels) projects.

## License

[MIT](LICENSE) © agentkyo. Not affiliated with Logitech, Saitek or Microsoft.
