using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;


class Program
{
    // Константы для символов ключей и дверей
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();

    // Метод для чтения входных данных
    static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
        {
            data.Add(line.ToCharArray().ToList());
        }
        return data;
    }

    static Point[] directions = new[] {
        new Point(0, -1) ,
        new Point(0, 1),
        new Point(-1, 0),
        new Point(1, 0)
    };

    static bool TryMoveAndAddKey(Maze maze, Point robotMove, HashSet<char> keys)
    {
        if (maze.InBounds(robotMove))
        {
            var mapObject = maze.Map[robotMove.X, robotMove.Y];
            if (mapObject == Maze.MazeObject.Empty)
                return true;
            if (mapObject == Maze.MazeObject.Door && keys.Contains(char.ToLower(maze.Doors[robotMove])))
                return true;
            if (mapObject == Maze.MazeObject.Key)
            {
                keys.Add(maze.Keys[robotMove]);
                return true;
            }
            return false;
        }
        return false;
    }

    static int FirstSolveTry(Maze maze)
    {
        var queue = new Queue<Situation>();
        var visited = new HashSet<Situation>();
        var start = new Situation(maze.RobotsStartPositions.ToArray(), 0, new HashSet<char>());

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var situation = queue.Dequeue();

            if (situation.Keys.Count == maze.Keys.Count)
                return situation.PathLength;

            for (var i = 0; i < 4; i++)
                foreach (var direction in directions)
                {
                    var newRobotPositions = (Point[])situation.RobotsPositions.Clone();
                    newRobotPositions[i] += direction;

                    var newSituation = new Situation(newRobotPositions, situation.PathLength + 1, situation.Keys);

                    if (TryMoveAndAddKey(maze, newRobotPositions[i], newSituation.Keys))
                    {
                        if (!visited.Contains(newSituation))
                        {
                            visited.Add(newSituation);
                            queue.Enqueue(newSituation);
                        }
                    }
                }
        }
        return -1;
    }

    static int SecondSolveTry(Maze maze)
    {
        var result = 0;
        foreach (var startPosition in maze.RobotsStartPositions)
        {
            result += GetRobotsMinPathMaxKeys(startPosition, maze);
        }
        return result;
    }

    static int GetRobotsMinPathMaxKeys(Point startPoint, Maze maze)
    {
        var queue = new Queue<State>();
        var visited = new Dictionary<string, int>();
        queue.Enqueue(new State(startPoint, 0, 0));
        var maxKeys = 0;
        var minLength = int.MaxValue;

        while (queue.Count > 0)
        {
            var state = queue.Dequeue();
            var visitedKey = state.RobotPosition.X.ToString() + state.RobotPosition.Y + state.Keys;

            if (visited.ContainsKey(visitedKey))
                continue;
            visited[visitedKey] = state.Length;

            if (state.Keys > maxKeys || (state.Keys == maxKeys && state.Length < minLength))
            {
                maxKeys = state.Keys;
                minLength = state.Length;
            }
            foreach (var direction in directions)
            {
                var newPosition = new Point(state.RobotPosition) + direction;
                var mapObject = maze.Map[newPosition.X, newPosition.Y];

                if (maze.InBounds(newPosition) && mapObject != Maze.MazeObject.Wall)
                {
                    int newKeys = state.Keys;
                    if (mapObject == Maze.MazeObject.Key)
                        newKeys |= (1 << (maze.Keys[newPosition] - 'a'));

                    queue.Enqueue(new State(newPosition, newKeys, state.Length + 1));
                }
            }
        }
        return minLength;
    }

    static int Solve(List<List<char>> data)
    {
        var maze = new Maze(data);

        var solveTask = Task.Run(() => FirstSolveTry(maze));
        if (solveTask.Wait(TimeSpan.FromSeconds(40)))
            return solveTask.Result;

        else
            return SecondSolveTry(maze);
    }

    static void Main()
    {
        var data = GetInput();
        int result = Solve(data);

        if (result == -1)
        {
            Console.WriteLine("No solution found");
        }
        else
        {
            Console.WriteLine(result);
        }
    }
}

