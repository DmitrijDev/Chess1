
namespace Chess
{
    internal class ColorTheme
    {
        public Color WhitePiecesColor { get; private set; }

        public Color BlackPiecesColor { get; private set; }

        public Color LightSquaresColor { get; private set; }

        public Color DarkSquaresColor { get; private set; }

        public Color HighlightColor { get; private set; }

        public Color BoardColor { get; private set; }

        public Color FormBackColor { get; private set; }

        public string Name { get; private set; }

        public static ColorTheme[] GetStandartThemes() => new ColorTheme[] { Simple, ColoredPieces, Winter, Spring, Summer, Autumn };

        public static ColorTheme Simple => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.SandyBrown,
            DarkSquaresColor = Color.Sienna,
            HighlightColor = Color.Blue,
            BoardColor = Color.SaddleBrown,
            FormBackColor = Color.Wheat,
            Name = "Стандартные"
        };

        public static ColorTheme ColoredPieces => new()
        {
            WhitePiecesColor = Color.Goldenrod,
            BlackPiecesColor = Color.DarkRed,
            LightSquaresColor = Color.White,
            DarkSquaresColor = Color.Black,
            HighlightColor = Color.LawnGreen,
            BoardColor = Color.Black,
            FormBackColor = Color.Khaki,
            Name = "Цветные фигуры"
        };

        public static ColorTheme Winter => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.DarkGray,
            DarkSquaresColor = Color.Gray,
            HighlightColor = Color.LightGreen,
            BoardColor = Color.Black,
            FormBackColor = Color.LightGray,
            Name = "Зима"
        };

        public static ColorTheme Spring => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.Gray,
            DarkSquaresColor = Color.SeaGreen,
            HighlightColor = Color.GreenYellow,
            BoardColor = Color.DimGray,
            FormBackColor = Color.LightSkyBlue,
            Name = "Весна"
        };

        public static ColorTheme Summer => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.DarkKhaki,
            DarkSquaresColor = Color.Chocolate,
            HighlightColor = Color.DarkBlue,
            BoardColor = Color.SaddleBrown,
            FormBackColor = Color.SandyBrown,
            Name = "Лето"
        };

        public static ColorTheme Autumn => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.Goldenrod,
            DarkSquaresColor = Color.SaddleBrown,
            HighlightColor = Color.Blue,
            BoardColor = Color.Maroon,
            FormBackColor = Color.Olive,
            Name = "Осень"
        };
    }
}
