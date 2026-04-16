# NL2Geo demo script

1. Open Rhino 8 and load the plugin build.
2. Run command: `NL2GEO`.
3. Enter prompt: `Create a box with width 5, height 3, and depth 4 meters`.
4. Confirm geometry creation and command log output.
5. Enter multi-step prompt: `Create a 3x3 grid of boxes and rotate them 45 degrees`.
6. Verify both pattern and transform operations execute in one undo group.
7. Test edge cases:
   - `Create a box 0.001mm wide`
   - `Make it bigger and smaller at the same time`
   - `Create a NURBS butterfly`
8. Confirm warnings/errors are user-friendly and no crash occurs.
