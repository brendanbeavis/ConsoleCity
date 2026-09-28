using ConsoleCity.Core;

namespace ConsoleCity.Game;

public static class PolicyEngine
{
    public static bool TrySetPolicy(GameProgressionState progression, GamePolicyId policyId, decimal intensity, SimulationTime currentTime, out GameProgressionState updatedState)
    {
        ArgumentNullException.ThrowIfNull(progression);
        if (intensity is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(intensity));
        }

        var definition = PolicyCatalog.Get(policyId);
        if (!definition.IsEligible(progression))
        {
            updatedState = progression;
            return false;
        }

        var policies = progression.ActivePolicies.Where(policy => policy.Id != policyId).ToList();
        policies.Add(new GamePolicyInstance(policyId, intensity, currentTime));
        updatedState = progression with
        {
            ActivePolicies = policies,
            Log = progression.Log.Concat([new ProgressionLogEntry(currentTime, "policy", $"{definition.Name} set to {intensity:P0}")]).ToList()
        };

        return true;
    }

    public static decimal ResolveDecimal(GameProgressionState progression, string target, decimal baseValue)
    {
        ArgumentNullException.ThrowIfNull(progression);

        var result = baseValue;
        foreach (var effect in progression.ActivePolicies
                     .Select(policy => PolicyCatalog.Get(policy.Id))
                     .SelectMany(definition => definition.Effects.Select(effect => new { Definition = definition, Effect = effect }))
                     .Where(item => string.Equals(item.Effect.Target, target, StringComparison.OrdinalIgnoreCase)))
        {
            var policy = progression.ActivePolicies.First(instance => instance.Id == effect.Definition.Id);
            var scaledValue = effect.Effect.Kind == ModifierEffectKind.Multiplicative
                ? 1m + ((effect.Effect.Value - 1m) * policy.Intensity)
                : effect.Effect.Value * policy.Intensity;

            result = effect.Effect.Kind switch
            {
                ModifierEffectKind.Additive => result + scaledValue,
                ModifierEffectKind.Multiplicative => result * scaledValue,
                ModifierEffectKind.Override => scaledValue,
                ModifierEffectKind.ClampMinimum => Math.Max(result, scaledValue),
                ModifierEffectKind.ClampMaximum => Math.Min(result, scaledValue),
                _ => result
            };
        }

        return result;
    }

    public static GameProgressionState Advance(GameProgressionState progression, SimulationTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(progression);

        if (currentTime.Tick < progression.CycleState.NextTransitionAt.Tick)
        {
            return progression;
        }

        var updatedCycle = progression.CycleState;
        var log = progression.Log.ToList();
        while (currentTime.Tick >= updatedCycle.NextTransitionAt.Tick)
        {
            updatedCycle = updatedCycle.Advance();
            log.Add(new ProgressionLogEntry(updatedCycle.StartedAt, "cycle", $"Cycle {updatedCycle.CycleNumber} has begun."));
        }

        return progression with
        {
            CycleState = updatedCycle,
            Log = log
        };
    }
}