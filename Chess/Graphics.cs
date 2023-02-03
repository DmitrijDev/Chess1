
namespace Chess
{
    internal static class Graphics
    {
        // Раскрашивает черно-белую картинку фигуры, при условии, что фигура - белая, фон - черный.
        public static Bitmap GetColoredPicture(Bitmap oldPicture, Color backColor, Color imageColor)
        {
            var newPicture = new Bitmap(oldPicture.Width, oldPicture.Height);
            var matrix = GetMatrix(oldPicture);

            for (var i = 0; i < newPicture.Width; ++i)
            {
                for (var j = 0; j < newPicture.Height; ++j)
                {
                    newPicture.SetPixel(i, j, matrix[i, j] > 0 ? imageColor : backColor);
                }
            }

            for (var i = 0; i < newPicture.Width; ++i)
            {
                for (var j = 0; j < newPicture.Height; ++j)
                {
                    if (matrix[i, j] == -2 || matrix[i, j] == 2 || matrix[i, j] == 3)
                    {
                        ErodePixel(newPicture, i, j, 1);
                    }
                }
            }

            return newPicture;
        }

        private static int[,] GetMatrix(Bitmap picture)
        {
            var result = new int[picture.Width, picture.Height];

            for (var i = 0; i < picture.Width; ++i)
            {
                for (var j = 0; j < picture.Height; ++j)
                {
                    // Красный пиксель.
                    if (picture.GetPixel(i, j).R > 127 && picture.GetPixel(i, j).G < 127 && picture.GetPixel(i, j).B < 127)
                    {
                        result[i, j] = 2;
                        continue;
                    }

                    // Зеленый пиксель.
                    if (picture.GetPixel(i, j).G > 127 && picture.GetPixel(i, j).R < 127 && picture.GetPixel(i, j).B < 127)
                    {
                        result[i, j] = 3;
                        continue;
                    }

                    // Белый пиксель.
                    if (picture.GetPixel(i, j).R > 127)
                    {
                        result[i, j] = 1;
                        continue;
                    }

                    var mustBeEroded = GetNeighboursColors(picture, i, j).Any(pixelColor => pixelColor.R > 127 && pixelColor.G < 127 && pixelColor.B < 127);
                    result[i, j] = mustBeEroded ? -2 : -1;
                }
            }

            return result;
        }

        public static IEnumerable<Color> GetNeighboursColors(Bitmap picture, int x, int y)
        {
            for (var i = x - 1; i <= x + 1; ++i)
            {
                for (var j = y - 1; j <= y + 1; ++j)
                {
                    if (i >= 0 && j >= 0 && i < picture.Width && j < picture.Height && !(i == x && j == y))
                    {
                        yield return picture.GetPixel(i, j);
                    }
                }
            }
        }

        public static void ErodePixel(Bitmap picture, int x, int y, int erosionDegree)
        {
            if (x < erosionDegree || x >= picture.Width - erosionDegree || y < erosionDegree || y >= picture.Height - erosionDegree)
            {
                return;
            }

            var redComponent = 0.0;
            var greenComponent = 0.0;
            var blueComponent = 0.0;

            var coefficient = 1.0 / ((erosionDegree * 2 + 1) * (erosionDegree * 2 + 1));

            for (var i = x - erosionDegree; i <= x + erosionDegree; ++i)
            {
                for (var j = y - erosionDegree; j <= y + erosionDegree; ++j)
                {
                    redComponent += picture.GetPixel(i, j).R * coefficient;
                    greenComponent += picture.GetPixel(i, j).G * coefficient;
                    blueComponent += picture.GetPixel(i, j).B * coefficient;
                }
            }

            var newColor = Color.FromArgb((int)Math.Round(redComponent), (int)Math.Round(greenComponent), (int)Math.Round(blueComponent));
            picture.SetPixel(x, y, newColor);
        }
    }
}
