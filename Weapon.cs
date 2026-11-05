namespace ZombieSurvival;

internal enum WeaponType
{
    Pistol,
    HeavyPistol,
    Rifle,
    Shotgun,
    SubmachineGun,
    SniperRifle,
    GrenadeLauncher,
    Flamethrower,
    DeathMachine
}

internal sealed record WeaponDefinition(
    WeaponType Type,
    string Name,
    int Damage,
    int MagazineCapacity,
    int ReserveCapacity,
    float FireInterval,
    float ReloadDuration,
    int Pellets = 1,
    float Spread = 0,
    float ProjectileSpeed = 620,
    float ProjectileLifetime = 1.6f,
    float ExplosionRadius = 0,
    int PierceCount = 0,
    bool IsFlame = false,
    bool IsAutomatic = false,
    bool IsRare = false)
{
    public static WeaponDefinition Get(WeaponType type) => type switch
    {
        WeaponType.Pistol => new(type, "PISTOL", 34, 12, 72, 0.24f, 1.25f),
        WeaponType.HeavyPistol => new(type, "HEAVY PISTOL", 68, 8, 48, 0.42f, 1.4f, ProjectileSpeed: 680),
        WeaponType.Rifle => new(type, "RIFLE", 42, 24, 144, 0.12f, 1.45f, IsAutomatic: true),
        WeaponType.Shotgun => new(type, "SHOTGUN", 22, 6, 48, 0.72f, 1.55f,
            Pellets: 7, Spread: 0.2f, ProjectileSpeed: 520, ProjectileLifetime: 0.42f),
        WeaponType.SubmachineGun => new(type, "SMG", 22, 32, 192, 0.075f, 1.5f, IsAutomatic: true),
        WeaponType.SniperRifle => new(type, "SNIPER", 110, 5, 35, 0.85f, 1.7f,
            ProjectileSpeed: 1000, ProjectileLifetime: 2.2f, PierceCount: 3),
        WeaponType.GrenadeLauncher => new(type, "GRENADE LAUNCHER", 95, 4, 24, 0.8f, 1.8f,
            ProjectileSpeed: 330, ProjectileLifetime: 2.5f, ExplosionRadius: 100, IsRare: true),
        WeaponType.Flamethrower => new(type, "FLAMETHROWER", 12, 120, 360, 0.045f, 2f,
            ProjectileSpeed: 260, ProjectileLifetime: 0.32f, ExplosionRadius: 34,
            IsFlame: true, IsAutomatic: true, IsRare: true),
        WeaponType.DeathMachine => new(type, "DEATH MACHINE", 100, 300, 0, 0.055f, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
