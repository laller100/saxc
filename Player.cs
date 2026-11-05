using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ZombieSurvival;

internal sealed class Player
{
    private sealed class AmmoState(WeaponDefinition definition)
    {
        public int Magazine { get; set; } = definition.MagazineCapacity;
        public int Reserve { get; set; } = definition.ReserveCapacity;
        public int UpgradeLevel { get; set; }
    }

    public const int MaxWeaponUpgradeLevel = 3;

    private readonly Dictionary<WeaponType, AmmoState> _weapons = new()
    {
        [WeaponType.Pistol] = new(WeaponDefinition.Get(WeaponType.Pistol))
    };
    private WeaponType _activeWeapon = WeaponType.Pistol;
    private WeaponType _savedWeapon = WeaponType.Pistol;
    private float _fireTimer;
    private float _reloadTimer;
    private bool _reloading;
    private int _deathMachineAmmo;
    private float _deathMachineTimer;

    public Vector2 Position { get; private set; } = new(640, 380);
    public int Health { get; private set; } = 100;
    public const int MaxHealth = 100;
    public bool IsDeathMachineActive { get; private set; }
    public bool IsReloading => _reloading;
    public WeaponDefinition CurrentWeapon => IsDeathMachineActive
        ? WeaponDefinition.Get(WeaponType.DeathMachine)
        : GetWeaponDefinition(_activeWeapon);
    public string WeaponName => IsDeathMachineActive ? "DEATH MACHINE" : CurrentWeapon.Name;
    public int Magazine => IsDeathMachineActive ? _deathMachineAmmo : GetActiveAmmo().Magazine;
    public int ReserveAmmo => IsDeathMachineActive ? 0 : GetActiveAmmo().Reserve;
    public int MagazineCapacity => IsDeathMachineActive ? 300 : CurrentWeapon.MagazineCapacity;
    public int Damage => IsDeathMachineActive ? 100 : CurrentWeapon.Damage;
    public float FireInterval => IsDeathMachineActive ? 0.055f : CurrentWeapon.FireInterval;
    public bool HasRifle => _weapons.ContainsKey(WeaponType.Rifle);
    public bool IsAutomatic => IsDeathMachineActive || CurrentWeapon.IsAutomatic;

