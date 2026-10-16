# Son Denge — AI Agent Rules

We are building a 48-hour Unity 2D game jam MVP about disaster preparedness.
Goal: a playable Preparation -> Earthquake -> Results loop.
Unity version: 6000.3.26f1 (Unity 6.3 LTS), URP 2D, new Input System.

Read docs/GDD.md and docs/API_CONTRACTS.md before editing.
Do NOT expand scope beyond your assigned feature.
Do NOT change public interfaces (Assets/Scripts/Core/Contracts.cs) without approval.
Do NOT edit Assets/Scenes/Main.unity unless you are the integration owner.
Do NOT modify ProjectSettings or Packages without approval.
Always preserve Unity .meta files, avoid reimport churn, and keep scripts compilable.
Write small, testable C# components; prefer clarity to abstractions.
When MCP is enabled, use only capabilities verified to exist in the installed MCP server.
Before coding, state planned files. After coding, report files changed, test result,
manual Unity steps, and open issues. Never claim tests ran if they did not.
Safety: clearly distinguish fantasy furniture manipulation from real-life earthquake advice.

## Code conventions
- Namespaces: `Jam.Core` (core + contracts), `Jam.UI`, and `Jam.<Area>` for your area
  (e.g. `Jam.Physics`, `Jam.Disasters`, `Jam.Preparation`, `Jam.Economy`, `Jam.Awareness`, `Jam.Player`).
- Core systems (`GameManager`, `AudioManager`, `SceneLoader`) are auto-created by `Bootstrap`
  in every scene; do not add them to scenes manually.
- Phase changes: `GameManager.Instance.SetPhase(GamePhase.X)`; listen via `GameManager.OnPhaseChanged`.
- Pause is not a phase: `GameManager.Instance.SetPaused(bool)` / `IsPaused` / `OnPauseChanged`.
- Input: use `UnityEngine.InputSystem` (legacy `Input` class is disabled).
