namespace ConsoleCity.Game;

public sealed record GameCommand(GameCommandType Type, string? Payload = null);
