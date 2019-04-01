using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperfastBlur;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Trippnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
            Console.ReadKey();
        }

        public void Run()
        {
            var Seed = Encoding.UTF8.GetBytes(string.Join("",
                Enumerable.Repeat(
                    string.Join("", DateTime.Now.Ticks.ToString().ToCharArray().Reverse().Take(5)).PadRight(5),
                    12800)));

            var Image = GenerateImage(new Size(1920, 1080), Seed);
            PrintLine($"INFO: File created: {Image.Split('\\').Last()}");
        }

        public static string GenerateImage(Size ImageSize, byte[] Seed, string ExportDirectory = null, long ImageQuality = 100, int BlurRadial = 0, int PixelSize = 5)
        {
            try
            {
                if (ExportDirectory == null)
                    ExportDirectory = StaticRooter("GeneratedImages");
                if (!Directory.Exists(ExportDirectory))
                    Directory.CreateDirectory(ExportDirectory);
                
                Image Render = null;
                using (Image RawImage = WriteImage(ImageSize, Seed, PixelSize))
                using (Render = new GaussianBlur(RawImage as Bitmap).Process(BlurRadial))
                {

                    var codecParams = new EncoderParameters(1);
                    codecParams.Param[0] = new EncoderParameter(Encoder.Quality, ImageQuality);

                    var FileName = StaticRooter($"{ExportDirectory}\\Trippnet.{DateTime.Now.Ticks}.jpg");
                    var xr = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.MimeType == "image/jpeg");
                    Render.Save(FileName, xr, codecParams);
                    return FileName;
                }
            }
            catch (Exception ex)
            {
                new Program().PrintLine($"ERROR: {ex.Message}");
            }

            return null;
        }

        #region Image Generation
        
        public static Image WriteImage(Size ImageSize, byte[] Seed, int PixelSize)
        {
            string[] DataColors = new string[] { "#ffa100", "#FF6600", "#FFCC00", "#CCFF00", "#fff2ff", "#00ddff", "#bd6dff", "#CC00FF" };
            string[] PaddingColors = { "#FF0066", "#FF0066" };
            List<Color> ColorList = new List<Color>();

            for (int ri = 0; ri < Seed.Length; ri++)
            {
                char[] CharList = Convert.ToString(System.Convert.ToInt32(Seed[ri]), 2).PadLeft(8, '0').ToArray();
                for (int i = 0; i < CharList.Length; i++)
                {
                    if (CharList[i] == '1')
                        ColorList.Add((Color)new ColorConverter().ConvertFromString(DataColors.Reverse().ToArray()[i].ToUpper()));
                }
                ColorList.Add((Color)new ColorConverter().ConvertFromString(PaddingColors[0].ToUpper()));
            }

            ColorList.Add((Color)new ColorConverter().ConvertFromString(PaddingColors[1].ToUpper()));

            Image AltImage = new Bitmap(ImageSize.Width, ImageSize.Height);
            Graphics graphics = Graphics.FromImage(AltImage);

            dypos = 0;
            dxpos = 0;
            for (int i = 0; i < ColorList.Count; i++)
            {
                Draw(new SolidBrush(ColorList[i]), ref graphics, AltImage.Size, PixelSize);
            }

            return AltImage;
        }
        
        public static int dxpos = 0;
        public static int dypos = 0;
        public static Color LastDrawnColor = Color.FromArgb(0, 0, 0, 0);

        public static void Draw(SolidBrush Color, ref Graphics graphics, Size ImageSize, int PixelSize)
        {
            int dpxs = PixelSize;

            if (dxpos + (dpxs) > ImageSize.Width)
            {
                dxpos = 0;
                dypos = dypos + dpxs;
            }
            if (Color.Color != LastDrawnColor || LastDrawnColor != System.Drawing.Color.FromArgb(0, 0, 0, 0))
                graphics.FillRectangle(Color, new Rectangle(new Point(dxpos, dypos), new Size(dpxs, dpxs)));

            dxpos += dpxs;
            LastDrawnColor = Color.Color;
        }


        public Image CropImage(Image Image)
        {
            Rectangle cropRect = new Rectangle(0, 0, (Image.Width / 2) + (Image.Width / 6), Image.Height - 1200);
            Bitmap src = Image as Bitmap;
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                    cropRect,
                    GraphicsUnit.Pixel);
            }

            return target;
        }

        #endregion
        #region Essentials
        public string LogPath = @"data\Logs.txt";
        public bool NoConsolePrint = false;
        public bool NoFilePrint = false;
        public void Print(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "")));
            if (!NoConsolePrint) Console.Write(Tag(String));
        }
        public void Print(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", ""))); if (!NoConsolePrint) Console.Write(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "")); if (!NoConsolePrint) Console.Write(String); }
        }
        public void PrintLine(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
            if (!NoConsolePrint) Console.WriteLine(Tag(String));
        }
        public void PrintLine(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
        }
        public void PrintLine()
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Environment.NewLine);
            if (!NoConsolePrint) Console.WriteLine();
        }
        public void PrintLines(string[] StringArray)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
                if (!NoConsolePrint) Console.WriteLine(Tag(String));
            }
        }
        public void PrintLines(string[] StringArray, bool DoTag)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
                else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
            }
        }
        public void Check()
        {
            if (!NoFilePrint && !System.IO.File.Exists(LogPath)) Touch(LogPath);
        }
        private bool WriteLock = false;
        public void WaitWrite(string Path, string Data)
        {
            while (WriteLock) { System.Threading.Thread.Sleep(20); }
            WriteLock = true;
            System.IO.File.AppendAllText(Path, Data);
            WriteLock = false;
        }
        public string[] ReadData(string DataDir)
        {
            if (System.IO.File.Exists(DataDir))
            {
                List<string> Data = System.IO.File.ReadAllLines(DataDir).ToList<string>();
                foreach (var Line in Data)
                {
                    if (Line == "\n" || Line == "\r" || Line == "\t" || string.IsNullOrWhiteSpace(Line))
                        Data.Remove(Line);
                }
                return Data.ToArray();
            }
            else
                return null;
        }
        public string ReadText(string TextDir)
        {
            if (System.IO.File.Exists(TextDir))
            {
                return System.IO.File.ReadAllText(TextDir);
            }
            return null;
        }
        public string SafeJoin(string[] Array)
        {
            if (Array != null && Array.Length != 0)
                return string.Join("\r\n", Array);
            else return "";
        }
        public void CleanLine()
        {
            Console.Write("\r");
            for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
            Console.Write("\r");
        }
        public void CleanLastLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            CleanLine();
        }
        public string Rooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public static string StaticRooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public string Tag(string Text)
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + Text;
        }
        public string Tag()
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] ";
        }
        public bool Touch(string Path)
        {
            try
            {
                System.Text.StringBuilder PathCheck = new System.Text.StringBuilder();
                string[] Direcories = Path.Split(System.IO.Path.DirectorySeparatorChar);
                foreach (var Directory in Direcories)
                {
                    PathCheck.Append(Directory);
                    string InnerPath = PathCheck.ToString();
                    if (System.IO.Path.HasExtension(InnerPath) == false)
                    {
                        PathCheck.Append("\\");
                        if (System.IO.Directory.Exists(InnerPath) == false) System.IO.Directory.CreateDirectory(InnerPath);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(InnerPath, "");
                    }
                }
                if (IsDirectory(Path) && System.IO.Directory.Exists(PathCheck.ToString())) { return true; }
                if (!IsDirectory(Path) && System.IO.File.Exists(PathCheck.ToString())) { return true; }
            }
            catch (Exception ex) { PrintLine("ERROR: Failed touching \"" + Path + "\". " + ex.Message, true); }
            return false;
        }
        public bool IsDirectory(string Path)
        {
            try
            {
                System.IO.FileAttributes attr = System.IO.File.GetAttributes(Path);
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                    return true;
                else
                    return false;
            }
            catch
            {
                if (System.IO.Path.HasExtension(Path)) return true;
                else return false;
            }
        }
        #endregion
    }
}
