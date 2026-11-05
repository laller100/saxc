using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ZombieSurvival;

public sealed class Game1 : Game
{
    private const int Width = 1280;
    private const int Height = 720;
    private static readonly Rectangle StartButton = new(440, 265, 400, 58);
    private static readonly Rectangle[] DifficultyButtons =
    [
        new(420, 375, 138, 48),
        new(571, 375, 138, 48),
        new(722, 375, 138, 48)
    ];
    private static readonly Rectangle SettingsButton = new(440, 460, 400, 52);
    private static readonly Rectangle ExitButton = new(440, 530, 400, 52);
    private static readonly Rectangle SoundEffectsButton = new(440, 250, 400, 52);
    private static readonly Rectangle ScreenShakeButton = new(440, 320, 400, 52);
    private static readonly Rectangle FullscreenButton = new(440, 390, 400, 52);
    private static readonly Rectangle BackButton = new(440, 470, 400, 52);
    private static readonly Rectangle ResumeButton = new(440, 245, 400, 54);
    private static readonly Rectangle PauseSettingsButton = new(440, 315, 400, 54);
    private static readonly Rectangle MainMenuButton = new(440, 385, 400, 54);
    private static readonly Rectangle PauseExitButton = new(440, 455, 400, 54);
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private SoundEffects _soundEffects = null!;
    private RenderTarget2D _scene = null!;
    private Texture2D _pixel = null!;
    private Texture2D _playerSprite = null!;
    private Texture2D _pistolSprite = null!;
    private Texture2D _rifleSprite = null!;
    private Texture2D _shotgunSprite = null!;
    private Texture2D _smgSprite = null!;
    private Texture2D _sniperSprite = null!;
    private Texture2D _grenadeLauncherSprite = null!;
    private Texture2D _flamethrowerSprite = null!;
    private Texture2D _deathMachineSprite = null!;
    private Texture2D _zombieSprite = null!;
    private Texture2D _bulletSprite = null!;
    private readonly Player _player = new();
    private readonly List<Zombie> _zombies = [];
    private readonly List<Bullet> _bullets = [];
    private readonly List<Particle> _particles = [];
    private readonly List<PowerUp> _powerUps = [];
    private readonly Random _random = new();
    private readonly GameMap _map;
    private readonly List<DefenseWindow> _defenses = [];
    private Vector2[] _mysteryBoxPositions = [];
    private Vector2 _upgradeStationPosition;
    private readonly Dictionary<PowerUpType, float> _activePowerUps = [];
    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;
    private int _round = 1;
    private int _score;
    private int _points = 500;
    private int _zombiesRemainingToSpawn;
    private float _spawnTimer;
    private float _intermissionTimer;
    private float _damageCooldown;
    private float _muzzleFlashTimer;
    private float _gunRecoil;
    private float _cameraShake;
    private float _damageFlashTimer;
    private Vector2 _lastShotPosition;
    private Vector2 _lastShotDirection = Vector2.UnitX;
    private bool _inIntermission;
    private bool _gameOver;
    private bool _hasStarted;
    private bool _showSettings;
    private bool _isPaused;
    private bool _settingsFromPause;
    private bool _screenShakeEnabled = true;
    private bool _soundEffectsEnabled = true;
    private GameDifficulty _difficulty = GameDifficulty.Normal;
    private const int MysteryBoxCost = 950;
    private const int FireSaleCost = 10;
    private static readonly int[] WeaponUpgradeCosts = [1500, 2500, 4000];

