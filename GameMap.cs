using Microsoft.Xna.Framework;

namespace ZombieSurvival;

internal sealed class GameMap
{
    private const int Width = 1280;
    private const int Height = 720;
    private const int NavigationCellSize = 32;
    private const int NavigationColumns = 40;
    private const int NavigationRows = 22;
    private static readonly Point[] NavigationDirections =
    [
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1)
    ];

    private static readonly Structure[] BaseStructures =
    [
        new(new Rectangle(88, 88, 262, 158), new Color(81, 75, 62), new Color(118, 104, 79), false),
        new(new Rectangle(930, 92, 250, 174), new Color(67, 78, 75), new Color(91, 112, 103), false),
        new(new Rectangle(92, 365, 222, 169), new Color(91, 66, 54), new Color(133, 87, 62), false),
        new(new Rectangle(390, 548, 220, 92), new Color(73, 78, 61), new Color(105, 118, 79), false),
        new(new Rectangle(865, 525, 244, 112), new Color(76, 72, 65), new Color(115, 101, 82), false),
        new(new Rectangle(457, 318, 70, 42), new Color(98, 68, 43), new Color(146, 98, 55), true),
        new(new Rectangle(757, 409, 76, 46), new Color(83, 72, 53), new Color(128, 104, 69), true),
        new(new Rectangle(577, 478, 88, 38), new Color(78, 77, 59), new Color(112, 116, 77), true),
        new(new Rectangle(357, 270, 46, 74), new Color(72, 80, 68), new Color(100, 119, 90), true),
        new(new Rectangle(700, 278, 46, 76), new Color(75, 79, 71), new Color(112, 118, 98), true)
    ];

    private readonly List<Structure> _structures = [];

    public Vector2[] BreachPoints { get; private set; } = [];

    public GameMap(Random random) => Randomize(random);

    public void Randomize(Random random)
    {
        _structures.Clear();
        BreachPoints =
        [
            GenerateBreachPoint(random, 0),
            GenerateBreachPoint(random, 1),
            GenerateBreachPoint(random, 2),
            GenerateBreachPoint(random, 3)
        ];

        foreach (Structure baseStructure in BaseStructures)
        {
            bool placed = false;
            for (int attempt = 0; attempt < 5000; attempt++)
            {
                Rectangle baseBounds = baseStructure.Bounds;
                int x = random.Next(38, Width - baseBounds.Width - 37);
                int y = random.Next(48, Height - baseBounds.Height - 15);
                var bounds = new Rectangle(x, y, baseBounds.Width, baseBounds.Height);
                if (bounds.Left < 38 || bounds.Right > Width - 38 ||
                    bounds.Top < 48 || bounds.Bottom > Height - 16 ||
                    IsNearPlayerStart(bounds) ||
                    _structures.Any(structure => Expanded(bounds, 20).Intersects(structure.Bounds)) ||
                    BreachPoints.Any(point => DistanceToRectangleSquared(point, bounds) < 70 * 70))
                    continue;

                _structures.Add(baseStructure with { Bounds = bounds });
                placed = true;
                break;
            }

            if (!placed)
                throw new InvalidOperationException("Could not place all randomized map structures.");
        }
    }

    public Vector2 MovePlayer(Vector2 position, Vector2 movement, float radius)
    {
        Vector2 result = position;
        Vector2 horizontal = new(movement.X, 0);
        Vector2 horizontalCandidate = result + horizontal;
        if (!IsBlocked(horizontalCandidate, radius))
            result = horizontalCandidate;

        Vector2 verticalCandidate = result + new Vector2(0, movement.Y);
        if (!IsBlocked(verticalCandidate, radius))
            result = verticalCandidate;

        return result;
    }

    public Vector2 MoveZombie(Vector2 position, Vector2 movement, Vector2 target, float radius)
    {
        float distance = movement.Length();
        if (distance <= 0)
            return position;

        float baseAngle = MathF.Atan2(movement.Y, movement.X);
        Vector2 bestPosition = position;
        float bestDistance = Vector2.DistanceSquared(position, target);
        float[] angleOffsets = [0, 0.65f, -0.65f, 1.3f, -1.3f, 2.1f, -2.1f, MathHelper.Pi];
        foreach (float offset in angleOffsets)
        {
            float angle = baseAngle + offset;
            Vector2 candidate = position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
            if (!IsPathClear(position, candidate, radius))
                continue;

            float candidateDistance = Vector2.DistanceSquared(candidate, target);
            if (candidateDistance < bestDistance)
            {
                bestDistance = candidateDistance;
                bestPosition = candidate;
            }
        }

        if (bestPosition == position)
        {
            foreach (float offset in new[] { -MathHelper.PiOver2, MathHelper.PiOver2 })
            {
                float angle = baseAngle + offset;
                Vector2 candidate = position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
                if (!IsPathClear(position, candidate, radius))
                    continue;

                float candidateDistance = Vector2.DistanceSquared(candidate, target);
                if (candidateDistance < bestDistance || bestPosition == position)
                {
                    bestDistance = candidateDistance;
                    bestPosition = candidate;
                }
            }
        }

        return bestPosition;
    }

    public bool IsBlocked(Vector2 position, float radius)
    {
        if (position.X - radius < 16 || position.X + radius > 1264 ||
            position.Y - radius < 44 || position.Y + radius > 704)
            return true;

        foreach (Structure structure in _structures)
        {
            Rectangle bounds = structure.Bounds;
            if (position.X + radius > bounds.Left && position.X - radius < bounds.Right &&
                position.Y + radius > bounds.Top && position.Y - radius < bounds.Bottom)
                return true;
        }

        return false;
    }

    public bool BlocksLine(Vector2 start, Vector2 end)
    {
        Vector2 difference = end - start;
        float distance = difference.Length();
        if (distance <= 1)
            return false;

        Vector2 step = difference / distance * 6f;
        int samples = (int)(distance / 6f);
        Vector2 point = start;
        for (int i = 0; i < samples; i++)
        {
            point += step;
            if (IsInsideStructure(point))
                return true;
        }
        return IsInsideStructure(end);
    }

    public Vector2 FindNextWaypoint(Vector2 start, Vector2 target, float radius)
    {
        if (IsPathClear(start, target, radius))
            return target;

        Point startCell = FindNearestNavigableCell(ToCell(start), start, radius);
        Point targetCell = FindNearestNavigableCell(ToCell(target), target, radius);
        if (startCell.X < 0 || targetCell.X < 0)
            return target;

        int[] previous = new int[NavigationColumns * NavigationRows];
        Array.Fill(previous, -1);
        int startIndex = ToIndex(startCell);
        int targetIndex = ToIndex(targetCell);
        previous[startIndex] = startIndex;
        var frontier = new Queue<Point>();
        frontier.Enqueue(startCell);

        while (frontier.Count > 0 && previous[targetIndex] == -1)
        {
            Point current = frontier.Dequeue();
            int currentIndex = ToIndex(current);
            foreach (Point direction in NavigationDirections)
            {
                Point next = new(current.X + direction.X, current.Y + direction.Y);
                if (!IsNavigable(next, radius))
                    continue;

                int nextIndex = ToIndex(next);
                if (previous[nextIndex] != -1)
                    continue;

                previous[nextIndex] = currentIndex;
                frontier.Enqueue(next);
            }
        }

        if (previous[targetIndex] == -1)
            return target;

        int pathIndex = targetIndex;
        while (previous[pathIndex] != startIndex && previous[pathIndex] != pathIndex)
            pathIndex = previous[pathIndex];

        Point waypointCell = new(pathIndex % NavigationColumns, pathIndex / NavigationColumns);
        return CellCenter(waypointCell);
    }

    public bool TryGetBlockPoint(Vector2 start, Vector2 end, out Vector2 blockPoint)
    {
        Vector2 difference = end - start;
        float distance = difference.Length();
        int samples = Math.Max(1, (int)MathF.Ceiling(distance / 5f));
        Vector2 step = difference / samples;
        Vector2 point = start;
        for (int i = 0; i < samples; i++)
        {
            point += step;
            if (!IsInsideStructure(point))
                continue;

            blockPoint = point;
            return true;
        }

        blockPoint = end;
        return false;
    }

    public void Draw(Action<Rectangle, Color> fill)
    {
        foreach (Structure structure in _structures)
        {
            Rectangle bounds = structure.Bounds;
            fill(new Rectangle(bounds.X + 4, bounds.Y + 6, bounds.Width, bounds.Height),
                new Color(18, 24, 22, 130));
            fill(bounds, new Color(25, 30, 28));
            fill(new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8),
                structure.WallColor);

            if (structure.IsCover)
            {
                fill(new Rectangle(bounds.X + 7, bounds.Y + 5, bounds.Width - 14, 7), structure.RoofColor);
                fill(new Rectangle(bounds.X + 9, bounds.Bottom - 7, bounds.Width - 18, 3),
                    new Color(48, 47, 38));
                fill(new Rectangle(bounds.X + bounds.Width / 2 - 2, bounds.Y + 6, 4, bounds.Height - 13),
                    new Color(48, 48, 39));
                continue;
            }

            fill(new Rectangle(bounds.X + 7, bounds.Y + 7, bounds.Width - 14, bounds.Height - 14),
                structure.RoofColor);
            fill(new Rectangle(bounds.X + 12, bounds.Y + 12, bounds.Width - 24, bounds.Height - 24),
                structure.WallColor);
            for (int x = bounds.X + 24; x < bounds.Right - 20; x += 42)
            {
                fill(new Rectangle(x, bounds.Y + 10, 20, 10), new Color(41, 54, 53));
                fill(new Rectangle(x + 2, bounds.Y + 12, 16, 2), new Color(118, 139, 128));
            }
            fill(new Rectangle(bounds.X + 12, bounds.Bottom - 16, bounds.Width - 24, 5),
                new Color(49, 48, 40));
            fill(new Rectangle(bounds.X + bounds.Width / 2 - 16, bounds.Bottom - 24, 32, 12),
                new Color(42, 42, 35));
        }
    }

    private bool IsInsideStructure(Vector2 point) =>
        _structures.Any(structure => structure.Bounds.Contains(point));

    private bool IsPathClear(Vector2 start, Vector2 end, float radius)
    {
        Vector2 difference = end - start;
        float distance = difference.Length();
        int samples = Math.Max(1, (int)MathF.Ceiling(distance / Math.Max(2f, radius)));
        for (int i = 1; i <= samples; i++)
        {
            Vector2 point = start + difference * (i / (float)samples);
            if (IsBlocked(point, radius))
                return false;
        }

        return true;
    }

    private static bool IsNearPlayerStart(Rectangle bounds)
    {
        Vector2 closest = new(
            MathHelper.Clamp(640, bounds.Left, bounds.Right),
            MathHelper.Clamp(380, bounds.Top, bounds.Bottom));
        return Vector2.DistanceSquared(closest, new Vector2(640, 380)) < 76 * 76;
    }

    private static int DistanceToRectangleSquared(Vector2 point, Rectangle bounds)
    {
        float x = MathHelper.Clamp(point.X, bounds.Left, bounds.Right);
        float y = MathHelper.Clamp(point.Y, bounds.Top, bounds.Bottom);
        return (int)Vector2.DistanceSquared(point, new Vector2(x, y));
    }

    private static Rectangle Expanded(Rectangle bounds, int amount) =>
        new(bounds.X - amount, bounds.Y - amount,
            bounds.Width + amount * 2, bounds.Height + amount * 2);

    private Vector2 GenerateBreachPoint(Random random, int side)
    {
        for (int attempt = 0; attempt < 500; attempt++)
        {
            Vector2 point = side switch
            {
                0 => new Vector2(30, random.Next(150, 571)),
                1 => new Vector2(1250, random.Next(150, 571)),
                2 => new Vector2(random.Next(260, 1021), 56),
                _ => new Vector2(random.Next(260, 1021), 692)
            };
            if (_structures.All(structure =>
                    DistanceToRectangleSquared(point, structure.Bounds) >= 70 * 70))
                return point;
        }

        throw new InvalidOperationException("Could not place a randomized map entrance.");
    }

    private static Point ToCell(Vector2 position) => new(
        Math.Clamp((int)((position.X - 16) / NavigationCellSize), 0, NavigationColumns - 1),
        Math.Clamp((int)((position.Y - 44) / NavigationCellSize), 0, NavigationRows - 1));

    private static int ToIndex(Point cell) => cell.Y * NavigationColumns + cell.X;

    private bool IsNavigable(Point cell, float radius)
    {
        if (cell.X < 0 || cell.X >= NavigationColumns || cell.Y < 0 || cell.Y >= NavigationRows)
            return false;

        return !IsBlocked(CellCenter(cell), radius);
    }

    private Point FindNearestNavigableCell(Point origin, Vector2 target, float radius)
    {
        Point best = new(-1, -1);
        float bestDistance = float.MaxValue;
        for (int y = Math.Max(0, origin.Y - 2); y <= Math.Min(NavigationRows - 1, origin.Y + 2); y++)
        for (int x = Math.Max(0, origin.X - 2); x <= Math.Min(NavigationColumns - 1, origin.X + 2); x++)
        {
            Point cell = new(x, y);
            if (!IsNavigable(cell, radius))
                continue;

            float distance = Vector2.DistanceSquared(CellCenter(cell), target);
            if (distance >= bestDistance)
                continue;

            best = cell;
            bestDistance = distance;
        }

        return best;
    }

    private static Vector2 CellCenter(Point cell) => new(
        16 + cell.X * NavigationCellSize + NavigationCellSize / 2f,
        44 + cell.Y * NavigationCellSize + NavigationCellSize / 2f);

    private sealed record Structure(Rectangle Bounds, Color WallColor, Color RoofColor, bool IsCover);
}
