using System.Numerics;
using Raylib_cs;
using ConsoleCity.Graphics.Models;

namespace ConsoleCity.Graphics.Input;

public sealed class RaylibInputReader
{
    public GraphicsInputState Read(Vector2 logicalMousePosition, Vector2 logicalMouseDelta)
        => new(
            logicalMousePosition,
            logicalMouseDelta,
            Raylib.GetMouseWheelMove(),
            Raylib.IsMouseButtonDown(MouseButton.Middle),
            Raylib.IsMouseButtonPressed(MouseButton.Left),
            Raylib.IsMouseButtonPressed(MouseButton.Right) || Raylib.IsKeyPressed(KeyboardKey.Enter),
            Raylib.IsKeyPressed(KeyboardKey.Escape),
            Raylib.IsKeyPressed(KeyboardKey.Space),
            Raylib.IsKeyPressed(KeyboardKey.Period),
            Raylib.IsKeyPressed(KeyboardKey.Equal) || Raylib.IsKeyPressed(KeyboardKey.KpAdd),
            Raylib.IsKeyPressed(KeyboardKey.Minus) || Raylib.IsKeyPressed(KeyboardKey.KpSubtract),
            Raylib.IsKeyPressed(KeyboardKey.B),
            Raylib.IsKeyPressed(KeyboardKey.Tab),
            Raylib.IsKeyPressed(KeyboardKey.Q));
}
