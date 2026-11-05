using Microsoft.Xna.Framework;

namespace ZombieSurvival;

internal enum PowerUpType
{
    MaxAmmo,
    DoublePoints,
    InstaKill,
    Nuke,
    Carpenter,
    FireSale,
    DeathMachine
}

internal abstract class PowerUp(Vector2 position, PowerUpType type)
{
    private const float SpawnChance = 0.14f;
    private float _animationTime;
    private float _remainingLifetime = 22f;

    public Vector2 Position { get; } = position;
    public PowerUpType Type { get; } = type;
    public bool IsExpired => _remainingLifetime <= 0;
    public float BobOffset => MathF.Sin(_animationTime * 4f) * 3f;

    public static PowerUp? TrySpawn(Random random, Vector2 position)
    {
        if (random.NextDouble() >= SpawnChance)
            return null;

        PowerUpType[] types = Enum.GetValues<PowerUpType>();
        return new DroppedPowerUp(position, types[random.Next(types.Length)]);
    }

    public void Update(float delta)
    {
        _animationTime += delta;
        _remainingLifetime -= delta;
    }

    public bool CanCollect(Vector2 playerPosition) =>
        Vector2.DistanceSquared(Position, playerPosition) <= 38 * 38;

    public static float Duration(PowerUpType type) => type switch
    {
        PowerUpType.DoublePoints or PowerUpType.InstaKill or PowerUpType.FireSale => 30f,
        PowerUpType.DeathMachine => 20f,
        _ => 0f
    };

    public static string Label(PowerUpType type) => type switch
    {
        PowerUpType.MaxAmmo => "AMMO",
        PowerUpType.DoublePoints => "2X POINTS",
        PowerUpType.InstaKill => "INSTA-KILL",
        PowerUpType.Nuke => "NUKE",
        PowerUpType.Carpenter => "CARPENTER",
        PowerUpType.FireSale => "FIRE SALE",
        PowerUpType.DeathMachine => "DEATH MACHINE",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public static Color Color(PowerUpType type) => type switch
    {
        PowerUpType.MaxAmmo => new Color(74, 160, 220),
        PowerUpType.DoublePoints => new Color(244, 194, 66),
        PowerUpType.InstaKill => new Color(216, 56, 49),
        PowerUpType.Nuke => new Color(113, 204, 100),
        PowerUpType.Carpenter => new Color(190, 132, 76),
        PowerUpType.FireSale => new Color(202, 92, 207),
        PowerUpType.DeathMachine => new Color(80, 207, 183),
        _ => Microsoft.Xna.Framework.Color.White
    };

    private sealed class DroppedPowerUp(Vector2 position, PowerUpType type) : PowerUp(position, type)
    {
    }
}