    public Game1()
    {
        _map = new GameMap(_random);
        ResetMapObjects();
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = Width,
            PreferredBackBufferHeight = Height,
            HardwareModeSwitch = false,
            SynchronizeWithVerticalRetrace = true
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = false;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        Window.Title = "LAST STAND - Pixel Zombie Survival";
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _soundEffects = new SoundEffects();
        _scene = new RenderTarget2D(GraphicsDevice, Width, Height, false,
            SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _playerSprite = CreateSprite([
            "......GG......",
            ".....GGGG.....",
            ".....GFFG.....",
            ".....GFFG.....",
            "......GG......",
            "...BBBBBBBB...",
            "..BBBBBBBBBB..",
            "..BBBBBBBBBB..",
            "...BBBBBBBB...",
            "......BB......",
            ".....B..B.....",
            ".....B..B.....",
            "....BB..BB....",
            "....BB..BB...."
        ], new Dictionary<char, Color> { ['G'] = new(238, 188, 143), ['F'] = new(38, 38, 44), ['B'] = new(56, 122, 176) });
        _pistolSprite = CreateSprite([
            "...OOOOOOOO....",
            "..OGGGGGGGOO...",
            ".OGGGGGGGGGGO..",
            "..OGGGGGGGOO...",
            "....OBBB.......",
            "....OBBB.......",
            "....OOO........"
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(151, 164, 164),
            ['B'] = new(116, 72, 48)
        });
        _rifleSprite = CreateSprite([
            "....OOOOOOOOOOOOOOOO....",
            "...OGGGGGGGGGGGGGGGGO...",
            "OOOOOGGGGGGGGGGGGGGGGOO",
            "...OGGGGGGGGGGGGGGGGO...",
            ".....OBBB...........",
            ".....OBBB...........",
            ".....OOOO..........."
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(108, 132, 112),
            ['B'] = new(116, 72, 48)
        });
        _shotgunSprite = CreateSprite([
            "..OOOOOOOOOOOOOO....",
            ".OGGGGGGGGGGGGGGOO..",
            "..OOOOOOOOOOOOOO....",
            "....OBBBBBB.........",
            "....OBBBBBB.........",
            "....OOOOOO.........."
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(124, 132, 133),
            ['B'] = new(126, 78, 47)
        });
        _smgSprite = CreateSprite([
            "...OOOOOOOOOO...",
            "..OGGGGGGGGOO...",
            "...OOOOOOOOOO...",
            ".....OBBB.......",
            ".....OBBB.......",
            ".....OOOO......."
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(119, 135, 125),
            ['B'] = new(102, 75, 49)
        });
        _sniperSprite = CreateSprite([
            "OOOOOOOOOOOOOOOOOOOOOOOOOO",
            "..OGGGGGGGGGGGGGGGGGGGGO...",
            "OOOOOOOOOOOOOOOOOOOOOOOOOO",
            ".......OBBB...............",
            ".......OBBB...............",
            ".......OOOO..............."
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(127, 145, 130),
            ['B'] = new(104, 76, 49)
        });
        _grenadeLauncherSprite = CreateSprite([
            "..OOOOOOOOOOOOOOOO..",
            ".OGGGGGGGGGGGGGGGOO.",
            "..OOOOOOOOOOOOOOOO..",
            "....OBBB............",
            "....OBBB............",
            "....OOOO............"
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(111, 132, 94),
            ['B'] = new(106, 77, 45)
        });
        _flamethrowerSprite = CreateSprite([
            "..OOOOOOOOOOOOOO...",
            ".OGGGGGGGGGGGGGGOO.",
            "..OOOOOOOOOOOOOO...",
            "....OBBB..RR.......",
            "....OBBB..RR.......",
            "....OOOO..........."
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(116, 131, 105),
            ['B'] = new(94, 78, 55),
            ['R'] = new(244, 92, 40)
        });
        _deathMachineSprite = CreateSprite([
            "..OOOOOOOOOOOOOOOOOOOO..",
            "OOOGGGGGGGGGGGGGGGGGGOOO",
            "..OOOOOOOOOOOOOOOOOOOO..",
            ".....OBBB.....OBBB......",
            ".....OBBB.....OBBB......",
            ".....OOOO.....OOOO......"
        ], new Dictionary<char, Color>
        {
            ['O'] = new(27, 30, 34),
            ['G'] = new(114, 139, 113),
            ['B'] = new(91, 94, 85)
        });
        _zombieSprite = CreateSprite([
            "......GG......",
            ".....GGGG.....",
            ".....GEEG.....",
            ".....GEEG.....",
            "......GG......",
            "...GGGGGGGG...",
            "..GGGGGGGGGG..",
            "..GGGGGGGGGG..",
            "...GGGGGGGG...",
            "......GG......",
            ".....G..G.....",
            ".....G..G.....",
            "....GG..GG....",
            "....GG..GG...."
        ], new Dictionary<char, Color> { ['G'] = new(91, 157, 83), ['E'] = new(231, 66, 54) });
        _bulletSprite = CreateSprite(["YY", "YY"], new Dictionary<char, Color> { ['Y'] = new(255, 222, 92) });
    }

    private Texture2D CreateSprite(string[] rows, Dictionary<char, Color> palette)
    {
        int width = rows.Max(row => row.Length);
        var texture = new Texture2D(GraphicsDevice, width, rows.Length);
        var data = new Color[width * rows.Length];
        for (int y = 0; y < rows.Length; y++)
        {
            int offset = (width - rows[y].Length) / 2;
            for (int x = 0; x < rows[y].Length; x++)
                data[y * width + offset + x] = palette.TryGetValue(rows[y][x], out Color color)
                    ? color
                    : Color.Transparent;
        }
        texture.SetData(data);
        return texture;
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();

        if (Pressed(keyboard, Keys.Escape))
        {
            if (_showSettings)
            {
                _showSettings = false;
                _isPaused = _settingsFromPause;
                _settingsFromPause = false;
            }
            else if (_hasStarted)
                _isPaused = !_isPaused;
            else
                _showSettings = false;
            _previousKeyboard = keyboard;
            _previousMouse = mouse;
            base.Update(gameTime);
            return;
        }

        if (_isPaused || _showSettings)
        {
            UpdatePauseMenuInput(mouse);
            _previousKeyboard = keyboard;
            _previousMouse = mouse;
            base.Update(gameTime);
            return;
        }

        if (!_hasStarted)
        {
            UpdateMenuInput(mouse);
            _previousKeyboard = keyboard;
            _previousMouse = mouse;
            base.Update(gameTime);
            return;
        }

        if (_gameOver)
        {
            if (Pressed(keyboard, Keys.Enter) || Pressed(keyboard, Keys.R))
                Restart();
            _previousKeyboard = keyboard;
            _previousMouse = mouse;
            base.Update(gameTime);
            return;
        }

        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _player.Update(gameTime, keyboard, _map);
        UpdatePowerUpEffects(delta);
        _damageCooldown = Math.Max(0, _damageCooldown - delta);
        _muzzleFlashTimer = Math.Max(0, _muzzleFlashTimer - delta);
        _gunRecoil = Math.Max(0, _gunRecoil - delta * 8f);
        _cameraShake = Math.Max(0, _cameraShake - delta * 1.8f);
        _damageFlashTimer = Math.Max(0, _damageFlashTimer - delta);

        foreach (Particle particle in _particles)
            particle.Update(delta);
        _particles.RemoveAll(particle => particle.IsExpired);
        UpdatePowerUpDrops(delta);

        if (Pressed(keyboard, Keys.R))
        {
            bool wasReloading = _player.IsReloading;
            _player.Reload();
            if (!wasReloading && _player.IsReloading)
                PlaySound(_soundEffects.PlayReload);
        }

        Point virtualMouse = GetVirtualMousePosition(mouse);
        Vector2 aim = new(virtualMouse.X, virtualMouse.Y);
        bool firePressed = mouse.LeftButton == ButtonState.Pressed &&
            (_player.IsAutomatic || _previousMouse.LeftButton == ButtonState.Released);
        if (firePressed && _player.TryShoot())
        {
            Vector2 direction = aim - _player.Position;
            if (direction.LengthSquared() > 0.01f)
            {
                direction.Normalize();
                WeaponDefinition weapon = _player.CurrentWeapon;
                PlaySound(() => _soundEffects.PlayShot(weapon.Type));
                _lastShotDirection = direction;
                _lastShotPosition = _player.Position + direction * 34;
                for (int pellet = 0; pellet < weapon.Pellets; pellet++)
                {
                    float spread = ((float)_random.NextDouble() * 2f - 1f) * weapon.Spread;
                    Vector2 shotDirection = Rotate(direction, spread);
                    _bullets.Add(new Bullet(_player.Position + shotDirection * 18,
                        shotDirection * weapon.ProjectileSpeed, weapon));
                }
                _muzzleFlashTimer = weapon.IsFlame ? 0.1f : 0.075f;
                _gunRecoil = 1f;
                _cameraShake = Math.Max(_cameraShake, 0.055f);

                Vector2 casingDirection = new(-direction.Y, direction.X);
                casingDirection *= _random.Next(2) == 0 ? -1 : 1;
                if (!weapon.IsFlame && weapon.Type != WeaponType.GrenadeLauncher)
                    _particles.Add(new Particle(_player.Position + casingDirection * 9,
                        casingDirection * _random.Next(45, 90) - direction * 25,
                        new Color(236, 177, 69), 0.48f, 2.5f, 145f));
                if (weapon.IsFlame)
                    AddBurst(_lastShotPosition, new Color(255, 115, 42), 4, 45f, 3f, -20f, 0.2f);
            }
        }
        if (_player.IsDeathMachineActive && _player.Magazine == 0)
            EndPowerUp(PowerUpType.DeathMachine);

        if (Pressed(keyboard, Keys.E))
            Interact();
        if (Pressed(keyboard, Keys.Q))
            _player.CycleWeapon();

        if (_inIntermission)
        {
            _intermissionTimer -= delta;
            if (_intermissionTimer <= 0)
            {
                _round++;
                BeginRound();
            }
        }
        else
        {
            _spawnTimer -= delta;
            if (_zombiesRemainingToSpawn > 0 && _spawnTimer <= 0)
            {
                SpawnZombie();
                _zombiesRemainingToSpawn--;
                float difficultySpawnMultiplier = _difficulty switch
                {
                    GameDifficulty.Easy => 1.2f,
                    GameDifficulty.Hard => 0.8f,
                    _ => 1f
                };
                _spawnTimer = Math.Max(0.2f, (0.9f - _round * 0.035f) * difficultySpawnMultiplier);
            }
        }

        foreach (Zombie zombie in _zombies)
        {
            zombie.Update(delta, _player.Position, _map);
            if (_damageCooldown <= 0 && Vector2.DistanceSquared(zombie.Position, _player.Position) < 620 &&
                !_map.BlocksLine(zombie.Position, _player.Position))
            {
                _player.TakeDamage(zombie.ContactDamage);
                PlaySound(_soundEffects.PlayHurt);
                _damageCooldown = 0.75f;
                _damageFlashTimer = 0.22f;
                _cameraShake = Math.Max(_cameraShake, 0.2f);
                AddBurst(_player.Position, new Color(196, 48, 42), 8, 95f, 2f, 55f, 0.32f);
                if (_player.Health <= 0)
                    _gameOver = true;
            }
        }
        foreach (DefenseWindow defense in _defenses)
            defense.Update(delta, _zombies);

        foreach (Bullet bullet in _bullets)
            bullet.Update(delta, _map);

        foreach (Bullet bullet in _bullets)
        {
            if (bullet.IsExplosive)
            {
                bool hitZombie = _zombies.Any(zombie => !zombie.IsDead &&
                    Vector2.DistanceSquared(bullet.Position, zombie.Position) <= 22 * 22);
                if (!hitZombie && !bullet.IsExpired)
                    continue;

                bullet.Expire();
                ApplyBlast(bullet.Position, bullet.ExplosionRadius, bullet.Damage, new Color(255, 164, 64));
                _cameraShake = Math.Max(_cameraShake, 0.12f);
                continue;
            }

            if (bullet.IsExpired)
                continue;

            foreach (Zombie zombie in _zombies)
            {
                if (zombie.IsDead || !bullet.CanHit(zombie) ||
                    Vector2.DistanceSquared(bullet.Position, zombie.Position) > 400)
                    continue;

                if (bullet.IsFlame)
                {
                    bullet.Expire();
                    ApplyBlast(bullet.Position, bullet.ExplosionRadius, bullet.Damage, new Color(255, 112, 38));
                }
                else
                {
                    DamageZombie(zombie, bullet.Damage);
                    bullet.Hit(zombie);
                }
            }
        }

        _zombies.RemoveAll(zombie => zombie.IsDead);
        _bullets.RemoveAll(bullet => bullet.IsExpired);
        if (!_inIntermission && _zombiesRemainingToSpawn == 0 && _zombies.Count == 0)
        {
            _inIntermission = true;
            _intermissionTimer = 5f;
        }

        _previousKeyboard = keyboard;
        _previousMouse = mouse;
        base.Update(gameTime);
    }

    private bool Pressed(KeyboardState current, Keys key) =>
        current.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);

