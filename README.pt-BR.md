# SaitekBridge for MSFS 2024

[English](README.md) · **Português** · [Español](README.es.md)

Driver gratuito e open source que faz os **Saitek / Logitech Pro Flight Radio Panel e Multi Panel**
voltarem a funcionar no **Microsoft Flight Simulator 2024**.

O plugin oficial da Logitech foi atualizado pela última vez em 2020, para o MSFS 2020. No MSFS 2024 os
painéis costumam ficar apagados: o Radio Panel não mostra nada e o Multi Panel acende alguns LEDs, mas nenhum
número. As alternativas que existem são pagas. O SaitekBridge é um programa pequeno que conversa com os
painéis pelo USB e com o simulador pelo SimConnect, e faz só isso.

## Funções

**Radio Panel**, nas duas fileiras:

| Modo | Displays (ativo / standby)            | Knob externo       | Knob interno       | ACT/STBY                     |
|------|---------------------------------------|--------------------|--------------------|------------------------------|
| COM1 | MHz ativo / standby                   | MHz                | kHz                | troca                        |
| COM2 | MHz ativo / standby                   | MHz                | kHz                | troca                        |
| NAV1 | MHz ativo / standby                   | MHz                | kHz                | troca                        |
| NAV2 | MHz ativo / standby                   | MHz                | kHz                | troca                        |
| ADF  | kHz ativo / standby                   | 100 kHz            | 10 kHz             | troca                        |
| DME  | distância NAV1 (nm) / velocidade (kt) | —                  | —                  | —                            |
| XPDR | código do transponder                 | 1º ou 3º dígito    | 2º ou 4º dígito    | alterna entre o 1º e o 2º par |

**Multi Panel**:

| Seletor | Display de cima        | Display de baixo     | Knob                 |
|---------|------------------------|----------------------|----------------------|
| ALT     | altitude do autopilot  | velocidade vertical  | altitude             |
| VS      | altitude do autopilot  | velocidade vertical  | velocidade vertical  |
| IAS     | velocidade do autopilot| —                    | velocidade           |
| HDG     | heading bug            | —                    | heading bug          |
| CRS     | curso do NAV1 (OBS)    | —                    | curso                |

- Os botões AP, HDG, NAV, IAS, ALT, VS, APR e REV ligam e desligam os modos do autopilot, e os LEDs
  acompanham o estado real do autopilot do avião.
- A chave AUTO THROTTLE arma e desarma o autothrottle.
- A alavanca de flaps e a roda de trim funcionam.

Os botões fazem o que o avião permite: o Cessna 172 não tem autothrottle nem modo IAS, e o NAV só engata
com plano de voo ou VOR sintonizado, igual no avião de verdade.

## Instalação

1. **Ligue os painéis direto numa porta USB da placa-mãe.** Não use hub USB 3.x: nele os painéis continuam
   mandando os botões, e podem até acender os LEDs, mas os displays de números ficam apagados.
2. Baixe o `SaitekBridge-vX.Y.Z-win-x64.zip` mais recente em [Releases](../../releases) e extraia numa
   pasta definitiva, por exemplo `C:\SaitekBridge`.
3. O SaitekBridge precisa do `SimConnect.dll`:
   - se o plugin dos painéis da Logitech estiver instalado, ele é encontrado automaticamente;
   - senão, instale o SDK do MSFS 2024 (Dev Mode → SDK) e copie `SimConnect SDK\lib\SimConnect.dll`
     para a mesma pasta do `SaitekBridge.exe`.
4. Dê dois cliques em **`install.bat`**. A partir de agora, o MSFS 2024 abre o SaitekBridge sozinho.
   A entrada do plugin da Logitech é desativada, para os dois não brigarem pelos painéis.
5. Abra o simulador e voe.

Para desfazer, rode `uninstall.bat`: o SaitekBridge sai e a entrada do plugin da Logitech volta a ficar
ativa. Uma cópia da configuração anterior fica salva como `exe.xml.bak`.

O SmartScreen do Windows pode avisar que o programa não é reconhecido, porque ele não tem assinatura
digital. Clique em "Mais informações" → "Executar assim mesmo", ou compile você mesmo a partir do código.

### Rodando manualmente

Também dá para só abrir o `SaitekBridge.exe`, antes ou depois do simulador. Ele espera os painéis e o
simulador, e reconecta se algum dos dois cair. Opções de linha de comando:

```
--install        abre o SaitekBridge junto com o MSFS 2024 (edita o exe.xml)
--uninstall      tira ele do exe.xml e reativa a entrada do plugin da Logitech
--debug          mostra cada relatório USB dos painéis e cada evento enviado ao simulador
--exit-with-sim  fecha quando o simulador fechar (usado pelo --install)
```

## Problemas comuns

| Sintoma | Solução |
|---------|---------|
| Os botões funcionam e os LEDs acendem, mas os números não | Tire o painel do hub USB e ligue direto na placa-mãe. |
| "SimConnect.dll not found" | Veja o passo 3 da instalação. |
| Os displays piscam ou mostram valores errados | Confira se o plugin da Logitech não está rodando junto (Gerenciador de Tarefas → `LogiMicrosoftFlightSimulator.exe`). |
| Um knob não faz nada em algum avião | Alguns add-ons complexos (PMDG, Fenix…) ignoram os eventos padrão do simulador. Os aviões padrão e a maioria dos aviões leves funcionam. |
| O painel não é detectado | Rode o `PanelTest` (veja abaixo) para testar o painel sem o simulador. |

## Compilando o código

Precisa do [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build SaitekBridge.slnx
dotnet run --project src/SaitekBridge -- --debug
```

O `tools/PanelTest` acende todos os displays com um padrão de teste e mostra os relatórios USB, sem precisar
do simulador. O protocolo USB dos dois painéis está documentado em [docs/PROTOCOL.md](docs/PROTOCOL.md).

## Contribuindo

Issues e pull requests são bem-vindos, principalmente suporte ao Switch Panel e ao BIP / FIP, e
mapeamentos específicos por avião.

## Agradecimentos

Os detalhes do protocolo foram conferidos com os projetos open source
[xsaitekpanels](https://github.com/sparker256/xsaitekpanels) (X-Plane) e
[DCSFlightpanels](https://github.com/DCS-Skunkworks/DCSFlightpanels).

## Licença

[MIT](LICENSE) © agentkyo. Sem vínculo com Logitech, Saitek ou Microsoft.