class State
{
    public Point RobotPosition;
    public int Keys;
    public int Length;

    public State(Point position, int keys, int length)
    {
        RobotPosition = new Point(position.X, position.Y);
        Keys = keys;
        Length = length;
    }
}

class Situation
{
    public Point[] RobotsPositions;
    public int PathLength = 0;
    public HashSet<char> Keys = new HashSet<char>();

    public Situation(Point[] robotsPositions, int pathLength, HashSet<char> keys)
    {
        RobotsPositions = robotsPositions;
        PathLength = pathLength;
        Keys = new HashSet<char>(keys);
    }

    public bool Equals(Situation other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null || RobotsPositions.Length != other.RobotsPositions.Length)
            return false;

        for (int i = 0; i < RobotsPositions.Length; i++)
        {
            if (!RobotsPositions[i].Equals(other.RobotsPositions[i]))
                return false;
        }

        var thisKeysSet = new HashSet<char>(Keys);
        var otherKeysSet = new HashSet<char>(other.Keys);
        return thisKeysSet.SetEquals(otherKeysSet);
    }

    public override bool Equals(object obj) => Equals(obj as Situation);

    public override int GetHashCode()
    {
        int hash = 11;

        foreach (var position in RobotsPositions)
            hash = hash * 17 + position.GetHashCode();

        foreach (var key in Keys.OrderBy(c => c))
            hash = hash * 17 + key.GetHashCode();

        return hash;
    }
}

class Point
{
    public int X;
    public int Y;

    public static Point Null = new Point(-1, -1);
    public static Point operator +(Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);
    public static Point operator -(Point p1, Point p2) => new Point(p1.X - p2.X, p1.Y - p2.Y);

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point(Point point)
    {
        X = point.X;
        Y = point.Y;

    }
    public override bool Equals(object obj)
    {
        return obj is Point other && X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 29;
            hash = hash * 41 + X;
            hash = hash * 41 + Y;
            return hash;
        }
    }

    public bool IsNull
        => X == -1 && Y == -1;
}

class Maze
{
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();

    public readonly MazeObject[,] Map;
    public readonly List<Point> RobotsStartPositions = new List<Point>();
    public readonly Dictionary<Point, char> Keys = new Dictionary<Point, char>();
    public readonly Dictionary<Point, char> Doors = new Dictionary<Point, char>();


    public Maze(List<List<char>> map)
    {
        Map = new MazeObject[map.Count, map[0].Count];
        for (var i = 0; i < map.Count; i++)
            for (var j = 0; j < map[0].Count; j++)
            {
                switch (map[i][j])
                {
                    case '#':
                        Map[i, j] = MazeObject.Wall;
                        break;
                    case '.':
                        Map[i, j] = MazeObject.Empty;
                        break;
                    case '@':
                        Map[i, j] = MazeObject.Robot;
                        RobotsStartPositions.Add(new Point(i, j));
                        break;
                    default:
                        if (keys_char.Contains(map[i][j]))
                        {
                            Map[i, j] = MazeObject.Key;
                            Keys[new Point(i, j)] = map[i][j];
                        }
                        else if (doors_char.Contains(map[i][j]))
                        {
                            Map[i, j] = MazeObject.Door;
                            Doors[new Point(i, j)] = map[i][j];
                        }
                        else
                            throw new ArgumentException("Неизвестный элемент в лабиринте");
                        break;
                }
            }
    }

    public bool InBounds(Point point)
        => point.X >= 0 && point.Y >= 0
           && Map.GetLength(0) > point.X
           && Map.GetLength(1) > point.Y;

    public enum MazeObject
    {
        Wall,
        Empty,
        Key,
        Door,
        Robot
    }
}