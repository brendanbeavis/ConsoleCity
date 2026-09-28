using ConsoleCity.Core;

namespace ConsoleCity.Game;

public static class ModifierEngine
{
    public static bool TryPurchaseModifier(GameProgressionState progression, GameModifierId modifierId, SimulationTime currentTime, out GameProgressionState updatedState)
    {
        ArgumentNullException.ThrowIfNull(progression);

        var definition = ModifierCatalog.Get(modifierId);
        if (!definition.IsEligible(progression) || progression.DevelopmentCredits < definition.CreditCost)
        {
            updatedState = progression;
            return false;
        }

        var existingInstance = progression.ActiveModifiers.FirstOrDefault(instance => instance.Id == modifierId);
        if (definition.StackingRule == ModifierStackingRule.Unique && existingInstance is not null)
        {
            updatedState = progression;
            return false;
        }

        var remainingModifiers = progression.ActiveModifiers.Where(instance => instance.Id != modifierId).ToList();
        var newInstance = new GameModifierInstance(
            modifierId,
            currentTime,
            definition.DurationTicks is null ? null : currentTime.Advance(definition.DurationTicks.Value),
            definition.StackingRule == ModifierStackingRule.Stack && existingInstance is not null ? existingInstance.Stacks + 1 : 1);

        if (definition.StackingRule == ModifierStackingRule.Stack && existingInstance is not null)
        {
            remainingModifiers.Add(existingInstance with { Stacks = existingInstance.Stacks + 1 });
        }
        else if (definition.StackingRule == ModifierStackingRule.RefreshDuration && existingInstance is not null)
        {
            remainingModifiers.Add(existingInstance with
            {
                AcquiredAt = currentTime,
                ExpiresAt = definition.DurationTicks is null ? null : currentTime.Advance(definition.DurationTicks.Value)
            });
        }
        else
        {
            remainingModifiers.Add(newInstance);
        }

        updatedState = progression with
        {
            DevelopmentCredits = progression.DevelopmentCredits - decimal.ToInt32(definition.CreditCost),
            ActiveModifiers = remainingModifiers,
            Log = progression.Log.Concat([new ProgressionLogEntry(currentTime, "modifier", $"{definition.Name} acquired for {definition.CreditCost:0.##} credits")]).ToList()
        };

        return true;
    }

    public static GameProgressionState Advance(GameProgressionState progression, SimulationTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(progression);

        var expired = progression.ActiveModifiers.Where(instance => instance.ExpiresAt is not null && instance.ExpiresAt.Value.Tick <= currentTime.Tick).ToList();
        if (expired.Count == 0)
        {
            return progression;
        }

        var remaining = progression.ActiveModifiers.Except(expired).ToList();
        var log = progression.Log.Concat(expired.Select(instance => new ProgressionLogEntry(currentTime, "modifier", $"{ModifierCatalog.Get(instance.Id).Name} expired"))).ToList();

        return progression with
        {
            ActiveModifiers = remaining,
            Log = log
        };
    }

    public static decimal ResolveDecimal(GameProgressionState progression, string target, decimal baseValue)
    {
        ArgumentNullException.ThrowIfNull(progression);

        var result = baseValue;
        foreach (var effect in GetEffects(progression, target))
        {
            result = effect.Kind switch
            {
                ModifierEffectKind.Additive => result + effect.Value,
                ModifierEffectKind.Multiplicative => result * effect.Value,
                ModifierEffectKind.Override => effect.Value,
                ModifierEffectKind.ClampMinimum => Math.Max(result, effect.Value),
                ModifierEffectKind.ClampMaximum => Math.Min(result, effect.Value),
                _ => result
            };
        }

        return result;
    }

    public static IReadOnlyList<GameModifierEffect> GetEffects(GameProgressionState progression, string target)
    {
        ArgumentNullException.ThrowIfNull(progression);

        return progression.ActiveModifiers
            .SelectMany(instance => Enumerable.Repeat(instance, Math.Max(1, instance.Stacks)))
            .SelectMany(instance => ModifierCatalog.Get(instance.Id).Effects)
            .Where(effect => string.Equals(effect.Target, target, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