    private void UpdateMenuInput(MouseState mouse)
    {
        if (mouse.LeftButton != ButtonState.Pressed ||
            _previousMouse.LeftButton == ButtonState.Pressed)
            return;

        PlaySound(_soundEffects.PlayPurchase);
        Point click = GetVirtualMousePosition(mouse);
        if (_showSettings)
        {
            if (SoundEffectsButton.Contains(click))
                _soundEffectsEnabled = !_soundEffectsEnabled;
            else if (ScreenShakeButton.Contains(click))
                _screenShakeEnabled = !_screenShakeEnabled;
            else if (FullscreenButton.Contains(click))
                SetFullscreen(!_graphics.IsFullScreen);
            else if (BackButton.Contains(click))
            {
                _showSettings = false;
                _isPaused = _settingsFromPause;
                _settingsFromPause = false;
            }
            return;
        }

        if (StartButton.Contains(click))
        {
            _hasStarted = true;
            Restart();
            return;
        }

        for (int i = 0; i < DifficultyButtons.Length; i++)
        {
            if (DifficultyButtons[i].Contains(click))
            {
                _difficulty = (GameDifficulty)i;
                return;
            }
        }

        if (SettingsButton.Contains(click))
            _showSettings = true;
        else if (ExitButton.Contains(click))
            ExitGame();
    }

    private void UpdatePauseMenuInput(MouseState mouse)
    {
        if (mouse.LeftButton != ButtonState.Pressed ||
            _previousMouse.LeftButton == ButtonState.Pressed)
            return;

        PlaySound(_soundEffects.PlayPurchase);
        Point click = GetVirtualMousePosition(mouse);
        if (_showSettings)
        {
            if (SoundEffectsButton.Contains(click))
                _soundEffectsEnabled = !_soundEffectsEnabled;
            else if (ScreenShakeButton.Contains(click))
                _screenShakeEnabled = !_screenShakeEnabled;
            else if (FullscreenButton.Contains(click))
                SetFullscreen(!_graphics.IsFullScreen);
            else if (BackButton.Contains(click))
            {
                _showSettings = false;
                _isPaused = _settingsFromPause;
                _settingsFromPause = false;
            }
            return;
        }

        if (ResumeButton.Contains(click))
            _isPaused = false;
        else if (PauseSettingsButton.Contains(click))
        {
            _settingsFromPause = true;
            _showSettings = true;
            _isPaused = false;
        }
        else if (MainMenuButton.Contains(click))
        {
            _hasStarted = false;
            _isPaused = false;
            _gameOver = false;
        }
        else if (PauseExitButton.Contains(click))
            ExitGame();
    }

    private void SetFullscreen(bool enabled)
    {
        if (_graphics.IsFullScreen == enabled)
            return;

        DisplayMode displayMode = GraphicsDevice.Adapter.CurrentDisplayMode;
        _graphics.IsFullScreen = enabled;
        _graphics.PreferredBackBufferWidth = enabled ? displayMode.Width : Width;
        _graphics.PreferredBackBufferHeight = enabled ? displayMode.Height : Height;
        _graphics.ApplyChanges();
    }

