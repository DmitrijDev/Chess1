
namespace Chess
{
    internal static class Graphics
    {
        private static int[,] _matrix; // 1 - соотв. пиксель картинки относ. к изображению фигуры, -1 - фона.
        private static bool[,] _isInBorderZone;

        // Раскрашивает черно-белую картинку фигуры, при условии, что фигура - белая, фон - черный.
        public static Bitmap GetColoredPicture(Bitmap oldPicture, Color backColor, Color imageColor)
        {
            var newPicture = new Bitmap(oldPicture);
            _matrix = new int[newPicture.Width, newPicture.Height];
            _isInBorderZone = new bool[newPicture.Width, newPicture.Height];

            for (var i = 0; i < newPicture.Width; ++i)
            {
                for (var j = 0; j < newPicture.Height; ++j)
                {
                    if (newPicture.GetPixel(i, j).R < 127)
                    {
                        _matrix[i, j] = -1;
                        newPicture.SetPixel(i, j, backColor);
                    }
                    else
                    {
                        _matrix[i, j] = 1;
                        newPicture.SetPixel(i, j, imageColor);
                    }
                }
            }

            for (var i = 0; i < newPicture.Width; ++i)
            {
                for (var j = 0; j < newPicture.Height; ++j)
                {
                    if (IsBorderPixel(newPicture, i, j, 1))
                    {
                        _isInBorderZone[i, j] = true;
                    }
                }
            }

            ErodeBorders(newPicture);

            return newPicture;
        }

        public static bool IsBorderPixel(Bitmap picture, int x, int y, int borderZoneSize)
        {
            if (x < borderZoneSize || x >= picture.Width - borderZoneSize || y < borderZoneSize || y >= picture.Height - borderZoneSize)
            {
                return false;
            }

            for (var i = x - borderZoneSize; i <= x + borderZoneSize; ++i)
            {
                for (var j = y - borderZoneSize; j <= y + borderZoneSize; ++j)
                {
                    if (_matrix[i, j] == -_matrix[x, y])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void ErodeBorders(Bitmap picture)
        {
            for (var i = 0; i < picture.Width; ++i)
            {
                for (var j = 0; j < picture.Height; ++j)
                {
                    if (_isInBorderZone[i, j])
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
    }
}
