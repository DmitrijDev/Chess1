
namespace Chess
{
    internal static class SettingsSaver
    {
        private static string _fileName = "UserSettings.bin";

        private static IEnumerable<byte> Split(int number)
        {
            var remained = number;

            var byte3 = (byte)(remained % 256);
            remained /= 256;

            var byte2 = (byte)(remained % 256);
            remained /= 256;

            var byte1 = (byte)(remained % 256);
            remained /= 256;

            yield return (byte)remained;
            yield return byte1;
            yield return byte2;
            yield return byte3;
        }

        private static int Join(IEnumerable<byte> bytes)
        {
            var result = 0;

            foreach (var b in bytes)
            {
                result *= 256;
                result += b;
            }

            return result;
        }

        private static IEnumerable<byte> GetSettingsBytes(this GameForm form)
        {
            yield return (byte)form.WindowState;

            var numbers = new int[] { form.Location.X, form.Location.Y, form.Width, form.Height, form.MinimumSize.Width,
            form.MinimumSize.Height , form.GamePanel.Location.X, form.GamePanel.Location.Y, form.GamePanel.ButtonSize };

            foreach (var n in numbers)
            {
                foreach (var b in Split(n))
                {
                    yield return b;
                }
            }

            yield return form.GamePanel.IsReversed ? (byte)1 : (byte)0;
            yield return (byte)form.ColorsMenu.SelectedItemIndex;
            yield return (byte)form.WhitePlayerMenu.SelectedItemIndex;
            yield return (byte)form.BlackPlayerMenu.SelectedItemIndex;
            yield return (byte)form.TimeMenu.SelectedItemIndex;
        }

        public static void SaveSettings(this GameForm form)
        {
            using (var writer = new BinaryWriter(new FileStream(_fileName, FileMode.Create, FileAccess.Write)))
            {
                var bytes = form.GetSettingsBytes().ToArray();
                writer.Write(bytes);
            }
        }

        public static int[] LoadSettings()
        {
            var bytes = new byte[42];
            var totalRead = 0;

            try
            {
                using (var reader = new BinaryReader(new FileStream(_fileName, FileMode.Open, FileAccess.Read)))
                {
                    int read;

                    do
                    {
                        read = reader.Read(bytes, totalRead, bytes.Length - totalRead);
                        totalRead += read;
                    }
                    while (read > 0);
                }
            }

            catch (FileNotFoundException)
            {
                return null;
            }

            if (totalRead < bytes.Length)
            {
                return null;
            }

            var settings = new int[15];
            settings[0] = bytes[0];

            for (int i = 1, j = 1; i <= 9; ++i, j += 4)
            {
                settings[i] = Join(new byte[] { bytes[j], bytes[j + 1], bytes[j + 2], bytes[j + 3] });
            }

            for (var i = 5; i >= 1; --i)
            {
                settings[settings.Length - i] = bytes[bytes.Length - i];
            }

            return settings;
        }
    }
}
