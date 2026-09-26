using System;

public enum RandomPickupOutcome
{
    None = 0,
    ReverseSteering = 1,
    FullFuel = 2
}

/// <summary>Deterministic 50/50 outcome source; a seed makes editor tests reproducible.</summary>
public sealed class RandomPickupOutcomeState
{
    private readonly Random random;

    public RandomPickupOutcomeState(int? seed = null)
    {
        random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public RandomPickupOutcome Next()
    {
        return random.Next(2) == 0
            ? RandomPickupOutcome.ReverseSteering
            : RandomPickupOutcome.FullFuel;
    }
}
