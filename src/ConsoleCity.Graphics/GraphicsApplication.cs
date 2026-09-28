using System.Numerics;
using ConsoleCity.Game;
using ConsoleCity.Graphics.Controllers;
using ConsoleCity.Graphics.Input;
using ConsoleCity.Graphics.Rendering;
using Raylib_cs;

namespace ConsoleCity.Graphics;

public sealed class GraphicsApplication
{
    private const int LogicalWidth = 1280;
    private const int LogicalHeight = 720;
    private const int WindowWidth = 1920;
    private const int WindowHeight = 1080;

    public void Run()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(WindowWidth, WindowHeight, "ConsoleCity Graphics");
        Raylib.SetTargetFPS(60);

        var renderTexture = Raylib.LoadRenderTexture(LogicalWidth, LogicalHeight);
        Raylib.SetTextureFilter(renderTexture.Texture, TextureFilter.Point);

        var session = new GameSession();
        var controller = new GraphicsInteractionController(session);
        controller.EnsureWorld();
        session.Start();

        var inputReader = new RaylibInputReader();
        var renderer = new MainScreenRenderer();

        try
        {
            while (!Raylib.WindowShouldClose())
            {
                var layout = renderer.BuildLayout(LogicalWidth, LogicalHeight);
                var destination = RenderScaleHelper.GetDestinationRectangle(LogicalWidth, LogicalHeight, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                var logicalMousePosition = RenderScaleHelper.MapWindowToLogical(Raylib.GetMousePosition(), destination, LogicalWidth, LogicalHeight);
                var logicalMouseDelta = RenderScaleHelper.ScaleWindowDeltaToLogical(Raylib.GetMouseDelta(), destination, LogicalWidth, LogicalHeight);
                var input = inputReader.Read(logicalMousePosition, logicalMouseDelta);
                controller.HandleInput(input, layout.Map);
                controller.Update(Raylib.GetFrameTime());
                var view = controller.BuildView();

                Raylib.BeginTextureMode(renderTexture);
                renderer.Render(view, controller.Camera, LogicalWidth, LogicalHeight);
                Raylib.EndTextureMode();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(0, 0, 0, 255));
                DrawScaledRenderTexture(renderTexture);
                Raylib.EndDrawing();
            }
        }
        finally
        {
            Raylib.UnloadRenderTexture(renderTexture);
            Raylib.CloseWindow();
        }
    }

    private static void DrawScaledRenderTexture(RenderTexture2D renderTexture)
    {
        var destination = RenderScaleHelper.GetDestinationRectangle(LogicalWidth, LogicalHeight, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        Raylib.DrawTexturePro(
            renderTexture.Texture,
            new Rectangle(0, 0, LogicalWidth, -LogicalHeight),
            destination,
            Vector2.Zero,
            0f,
            Color.White);
    }
}
