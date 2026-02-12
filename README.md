# ExecutiveTycoon (CEO Simulator for GTA V SP)

ExecutiveTycoon is a single-player ScriptHookVDotNet3 mod that turns GTA V into an executive strategy sim.

- No manual delivery/heist missions
- Real-time business simulation
- Office ownership with one active HQ
- Desktop-driven management UI (LemonUI)
- Separate save system (no overwrite of GTAO_Businesses or other economy mods)

## Compatibility Targets

- GTA V Steam build `1.0.3725.0`
- ScriptHookV + ScriptHookVDotNet3
- LemonUI.SHVDN3
- Newtonsoft.Json
- Designed to co-exist with:
  - `EnableAllInteriors.dll`
  - `GTAO_Businesses.dll`
  - `GTAO_MansionsInSP.dll`

## V1 Features

- Office broker purchase flow (LemonUI fallback)
- GTAO executive office catalog (Arcadius, Maze Bank West, Lombank, Maze Tower)
- Multiple owned offices, one active HQ
- Executive Desktop marker in active HQ (`E` to open dashboard)
- Two-ledger model:
  - Personal funds (GTA player money)
  - Company treasury (internal)
- 5-second simulation pulse + daily cycle (`10 min real = 1 business day`)
- Weekly report every 7 business days
- Manager hiring (COO, CFO, Head of Security)
- 5 directives:
  - Expand Region
  - Increase Compliance
  - Decrease Compliance
  - Increase Security Budget
  - Launch Product Line
- Autosave every 2 minutes + manual save from dashboard

## Build (Visual Studio)

1. Open `ExecutiveTycoon.sln` in Visual Studio 2022.
2. Ensure references resolve in `src/ExecutiveTycoon/ExecutiveTycoon.csproj`:
   - `lib/ScriptHookVDotNet3.dll`
   - `lib/LemonUI.SHVDN3.dll`
   - `lib/Newtonsoft.Json.dll`
3. Build `Release` to produce `ExecutiveTycoon.dll`.

## Install

1. Copy `ExecutiveTycoon.dll` to GTA V `scripts/`.
2. Copy `scripts/ExecutiveTycoon.ini` to GTA V `scripts/`.
3. Ensure folder exists:
   - `scripts/ExecutiveTycoon/`
   - `scripts/ExecutiveTycoon/logs/`
4. Launch GTA V story mode.

## Runtime Files

ExecutiveTycoon writes only to:

- `scripts/ExecutiveTycoon/config.ini`
- `scripts/ExecutiveTycoon/save.json`
- `scripts/ExecutiveTycoon/logs/*.log`

## Keybinds

- `E` near active HQ desktop marker: Open CEO dashboard
- `F7` default: Quick KPI overlay

All keybinds and pacing are configurable in `scripts/ExecutiveTycoon.ini`.

## V2 Roadmap (not implemented)

- Full department model (Ops, Finance, Legal, Security, Sales, HR)
- Additional executive/region manager hiring pool
- Event chains with multi-stage outcomes
- Debt instruments and credit rating
- Competitor pressure and macro cycle simulation
- Board goals/quarter planning and scorecards
