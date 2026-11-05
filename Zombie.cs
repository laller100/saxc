using Microsoft.Xna.Framework;

namespace ZombieSurvival;

internal enum GameDifficulty
{
    Easy,
    Normal,
    Hard
}

internal sealed class Zombie(Vector2 position, int round, GameDifficulty difficulty)
{
    public Vector2 Position { get; private set; } = position;
    public int MaxHealth { get; } = Math.Max(1, (int)MathF.Ceiling((60 + (round - 1) * 18) *
        (difficulty == GameDifficulty.Easy ? 0.75f : difficulty == GameDifficulty.Hard ? 1.4f : 1f)));
    public int Health { get; private set; } = Math.Max(1, (int)MathF.Ceiling((60 + (round - 1) * 18) *
        (difficulty == GameDifficulty.Easy ? 0.75f : difficulty == GameDifficulty.Hard ? 1.4f : 1f)));
    public bool IsDead => Health <= 0;
    public int ContactDamage { get; } = Math.Max(1, (int)MathF.Ceiling(
        (12 + Math.Min(round - 1, 10) * 2) *
        (difficulty == GameDifficulty.Easy ? 0.7f : difficulty == GameDifficulty.Hard ? 1.5f : 1f)));
    public float AnimationTime { get; private set; }
    public float HitFlash { get; private set; }
    private readonly float _speed = (48f + Math.Min(round - 1, 20) * 3.5f) *
        (difficulty == GameDifficulty.Easy ? 0.8f : difficulty == GameDifficulty.Hard ? 1.2f : 1f);
    private float _pathRefreshTimer;
    private Vector2 _waypoint;

    public void Update(float delta, Vector2 playerPosition, GameMap map)
    {
        AnimationTime += delta;
        HitFlash = Math.Max(0, HitFlash - delta);
        _pathRefreshTimer -= delta;
        if (_pathRefreshTimer <= 0 || Vector2.DistanceSquared(Position, _waypoint) < 18 * 18)
        {
            _waypoint = map.FindNextWaypoint(Position, playerPosition, 10f);
            _pathRefreshTimer = 0.3f;
        }

        Vector2 direction = _waypoint - Position;
        if (direction.LengthSquared() > 0.01f)
        {
            direction.Normalize();
            Position = map.MoveZombie(Position, direction * _speed * delta, _waypoint, 10f);
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        HitFlash = 0.12f;
    }
}
