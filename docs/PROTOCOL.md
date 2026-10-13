# Saitek / Logitech panel USB protocol

Both panels are plain USB HID devices with vendor id `0x06A3`. No driver is needed to talk to them:
the displays and LEDs are written with a **HID feature report**, and buttons, knobs and switches
arrive as **HID input reports**. All values below were verified on real hardware.

| Panel       | Product id | Feature report | Input report |
|-------------|------------|----------------|--------------|
| Radio Panel | `0x0D05`   | 23 bytes       | 4 bytes      |
| Multi Panel | `0x0D06`   | 13 bytes       | 4 bytes      |

Byte 0 of every report is the report id and is always `0x00`.

## Display cells

Every display window has 5 cells, written left to right, one byte per cell:

| Value         | Shows                                        |
|---------------|----------------------------------------------|
| `0x00`-`0x09` | digit 0-9                                    |
| `0x0F`        | blank                                        |
| `0xD0` OR'ed  | same digit plus the decimal point after it   |

Example: `118.25` → `01 01 D8 02 05`.

## Radio Panel (`0x0D05`)

### Feature report (displays)

| Bytes | Window                       |
|-------|------------------------------|
| 1-5   | upper stack, active (left)   |
| 6-10  | upper stack, standby (right) |
| 11-15 | lower stack, active          |
| 16-20 | lower stack, standby         |

The remaining bytes are unused (zero).

### Input report

| Byte | Bits | Meaning                                                    |
|------|------|------------------------------------------------------------|
| 1    | 0-6  | upper selector: COM1, COM2, NAV1, NAV2, ADF, DME, XPDR      |
| 1    | 7    | lower selector: COM1                                       |
| 2    | 0-5  | lower selector: COM2, NAV1, NAV2, ADF, DME, XPDR            |
| 2    | 6    | upper ACT/STBY button                                      |
| 2    | 7    | lower ACT/STBY button                                      |
| 3    | 0 / 1 | upper inner (fine) knob: one detent clockwise / counter-clockwise |
| 3    | 2 / 3 | upper outer (coarse) knob: clockwise / counter-clockwise  |
| 3    | 4-7  | same as bits 0-3, for the lower stack                      |

The selectors report their position as a level (exactly one bit set per stack). Knob bits
are pulses: each detent sends a report with the bit set, followed by one with it cleared.

## Multi Panel (`0x0D06`)

### Feature report (displays and LEDs)

| Byte | Meaning                                                              |
|------|----------------------------------------------------------------------|
| 1-5  | top row                                                              |
| 6-10 | bottom row                                                           |
| 11   | button LEDs: bit 0 AP, 1 HDG, 2 NAV, 3 IAS, 4 ALT, 5 VS, 6 APR, 7 REV |

### Input report

| Byte | Bits | Meaning                                              |
|------|------|------------------------------------------------------|
| 1    | 0-4  | selector: ALT, VS, IAS, HDG, CRS (level)             |
| 1    | 5 / 6 | knob detent: clockwise / counter-clockwise (pulse)  |
| 1    | 7    | AP button                                            |
| 2    | 0-6  | buttons: HDG, NAV, IAS, ALT, VS, APR, REV            |
| 2    | 7    | AUTO THROTTLE switch in ARM (level)                  |
| 3    | 0 / 1 | flaps lever up / down (held)                        |
| 3    | 2 / 3 | pitch trim wheel: nose down / nose up (pulse)       |

## Known hardware pitfall

On USB 3.x hubs the panels may enumerate and send input, and even light the Multi Panel button LEDs,
while every 7-segment display stays dark. Plug the panels straight into a motherboard USB port.