    private void ExitGame()
    {
        if (_graphics.IsFullScreen)
            SetFullscreen(false);
        Exit();
    }

    private void PlaySound(Action play)
    {
        if (_soundEffectsEnabled)
            play();
    }

    private Point GetVirtualMousePosition(MouseState mouse)
    {
        Rectangle destination = GetSceneDestinationRectangle(Vector2.Zero);
        return new Point(
            (int)((mouse.X - destination.X) * (Width / (float)destination.Width)),
            (int)((mouse.Y - destination.Y) * (Height / (float)destination.Height)));
    }

    private Rectangle GetSceneDestinationRectangle(Vector2 offset)
    {
        int backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float scale = Math.Min(backBufferWidth / (float)Width, backBufferHeight / (float)Height);
        int destinationWidth = (int)(Width * scale);
        int destinationHeight = (int)(Height * scale);
        return new Rectangle(
            (backBufferWidth - destinationWidth) / 2 + (int)offset.X,
            (backBufferHeight - destinationHeight) / 2 + (int)offset.Y,
            destinationWidth,
            destinationHeight);
    }

    private static Vector2 Rotate(Vector2 direction, float angle)
    {
        float cosine = MathF.Cos(angle);
        float sine = MathF.Sin(angle);
        return new Vector2(direction.X * cosine - direction.Y * sine,
            direction.X * sine + direction.Y * cosine);
    }

    private void ApplyBlast(Vector2 position, float radius, int damage, Color effectColor)
    {
        AddBurst(position, effectColor, 12, 150f, 4f, 50f, 0.45f);
        float radiusSquared = radius * radius;
        foreach (Zombie zombie in _zombies)
        {
            if (!zombie.IsDead && Vector2.DistanceSquared(position, zombie.Position) <= radiusSquared)
                DamageZombie(zombie, damage);
        }
    }

    private void DamageZombie(Zombie zombie, int damage)
    {
        bool instaKill = _activePowerUps.ContainsKey(PowerUpType.InstaKill);
        zombie.TakeDamage(instaKill ? zombie.Health : damage);
        AwardPoints(10);
        AddBurst(zombie.Position, new Color(166, 42, 38), 6, 105f, 2f, 30f, 0.3f);
        if (!zombie.IsDead)
        {
            PlaySound(_soundEffects.PlayHit);
            return;
        }

        PlaySound(_soundEffects.PlayKill);
        AwardPoints(50);
        _score++;
        AddBurst(zombie.Position, new Color(119, 32, 34), 14, 135f, 3f, 65f, 0.52f);
        PowerUp? drop = PowerUp.TrySpawn(_random, zombie.Position);
        if (drop is not null)
            _powerUps.Add(drop);
    }

    private void UpdatePowerUpDrops(float delta)
    {
        for (int i = _powerUps.Count - 1; i >= 0; i--)
        {
            PowerUp powerUp = _powerUps[i];
            powerUp.Update(delta);
            if (powerUp.IsExpired)
            {
                _powerUps.RemoveAt(i);
                continue;
            }

            if (!powerUp.CanCollect(_player.Position))
                continue;

            _powerUps.RemoveAt(i);
            PlaySound(_soundEffects.PlayPickup);
            ActivatePowerUp(powerUp.Type);
        }
    }

    private void UpdatePowerUpEffects(float delta)
    {
        foreach (PowerUpType type in _activePowerUps.Keys.ToArray())
        {
            float remaining = _activePowerUps[type] - delta;
            if (remaining <= 0)
                EndPowerUp(type);
            else
                _activePowerUps[type] = remaining;
        }
    }

    private void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.MaxAmmo:
                _player.RefillAmmo();
                break;
            case PowerUpType.DoublePoints:
            case PowerUpType.InstaKill:
            case PowerUpType.FireSale:
                _activePowerUps[type] = PowerUp.Duration(type);
                break;
            case PowerUpType.Nuke:
                foreach (Zombie zombie in _zombies)
                {
                    if (zombie.IsDead)
                        continue;

                    _score++;
                    AwardPoints(50);
                    AddBurst(zombie.Position, new Color(119, 32, 34), 14, 135f, 3f, 65f, 0.52f);
                }
                _zombies.Clear();
                AwardPoints(400);
                _cameraShake = Math.Max(_cameraShake, 0.3f);
                break;
            case PowerUpType.Carpenter:
                foreach (DefenseWindow defense in _defenses)
                    defense.Repair();
                AwardPoints(50);
                break;
            case PowerUpType.DeathMachine:
                _player.ActivateDeathMachine();
                _activePowerUps[type] = PowerUp.Duration(type);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    private void EndPowerUp(PowerUpType type)
    {
        _activePowerUps.Remove(type);
        if (type == PowerUpType.DeathMachine)
            _player.DeactivateDeathMachine();
    }

    private void AwardPoints(int amount)
    {
        int multiplier = _activePowerUps.ContainsKey(PowerUpType.DoublePoints) ? 2 : 1;
        _points += amount * multiplier;
    }

    private void Interact()
    {
        if (Vector2.DistanceSquared(_player.Position, _upgradeStationPosition) < 65 * 65)
        {
            int level = _player.GetWeaponUpgradeLevel();
            if (level < WeaponUpgradeCosts.Length && _points >= WeaponUpgradeCosts[level] &&
                _player.UpgradeCurrentWeapon())
            {
                _points -= WeaponUpgradeCosts[level];
                PlaySound(_soundEffects.PlayPurchase);
            }
            return;
        }

        bool fireSale = _activePowerUps.ContainsKey(PowerUpType.FireSale);
        for (int i = 0; i < _mysteryBoxPositions.Length; i++)
        {
            if ((!fireSale && i != 0) ||
                Vector2.DistanceSquared(_player.Position, _mysteryBoxPositions[i]) >= 65 * 65)
                continue;

            int cost = fireSale ? FireSaleCost : MysteryBoxCost;
            if (_points < cost)
                return;

            _points -= cost;
            _player.GrantWeapon(RollBoxWeapon());
            PlaySound(_soundEffects.PlayPurchase);
            return;
        }
    }

    private WeaponType RollBoxWeapon()
    {
        if (_random.NextDouble() < 0.08)
            return _random.Next(2) == 0 ? WeaponType.GrenadeLauncher : WeaponType.Flamethrower;

        WeaponType[] commonWeapons =
        [
            WeaponType.Pistol,
            WeaponType.HeavyPistol,
            WeaponType.Rifle,
            WeaponType.Shotgun,
            WeaponType.SubmachineGun,
            WeaponType.SubmachineGun,
            WeaponType.SniperRifle
        ];
        return commonWeapons[_random.Next(commonWeapons.Length)];
    }

