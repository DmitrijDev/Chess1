
namespace Chess
{
    internal static class SettingsSaver
    {
        private readonly static string _fileName = "UserSettings.bin";

        private static IEnumerable<byte> Split(int number)
        {
            var remained = number;

            var byte3 = (byte)(remained % (byte.MaxValue + 1));
            remained /= byte.MaxValue + 1;

            var byte2 = (byte)(remained % (byte.MaxValue + 1));
            remained /= byte.MaxValue + 1;

            var byte1 = (byte)(remained % (byte.MaxValue + 1));
            remained /= byte.MaxValue + 1;

            yield return (byte)remained;
            yield return byte1;
            yield return byte2;
            yield return byte3;
        }

        private static int Join(params byte[] bytes)
        {
            var result = 0;

            foreach (var b in bytes.Take(4))
            {
                result *= byte.MaxValue + 1;
                result += b;
            }

            return result;
        }

        private static IEnumerable<byte> GetSettingsBytes(this GameForm form)
        {
            yield return (byte)form.WindowState;
            yield return form.GamePanel.IsReversed ? (byte)1 : (byte)0;
            yield return (byte)form.ColorsMenu.SelectedItemIndex;
            yield return (byte)form.WhitePlayerMenu.SelectedItemIndex;
            yield return (byte)form.BlackPlayerMenu.SelectedItemIndex;
            yield return (byte)form.TimeMenu.SelectedItemIndex;

            var numbers = new int[] { form.Location.X, form.Location.Y, form.Width, form.Height, form.MinimumSize.Width,
            form.MinimumSize.Height , form.GamePanel.Location.X, form.GamePanel.Location.Y, form.GamePanel.ButtonSize };

            foreach (var n in numbers)
            {
                foreach (var b in Split(n))
                {
                    yield return b;
                }
            }
        }

        public static void SaveSetting(this GameForm form)
        {
            using (var writer = new BinaryWriter(new FileStream(_fileName, FileMode.Create, FileAccess.Write)))
            {
                var bytes = form.GetSettingsBytes().ToArray();
                writer.Write(bytes);
            }
        }

        public static FormSetting LoadSetting()
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

            var ints = new int[9];

            for (int i = 0, j = 6; i < 9; ++i, j += 4)
            {
                ints[i] = Join(bytes[j], bytes[j + 1], bytes[j + 2], bytes[j + 3]);
            }

            var setting = new FormSetting()
            {
                WindowState = (FormWindowState)bytes[0],
                BoardIsReversed = bytes[1] == 1,
                ColorSetIndex = bytes[2],

                ProgramPlaysForWhite = bytes[3] == 1,
                ProgramPlaysForBlack = bytes[4] == 1,

                TimeMenuSelectedItemIndex = bytes[5],

                FormX = ints[0],
                FormY = ints[1],

                FormWidth = ints[2],
                FormHeight = ints[3],

                FormMinWidth = ints[4],
                FormMinHeight = ints[5],

                BoardX = ints[6],
                BoardY = ints[7],
                ButtonSize = ints[8]
            };

            return setting;
        }
    }
}
