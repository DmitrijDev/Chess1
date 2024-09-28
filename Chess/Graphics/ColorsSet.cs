
namespace Chess
{
    internal class ColorsSet
    {
        public string Name { get; private set; }

        public Color FormBackColor { get; private set; }

        public Color BoardColor { get; private set; }

        public Color LightSquaresColor { get; private set; }

        public Color DarkSquaresColor { get; private set; }

        public Color WhitePiecesColor { get; private set; }

        public Color BlackPiecesColor { get; private set; }       

        public Color HighlightColor { get; private set; }

        public Color OutlineColor { get; private set; }

        public static ColorsSet[] GetStandartSets() => new ColorsSet[] { SimpleSet, ColoredPiecesSet, WinterSet, SpringSet, SummerSet, AutumnSet };

        public static ColorsSet SimpleSet => new()
        {
            Name = "Стандартные",
            FormBackColor = Color.Wheat,
            BoardColor = Color.SaddleBrown,
            LightSquaresColor = Color.SandyBrown,
            DarkSquaresColor = Color.Sienna,
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,            
            HighlightColor = Color.DarkCyan,
            OutlineColor = Color.Lime,
        };

        public static ColorsSet ColoredPiecesSet => new()
        {
            Name = "Цветные фигуры",
            FormBackColor = Color.Khaki,
            BoardColor = Color.Black,
            LightSquaresColor = Color.White,
            DarkSquaresColor = Color.Black,
            WhitePiecesColor = Color.Goldenrod,
            BlackPiecesColor = Color.DarkRed,            
            HighlightColor = Color.LawnGreen,
            OutlineColor = Color.Aqua,
        };

        public static ColorsSet WinterSet => new()
        {
            Name = "Зима",
            FormBackColor = Color.LightGray,
            BoardColor = Color.Black,
            LightSquaresColor = Color.DarkGray,
            DarkSquaresColor = Color.Gray,
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,            
            HighlightColor = Color.DodgerBlue,
            OutlineColor = Color.Green,
        };

        public static ColorsSet SpringSet => new()
        {
            Name = "Весна",
            FormBackColor = Color.LightSkyBlue,
            BoardColor = Color.DimGray,
            LightSquaresColor = Color.Gray,
            DarkSquaresColor = Color.SeaGreen,
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,            
            HighlightColor = Color.GreenYellow,
            OutlineColor = Color.MediumVioletRed,
        };

        public static ColorsSet SummerSet => new()
        {
            Name = "Лето",
            FormBackColor = Color.SandyBrown,
            BoardColor = Color.SaddleBrown,
            LightSquaresColor = Color.DarkKhaki,
            DarkSquaresColor = Color.Chocolate,
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,            
            HighlightColor = Color.DarkCyan,
            OutlineColor = Color.Green,
        };

        public static ColorsSet AutumnSet => new()
        {
            Name = "Осень",
            FormBackColor = Color.Olive,
            BoardColor = Color.Maroon,
            LightSquaresColor = Color.Goldenrod,
            DarkSquaresColor = Color.SaddleBrown,
            WhitePiecesColor = Color.White,
            BlackPiecesColor = Color.Black,            
            HighlightColor = Color.BlueViolet,
            OutlineColor = Color.DarkCyan,
        };
    }
}