    private void BeginRound()
    {
        PlaySound(_soundEffects.PlayWave);
        _inIntermission = false;
        int baseZombies = _difficulty switch
        {
            GameDifficulty.Easy => 4,
            GameDifficulty.Hard => 8,
            _ => 6
        };
        int additionalPerRound = _difficulty switch
        {
            GameDifficulty.Easy => 4,
            GameDifficulty.Hard => 6,
            _ => 5
        };
        _zombiesRemainingToSpawn = baseZombies + (_round - 1) * additionalPerRound;
        _spawnTimer = 0.5f;
    }

    private void SpawnZombie()
    {
        int breachIndex = _random.Next(_map.BreachPoints.Length);
        Vector2 breach = _map.BreachPoints[breachIndex];
        Vector2 position = breachIndex switch
        {
            0 or 1 => new Vector2(breach.X, Math.Clamp(
                breach.Y + _random.Next(-40, 41), 56, 692)),
            _ => new Vector2(Math.Clamp(
                breach.X + _random.Next(-40, 41), 30, 1250), breach.Y)
        };
        _zombies.Add(new Zombie(position, _round, _difficulty));
    }

    private void Restart()
    {
        _map.Randomize(_random);
        ResetMapObjects();
        _player.Reset();
        _zombies.Clear();
        _bullets.Clear();
        _particles.Clear();
        _powerUps.Clear();
        _activePowerUps.Clear();
        _muzzleFlashTimer = 0;
        _gunRecoil = 0;
        _cameraShake = 0;
        _damageFlashTimer = 0;
        _round = 1;
        _score = 0;
        _points = 500;
        _gameOver = false;
        BeginRound();
    }

    private void ResetMapObjects()
    {
        _defenses.Clear();
        foreach (Vector2 breach in _map.BreachPoints)
            _defenses.Add(new DefenseWindow(breach));

        _mysteryBoxPositions = GenerateMysteryBoxPositions();
        _upgradeStationPosition = GenerateUpgradeStationPosition();
    }

    private Vector2[] GenerateMysteryBoxPositions()
    {
        var positions = new List<Vector2>(3);
        for (int i = 0; i < 3; i++)
        {
            bool placed = false;
            for (int attempt = 0; attempt < 500; attempt++)
            {
                var candidate = new Vector2(_random.Next(90, 1191), _random.Next(100, 651));
                if (_map.IsBlocked(candidate, 35) ||
                    Vector2.DistanceSquared(candidate, new Vector2(640, 380)) < 180 * 180 ||
                    positions.Any(position =>
                        Vector2.DistanceSquared(candidate, position) < 190 * 190))
                    continue;

                positions.Add(candidate);
                placed = true;
                break;
            }

            if (!placed)
                throw new InvalidOperationException("Could not place all randomized Mystery Boxes.");
        }

        return positions.ToArray();
    }

    private Vector2 GenerateUpgradeStationPosition()
    {
        for (int attempt = 0; attempt < 500; attempt++)
        {
            var candidate = new Vector2(_random.Next(90, 1191), _random.Next(100, 651));
            if (_map.IsBlocked(candidate, 35) ||
                Vector2.DistanceSquared(candidate, new Vector2(640, 380)) < 180 * 180 ||
                _mysteryBoxPositions.Any(position =>
                    Vector2.DistanceSquared(candidate, position) < 200 * 200))
                continue;

            return candidate;
        }

        throw new InvalidOperationException("Could not place the randomized weapon upgrade station.");
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_scene);
        GraphicsDevice.Clear(_hasStarted ? new Color(35, 48, 42) : new Color(14, 23, 24));
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        if (_hasStarted)
        {
            DrawWorld();
            DrawDamageOverlay();
            DrawHud();
            if (_gameOver)
                DrawGameOver();
            if (_showSettings)
                DrawMenu();
            else if (_isPaused)
                DrawPauseMenu();
        }
        else
        {
            DrawMenu();
        }
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        Vector2 shake = _hasStarted && _screenShakeEnabled && _cameraShake > 0
            ? new Vector2((float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1)) * _cameraShake * 12f
            : Vector2.Zero;
        _spriteBatch.Draw(_scene, GetSceneDestinationRectangle(shake), Color.White);
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawMenu()
    {
        for (int y = 0; y < Height; y += 48)
        for (int x = 0; x < Width; x += 48)
        {
            bool alternate = ((x / 48) + (y / 48)) % 2 == 0;
            Fill(new Rectangle(x, y, 48, 48),
                alternate ? new Color(18, 31, 31) : new Color(16, 27, 29));
        }

        Fill(new Rectangle(0, 0, Width, 9), new Color(88, 172, 142));
        Fill(new Rectangle(0, Height - 9, Width, 9), new Color(88, 172, 142));
        Fill(new Rectangle(0, 0, 9, Height), new Color(88, 172, 142));
        Fill(new Rectangle(Width - 9, 0, 9, Height), new Color(88, 172, 142));
        Fill(new Rectangle(360, 90, 560, 560), new Color(8, 15, 17, 220));

        if (_showSettings)
        {
            DrawCenteredText("SETTINGS", 170, 4, new Color(238, 210, 135));
            DrawMenuButton(SoundEffectsButton, $"SOUND EFFECTS {(_soundEffectsEnabled ? "ON" : "OFF")}");
            DrawMenuButton(ScreenShakeButton, $"SCREEN SHAKE {(_screenShakeEnabled ? "ON" : "OFF")}");
            DrawMenuButton(FullscreenButton, $"FULLSCREEN {(_graphics.IsFullScreen ? "ON" : "OFF")}");
            DrawMenuButton(BackButton, "BACK");
            DrawCenteredText("ESC TO RETURN", 590, 1, new Color(155, 175, 166));
            return;
        }

        DrawCenteredText("LAST STAND", 130, 5, new Color(238, 210, 135));
        DrawCenteredText("PIXEL ZOMBIE SURVIVAL", 205, 2, new Color(170, 197, 178));
        DrawMenuButton(StartButton, "START GAME");
        DrawCenteredText("SELECT DIFFICULTY", 345, 2, new Color(198, 213, 194));
        string[] difficultyLabels = ["EASY", "NORMAL", "HARD"];
        for (int i = 0; i < DifficultyButtons.Length; i++)
            DrawMenuButton(DifficultyButtons[i], difficultyLabels[i], i == (int)_difficulty);
        DrawMenuButton(SettingsButton, "SETTINGS");
        DrawMenuButton(ExitButton, "EXIT");
    }

