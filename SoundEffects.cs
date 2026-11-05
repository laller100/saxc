using Microsoft.Xna.Framework.Audio;

namespace ZombieSurvival;

internal sealed class SoundEffects : IDisposable
{
    private const int SampleRate = 22050;
    private const int VariationCount = 4;
    private readonly Random _random = new();
    private readonly Dictionary<WeaponType, SoundEffect[]> _shots = new();
    private readonly SoundEffect[] _hits = CreateVariations(0.09f, (random, variation) =>
        Create(0.09f, CreateImpactSample(random, 250f + variation * 35f, 0.3f)));
    private readonly SoundEffect[] _kills = CreateVariations(0.2f, (random, variation) =>
        Create(0.2f, CreateKillSample(variation)));
    private readonly SoundEffect[] _hurts = CreateVariations(0.18f, (random, _) =>
        Create(0.18f, CreateGunSample(random, 105f, 0.18f, 0.18f, 0.25f, 0.22f, 0.4f)));
    private readonly SoundEffect[] _pickups = CreateVariations(0.32f, (_, variation) =>
        Create(0.32f, CreatePickupSample(variation)));
    private readonly SoundEffect[] _waves = CreateVariations(0.48f, (_, variation) =>
        Create(0.48f, CreateWaveSample(variation)));
    private readonly SoundEffect[] _purchases = CreateVariations(0.16f, (_, variation) =>
        Create(0.16f, CreatePurchaseSample(variation)));
    private readonly SoundEffect[] _reloads = CreateVariations(0.28f, (random, variation) =>
        Create(0.28f, CreateReloadSample(random, variation)));

    public SoundEffects()
    {
        foreach (WeaponType type in Enum.GetValues<WeaponType>())
        {
            _shots.Add(type, CreateVariations(ShotDuration(type), (random, variation) =>
                Create(ShotDuration(type), CreateShotSample(random, type, variation))));
        }
    }

    public void PlayShot(WeaponType type) => Play(_shots[type], 0.75f, 0.08f);
    public void PlayHit() => Play(_hits, 0.55f);
    public void PlayKill() => Play(_kills, 0.65f);
    public void PlayHurt() => Play(_hurts, 0.75f);
    public void PlayPickup() => Play(_pickups, 0.7f);
    public void PlayWave() => Play(_waves, 0.65f);
    public void PlayPurchase() => Play(_purchases, 0.6f);
    public void PlayReload() => Play(_reloads, 0.5f);

    public void Dispose()
    {
        foreach (SoundEffect[] sounds in _shots.Values)
            DisposeAll(sounds);
        DisposeAll(_hits);
        DisposeAll(_kills);
        DisposeAll(_hurts);
        DisposeAll(_pickups);
        DisposeAll(_waves);
        DisposeAll(_purchases);
        DisposeAll(_reloads);
    }

    private void Play(SoundEffect[] variations, float volume, float pitchVariation = 0)
    {
        int index = _random.Next(variations.Length);
        float pitch = (float)(_random.NextDouble() * 2 - 1) * pitchVariation;
        variations[index].Play(volume, pitch, 0);
    }

    private static void DisposeAll(IEnumerable<SoundEffect> sounds)
    {
        foreach (SoundEffect sound in sounds)
            sound.Dispose();
    }

    private static SoundEffect[] CreateVariations(float duration,
        Func<Random, int, SoundEffect> create) =>
        Enumerable.Range(0, VariationCount)
            .Select(variation => create(new Random(Guid.NewGuid().GetHashCode()), variation))
            .ToArray();

    private static float ShotDuration(WeaponType type) => type switch
    {
        WeaponType.Shotgun or WeaponType.GrenadeLauncher => 0.38f,
        WeaponType.SniperRifle or WeaponType.HeavyPistol or WeaponType.DeathMachine => 0.3f,
        WeaponType.Flamethrower => 0.22f,
        _ => 0.2f
    };

    private static Func<float, float> CreateShotSample(Random random, WeaponType type, int variation)
    {
        return type switch
        {
            WeaponType.Flamethrower => CreateGunSample(random, 85f, 0.04f, 0.42f, 0.12f, 0.55f, 0.3f),
            WeaponType.GrenadeLauncher => CreateGunSample(random, 72f, 0.58f, 0.48f, 0.12f, 0.32f, 0.55f),
            WeaponType.Shotgun => CreateGunSample(random, 92f, 0.72f, 0.52f, 0.1f, 0.28f, 0.6f),
            WeaponType.SniperRifle => CreateGunSample(random, 125f, 0.7f, 0.31f, 0.12f, 0.38f, 0.57f),
            WeaponType.HeavyPistol => CreateGunSample(random, 105f, 0.54f, 0.34f, 0.16f, 0.34f, 0.56f),
            WeaponType.DeathMachine => CreateGunSample(random, 95f, 0.42f, 0.28f, 0.1f, 0.34f, 0.5f),
            WeaponType.Rifle => CreateGunSample(random, 145f + variation * 3, 0.36f, 0.24f, 0.13f, 0.4f, 0.52f),
            WeaponType.SubmachineGun => CreateGunSample(random, 175f + variation * 4, 0.26f, 0.2f, 0.11f, 0.42f, 0.48f),
            _ => CreateGunSample(random, 135f + variation * 4, 0.32f, 0.28f, 0.15f, 0.37f, 0.5f)
        };
    }

