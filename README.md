# Steampunk Cave

Steampunk Cave is a Unity game project about exploring underground tunnels
with a swarm of robots. The project combines exploration, resource gathering,
branching progression, crafting, random events and combat.

This README describes the project as a whole: how to open it, where the main
systems are located and what is currently known about its status. It is not
intended to imply that every system or asset in the repository was created by
one person.

> **Status:** prototype / active development
> **Engine:** Unity `6000.3.0f1`
> **Language:** C#

## Contents

- [Project overview](#project-overview)
- [Main systems](#main-systems)
- [Requirements](#requirements)
- [Opening the project](#opening-the-project)
- [Scenes](#scenes)
- [Controls](#controls)
- [Repository structure](#repository-structure)
- [Code tour](#code-tour)
- [Save data](#save-data)
- [Testing](#testing)
- [Third-party content](#third-party-content)
- [License](#license)

## Project overview

The player explores procedurally assembled cave sections, controls a group of
robots and makes progression choices on a branching map. The game also
contains a restroom/hub area with crafting, events and management screens.

The project is built as a Unity Editor project rather than a distributable
release. Some scenes are development, test or trailer scenes and may require
specific objects, assets or editor setup to work correctly.

## Main systems

### Tunnel generation

The tunnel system assembles level sections from prefab pools. Depending on its
configuration, it can create single or double tunnel sections, place link and
end segments, spawn resources and prepare navigation data. The main runtime
entry point is [`TunnelGenerator.cs`](Assets/_Scripts/Tunnel%20Generation/TunnelGenerator.cs).

Configuration is kept separately in
[`TunnelGeneratorConfig.cs`](Assets/_Scripts/Tunnel%20Generation/TunnelGeneratorConfig.cs).

### Map progression

The map is represented as a layered graph of path nodes. Nodes are generated
with configurable row, column and total-node ranges, connected to subsequent
rows and pruned when they become unreachable or lead to dead ends.

Relevant files:

- [`MapGenerator.cs`](Assets/_Scripts/Restroom/Map/MapGenerator.cs)
- [`MapState.cs`](Assets/_Scripts/Restroom/Map/MapState.cs)

### Units, swarm and interactions

The project contains systems for robot data, health, energy, movement,
interactions, mining, equipment, abilities and enemy behaviour. These systems
are spread across [`Assets/_Scripts/Units`](Assets/_Scripts/Units) and
[`Assets/_Scripts/Swarm`](Assets/_Scripts/Swarm).

### Crafting and inventory

Crafting and inventory data use Unity `ScriptableObject` assets. Inventory
operations include adding, removing, transferring and serialising items.
Relevant code is located in [`Assets/_Scripts/ScriptableObjectDorian`](Assets/_Scripts/ScriptableObjectDorian)
and [`Assets/_Scripts/Restroom/Crafting`](Assets/_Scripts/Restroom/Crafting).

### Events and interface

Random events are configured as data and displayed through UI Toolkit. The
interface uses UXML layouts and USS styles from
[`Assets/UI Toolkit`](Assets/UI%20Toolkit), with runtime controllers in
[`Assets/_Scripts/Restroom/Events`](Assets/_Scripts/Restroom/Events) and
[`Assets/_Scripts/UI`](Assets/_Scripts/UI).

### Audio, visual effects and tools

The project integrates FMOD for audio and contains project shaders, visual
effects and several small Unity Editor tools. Editor tools are available from
the Unity **Tools** menu after the project has compiled.

## Requirements

- Unity Hub;
- Unity Editor `6000.3.0f1`;
- a machine capable of running Unity 6;
- access to the FMOD integration and its required project data;
- Git LFS, if large binary files are configured for LFS in the repository.

The package versions are defined in
[`Packages/manifest.json`](Packages/manifest.json) and locked in
[`Packages/packages-lock.json`](Packages/packages-lock.json).

## Opening the project

1. Clone the repository:

   ```bash
   git clone https://gitlab.com/Rumsse/steampunk-cave.git
   cd steampunk-cave
   ```

2. Open the project in Unity Hub.
3. Select Unity `6000.3.0f1`.
4. Wait for Unity to import the assets and resolve the packages.
5. Open one of the scenes listed below.
6. Press **Play** in the Unity Editor.

The repository does not currently define a reproducible command-line build
pipeline or a packaged release. Build target and scene configuration should be
checked in Unity's Build Profiles/Build Settings before making a build.

## Scenes

The main scenes are stored in [`Assets/_Scenes`](Assets/_Scenes):

- `Main Menu.unity` — main menu;
- `Tunnel Generation.unity` — tunnel generation scene;
- `Restroom Asylum.unity` — restroom/hub area;
- `Boss Scene.unity` — boss encounter;
- `Tutorials/Tutorial.unity` — tutorial scene;
- `Tutorials/Restroom Tutorial.unity` — restroom tutorial;
- `Test Tunel Generation Test.unity` — tunnel-generation test scene.

## Controls

Input is configured with Unity's Input System. The action asset is
[`InputSystem_Actions.inputactions`](Assets/InputSystem_Actions.inputactions).

The project uses actions for unit selection and commands, movement, abilities,
form switching, camera control and menu navigation. The exact keyboard and
gamepad bindings should be checked in the Input Actions editor because they
may change during development.

## Repository structure

```text
Assets/
├── _Scripts/                  C# gameplay, UI, tools and utility code
├── _Scenes/                   Unity scenes
├── _ScriptableObjects/        Data and configuration assets
├── UI Toolkit/                UXML layouts and USS styles
├── Prefabs/                   Reusable scene objects
├── Plugins/                   Integrations and third-party plugins
├── Shaders/                   Project shaders
└── Free Assets/               External/free asset content
Packages/                      Unity package manifest and lock file
ProjectSettings/               Unity project configuration
```

## Code tour

The following files are relatively focused starting points for someone reading
the code. They should be considered an orientation guide, not a claim that
they represent the complete architecture or that every file is production
ready.

### Map generation

[`MapGenerator.cs`](Assets/_Scripts/Restroom/Map/MapGenerator.cs) is a compact,
mostly self-contained example of layered graph generation, connection
creation, dead-end pruning and best-effort node-count selection.

### Editor texture search

[`TextureSearcherWindow.cs`](Assets/_Scripts/Editor/TextureSearcherWindow.cs)
is a Unity `EditorWindow` that searches the active scene for a selected
texture across sprites, UI images, particle systems and mesh materials.

### Editor batch tools

[`BulkRenamerWindow.cs`](Assets/_Scripts/Editor/BulkRenamerWindow.cs) and
[`GlobalSpacingChanger.cs`](Assets/_Scripts/Editor/GlobalSpacingChanger.cs)
are smaller workflow tools for renaming selected assets/objects and applying
TextMesh Pro spacing to loaded scenes and prefabs.

### Tunnel configuration

[`TunnelGeneratorConfig.cs`](Assets/_Scripts/Tunnel%20Generation/TunnelGeneratorConfig.cs)
is a better focused entry point into tunnel generation than the coordinating
runtime class. It contains prefab-pool configuration, random section lengths
and editor-time validation.

Before publishing code as a personal portfolio sample, verify authorship and
review the current version of the file. This repository has a team history,
and not every gameplay script should be attributed to the same contributor.

## Save data

The project contains autosave and slot-based save functionality. It stores
swarm data, inventories, gadgets, map progression, events and play time under
Unity's `Application.persistentDataPath`.

The relevant entry point is
[`SaveManager.cs`](Assets/_Scripts/Save%20System/SaveManager.cs). The current
save implementation is prototype code: the encryption key is present in the
source, so the encryption must not be treated as protection against a
determined player or as a secure secret-storage mechanism.

## Repository

[gitlab.com/Rumsse/steampunk-cave](https://gitlab.com/Rumsse/steampunk-cave)