    public void Update(GameTime gameTime, KeyboardState keyboard, GameMap map)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 direction = Vector2.Zero;
        if (keyboard.IsKeyDown(Keys.W)) direction.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S)) direction.Y += 1;
        if (keyboard.IsKeyDown(Keys.A)) direction.X -= 1;
        if (keyboard.IsKeyDown(Keys.D)) direction.X += 1;

        if (direction.LengthSquared() > 0)
        {
            direction.Normalize();
            Position = map.MovePlayer(Position, direction * 230f * delta, 10f);
        }

        _fireTimer = Math.Max(0, _fireTimer - delta);

        if (_reloading)
        {
            _reloadTimer -= delta;
            if (_reloadTimer <= 0)
            {
                AmmoState ammo = GetActiveAmmo();
                int loaded = Math.Min(CurrentWeapon.MagazineCapacity - ammo.Magazine, ammo.Reserve);
                ammo.Magazine += loaded;
                ammo.Reserve -= loaded;
                _reloading = false;
            }
        }

        if (IsDeathMachineActive)
        {
            _deathMachineTimer -= delta;
            if (_deathMachineTimer <= 0 || _deathMachineAmmo <= 0)
                DeactivateDeathMachine();
        }
    }

    public bool TryShoot()
    {
        if (_reloading || _fireTimer > 0 || Magazine <= 0)
            return false;

        if (IsDeathMachineActive)
            _deathMachineAmmo--;
        else
            GetActiveAmmo().Magazine--;
        _fireTimer = FireInterval;
        return true;
    }

    public void Reload()
    {
        if (IsDeathMachineActive || _reloading || Magazine >= MagazineCapacity || ReserveAmmo <= 0)
            return;

        _reloading = true;
        _reloadTimer = CurrentWeapon.ReloadDuration;
    }

    public void CycleWeapon()
    {
        if (IsDeathMachineActive || _weapons.Count < 2)
            return;

        WeaponType[] owned = _weapons.Keys.OrderBy(type => type).ToArray();
        int nextIndex = (Array.IndexOf(owned, _activeWeapon) + 1) % owned.Length;
        _activeWeapon = owned[nextIndex];
        _reloading = false;
        _reloadTimer = 0;
        _fireTimer = 0;
    }

    public bool GrantWeapon(WeaponType type)
    {
        WeaponDefinition definition = WeaponDefinition.Get(type);
        bool isNew = !_weapons.TryGetValue(type, out AmmoState? ammo);
        if (isNew || ammo is null)
        {
            ammo = new AmmoState(definition);
            _weapons.Add(type, ammo);
        }
        else
        {
            WeaponDefinition ownedDefinition = GetWeaponDefinition(type);
            ammo.Magazine = ownedDefinition.MagazineCapacity;
            ammo.Reserve = ownedDefinition.ReserveCapacity;
        }

        _activeWeapon = type;
        _reloading = false;
        _reloadTimer = 0;
        _fireTimer = 0;
        return isNew;
    }

    public int GetWeaponUpgradeLevel() => GetActiveAmmo().UpgradeLevel;

    public bool UpgradeCurrentWeapon()
    {
        if (IsDeathMachineActive)
            return false;

        AmmoState ammo = GetActiveAmmo();
        if (ammo.UpgradeLevel >= MaxWeaponUpgradeLevel)
            return false;

        ammo.UpgradeLevel++;
        WeaponDefinition upgraded = GetWeaponDefinition(_activeWeapon);
        ammo.Magazine = upgraded.MagazineCapacity;
        ammo.Reserve = upgraded.ReserveCapacity;
        _reloading = false;
        _reloadTimer = 0;
        _fireTimer = 0;
        return true;
    }

    public void BuyRifle() => GrantWeapon(WeaponType.Rifle);

    public void RefillAmmo()
    {
        foreach ((WeaponType type, AmmoState ammo) in _weapons)
        {
            WeaponDefinition definition = GetWeaponDefinition(type);
            ammo.Magazine = definition.MagazineCapacity;
            ammo.Reserve = definition.ReserveCapacity;
        }
        if (IsDeathMachineActive)
            _deathMachineAmmo = 300;
        _reloading = false;
        _reloadTimer = 0;
    }

    public void ActivateDeathMachine()
    {
        if (IsDeathMachineActive)
        {
            _deathMachineAmmo = 300;
            _deathMachineTimer = 20f;
            return;
        }

        _savedWeapon = _activeWeapon;
        IsDeathMachineActive = true;
        _deathMachineAmmo = 300;
        _deathMachineTimer = 20f;
        _reloading = false;
        _fireTimer = 0;
    }

    public void DeactivateDeathMachine()
    {
        if (!IsDeathMachineActive)
            return;

        IsDeathMachineActive = false;
        _activeWeapon = _savedWeapon;
        _deathMachineAmmo = 0;
        _deathMachineTimer = 0;
        _fireTimer = 0;
    }

    public void TakeDamage(int amount) => Health = Math.Max(0, Health - amount);

    public void Reset()
    {
        Position = new Vector2(640, 380);
        Health = MaxHealth;
        _weapons.Clear();
        _weapons.Add(WeaponType.Pistol, new AmmoState(WeaponDefinition.Get(WeaponType.Pistol)));
        _activeWeapon = WeaponType.Pistol;
        IsDeathMachineActive = false;
        _fireTimer = 0;
        _reloadTimer = 0;
        _reloading = false;
        _deathMachineAmmo = 0;
        _deathMachineTimer = 0;
    }

    private AmmoState GetActiveAmmo() => _weapons[_activeWeapon];

    private WeaponDefinition GetWeaponDefinition(WeaponType type)
    {
        WeaponDefinition definition = WeaponDefinition.Get(type);
        if (!_weapons.TryGetValue(type, out AmmoState? ammo) || ammo is null ||
            ammo.UpgradeLevel == 0)
            return definition;

        float multiplier = MathF.Pow(1.5f, ammo.UpgradeLevel);
        float magazineMultiplier = 1f + ammo.UpgradeLevel * 0.25f;
        return definition with
        {
            Damage = (int)MathF.Ceiling(definition.Damage * multiplier),
            MagazineCapacity = (int)MathF.Ceiling(definition.MagazineCapacity * magazineMultiplier),
            ReserveCapacity = (int)MathF.Ceiling(definition.ReserveCapacity * magazineMultiplier),
            FireInterval = definition.FireInterval * MathF.Pow(0.9f, ammo.UpgradeLevel)
        };
    }
}
