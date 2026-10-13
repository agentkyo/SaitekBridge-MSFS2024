namespace SaitekBridge.Sim;

/// <summary>Every simulation variable the panels display. The enum value is also the SimConnect define/request id.</summary>
public enum SimVar
{
    Com1Active, Com1Standby, Com2Active, Com2Standby,
    Nav1Active, Nav1Standby, Nav2Active, Nav2Standby,
    AdfActive, AdfStandby,
    Dme1Distance, Dme1Speed, Dme2Distance, Dme2Speed,
    Transponder,

    ApAltitude, ApVerticalSpeed, ApAirspeed, ApHeading, Nav1Obs,

    ApMaster, ApHeadingLock, ApNav1Lock, ApAirspeedHold,
    ApAltitudeLock, ApVerticalHold, ApApproachHold, ApBackcourseHold,
    AutothrottleArm,
}

public static class SimVars
{
    public static readonly IReadOnlyDictionary<SimVar, (string Name, string Units)> Definitions =
        new Dictionary<SimVar, (string, string)>
        {
            [SimVar.Com1Active] = ("COM ACTIVE FREQUENCY:1", "MHz"),
            [SimVar.Com1Standby] = ("COM STANDBY FREQUENCY:1", "MHz"),
            [SimVar.Com2Active] = ("COM ACTIVE FREQUENCY:2", "MHz"),
            [SimVar.Com2Standby] = ("COM STANDBY FREQUENCY:2", "MHz"),
            [SimVar.Nav1Active] = ("NAV ACTIVE FREQUENCY:1", "MHz"),
            [SimVar.Nav1Standby] = ("NAV STANDBY FREQUENCY:1", "MHz"),
            [SimVar.Nav2Active] = ("NAV ACTIVE FREQUENCY:2", "MHz"),
            [SimVar.Nav2Standby] = ("NAV STANDBY FREQUENCY:2", "MHz"),
            [SimVar.AdfActive] = ("ADF ACTIVE FREQUENCY:1", "KHz"),
            [SimVar.AdfStandby] = ("ADF STANDBY FREQUENCY:1", "KHz"),
            [SimVar.Dme1Distance] = ("NAV DME:1", "Nautical miles"),
            [SimVar.Dme1Speed] = ("NAV DMESPEED:1", "Knots"),
            [SimVar.Dme2Distance] = ("NAV DME:2", "Nautical miles"),
            [SimVar.Dme2Speed] = ("NAV DMESPEED:2", "Knots"),
            [SimVar.Transponder] = ("TRANSPONDER CODE:1", "Bco16"),

            [SimVar.ApAltitude] = ("AUTOPILOT ALTITUDE LOCK VAR", "Feet"),
            [SimVar.ApVerticalSpeed] = ("AUTOPILOT VERTICAL HOLD VAR", "Feet per minute"),
            [SimVar.ApAirspeed] = ("AUTOPILOT AIRSPEED HOLD VAR", "Knots"),
            [SimVar.ApHeading] = ("AUTOPILOT HEADING LOCK DIR", "Degrees"),
            [SimVar.Nav1Obs] = ("NAV OBS:1", "Degrees"),

            [SimVar.ApMaster] = ("AUTOPILOT MASTER", "Bool"),
            [SimVar.ApHeadingLock] = ("AUTOPILOT HEADING LOCK", "Bool"),
            [SimVar.ApNav1Lock] = ("AUTOPILOT NAV1 LOCK", "Bool"),
            [SimVar.ApAirspeedHold] = ("AUTOPILOT AIRSPEED HOLD", "Bool"),
            [SimVar.ApAltitudeLock] = ("AUTOPILOT ALTITUDE LOCK", "Bool"),
            [SimVar.ApVerticalHold] = ("AUTOPILOT VERTICAL HOLD", "Bool"),
            [SimVar.ApApproachHold] = ("AUTOPILOT APPROACH HOLD", "Bool"),
            [SimVar.ApBackcourseHold] = ("AUTOPILOT BACKCOURSE HOLD", "Bool"),
            [SimVar.AutothrottleArm] = ("AUTOPILOT THROTTLE ARM", "Bool"),
        };
}
