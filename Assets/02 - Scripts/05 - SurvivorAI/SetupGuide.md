# Survivor AI Setup Guide

Since you mentioned you are new to Unity, here is a step-by-step guide to setting up the AI behaviors I implemented.

## 1. Create Prefabs
You need to create 4 objects (Human, Predator, Prey, Vegetable).

1.  **Right-click** in the Hierarchy -> **3D Object** -> **Capsule** (or Cube/Sphere).
2.  Name it "Human".
3.  **Add Component** -> Search for `EntityIdentity`.
    *   Set **Type** to `Human`.
4.  **Add Component** -> Search for `SurvivorAgent`.
    *   You can tweak `Vision Radius`, `Move Speed`, etc.
5.  Drag the "Human" object from the Hierarchy into the `Assets/05 - Prefabs` (or any folder) to create a **Prefab**.
6.  Delete the "Human" from the Hierarchy.

Repeat for **Predator**:
*   Type: `Predator`
*   Add `SurvivorAgent`.
*   Maybe change color (Create Material -> Red -> Drag on object) to distinguish.

Repeat for **Prey**:
*   Type: `Prey`
*   Add `SurvivorAgent`.

Repeat for **Vegetable**:
*   Type: `Vegetable`
*   **Do NOT** add `SurvivorAgent` (vegetables don't move).
*   Maybe use a Sphere or a small Cube.

## 2. Set up the Spawner
1.  Create an **Empty GameObject** in the scene (Right-click -> Create Empty).
2.  Name it "GameManager" or "Spawner".
3.  **Add Component** -> `SurvivorSpawner`.
4.  Drag your Prefabs from the Project window into the slots (`Human Prefab`, `Predator Prefab`, etc.).
5.  Adjust the counts (e.g., 5 Humans, 2 Predators).

## 3. Play
1.  Press the **Play** button at the top.
2.  You should see objects spawning on the terrain and moving around.
3.  Humans should chase Vegetables/Prey and run from Predators.

## 4. Setup Fishes (Boids)
1. Create a new Capsule or Fish model.
2. Add `SurvivorAgent` and `EntityIdentity` components.
3. Set `EntityIdentity` -> `Type` to `Fish`.
4. In `SurvivorAgent`, adjust Boid parameters under "Boids (Fish Only)":
   - `Separation Weight`: How strongly they push apart.
   - `Alignment Weight`: How much they steer same direction.
   - `Cohesion Weight`: How much they group together.
   - `Neighbor Radius`: How far they see friends.

## 5. HP & Energy Mechanics
- **Energy**:
  - Humans lose energy over time.
  - Moving costs more energy than standing still.
  - Running (Chase/Flee) costs the most energy.
  - Eating food restores Energy and HP.
- **HP**:
  - If Energy hits 0, HP starts dropping (Starvation).
  - Predators deal damage to HP instead of instant kill.
  - If HP hits 0, the agent dies.

## 6. Testing
- Press Play.
- Watch Fishes flock together.
- Watch Humans eat to survive and run from Predators.
- Watch Energy bars (debug or inspector) go down.

## Notes
*   **Terrain**: The scripts assume there is a `CustomTerrain` object in the scene (from your existing project). If not, they will just move on a flat plane.
*   **Visuals**: You can replace the Capsules with actual character models later by dragging the model into the Prefab and making it a child, then disabling the MeshRenderer of the capsule.
