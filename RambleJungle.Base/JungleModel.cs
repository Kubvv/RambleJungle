﻿namespace RambleJungle.Base
{
    using RambleJungle.Base.Tools;
    using System.Drawing;

    public class JungleModel
    {
        public List<JungleObject> Jungle { get; private set; } = new List<JungleObject>();

        public JungleModel()
        {
        }

        public event EventHandler? JungleGenerated;

        /// <summary>
        /// Fills jungle with objects
        /// </summary>
        public void PrepareJungle(WeaponModel weaponModel)
        {
            if (weaponModel == null)
            {
                throw new ArgumentNullException(nameof(weaponModel));
            }

            Jungle.Clear();

            // insert all objects
            foreach (JungleObjectType jungleObjectType in Enum.GetValues(typeof(JungleObjectType)))
            {
                if (jungleObjectType == JungleObjectType.EmptyField || jungleObjectType == JungleObjectType.Rambler)
                {
                    // empty fields and rambler are last to insert
                }
                else if (jungleObjectType == JungleObjectType.DenseJungle)
                {
                    int denseJungleCount = Config.Random.Next(Config.JungleObjectsCount[jungleObjectType]) + 1;
                    for (int i = 0; i < denseJungleCount; i++)
                    {
                        Jungle.Add(new JungleObject(jungleObjectType));
                    }
                }
                else if (jungleObjectType == JungleObjectType.LostWeapon)
                {
                    if (weaponModel == null) throw new NullReferenceException();
                    for (int i = 0; i < Config.JungleObjectsCount[jungleObjectType]; i++)
                    {
                        Weapon randomWeapon = weaponModel.ExclusiveRandomWeapon();
                        Jungle.Add(new JungleObject(jungleObjectType, randomWeapon, randomWeapon.Name));
                    }
                }
                else if (Config.Beasts.Contains(jungleObjectType))
                {
                    for (int i = 0; i < Config.JungleObjectsCount[jungleObjectType]; i++)
                    {
                        Jungle.Add(new Beast(jungleObjectType));
                    }
                }
                else if (Config.Arsenals.Contains(jungleObjectType))
                {
                    for (int i = 0; i < Config.JungleObjectsCount[jungleObjectType]; i++)
                    {
                        JungleArsenal jungleArsenal = new(jungleObjectType);
                        for (int j = 0; j < Config.JUNGLEARSENALWEAPONCOUNT; j++)
                        {
                            jungleArsenal.AddWeapon(weaponModel.ExclusiveRandomWeapon());
                        }
                        Jungle.Add(jungleArsenal);
                    }
                }
                else
                {
                    for (int i = 0; i < Config.JungleObjectsCount[jungleObjectType]; i++)
                    {
                        Jungle.Add(new JungleObject(jungleObjectType));
                    }
                }
            }

            // fill the rest with empty fields
            for (int i = Jungle.Count; i < Config.JungleHeight * Config.JungleWidth; i++)
            {
                Jungle.Add(new JungleObject(JungleObjectType.EmptyField));
            }
        }

        /// <summary>
        /// Puts jungle objects at random positions
        /// </summary>
        public void GenerateJungle()
        {
            List<Point> coordinates = new(), denseJungleLocations = new();

            bool everyFieldIsReachable = false;
            while (!everyFieldIsReachable)
            {
                // generate all possible locations
                coordinates.Clear();
                for (int row = 0; row < Config.JungleHeight; row++)
                {
                    for (int col = 0; col < Config.JungleWidth; col++)
                    {
                        coordinates.Add(new Point(col, row));
                    }
                }

                // choose random location for dense jungle
                denseJungleLocations.Clear();
                foreach (JungleObject jungleObject in Jungle.Where(jo => jo.JungleObjectType == JungleObjectType.DenseJungle))
                {
                    jungleObject.Reset();
                    int coordinate = Config.Random.Next(coordinates.Count);
                    jungleObject.SetCoordinates(coordinates[coordinate]);
                    denseJungleLocations.Add(coordinates[coordinate]);
                    coordinates.RemoveAt(coordinate);
                }

                List<Point> filledJungle = SmithsFill.FloodFill(Config.JungleWidth, Config.JungleHeight, denseJungleLocations, coordinates[0]);
                everyFieldIsReachable = filledJungle.Count == Config.JungleWidth * Config.JungleHeight;
            }

            // choose random location for every jungle object but dense jungle
            foreach (JungleObject jungleObject in Jungle.Where(jo => jo.JungleObjectType != JungleObjectType.DenseJungle))
            {
                if (jungleObject is Beast beast)
                {
                    beast.Reset();
                }
                else
                {
                    jungleObject.Reset();
                }
                bool acceptabePosition = false;
                while (!acceptabePosition)
                {
                    int coordinateIndex = Config.Random.Next(coordinates.Count);
                    Point coordinate = coordinates[coordinateIndex];
                    if (jungleObject.JungleObjectType == JungleObjectType.Radar)
                    {
                        acceptabePosition = Config.WastedRadars || (IsNotBorder(coordinate) && !IsNeighbourOf(coordinate, JungleObjectType.Radar));
                    }
                    else
                    {
                        acceptabePosition = true;
                    }
                    if (acceptabePosition)
                    {
                        jungleObject.SetCoordinates(coordinate);
                        coordinates.RemoveAt(coordinateIndex);
                    }
                }
            }

            JungleGenerated?.Invoke(this, new EventArgs());
        }

        private static bool IsNotBorder(Point coordinate) => coordinate.X > 0 && coordinate.X < Config.JungleWidth - 1 && coordinate.Y > 0 && coordinate.Y < Config.JungleHeight - 1;

        private bool IsNeighbourOf(Point coordinate, JungleObjectType jungleObjectType)
        {
            IEnumerable<Point> neighbours = FindNeighboursTo(coordinate, 1);
            bool found = false;
            int index = 0;
            while (!found && index < neighbours.Count())
            {
                JungleObject? jungleObject = GetJungleObjectAt(neighbours.ElementAt(index));
                found = jungleObject != null && jungleObject.JungleObjectType == jungleObjectType;
                index++;
            }
            return found;
        }

        /// <summary>
        /// Calculates, how much of the jungle is explored.
        /// Explored fields (visited, marked or pointed) to all fields minus dense jungle
        /// </summary>
        /// <returns>Percent (0-100), showing explored fields to all visitable fields ratio.</returns>
        public double ExplorationProgress()
        {
            int explorableFields = Config.JungleWidth * Config.JungleHeight;
            foreach (JungleObjectType jungleObjectType in Config.VisibleItems)
            {
                explorableFields -= Jungle.Count(jo => jo.JungleObjectType == jungleObjectType);
            }
            int exploredFields = Jungle.Count(jo => Statuses.Explored.HasFlag(jo.Status) &&
                !Config.VisibleItems.Contains(jo.JungleObjectType));
            return exploredFields * 100.0 / explorableFields;
        }

        /// <summary>
        /// Finds object of given type nearest to given coordinates
        /// </summary>
        /// <param name="coordinates">coordinates</param>
        /// <param name="jungleObjectType">Jungle object type</param>
        /// <returns>Found object or null</returns>
        public JungleObject? FindNearestTo(Point coordinates, JungleObjectType jungleObjectType, Statuses statuses)
        {
            int[,] nearestObjects = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
            int[,] nearObjects = { { -1, -1 }, { 1, -1 }, { 1, -1 }, { 1, 1 }, { 1, 1 }, { -1, 1 }, { -1, 1 }, { -1, -1 } };
            int distance = 1;
            JungleObject? jungleObject = null;
            while ((distance < Config.JungleHeight || distance < Config.JungleWidth) && jungleObject == null)
            {
                int[,] factors = new int[nearestObjects.GetUpperBound(0) + 1, 2];
                for (int index = 0; index <= factors.GetUpperBound(0); index++)
                {
                    factors[index, 0] = nearestObjects[index, 0] * distance;
                    factors[index, 1] = nearestObjects[index, 1] * distance;
                }
                jungleObject = FindObjectByFactors(coordinates, factors, jungleObjectType, statuses);

                int deviation = 1;
                while (deviation <= distance && jungleObject == null)
                {
                    factors = new int[nearObjects.GetUpperBound(0) + 1, 2];
                    for (int index = 0; index <= factors.GetUpperBound(0); index++)
                    {
                        bool distanceFirst = index == 2 || index == 3 || index == 6 || index == 7;
                        factors[index, 0] = nearObjects[index, 0] * (distanceFirst ? distance : deviation);
                        factors[index, 1] = nearObjects[index, 1] * (distanceFirst ? deviation : distance);
                    }
                    jungleObject = FindObjectByFactors(coordinates, factors, jungleObjectType, statuses);
                    deviation++;
                }

                distance++;
            }
            return jungleObject;
        }

        private JungleObject? FindObjectByFactors(Point coordinates, int[,] factors, JungleObjectType jungleObjectType, Statuses statuses)
        {
            JungleObject? result = null;
            int index = 0;
            while (index <= factors.GetUpperBound(0) && result == null)
            {
                Point point = new(coordinates.X + factors[index, 0], coordinates.Y + factors[index, 1]);
                JungleObject? jungleObject = GetJungleObjectAt(point);
                if (jungleObject != null && jungleObject.JungleObjectType == jungleObjectType && (jungleObject.Status & statuses) > 0)
                {
                    result = jungleObject;
                }
                index++;
            }
            return result;
        }

        public void MarkHiddenObjects()
        {
            foreach (JungleObject jungleObject in Jungle)
            {
                if (jungleObject.Status == Statuses.Hidden || jungleObject.Status == Statuses.Pointed || Config.DebugMode)
                {
                    jungleObject.SetStatus(Statuses.Marked);
                }
            }
        }

        public void PointHiddenGoodItems()
        {
            foreach (JungleObject jungleObject in Jungle)
            {
                if (Config.GoodItems.Contains(jungleObject.JungleObjectType) && jungleObject.Status == Statuses.Hidden)
                {
                    jungleObject.SetStatus(Statuses.Pointed);
                }
            }
        }

        /// <summary>
        /// Finds a list of surrounding points at a given distance.
        /// </summary>
        /// <param name="coordinates">Selected center point.</param>
        /// <param name="distance">Distance from the selected point.</param>
        /// <returns>List of Points.</returns>
        public static IEnumerable<Point> FindNeighboursTo(Point coordinates, int distance)
        {
            var neighbours = new List<Point>();
            for (var x = -distance; x <= distance; x++)
            {
                for (var y = -distance; y <= distance; y++)
                {
                    if (Math.Abs(x) == distance || Math.Abs(y) == distance)
                    {
                        neighbours.Add(new Point(coordinates.X + x, coordinates.Y + y));
                    }
                }
            }
            return neighbours;
        }

        /// <summary>
        /// Changes the status of points to pointed.
        /// </summary>
        /// <param name="points">List of points.</param>
        public void SetPointedAt(List<Point> points)
        {
            points.ForEach(point => SetPointedAt(point, false));
        }

        /// <summary>
        /// Changes the status of a point to pointed.
        /// </summary>
        /// <param name="point">Point to change a status.</param>
        /// <param name="beastOrBadOnly">Change the status only for the beast points.</param>
        public void SetPointedAt(Point point, bool beastOrBadOnly)
        {
            JungleObject? jungleObject = GetJungleObjectAt(point);
            if (jungleObject != null)
            {
                bool canPointField = jungleObject.JungleObjectType != JungleObjectType.EmptyField &&
                    (Config.Beasts.Contains(jungleObject.JungleObjectType) || Config.BadItems.Contains(jungleObject.JungleObjectType) || !beastOrBadOnly);
                if (Config.DebugMode)
                {
                    if (canPointField && jungleObject.Status == Statuses.Visible && !Config.VisibleItems.Contains(jungleObject.JungleObjectType))
                    {
                        jungleObject.SetStatus(Statuses.Pointed);
                    }
                }
                else if (canPointField && jungleObject.Status == Statuses.Hidden)
                {
                    jungleObject.SetStatus(Statuses.Pointed);
                }
            }
        }

        /// <summary>
        /// Searches for jungle object at given coordinates.
        /// </summary>
        /// <param name="point">Coordinates inside or outside jungle.</param>
        /// <returns>Jungle object at coordinates or null, if coordinates are outside jungle.</returns>
        public JungleObject? GetJungleObjectAt(Point point)
        {
            JungleObject? jungleObject = null;
            if (point.X >= 0 && point.X < Config.JungleWidth && point.Y >= 0 && point.Y < Config.JungleHeight)
            {
                jungleObject = Jungle.FirstOrDefault(jo => jo.Coordinates.X == point.X && jo.Coordinates.Y == point.Y);
            }
            return jungleObject;
        }

        /// <summary>
        /// Finds random unvisited jungle object of given type
        /// </summary>
        /// <param name="jungleObjectType">Type of the searched jungle object</param>
        /// <returns>Found object or null</returns>
        public JungleObject? GetRandomJungleObject(JungleObjectType jungleObjectType)
        {
            JungleObject? result = null;
            int jungleObjectCount = CountOf(jungleObjectType);
            if (jungleObjectCount > 0)
            {
                int randomJungleObject = Config.Random.Next(jungleObjectCount) + 1;
                int counter = 0;
                foreach (JungleObject jungleObject in Jungle)
                {
                    if (jungleObject.JungleObjectType == jungleObjectType && jungleObject.Status != Statuses.Visited)
                    {
                        counter += 1;
                        if (counter == randomJungleObject)
                        {
                            result = jungleObject;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Counts unvisited objects of given type in the jungle
        /// </summary>
        /// <param name="jungleObjectType">type of the jungle object to count</param>
        /// <returns>count of objects</returns>
        public int CountOf(JungleObjectType jungleObjectType)
        {
            return Jungle.Count(jo => jo.JungleObjectType == jungleObjectType && jo.Status != Statuses.Visited);
        }

        public List<JungleObject> GetJungleObjects(List<JungleObjectType> jungleObjectTypes)
        {
            List<JungleObject> result = new();
            if (jungleObjectTypes != null)
            {
                foreach (JungleObjectType jungleObjectType in jungleObjectTypes)
                {
                    JungleObject? jungleObject = Jungle.FirstOrDefault(jo => jo.JungleObjectType == jungleObjectType);
                    if (jungleObject != null)
                    {
                        result.Add(jungleObject);
                    }
                }
            }
            return result;
        }

        public List<JungleObject> GetJungleObjects(JungleObjectType jungleObjectType)
        {
            List<JungleObject> result = new();
            foreach (JungleObject jungleObject in Jungle.Where(jo => jo.JungleObjectType == jungleObjectType))
            {
                result.Add(jungleObject);
            }
            return result;
        }
    }
}