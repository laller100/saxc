using Microsoft.Xna.Framework;

namespace ZombieSurvival;

internal sealed class DefenseWindow(Vector2 position)
{
    private float _damageProgress;

    public const int MaxHealth = 100;
    public Vector2 Position { get; } = position;
    public int Health { get; private set; } = MaxHealth;

    public void Update(float delta, IReadOnlyList<Zombie> zombies)
    {
        int attackers = zombies.Count(zombie =>
            Vector2.DistanceSquared(zombie.Position, Position) < 44 * 44);
        if (attackers > 0)
        {
            _damageProgress += attackers * 8f * delta;
            int damage = (int)_damageProgress;
            Health = Math.Max(0, Health - damage);
            _damageProgress -= damage;
        }
    }

    public void Repair()
    {
        Health = MaxHealth;
        _damageProgress = 0;
    }
}
