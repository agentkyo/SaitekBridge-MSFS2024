using System.Globalization;
using SaitekBridge.Sim;

namespace SaitekBridge.Panels;

/// <summary>
/// Saitek/Logitech Pro Flight Radio Panel (PID 0x0D05): two stacks, each with a 7-position mode
/// selector, a dual concentric knob and an ACT/STBY button, and two 5-digit windows.
/// </summary>
/// <remarks>
/// Input report (after report id): byte 1 bits 0-6 = upper selector COM1..XPDR, bit 7 = lower COM1;
/// byte 2 bits 0-5 = lower selector COM2..XPDR, bit 6 = upper ACT/STBY, bit 7 = lower ACT/STBY;
/// byte 3 = knob detents, upper stack in bits 0-3 and lower in bits 4-7
/// (fine +, fine -, coarse +, coarse -).
/// Feature report: 4 windows of 5 cells: upper active, upper standby, lower active, lower standby.
/// </remarks>
public sealed class RadioPanel : HidPanel
{
    public enum Mode { Com1, Com2, Nav1, Nav2, Adf, Dme, Xpdr }

    private sealed class Stack
    {
        public Mode Mode;
        /// <summary>Transponder only: true while the knobs edit the last two digits.</summary>
        public bool XpdrLowDigits;
    }

    private readonly Stack upper = new() { Mode = Mode.Com1 };
    private readonly Stack lower = new() { Mode = Mode.Com2 };

    public RadioPanel() : base(0x0D05, "Radio Panel") { }

    protected override void OnInput(byte[] report, byte[] previous, SimClient sim)
    {
        int selectors = report[1] | (report[2] & 0x3F) << 8;
        upper.Mode = SelectorMode(selectors & 0x7F, upper.Mode);
        lower.Mode = SelectorMode(selectors >> 7, lower.Mode);

        int buttons = Pressed(report, previous, 2);
        if ((buttons & 0x40) != 0) Swap(upper, sim);
        if ((buttons & 0x80) != 0) Swap(lower, sim);

        int knobs = Pressed(report, previous, 3);
        Turn(upper, knobs & 0x0F, sim);
        Turn(lower, knobs >> 4, sim);
    }

    private static Mode SelectorMode(int bits, Mode current)
    {
        for (int i = 0; i < 7; i++)
            if ((bits & (1 << i)) != 0) return (Mode)i;
        return current;
    }

    private static void Swap(Stack stack, SimClient sim)
    {
        switch (stack.Mode)
        {
            case Mode.Com1: sim.Send("COM_STBY_RADIO_SWAP"); break;
            case Mode.Com2: sim.Send("COM2_RADIO_SWAP"); break;
            case Mode.Nav1: sim.Send("NAV1_RADIO_SWAP"); break;
            case Mode.Nav2: sim.Send("NAV2_RADIO_SWAP"); break;
            case Mode.Adf: sim.Send("ADF1_RADIO_SWAP"); break;
            case Mode.Xpdr: stack.XpdrLowDigits = !stack.XpdrLowDigits; break;
        }
    }

    /// <param name="detents">Bit 0 fine +, bit 1 fine -, bit 2 coarse +, bit 3 coarse -.</param>
    private static void Turn(Stack stack, int detents, SimClient sim)
    {
        if (detents == 0) return;
        var (coarse, fine) = stack.Mode switch
        {
            Mode.Com1 => ("COM_RADIO_WHOLE", "COM_RADIO_FRACT"),
            Mode.Com2 => ("COM2_RADIO_WHOLE", "COM2_RADIO_FRACT"),
            Mode.Nav1 => ("NAV1_RADIO_WHOLE", "NAV1_RADIO_FRACT"),
            Mode.Nav2 => ("NAV2_RADIO_WHOLE", "NAV2_RADIO_FRACT"),
            Mode.Adf => ("ADF_100", "ADF_10"),
            Mode.Xpdr => stack.XpdrLowDigits ? ("XPNDR_10", "XPNDR_1") : ("XPNDR_1000", "XPNDR_100"),
            _ => (null, null),
        };
        if (coarse == null || fine == null) return;
        if ((detents & 0x1) != 0) sim.Send(fine + "_INC");
        if ((detents & 0x2) != 0) sim.Send(fine + "_DEC");
        if ((detents & 0x4) != 0) sim.Send(coarse + "_INC");
        if ((detents & 0x8) != 0) sim.Send(coarse + "_DEC");
    }

    protected override void Render(byte[] feature, SimClient sim)
    {
        if (!sim.Connected)
        {
            BlankFeature(feature);
            return;
        }
        RenderStack(upper, feature.AsSpan(1, 10), sim);
        RenderStack(lower, feature.AsSpan(11, 10), sim);
    }

    private static void RenderStack(Stack stack, Span<byte> windows, SimClient sim)
    {
        var (active, standby) = stack.Mode switch
        {
            Mode.Com1 => (Mhz(sim[SimVar.Com1Active]), Mhz(sim[SimVar.Com1Standby])),
            Mode.Com2 => (Mhz(sim[SimVar.Com2Active]), Mhz(sim[SimVar.Com2Standby])),
            Mode.Nav1 => (Mhz(sim[SimVar.Nav1Active]), Mhz(sim[SimVar.Nav1Standby])),
            Mode.Nav2 => (Mhz(sim[SimVar.Nav2Active]), Mhz(sim[SimVar.Nav2Standby])),
            Mode.Adf => (Whole(sim[SimVar.AdfActive]), Whole(sim[SimVar.AdfStandby])),
            Mode.Dme => (Tenths(sim[SimVar.Dme1Distance]), Whole(sim[SimVar.Dme1Speed])),
            Mode.Xpdr => (Squawk(sim[SimVar.Transponder]), ""),
            _ => ("", ""),
        };
        SevenSegment.Write(windows[..5], active);
        SevenSegment.Write(windows[5..], standby);
    }

    /// <summary>118.255 MHz -> "118.25" (8.33 kHz channels are truncated to fit 5 digits).</summary>
    private static string Mhz(double mhz)
    {
        if (mhz <= 0) return "";
        var hundredths = ((int)Math.Floor(mhz * 100 + 0.001)).ToString(CultureInfo.InvariantCulture);
        return hundredths.Length > 2 ? hundredths[..^2] + "." + hundredths[^2..] : hundredths;
    }

    private static string Whole(double value) => value <= 0 ? "" : ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture);

    private static string Tenths(double value) => value <= 0 ? "" : value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>The transponder code arrives as BCO16: one octal digit per nibble (0x1200 = 1200).</summary>
    private static string Squawk(double bco16) => ((int)bco16).ToString("X4", CultureInfo.InvariantCulture);

    protected override void BlankFeature(byte[] feature)
    {
        for (int w = 0; w < 4; w++) SevenSegment.Clear(feature.AsSpan(1 + w * 5, 5));
    }
}
