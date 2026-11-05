using Microsoft.Xna.Framework;

namespace ZombieSurvival;

internal sealed class Bullet(Vector2 position, Vector2 velocity, WeaponDefinition weapon)
{
    public Vector2 Position { get; private set; } = position;
    public int Damage { get; } = weapon.Damage;
    public bool IsExplosive { get; } = weapon.ExplosionRadius > 0 && !weapon.IsFlame;
    public bool IsFlame { get; } = weapon.IsFlame;
    public float ExplosionRadius { get; } = weapon.ExplosionRadius;
    public bool IsExpired { get; private set; }
    private int _remainingPierces = weapon.PierceCount;
    private readonly HashSet<Zombie> _hitZombies = [];
    private float _lifetime = weapon.ProjectileLifetime;

    public void Update(float delta, GameMap map)
    {
        Vector2 nextPosition = Position + velocity * delta;
        if (map.TryGetBlockPoint(Position, nextPosition, out Vector2 blockPoint))
        {
            Position = blockPoint;
            IsExpired = true;
        }
        else
        {
            Position = nextPosition;
        }

        _lifetime -= delta;
        IsExpired |= _lifetime <= 0 || Position.X < 0 || Position.X > 1280 ||
                     Position.Y < 0 || Position.Y > 720;
    }

    public bool CanHit(Zombie zombie) => !_hitZombies.Contains(zombie);

    public bool Hit(Zombie zombie)
    {
        _hitZombies.Add(zombie);
        if (_remainingPierces <= 0)
            IsExpired = true;
        else
            _remainingPierces--;
        return IsExpired;
    }

    public void Expire() => IsExpired = true;
}
