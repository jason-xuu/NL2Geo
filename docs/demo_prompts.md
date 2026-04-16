# Demo prompts

## Single-step prompts (5)

1. `Create a box with width 5, height 3, and depth 4 meters`
   - Expected: `create_box` with three positive dimensions.
2. `Create a cylinder radius 2 height 8`
   - Expected: `create_cylinder` with radius/height.
3. `Move the selected objects 10 units in X`
   - Expected: `move` transform; requires non-empty selection.
4. `Rotate selected objects 45 degrees around Z`
   - Expected: `rotate` transform with angle + axis.
5. `Create a 3 by 3 grid of boxes`
   - Expected: `array_grid` pattern operation.

## Multi-step prompts (3)

6. `Create a box 4x4x4 and move it 10 units in X`
   - Expected: two operations (`create_box`, `move`).
7. `Create a cylinder radius 2 height 6, then rotate it 30 degrees`
   - Expected: `create_cylinder` then `rotate`.
8. `Create a 3x3 grid of boxes and rotate them 45 degrees`
   - Expected: `array_grid` plus `rotate`.

## Edge-case prompts (5 required by plan)

9. `Create a box 0.001mm wide`
   - Expected: warning for unrealistic unit range.
10. `Create a sphere`
   - Expected: missing dimensions; infer defaults or request clarification.
11. `Make it bigger and smaller at the same time`
   - Expected: reject conflicting intent with explanation.
12. `Create a NURBS butterfly`
   - Expected: unsupported operation with supported-operation hint.
13. `Move the selected objects up 5m`
   - Expected: null-selection handling when no active selection.
