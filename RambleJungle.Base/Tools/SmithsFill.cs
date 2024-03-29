﻿namespace RambleJungle.Base.Tools
{
    using System.Drawing;

    public static class SmithsFill
    {
        public const int EMPTYPOINT = 0;
        public const int OUTLINEPOINT = 1;
        public const int FILLEDPOINT = 2;

        private static readonly Stack<Segment> stack = new();

        /// <summary>
        /// Wypełnienie powodziowe prostokątnego obszaru, zaczynając od punktu startowego.
        /// Wypełnienie jest ograniczone przez ośmio-spójne kontury (wypełnienie nie "wycieka" po skosie).
        /// </summary>
        /// <param name="width">Szerokość obszaru.</param>
        /// <param name="height">Wysokość obszaru.</param>
        /// <param name="outline">Współrzędne punktów, tworzących kontury.</param>
        /// <param name="start">Współrzędne punktu startowego.</param>
        /// <returns>Współrzędne punktów, tworzących kontury oraz punktów wypełnionych.</returns>
        public static List<Point> FloodFill(int width, int height, List<Point> outline, Point start)
        {
            List<Point> result = new();

            int[,] rectangle = new int[width, height];
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    rectangle[col, row] = EMPTYPOINT;
                }
            }

            foreach (Point point in outline)
            {
                rectangle[(int)point.X, (int)point.Y] = OUTLINEPOINT;
            }

            FloodFill(ref rectangle, (int)start.X, (int)start.Y);

            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    if (rectangle[col, row] != EMPTYPOINT)
                    {
                        result.Add(new Point(col, row));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Wypełnienie powodziowe prostokątnego obszaru, zaczynając od punktu startowego.
        /// Wypełnienie jest ograniczone przez ośmio-spójne kontury (wypełnienie nie "wycieka" po skosie).
        /// W wyniku działania funkcji, punkty EMPTYPOINT, do których można dojść od punktu startowego,
        /// poruszając się w lewo, prawo, górę lub dół, zamieniane są na FILLEDPOINT.
        /// </summary>
        /// <param name="rectangle">Obszar do wypełnienia (EMPTYPOINT), zawierający kontury (OUTLINEPOINT).</param>
        /// <param name="startX">Współrzędna X punktu startowego.</param>
        /// <param name="startY">Współrzędna Y punktu startowego.</param>
        public static void FloodFill(ref int[,] rectangle, int startX, int startY)
        {
            if (rectangle == null || rectangle[startX, startY] != EMPTYPOINT) return;

            stack.Clear();

            FindSegment(rectangle, startX, startY);

            while (stack.Count > 0)
            {
                Segment segment = stack.Pop();
                if (segment != null)
                {
                    for (int i = segment.X; i < segment.X + segment.Length; i++)
                    {
                        rectangle[i, segment.Y] = FILLEDPOINT;
                    }

                    if (segment.Y > 0)
                    {
                        SearchSegment(rectangle, segment.X, segment.Y - 1, segment.Length);
                    }
    
                    if (segment.Y < rectangle.GetLength(1) - 1)
                    {
                        SearchSegment(rectangle, segment.X, segment.Y + 1, segment.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Odnalezienie początku i długości niewypełnionego, poziomego segmentu,
        /// zawierającego podany punkt startowy.
        /// </summary>
        /// <param name="rectangle">Wypełniany obszar.</param>
        /// <param name="startX">Współrzędna X punktu startowego.</param>
        /// <param name="startY">Współrzędna Y punktu startowego.</param>
        /// <returns>Współrzędna X punktu, należącego do potencjalnego, następnego segmentu.</returns>
        private static int FindSegment(int[,] rectangle, int startX, int startY)
        {
            int x1 = startX, x2 = startX;
            while (x1 >= 0 && rectangle[x1, startY] == EMPTYPOINT)
            {
                x1--;
            }
            x1++;
            while (x2 < rectangle.GetLength(0) && rectangle[x2, startY] == EMPTYPOINT)
            {
                x2++;
            }
            stack.Push(new Segment(x1, startY, x2 - x1));
            return x2 + 1;
        }

        /// <summary>
        /// Odnalezienie początku i długości niewypełnionego, poziomego segmentu,
        /// zawierającego podany punkt startowy.
        /// </summary>
        /// <param name="rectangle">Wypełniany obszar.</param>
        /// <param name="x">Współrzędna X początku segmentu.</param>
        /// <param name="y">Współrzędna Y początku segmentu.</param>
        /// <param name="lenght">Długość segmentu.</param>
        private static void SearchSegment(int[,] rectangle, int x, int y, int lenght)
        {
            int x1 = x, x2 = x + lenght - 1;
            while (x1 <= x2)
            {
                if (rectangle[x1, y] == FILLEDPOINT)
                {
                    x1 += 2;
                }
                else if (rectangle[x1, y] != EMPTYPOINT)
                {
                    x1++;
                }
                else
                {
                    x1 = FindSegment(rectangle, x1, y);
                }
            }
        }
    }

    internal class Segment
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Length { get; private set; }

        public Segment(int x, int y, int length)
        {
            X = x;
            Y = y;
            Length = length;
        }
    }
}
