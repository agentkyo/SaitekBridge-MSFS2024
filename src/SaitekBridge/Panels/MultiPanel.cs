using System.Globalization;
using SaitekBridge.Sim;

namespace SaitekBridge.Panels;

/// <summary>
/// Saitek/Logitech Pro Flight Multi Panel (PID 0x0D06): autopilot selector, knob, mode buttons with
/// LEDs, autothrottle switch, flaps lever and pitch trim wheel, and two 5-digit windows.
/// </summary>
/// <remarks>
/// Input report (after report id): byte 1 bits 0-4 = selector ALT/VS/IAS/HDG/CRS, bit 5 = knob +,
/// bit 6 = knob -, bit 7 = AP; byte 2 = HDG/NAV/IAS/ALT/VS/APR/REV buttons in bits 0-6 and
/// autothrottle ARM in bit 7; byte 3 bit 0 = flaps up, bit 1 = flaps down, bit 2 = trim down, bit 3 = trim up.
/// Feature report: 5 cells top row, 5 cells bottom row, then one byte of button LEDs
/// (bit 0 AP, 1 HDG, 2 NAV, 3 IAS, 4 ALT, 5 VS, 6 APR, 7 REV).
/// </remarks>
public sealed class MultiPanel : HidPanel
{
    public enum Mode { Alt, Vs, Ias, Hdg, Crs }

    private static readonly string[] ButtonEvents =
    {
        "AP_PANEL_HEADING_HOLD", "AP_NAV1_HOLD", "AP_PANEL_SPEED_HOLD", "AP_PANEL_ALTITUDE_HOLD",
        "AP_PANEL_VS_HOLD", "AP_APR_HOLD", "AP_BC_HOLD",
    };

    private static readonly SimVar[] LedVars =
    {
        SimVar.ApMaster, SimVar.ApHeadingLock, SimVar.ApNav1Lock, SimVar.ApAirspeedHold,
        SimVar.ApAltitudeLock, SimVar.ApVerticalHold, SimVar.ApApproachHold, SimVar.ApBackcourseHold,
    };

    private Mode mode = Mode.Alt;

    public MultiPanel() : base(0x0D06, "Multi Panel") { }

    protected override void OnInput(byte[] report, byte[] previous, SimClient sim)
    {
        for (int i = 0; i < 5; i++)
            if ((report[1] & (1 << i)) != 0) mode = (Mode)i;

        int b1 = Pressed(report, previous, 1);
        if ((b1 & 0x20) != 0) Turn(sim, "INC");
        if ((b1 & 0x40) != 0) Turn(sim, "DEC");
        if ((b1 & 0x80) != 0) sim.Send("AP_MASTER");

        int b2 = Pressed(report, previous, 2);
        for (int i = 0; i < ButtonEvents.Length; i++)
            if ((b2 & (1 << i)) != 0) sim.Send(ButtonEvents[i]);

        // The autothrottle switch is a latching toggle: follow its position, only on change.
        bool armSwitch = (report[2] & 0x80) != 0;
        if (armSwitch != ((previous[2] & 0x80) != 0) && armSwitch != sim.IsOn(SimVar.AutothrottleArm))
            sim.Send("AUTO_THROTTLE_ARM");

        int b3 = Pressed(report, previous, 3);
        if ((b3 & 0x01) != 0) sim.Send("FLAPS_DECR");
        if ((b3 & 0x02) != 0) sim.Send("FLAPS_INCR");
        if ((b3 & 0x04) != 0) sim.Send("ELEV_TRIM_DN");
        if ((b3 & 0x08) != 0) sim.Send("ELEV_TRIM_UP");
    }

    private void Turn(SimClient sim, string direction) => sim.Send(mode switch
    {
        Mode.Alt => "AP_ALT_VAR_",
        Mode.Vs => "AP_VS_VAR_",
        Mode.Ias => "AP_SPD_VAR_",
        Mode.Hdg => "HEADING_BUG_",
        _ => "VOR1_OBI_",
    } + direction);

    protected override void Render(byte[] feature, SimClient sim)
    {
        if (!sim.Connected)
        {
            BlankFeature(feature);
            return;
        }

        var (top, bottom) = mode switch
        {
            Mode.Alt or Mode.Vs => (Whole(sim[SimVar.ApAltitude]), Whole(sim[SimVar.ApVerticalSpeed])),
            Mode.Ias => (Whole(sim[SimVar.ApAirspeed]), ""),
            Mode.Hdg => (Degrees(sim[SimVar.ApHeading]), ""),
            _ => (Degrees(sim[SimVar.Nav1Obs]), ""),
        };
        SevenSegment.Write(feature.AsSpan(1, 5), top);
        SevenSegment.Write(feature.AsSpan(6, 5), bottom);

        byte leds = 0;
        for (int i = 0; i < LedVars.Length; i++)
            if (sim.IsOn(LedVars[i])) leds |= (byte)(1 << i);
        feature[11] = leds;
    }

    private static string Whole(double value) => ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture);

    private static string Degrees(double value) => (((int)Math.Round(value) % 360 + 360) % 360).ToString("000", CultureInfo.InvariantCulture);

    protected override void BlankFeature(byte[] feature)
    {
        SevenSegment.Clear(feature.AsSpan(1, 5));
        SevenSegment.Clear(feature.AsSpan(6, 5));
        feature[11] = 0;
    }
}
