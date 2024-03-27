
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

            ErodeBorders(newPicture, matrix);
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
                    if (picture.GetPixel(i, j).R > byte.MaxValue / 2 && picture.GetPixel(i, j).G < byte.MaxValue / 2 && picture.GetPixel(i, j).B < byte.MaxValue / 2)
                    {
                        result[i, j] = 2;
                        continue;
                    }

                    // Зеленый пиксель.
                    if (picture.GetPixel(i, j).G > byte.MaxValue / 2 && picture.GetPixel(i, j).R < byte.MaxValue / 2 && picture.GetPixel(i, j).B < byte.MaxValue / 2)
                    {
                        result[i, j] = 3;
                        continue;
                    }

                    // Белый пиксель.
                    if (picture.GetPixel(i, j).R > byte.MaxValue / 2)
                    {
                        result[i, j] = 1;
                        continue;
                    }

                    var mustBeEroded = GetNeighboursColors(picture, i, j).
                        Any(pixelColor => pixelColor.R > byte.MaxValue / 2 && pixelColor.G < byte.MaxValue / 2 && pixelColor.B < byte.MaxValue / 2);

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

        private static void ErodeBorders(Bitmap picture, int[,] matrix)
        {
            for (var i = 0; i < picture.Width; ++i)
            {
                for (var j = 0; j < picture.Height; ++j)
                {
                    if (matrix[i, j] > 1 || matrix[i, j] < -1)
                    {
                        ErodePixel(picture, i, j, 1);
                    }
                }
            }

            for (var i = picture.Width - 1; i >= 0; --i)
            {
                for (var j = picture.Height - 1; j >= 0; --j)
                {
                    if (matrix[i, j] > 1 || matrix[i, j] < -1)
                    {
                        ErodePixel(picture, i, j, 1);
                    }
                }
            }

            for (var i = 0; i < picture.Width; ++i)
            {
                for (var j = picture.Height - 1; j >= 0; --j)
                {
                    if (matrix[i, j] > 1 || matrix[i, j] < -1)
                    {
                        ErodePixel(picture, i, j, 1);
                    }
                }
            }

            for (var i = picture.Width - 1; i >= 0; --i)
            {
                for (var j = 0; j < picture.Height; ++j)
                {
                    if (matrix[i, j] > 1 || matrix[i, j] < -1)
                    {
                        ErodePixel(picture, i, j, 1);
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

        public static void FocusPixel(Bitmap picture, int x, int y)
        {
            if (x == 0 || x >= picture.Width - 1 || y == 0 || y >= picture.Height - 1)
            {
                return;
            }

            var coefficients = new int[3][]
            {
              new int[3]  { 0, -1, 0 },
              new int[3] { -1, 5, -1 },
              new int[3] { 0, -1, 0 }
            };

            var redComponent = 0;
            var greenComponent = 0;
            var blueComponent = 0;

            for (var i = x - 1; i <= x + 1; ++i)
            {
                for (var j = y - 1; j <= y + 1; ++j)
                {
                    var coefficient = coefficients[i - x + 1][j - y + 1];

                    redComponent += coefficient * picture.GetPixel(i, j).R;
                    greenComponent += coefficient * picture.GetPixel(i, j).G;
                    blueComponent += coefficient * picture.GetPixel(i, j).B;
                }
            }

            redComponent = redComponent > byte.MaxValue ? byte.MaxValue : redComponent >= 0 ? redComponent : 0;
            greenComponent = greenComponent > byte.MaxValue ? byte.MaxValue : greenComponent >= 0 ? greenComponent : 0;
            blueComponent = blueComponent > byte.MaxValue ? byte.MaxValue : blueComponent >= 0 ? blueComponent : 0;

            var newColor = Color.FromArgb(redComponent, greenComponent, blueComponent);
            picture.SetPixel(x, y, newColor);
        }
    }
}
