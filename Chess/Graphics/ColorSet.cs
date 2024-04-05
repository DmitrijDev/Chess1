
namespace Chess
{
    internal class ColorSet
    {
        public Color WhitePiecesColor { get; private set; }

        public Color BlackPiecesColor { get; private set; }

        public Color LightSquaresColor { get; private set; }

        public Color DarkSquaresColor { get; private set; }

        public Color HighlightColor { get; private set; }

        public Color OutlineColor { get; private set; }

        public Color BoardColor { get; private set; }

        public Color FormBackColor { get; private set; }

        public string Name { get; private set; }

        public static ColorSet[] GetStandartSets() => new ColorSet[] { Simple, ColoredPieces, Winter, Spring, Summer, Autumn };

        public static ColorSet Simple => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.SandyBrown,
            DarkSquaresColor = Color.Sienna,
            HighlightColor = Color.DarkCyan,
            OutlineColor = Color.Lime,
            BoardColor = Color.SaddleBrown,
            FormBackColor = Color.Wheat,
            Name = "Стандартные"
        };

        public static ColorSet ColoredPieces => new()
        {
            WhitePiecesColor = Color.Goldenrod,
            BlackPiecesColor = Color.DarkRed,
            LightSquaresColor = Color.White,
            DarkSquaresColor = Color.Black,
            HighlightColor = Color.LawnGreen,
            OutlineColor = Color.Aqua,
            BoardColor = Color.Black,
            FormBackColor = Color.Khaki,
            Name = "Цветные фигуры"
        };

        public static ColorSet Winter => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.DarkGray,
            DarkSquaresColor = Color.Gray,
            HighlightColor = Color.Green,
            OutlineColor = Color.Blue,
            BoardColor = Color.Black,
            FormBackColor = Color.LightGray,
            Name = "Зима"
        };

        public static ColorSet Spring => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.Gray,
            DarkSquaresColor = Color.SeaGreen,
            HighlightColor = Color.GreenYellow,
            OutlineColor = Color.MediumVioletRed,
            BoardColor = Color.DimGray,
            FormBackColor = Color.LightSkyBlue,
            Name = "Весна"
        };

        public static ColorSet Summer => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.DarkKhaki,
            DarkSquaresColor = Color.Chocolate,
            HighlightColor = Color.DarkCyan,
            OutlineColor = Color.Green,
            BoardColor = Color.SaddleBrown,
            FormBackColor = Color.SandyBrown,
            Name = "Лето"
        };

        public static ColorSet Autumn => new()
        {
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,
            LightSquaresColor = Color.Goldenrod,
            DarkSquaresColor = Color.SaddleBrown,
            HighlightColor = Color.BlueViolet,
            OutlineColor = Color.DarkCyan,
            BoardColor = Color.Maroon,
            FormBackColor = Color.Olive,
            Name = "Осень"
        };
    }
}
