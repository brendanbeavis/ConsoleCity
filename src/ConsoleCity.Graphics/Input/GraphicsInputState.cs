using System.Numerics;

namespace ConsoleCity.Graphics.Input;

public sealed record GraphicsInputState(
    Vector2 MousePosition,
    Vector2 MouseDelta,
    float MouseWheelDelta,
    bool IsPanning,
    bool SelectPressed,
    bool ConfirmPressed,
    bool CancelPressed,
    bool TogglePausePressed,
    bool StepPressed,
    bool FasterPressed,
    bool SlowerPressed,
    bool ToggleBuildModePressed,
    bool NextBuildTypePressed,
    bool PreviousBuildTypePressed);