    private void DrawPauseMenu()
    {
        Fill(new Rectangle(0, 0, Width, Height), new Color(0, 0, 0, 185));
        Fill(new Rectangle(360, 165, 560, 400), new Color(8, 15, 17, 245));
        DrawCenteredText("PAUSED", 190, 4, new Color(238, 210, 135));
        DrawMenuButton(ResumeButton, "RESUME");
        DrawMenuButton(PauseSettingsButton, "SETTINGS");
        DrawMenuButton(MainMenuButton, "MAIN MENU");
        DrawMenuButton(PauseExitButton, "EXIT");
    }

    private void DrawMenuButton(Rectangle bounds, string label, bool selected = false)
    {
        Point mouse = GetVirtualMousePosition(Mouse.GetState());
        bool hovered = bounds.Contains(mouse);
        Color border = selected || hovered ? new Color(238, 190, 92) : new Color(86, 116, 105);
        Fill(bounds, border);
        Fill(new Rectangle(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 6),
            hovered || selected ? new Color(47, 67, 59) : new Color(27, 42, 40));
        int textWidth = label.Length * 12;
        PixelFont.Draw(_spriteBatch, _pixel, label,
            new Vector2(bounds.Center.X - textWidth / 2f, bounds.Center.Y - 7),
            hovered || selected ? new Color(255, 230, 172) : Color.White, 2);
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        int textWidth = text.Length * 6 * scale;
        PixelFont.Draw(_spriteBatch, _pixel, text, new Vector2((Width - textWidth) / 2f, y), color, scale);
    }

    private void DrawWorld()
    {
        for (int y = 36; y < Height; y += 48)
        for (int x = 0; x < Width; x += 48)
        {
            bool alternate = ((x / 48) + (y / 48)) % 2 == 0;
            Fill(new Rectangle(x, y, 48, 48), alternate ? new Color(48, 63, 53) : new Color(44, 58, 49));
            Fill(new Rectangle(x + 7, y + 9, 2, 2), new Color(68, 78, 59));
            Fill(new Rectangle(x + 34, y + 31, 3, 2), new Color(63, 73, 56));
        }

        Fill(new Rectangle(0, 0, Width, 36), new Color(26, 31, 31));
        Fill(new Rectangle(0, Height - 8, Width, 8), new Color(26, 31, 31));
        Fill(new Rectangle(0, 36, 8, Height - 44), new Color(26, 31, 31));
        Fill(new Rectangle(Width - 8, 36, 8, Height - 44), new Color(26, 31, 31));

        _map.Draw(Fill);
        DrawDefenseWindows();
        DrawMysteryBoxes();
        DrawUpgradeStation();

        foreach (PowerUp powerUp in _powerUps)
            DrawPowerUp(powerUp);

        foreach (Zombie zombie in _zombies)
        {
            float bob = MathF.Sin(zombie.AnimationTime * 9f) * 1.5f;
            Vector2 zombiePosition = zombie.Position + new Vector2(0, bob);
            Fill(new Rectangle((int)zombie.Position.X - 15, (int)zombie.Position.Y + 10, 30, 7),
                new Color(20, 25, 20, 105));

            Color tint = zombie.HitFlash > 0 ? new Color(255, 190, 170) : Color.White;
            _spriteBatch.Draw(_zombieSprite, zombiePosition, null, tint, 0,
                new Vector2(_zombieSprite.Width / 2f, _zombieSprite.Height / 2f), 2f, SpriteEffects.None, 0);

            if (zombie.Health < zombie.MaxHealth || zombie.HitFlash > 0)
            {
                Fill(new Rectangle((int)zombie.Position.X - 13, (int)zombie.Position.Y - 19, 26, 3),
                    new Color(35, 29, 27));
                Fill(new Rectangle((int)zombie.Position.X - 13, (int)zombie.Position.Y - 19,
                    Math.Max(1, 26 * zombie.Health / zombie.MaxHealth), 3), new Color(190, 57, 48));
            }
        }

        foreach (Particle particle in _particles)
            particle.Draw(_spriteBatch, _pixel);
        foreach (Bullet bullet in _bullets)
        {
            Color bulletColor = bullet.IsExplosive
                ? new Color(255, 142, 57)
                : bullet.IsFlame ? new Color(255, 92, 32) : Color.White;
            _spriteBatch.Draw(_bulletSprite, bullet.Position, null, bulletColor, 0,
                new Vector2(1, 1), 2f, SpriteEffects.None, 0);
        }

        Point virtualMouse = GetVirtualMousePosition(Mouse.GetState());
        Vector2 mouse = new(virtualMouse.X, virtualMouse.Y);
        float rotation = MathF.Atan2(mouse.Y - _player.Position.Y, mouse.X - _player.Position.X);
        _spriteBatch.Draw(_playerSprite, _player.Position, null, Color.White, 0,
            new Vector2(_playerSprite.Width / 2f, _playerSprite.Height / 2f), 2f, SpriteEffects.None, 0);

        Texture2D weaponSprite = _player.CurrentWeapon.Type switch
        {
            WeaponType.Rifle => _rifleSprite,
            WeaponType.Shotgun => _shotgunSprite,
            WeaponType.SubmachineGun => _smgSprite,
            WeaponType.SniperRifle => _sniperSprite,
            WeaponType.GrenadeLauncher => _grenadeLauncherSprite,
            WeaponType.Flamethrower => _flamethrowerSprite,
            WeaponType.DeathMachine => _deathMachineSprite,
            _ => _pistolSprite
        };
        Vector2 weaponOrigin = new(Math.Min(weaponSprite.Width * 0.25f, 8), weaponSprite.Height / 2f);
        Vector2 aimDirection = new(MathF.Cos(rotation), MathF.Sin(rotation));
        Vector2 weaponPosition = _player.Position + aimDirection * (8f - _gunRecoil * 3f);
        _spriteBatch.Draw(weaponSprite, weaponPosition, null, Color.White, rotation, weaponOrigin,
            2f, SpriteEffects.None, 0);

        if (_muzzleFlashTimer > 0)
        {
            float flashScale = _muzzleFlashTimer / 0.075f;
            DrawFlash(_lastShotPosition, _lastShotDirection, 15f * flashScale, 7f,
                new Color(255, 241, 150));
            DrawFlash(_lastShotPosition, _lastShotDirection, 10f * flashScale, 3f,
                new Color(255, 145, 54));
        }

        DrawInteractionPrompt();
    }

