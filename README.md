# CEOSimulatorSP

A GTA V single-player ScriptHookVDotNet v3 mod that simulates executive life as the CEO of a Fortune 500-style corporation. Run the company from your HQ, handle board politics, resolve crises, and manage reputation, morale, risk, and stock performance.

## Features
- **Executive HQ interactions**: Executive Desk (CEO Dashboard), Assistant Desk (Daily Briefing), Boardroom Chair (Board Meeting).
- **Daily corporate events**: 2–4 events per day with meaningful choices and outcomes.
- **Persistent CEO stats**: Power, Public Reputation, Board Approval, Employee Morale, Risk Level, Stock Value, Cash.
- **Board meeting system**: Performance reviews, votes, and possible CEO removal with recovery path.
- **World reactions**: Protesters, security guards, and regulators based on your decisions.
- **Save/Load**: JSON saves stored in `/scripts/CEOSimulatorSP/`.

## Installation
1. Install **ScriptHookV** and **ScriptHookVDotNet v3**.
2. Install **NativeUI** (copy `NativeUI.dll` into your GTA V `scripts` folder).
3. Drop the compiled `CEOSimulatorSP.dll` into your GTA V `scripts` folder.
4. Launch GTA V Story Mode.

> Optional: Enable All Interiors (EAI) can be used to access interiors, but this mod does not require it.

## Keybinds
- **Open Dashboard (quick access)**: `F7`
- **Interact (markers)**: `E`

You can change these in `/scripts/CEOSimulatorSP/Config.json`.

## Gameplay Loop
1. Enter HQ and use:
   - **Executive Desk** → CEO Dashboard
   - **Assistant Desk** → Daily Briefing
   - **Boardroom Chair** → Board Meeting
2. Resolve daily events from the dashboard.
3. End the day to generate new events and apply overhead costs.
4. Track board approval and avoid termination threats.

## Configuration
Edit `/scripts/CEOSimulatorSP/Config.json` to adjust markers and tuning:
- `OfficeMarker`, `DeskMarker`, `AssistantMarker`, `BoardroomChairMarker`
- `HQExteriorProtestSpawn`
- `HQInteriorSecuritySpawnPoints`
- `InvestigatorSpawn`
- `Keybinds`
- `Tuning` thresholds and daily overhead

A default `Config.json` is generated automatically if missing.

## Save/Log Files
- **Save file**: `/scripts/CEOSimulatorSP/save.json`
- **Log file**: `/scripts/CEOSimulatorSP/log.txt`

Saves are written on:
- End Day
- Manual Save
- Script abort

## Troubleshooting
- **Menu not showing**: Ensure `NativeUI.dll` is in the `scripts` folder.
- **Script not loading**: Verify ScriptHookVDotNet v3 is installed and the DLL is in `scripts`.
- **Markers not in the right place**: Edit `Config.json` marker coordinates to match your interior.
- **No events showing**: Use the Dashboard → End Day to generate events.

## Development Notes
- Built for ScriptHookVDotNet v3
- Uses NativeUI for menus
- Events are stored and generated from a built-in library (25+ events)

Enjoy running your corporate empire.
