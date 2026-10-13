# SaitekBridge

[English](README.md) · [Português](README.pt-BR.md) · **Español**

Driver gratuito y de código abierto que hace que los **Saitek / Logitech Pro Flight Radio Panel y Multi
Panel** vuelvan a funcionar en **Microsoft Flight Simulator 2024**.

El plugin oficial de Logitech se actualizó por última vez en 2020, para MSFS 2020. En MSFS 2024 los paneles
suelen quedarse apagados: el Radio Panel no muestra nada y el Multi Panel enciende algunos LEDs, pero ningún
número. Las alternativas que existen son de pago. SaitekBridge es un programa pequeño que habla con los
paneles por USB y con el simulador por SimConnect, y solo hace eso.

## Funciones

**Radio Panel**, en ambas filas:

| Modo | Pantallas (activa / standby)           | Perilla exterior    | Perilla interior    | ACT/STBY                       |
|------|----------------------------------------|---------------------|---------------------|--------------------------------|
| COM1 | MHz activa / standby                   | MHz                 | kHz                 | intercambia                    |
| COM2 | MHz activa / standby                   | MHz                 | kHz                 | intercambia                    |
| NAV1 | MHz activa / standby                   | MHz                 | kHz                 | intercambia                    |
| NAV2 | MHz activa / standby                   | MHz                 | kHz                 | intercambia                    |
| ADF  | kHz activa / standby                   | 100 kHz             | 10 kHz              | intercambia                    |
| DME  | distancia NAV1 (nm) / velocidad (kt)   | —                   | —                   | —                              |
| XPDR | código del transpondedor               | 1.º o 3.er dígito   | 2.º o 4.º dígito    | alterna entre el 1.er y el 2.º par |

**Multi Panel**:

| Selector | Pantalla superior        | Pantalla inferior    | Perilla              |
|----------|--------------------------|----------------------|----------------------|
| ALT      | altitud del autopiloto   | velocidad vertical   | altitud              |
| VS       | altitud del autopiloto   | velocidad vertical   | velocidad vertical   |
| IAS      | velocidad del autopiloto | —                    | velocidad            |
| HDG      | heading bug              | —                    | heading bug          |
| CRS      | curso del NAV1 (OBS)     | —                    | curso                |

- Los botones AP, HDG, NAV, IAS, ALT, VS, APR y REV activan y desactivan los modos del autopiloto, y los
  LEDs siguen el estado real del autopiloto del avión.
- El interruptor AUTO THROTTLE arma y desarma el autothrottle.
- La palanca de flaps y la rueda de compensación (trim) funcionan.

Los botones hacen lo que el avión permite: el Cessna 172 no tiene autothrottle ni modo IAS, y NAV solo se
activa con un plan de vuelo o un VOR sintonizado, igual que en el avión real.

## Instalación

1. **Conecta los paneles directamente a un puerto USB de la placa base.** No uses un hub USB 3.x: en él los
   paneles siguen enviando los botones, e incluso pueden encender los LEDs, pero las pantallas de números
   se quedan apagadas.
2. Descarga el `SaitekBridge-vX.Y.Z-win-x64.zip` más reciente en [Releases](../../releases) y descomprímelo
   en una carpeta definitiva, por ejemplo `C:\SaitekBridge`.
3. SaitekBridge necesita `SimConnect.dll`:
   - si el plugin de paneles de Logitech está instalado, se encuentra automáticamente;
   - si no, instala el SDK de MSFS 2024 (Dev Mode → SDK) y copia `SimConnect SDK\lib\SimConnect.dll` en la
     misma carpeta que `SaitekBridge.exe`.
4. Haz doble clic en **`install.bat`**. A partir de ahora, MSFS 2024 abrirá SaitekBridge automáticamente.
   La entrada del plugin de Logitech se desactiva para que los dos no se peleen por los paneles.
5. Abre el simulador y a volar.

Para deshacerlo, ejecuta `uninstall.bat`: se quita SaitekBridge y la entrada del plugin de Logitech vuelve a
activarse. Se guarda una copia de la configuración anterior como `exe.xml.bak`.

SmartScreen de Windows puede avisar que el programa no es reconocido, porque no tiene firma digital. Haz clic
en "Más información" → "Ejecutar de todas formas", o compílalo tú mismo desde el código fuente.

### Ejecución manual

También puedes abrir `SaitekBridge.exe` directamente, antes o después del simulador. Espera a los paneles y
al simulador, y se reconecta si alguno de los dos se cae. Opciones de línea de comandos:

```
--install        abre SaitekBridge junto con MSFS 2024 (edita exe.xml)
--uninstall      lo quita de exe.xml y reactiva la entrada del plugin de Logitech
--debug          muestra cada reporte USB de los paneles y cada evento enviado al simulador
--exit-with-sim  se cierra cuando se cierra el simulador (usado por --install)
```

## Problemas comunes

| Síntoma | Solución |
|---------|----------|
| Los botones funcionan y los LEDs encienden, pero no hay números | Quita el panel del hub USB y conéctalo directamente a la placa base. |
| "SimConnect.dll not found" | Mira el paso 3 de la instalación. |
| Las pantallas parpadean o muestran valores incorrectos | Comprueba que el plugin de Logitech no esté ejecutándose a la vez (Administrador de tareas → `LogiMicrosoftFlightSimulator.exe`). |
| Una perilla no hace nada en algún avión | Algunos add-ons complejos (PMDG, Fenix…) ignoran los eventos estándar del simulador. Los aviones por defecto y la mayoría de los aviones ligeros funcionan. |
| El panel no se detecta | Ejecuta `PanelTest` (ver abajo) para probar el panel sin el simulador. |

## Compilar desde el código

Requiere el [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build SaitekBridge.slnx
dotnet run --project src/SaitekBridge -- --debug
```

`tools/PanelTest` enciende todas las pantallas con un patrón de prueba y muestra los reportes USB, sin
necesidad del simulador. El protocolo USB de ambos paneles está documentado en
[docs/PROTOCOL.md](docs/PROTOCOL.md).

## Contribuir

Issues y pull requests son bienvenidos, sobre todo soporte para el Switch Panel y el BIP / FIP, y
asignaciones específicas por avión.

## Agradecimientos

Los detalles del protocolo se contrastaron con los proyectos de código abierto
[xsaitekpanels](https://github.com/sparker256/xsaitekpanels) (X-Plane) y
[DCSFlightpanels](https://github.com/DCS-Skunkworks/DCSFlightpanels).

## Licencia

[MIT](LICENSE) © agentkyo. Sin relación con Logitech, Saitek ni Microsoft.