    private void DrawFlash(Vector2 position, Vector2 direction, float length, float thickness, Color color)
    {
        float rotation = MathF.Atan2(direction.Y, direction.X);
        _spriteBatch.Draw(_pixel, position, null, color, rotation, Vector2.Zero,
            new Vector2(length, thickness), SpriteEffects.None, 0);
    }

    private void AddBurst(Vector2 position, Color color, int count, float speed, float size,
        float gravity, float lifetime)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_random.NextDouble() * MathHelper.TwoPi);
            float particleSpeed = speed * (0.35f + (float)_random.NextDouble() * 0.65f);
            Vector2 velocity = new(MathF.Cos(angle) * particleSpeed, MathF.Sin(angle) * particleSpeed);
            float particleLifetime = lifetime * (0.65f + (float)_random.NextDouble() * 0.7f);
            _particles.Add(new Particle(position, velocity, color, particleLifetime, size, gravity));
        }
    }

    private void DrawDefenseWindows()
    {
        foreach (DefenseWindow defense in _defenses)
        {
            int x = (int)defense.Position.X - 20;
            int y = (int)defense.Position.Y - 14;
            Fill(new Rectangle(x, y, 40, 28), new Color(24, 29, 27));
            Fill(new Rectangle(x + 3, y + 3, 34, 20), new Color(75, 94, 89));
            for (int plank = 0; plank < 3; plank++)
            {
                if (defense.Health <= plank * DefenseWindow.MaxHealth / 3)
                    continue;

                int plankY = y + 4 + plank * 6;
                Fill(new Rectangle(x + 4, plankY, 32, 4), new Color(139, 101, 61));
                Fill(new Rectangle(x + 8, plankY + 1, 24, 1), new Color(169, 127, 76));
            }
            Fill(new Rectangle(x + 5, y + 24, 30, 3), new Color(38, 31, 28));
            Fill(new Rectangle(x + 5, y + 24,
                30 * defense.Health / DefenseWindow.MaxHealth, 3), new Color(199, 70, 53));
        }
    }

    private void DrawMysteryBoxes()
    {
        bool fireSale = _activePowerUps.ContainsKey(PowerUpType.FireSale);
        for (int i = 0; i < _mysteryBoxPositions.Length; i++)
        {
            if (!fireSale && i != 0)
                continue;

            Vector2 position = _mysteryBoxPositions[i];
            Color trim = fireSale ? PowerUp.Color(PowerUpType.FireSale) : new Color(181, 139, 69);
            Fill(new Rectangle((int)position.X - 23, (int)position.Y - 18, 46, 36), new Color(29, 30, 27));
            Fill(new Rectangle((int)position.X - 20, (int)position.Y - 15, 40, 30), trim);
            Fill(new Rectangle((int)position.X - 16, (int)position.Y - 11, 32, 22), new Color(61, 58, 44));
            PixelFont.Draw(_spriteBatch, _pixel, "BOX", new Vector2(position.X - 9, position.Y - 3), Color.White, 2);
        }
    }

    private void DrawUpgradeStation()
    {
        Vector2 position = _upgradeStationPosition;
        Color trim = new(80, 183, 199);
        Fill(new Rectangle((int)position.X - 23, (int)position.Y - 19, 46, 38), new Color(22, 29, 30));
        Fill(new Rectangle((int)position.X - 19, (int)position.Y - 15, 38, 30), trim);
        Fill(new Rectangle((int)position.X - 14, (int)position.Y - 10, 28, 20), new Color(36, 54, 57));
        Fill(new Rectangle((int)position.X - 3, (int)position.Y - 7, 6, 14), Color.White);
        Fill(new Rectangle((int)position.X - 7, (int)position.Y - 3, 14, 6), Color.White);
        PixelFont.Draw(_spriteBatch, _pixel, "UP", new Vector2(position.X - 6, position.Y + 22),
            new Color(207, 245, 248), 1);
    }

    private void DrawPowerUp(PowerUp powerUp)
    {
        Vector2 position = powerUp.Position + new Vector2(0, powerUp.BobOffset);
        Color color = PowerUp.Color(powerUp.Type);
        Fill(new Rectangle((int)position.X - 22, (int)position.Y - 23, 44, 44), new Color(color, 55));
        Fill(new Rectangle((int)position.X - 17, (int)position.Y - 18, 34, 34), new Color(23, 27, 25));
        Fill(new Rectangle((int)position.X - 14, (int)position.Y - 15, 28, 28), color);

        switch (powerUp.Type)
        {
            case PowerUpType.MaxAmmo:
                Fill(new Rectangle((int)position.X - 8, (int)position.Y - 8, 16, 16), Color.White);
                Fill(new Rectangle((int)position.X - 5, (int)position.Y - 5, 3, 10), color);
                Fill(new Rectangle((int)position.X + 2, (int)position.Y - 5, 3, 10), color);
                break;
            case PowerUpType.DoublePoints:
                PixelFont.Draw(_spriteBatch, _pixel, "2X", position + new Vector2(-9, -4), Color.White, 2);
                break;
            case PowerUpType.InstaKill:
                Fill(new Rectangle((int)position.X - 3, (int)position.Y - 9, 6, 18), Color.White);
                Fill(new Rectangle((int)position.X - 9, (int)position.Y - 3, 18, 6), Color.White);
                break;
            case PowerUpType.Nuke:
                Fill(new Rectangle((int)position.X - 3, (int)position.Y - 10, 6, 11), Color.White);
                Fill(new Rectangle((int)position.X - 8, (int)position.Y - 6, 16, 5), Color.White);
                Fill(new Rectangle((int)position.X - 5, (int)position.Y + 1, 10, 6), Color.White);
                break;
            case PowerUpType.Carpenter:
                Fill(new Rectangle((int)position.X - 9, (int)position.Y - 3, 18, 6), Color.White);
                Fill(new Rectangle((int)position.X - 3, (int)position.Y - 9, 6, 18), Color.White);
                break;
            case PowerUpType.FireSale:
                Fill(new Rectangle((int)position.X - 8, (int)position.Y - 7, 16, 14), Color.White);
                Fill(new Rectangle((int)position.X + 2, (int)position.Y - 4, 2, 2), color);
                break;
            case PowerUpType.DeathMachine:
                for (int bullet = -1; bullet <= 1; bullet++)
                    Fill(new Rectangle((int)position.X + bullet * 6 - 2, (int)position.Y - 8, 4, 16), Color.White);
                break;
        }

        string label = PowerUp.Label(powerUp.Type);
        int labelWidth = label.Length * 6;
        PixelFont.Draw(_spriteBatch, _pixel, label,
            new Vector2(position.X - labelWidth / 2f, position.Y + 20), Color.White, 1);
    }

    private void DrawInteractionPrompt()
    {
        if (Vector2.DistanceSquared(_player.Position, _upgradeStationPosition) < 100 * 100)
        {
            int level = _player.GetWeaponUpgradeLevel();
            string prompt = level >= WeaponUpgradeCosts.Length
                ? $"{_player.WeaponName} MAX UPGRADE"
                : $"E UPGRADE {_player.WeaponName} {WeaponUpgradeCosts[level]}";
            PixelFont.Draw(_spriteBatch, _pixel, prompt,
                new Vector2(_upgradeStationPosition.X - prompt.Length * 3, _upgradeStationPosition.Y + 34),
                new Color(207, 245, 248), 1);
            return;
        }

        bool fireSale = _activePowerUps.ContainsKey(PowerUpType.FireSale);
        for (int i = 0; i < _mysteryBoxPositions.Length; i++)
        {
            if ((!fireSale && i != 0) ||
                Vector2.DistanceSquared(_player.Position, _mysteryBoxPositions[i]) >= 100 * 100)
                continue;

            int cost = fireSale ? FireSaleCost : MysteryBoxCost;
            PixelFont.Draw(_spriteBatch, _pixel, $"E MYSTERY BOX {cost}",
                new Vector2(_mysteryBoxPositions[i].X - 56, _mysteryBoxPositions[i].Y + 28),
                new Color(255, 232, 173), 1);
            return;
        }

    }

    private void DrawHud()
    {
        Fill(new Rectangle(0, 0, Width, 36), new Color(17, 21, 22, 245));
        PixelFont.Draw(_spriteBatch, _pixel, $"ROUND {_round}", new Vector2(18, 11), new Color(244, 202, 91), 2);
        PixelFont.Draw(_spriteBatch, _pixel, $"POINTS {_points}", new Vector2(210, 11), Color.White, 2);
        PixelFont.Draw(_spriteBatch, _pixel, $"KILLS {_score}", new Vector2(450, 11), Color.White, 2);
        PixelFont.Draw(_spriteBatch, _pixel, $"{_player.WeaponName} {_player.Magazine}/{_player.ReserveAmmo}",
            new Vector2(660, 11), Color.White, 2);

        PixelFont.Draw(_spriteBatch, _pixel, "HP", new Vector2(970, 11), Color.White, 2);
        Fill(new Rectangle(1007, 11, 150, 14), new Color(65, 38, 38));
        Fill(new Rectangle(1007, 11, (int)(150f * _player.Health / Player.MaxHealth), 14),
            _player.Health > 30 ? new Color(77, 188, 96) : new Color(214, 64, 54));
        PixelFont.Draw(_spriteBatch, _pixel, $"{_player.Health}", new Vector2(1170, 11), Color.White, 2);

        PixelFont.Draw(_spriteBatch, _pixel, "WASD MOVE   MOUSE AIM/FIRE   R RELOAD   Q SWAP   E INTERACT   ESC QUIT",
            new Vector2(18, 685), new Color(219, 224, 206), 1);

        if (_player.IsReloading)
            PixelFont.Draw(_spriteBatch, _pixel, "RELOADING", new Vector2(18, 55), new Color(248, 211, 109), 2);
        PowerUpType[] effectOrder =
        [
            PowerUpType.DoublePoints,
            PowerUpType.InstaKill,
            PowerUpType.FireSale,
            PowerUpType.DeathMachine
        ];
        int effectLine = 0;
        foreach (PowerUpType type in effectOrder)
        {
            if (!_activePowerUps.TryGetValue(type, out float remaining))
                continue;

            string label = $"{PowerUp.Label(type)} {(int)MathF.Ceiling(remaining)}S";
            PixelFont.Draw(_spriteBatch, _pixel, label, new Vector2(990, 53 + effectLine * 11),
                PowerUp.Color(type), 1);
            effectLine++;
        }
        if (_inIntermission && !_gameOver)
            PixelFont.Draw(_spriteBatch, _pixel, $"ROUND {_round} CLEAR - NEXT WAVE IN {(int)MathF.Ceiling(_intermissionTimer)}",
                new Vector2(390, 58), new Color(248, 211, 109), 2);
    }

    private void DrawGameOver()
    {
        Fill(new Rectangle(0, 0, Width, Height), new Color(0, 0, 0, 190));
        PixelFont.Draw(_spriteBatch, _pixel, "GAME OVER", new Vector2(500, 270), new Color(224, 71, 59), 5);
        PixelFont.Draw(_spriteBatch, _pixel, $"ROUND {_round}   KILLS {_score}", new Vector2(500, 330), Color.White, 3);
        PixelFont.Draw(_spriteBatch, _pixel, "PRESS ENTER OR R TO RESTART", new Vector2(440, 390), new Color(245, 204, 94), 2);
    }

    private void DrawDamageOverlay()
    {
        if (_damageFlashTimer <= 0)
            return;

        int alpha = (int)(38 * _damageFlashTimer / 0.22f);
        Fill(new Rectangle(0, 36, Width, Height - 44), new Color(128, 12, 12, alpha));
        Fill(new Rectangle(0, 36, 12, Height - 44), new Color(190, 30, 24, alpha * 2));
        Fill(new Rectangle(Width - 12, 36, 12, Height - 44), new Color(190, 30, 24, alpha * 2));
    }

    private sealed class Particle(Vector2 position, Vector2 velocity, Color color,
        float lifetime, float size, float gravity)
    {
        private readonly float _lifetime = lifetime;
        public Vector2 Position { get; private set; } = position;
        public Vector2 Velocity { get; private set; } = velocity;
        public bool IsExpired { get; private set; }

        public void Update(float delta)
        {
            Position += Velocity * delta;
            Velocity += new Vector2(0, gravity * delta);
            _lifetimeRemaining -= delta;
            IsExpired = _lifetimeRemaining <= 0;
        }

        private float _lifetimeRemaining = lifetime;

        public void Draw(SpriteBatch batch, Texture2D pixel)
        {
            float opacity = MathHelper.Clamp(_lifetimeRemaining / _lifetime, 0, 1);
            int particleSize = Math.Max(1, (int)MathF.Ceiling(size * opacity));
            batch.Draw(pixel, new Rectangle((int)Position.X - particleSize / 2,
                (int)Position.Y - particleSize / 2, particleSize, particleSize), color * opacity);
        }
    }

    private void Fill(Rectangle rectangle, Color color) => _spriteBatch.Draw(_pixel, rectangle, color);

    protected override void Dispose(bool disposing)
    {
        if (disposing && _soundEffects is not null)
            _soundEffects.Dispose();
        base.Dispose(disposing);
    }
}