    private static Func<float, float> CreateGunSample(Random random, float bodyFrequency,
        float bodyLevel, float crackLevel, float decay, float noiseLevel, float tailLevel)
    {
        const float duration = 0.5f;
        float previousNoise = 0;
        float previousTailNoise = 0;
        float[] noise = new float[(int)(SampleRate * duration)];
        float[] tailNoise = new float[noise.Length];
        for (int i = 0; i < noise.Length; i++)
        {
            noise[i] = (float)(random.NextDouble() * 2 - 1);
            tailNoise[i] = (float)(random.NextDouble() * 2 - 1);
        }

        return time =>
        {
            int index = Math.Clamp((int)(time * SampleRate), 0, noise.Length - 1);
            float crackEnvelope = MathF.Exp(-time * 85f);
            float bodyEnvelope = MathF.Exp(-time / decay);
            float filteredNoise = previousNoise += 0.22f * (noise[index] - previousNoise);
            float filteredTail = previousTailNoise += 0.08f * (tailNoise[index] - previousTailNoise);
            float body = MathF.Sin(MathF.Tau * bodyFrequency * time) * bodyLevel * bodyEnvelope;
            float harmonics = MathF.Sin(MathF.Tau * bodyFrequency * 1.83f * time) *
                bodyLevel * 0.22f * bodyEnvelope;
            float crack = noise[index] * crackLevel * crackEnvelope;
            float mechanicalTail = filteredNoise * noiseLevel * MathF.Exp(-time * 24f);
            float roomTail = filteredTail * tailLevel * MathF.Exp(-Math.Max(0, time - 0.025f) * 11f);
            return (crack + body + harmonics + mechanicalTail + roomTail) * 0.72f;
        };
    }

    private static Func<float, float> CreateImpactSample(Random random, float frequency, float level)
    {
        float[] noise = CreateNoise(random, 2205);
        return time =>
        {
            int index = Math.Clamp((int)(time * SampleRate), 0, noise.Length - 1);
            float envelope = MathF.Exp(-time * 34f);
            return (MathF.Sin(MathF.Tau * (frequency - time * 450f) * time) * 0.55f +
                noise[index] * 0.45f) * envelope * level;
        };
    }

    private static Func<float, float> CreateKillSample(int variation)
    {
        float firstFrequency = 560f + variation * 37f;
        float secondFrequency = 350f + variation * 24f;
        return time =>
        {
            float envelope = MathF.Exp(-time * 12f);
            float frequency = time < 0.07f ? firstFrequency : secondFrequency;
            return MathF.Sin(MathF.Tau * frequency * time) * envelope * 0.3f;
        };
    }

    private static Func<float, float> CreatePickupSample(int variation)
    {
        float first = 480f + variation * 31f;
        float second = 700f + variation * 43f;
        return time =>
        {
            float envelope = MathF.Exp(-time * 7f);
            float frequency = time < 0.13f ? first : second;
            return MathF.Sin(MathF.Tau * frequency * time) * envelope * 0.27f;
        };
    }

    private static Func<float, float> CreateWaveSample(int variation)
    {
        float baseFrequency = 250f + variation * 22f;
        return time =>
        {
            float envelope = MathF.Exp(-time * 3f);
            float frequency = baseFrequency + time * (560f + variation * 25f);
            float chord = MathF.Sin(MathF.Tau * frequency * time) +
                MathF.Sin(MathF.Tau * frequency * 1.25f * time) * 0.35f;
            return chord * envelope * 0.15f;
        };
    }

    private static Func<float, float> CreatePurchaseSample(int variation)
    {
        float start = 800f + variation * 45f;
        return time =>
        {
            float envelope = MathF.Exp(-time * 12f);
            float frequency = start - time * (2400f + variation * 100f);
            return MathF.Sin(MathF.Tau * frequency * time) * envelope * 0.24f;
        };
    }

    private static Func<float, float> CreateReloadSample(Random random, int variation)
    {
        float[] noise = CreateNoise(random, (int)(SampleRate * 0.28f));
        float pitch = variation * 24f;
        return time =>
        {
            int index = Math.Clamp((int)(time * SampleRate), 0, noise.Length - 1);
            float envelope = MathF.Exp(-time * 9f);
            float clack = MathF.Sin(MathF.Tau * (320f + pitch) * time);
            float click = MathF.Sin(MathF.Tau * (720f + pitch * 2) * time);
            float transient = time < 0.045f ? noise[index] * 0.35f : 0f;
            return (clack * 0.13f + click * 0.05f + transient) * envelope;
        };
    }

    private static float[] CreateNoise(Random random, int length)
    {
        var noise = new float[length];
        for (int i = 0; i < noise.Length; i++)
            noise[i] = (float)(random.NextDouble() * 2 - 1);
        return noise;
    }

    private static SoundEffect Create(float duration, Func<float, float> sample)
    {
        int sampleCount = (int)(SampleRate * duration);
        using var stream = new MemoryStream(44 + sampleCount * sizeof(short));
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            writer.Write("RIFF"u8);
            writer.Write(36 + sampleCount * sizeof(short));
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SampleRate);
            writer.Write(SampleRate * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(sampleCount * sizeof(short));

            for (int i = 0; i < sampleCount; i++)
            {
                float value = Math.Clamp(sample(i / (float)SampleRate), -1f, 1f);
                writer.Write((short)(value * short.MaxValue));
            }
        }

        stream.Position = 0;
        return SoundEffect.FromStream(stream);
    }
}
