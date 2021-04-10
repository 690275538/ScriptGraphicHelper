using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Point = Avalonia.Point;

namespace ScriptGraphicHelper.Models
{
    public struct CompareResult
    {
        public bool Result { get; set; }
        public string ErrorMessage { get; set; }

        public CompareResult(bool result)
        {
            Result = result;
            ErrorMessage = string.Empty;
        }

        public CompareResult(bool result, string message)
        {
            Result = result;
            ErrorMessage = message;
        }
    }

    public static class GraphicHelper
    {
        public static int Width { get; set; } = 0;
        public static int Height { get; set; } = 0;
        public static int FormatSize { get; set; }
        public static int RowStride { get; set; }
        public static byte[] ScreenData { get; set; }

        public static void KeepScreen(SKBitmap bitmap)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;
            ScreenData = new byte[bitmap.RowBytes * Height];
            RowStride = bitmap.RowBytes;
            FormatSize = bitmap.RowBytes / Width;
            Marshal.Copy(bitmap.GetPixels(), ScreenData, 0, ScreenData.Length);
        }

        public static byte[] GetRectData(Range range)
        {

            int sx = (int)range.Left;
            int sy = (int)range.Top;
            int ex = (int)range.Right;
            int ey = (int)range.Bottom;
            int width = ex - sx + 1;
            int height = ey - sy + 1;
            byte[] data = new byte[width * height * 4];
            int site = 0;
            for (int i = sy; i <= ey; i++)
            {
                int location = sx * 4 + Width * 4 * i;
                for (int j = sx; j <= ex; j++)
                {
                    data[site] = ScreenData[location];
                    data[site + 1] = ScreenData[location + 1];
                    data[site + 2] = ScreenData[location + 2];
                    data[site + 3] = ScreenData[location + 3];
                    location += 4;
                    site += 4;
                }
            }
            return data;
        }

        public static async Task<Bitmap> TurnRight()
        {
            var task = Task.Run(() =>
            {
                byte[] data = new byte[RowStride * Height];
                int step = 0;
                for (int j = 0; j < Width; j++)
                {
                    for (int i = Height - 1; i >= 0; i--)
                    {
                        int location = j * FormatSize + i * RowStride;
                        data[step] = ScreenData[location];
                        data[step + 1] = ScreenData[location + 1];
                        data[step + 2] = ScreenData[location + 2];
                        data[step + 3] = 255;
                        step += 4;
                    }
                }
                SKBitmap sKBitmap = new(new SKImageInfo(Height, Width));
                Marshal.Copy(data, 0, sKBitmap.GetPixels(), data.Length);
                KeepScreen(sKBitmap);
                var bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, sKBitmap.GetPixels(), new PixelSize(Width, Height), new Vector(96, 96), sKBitmap.RowBytes);
                sKBitmap.Dispose();
                return bitmap;
            });
            return await task;
        }

        public static byte[] GetPixel(int x, int y)
        {
            byte[] retRGB = new byte[] { 0, 0, 0 };
            try
            {
                if (x < Width && y < Height)
                {
                    int location = x * FormatSize + y * RowStride;
                    retRGB[0] = ScreenData[location + 2];
                    retRGB[1] = ScreenData[location + 1];
                    retRGB[2] = ScreenData[location];
                }
            }
            catch
            {
                retRGB = new byte[] { 0, 0, 0 };
            }
            return retRGB;
        }

        public static CompareResult AnchorsCompareColor(double width, double height, string colorString, int sim = 95)
        {
            string[] compareColorArr = colorString.Trim('"').Split(',');

            double multiple = Height / height;
            string result = string.Empty;
            for (int i = 0; i < compareColorArr.Length; i++)
            {
                string[] compareColor = compareColorArr[i].Split('|');
                double findX = int.Parse(compareColor[1]);
                double findY = int.Parse(compareColor[2]);
                if (compareColor[0] == "Left" || compareColor[0] == "None")
                {
                    findX = Math.Floor(findX * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                else if (compareColor[0] == "Center")
                {
                    findX = Math.Floor(Width / 2 - 1 - (width / 2 - findX - 1) * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                else if (compareColor[0] == "Right")
                {
                    findX = Math.Floor(Width - 1 - (width - findX - 1) * multiple);
                    findY = Math.Floor(findY * multiple);
                }
                result += findX.ToString() + "|" + findY.ToString() + "|" + compareColor[3] + ",";
            }
            result = result.Trim(',');
            return CompareColorEx(result, sim);
        }

        public static Point AnchorsFindColor(Range rect, double width, double height, string colorString, int sim = 95)
        {
            string compareColorStr = colorString.Trim('"');
            string[] compareColorArr = compareColorStr.Split(',');
            if (compareColorArr.Length < 2)
            {
                return new Point(-1, -1);
            }
            double multiple = Height / height;
            string[] startColorArr = compareColorArr[0].Split('|');
            double x = int.Parse(startColorArr[1]);
            double y = int.Parse(startColorArr[2]);
            double startX = -1;
            double startY = -1;
            if (startColorArr[0] == "Left" || startColorArr[0] == "None")
            {
                startX = Math.Floor(x * multiple);
                startY = Math.Floor(y * multiple);
            }
            else if (startColorArr[0] == "Center")
            {
                startX = Math.Floor(Width / 2 - 1 - (width / 2 - x - 1) * multiple);
                startY = Math.Floor(y * multiple);
            }
            else if (startColorArr[0] == "Right")
            {
                startX = Math.Floor(Width - 1 - (width - x - 1) * multiple);
                startY = Math.Floor(y * multiple);
            }

            string result = string.Empty;
            for (int i = 1; i < compareColorArr.Length; i++)
            {
                string[] compareColor = compareColorArr[i].Split('|');
                double findX = int.Parse(compareColor[1]);
                double findY = int.Parse(compareColor[2]);
                if (compareColor[0] == "Left" || compareColor[0] == "None")
                {
                    findX = Math.Floor(findX * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                else if (compareColor[0] == "Center")
                {
                    findX = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - findX) * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                else if (compareColor[0] == "Right")
                {
                    findX = Math.Floor(Width - 1 - (width - findX - 1) * multiple) - startX;
                    findY = Math.Floor(findY * multiple) - startY;
                }
                result += findX.ToString() + "|" + findY.ToString() + "|" + compareColor[3] + ",";
            }
            result = result.Trim(',');

            if (rect.Mode_1 == 0 || rect.Mode_1 == -1)
            {
                rect.Left = Math.Floor(rect.Left * multiple);
            }
            else if (rect.Mode_1 == 1)
            {
                rect.Left = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - rect.Left) * multiple);
            }
            else if (rect.Mode_1 == 2)
            {
                rect.Left = Math.Floor(Width - 1 - (width - rect.Left - 1) * multiple);
            }
            if (rect.Mode_2 == 0 || rect.Mode_2 == -1)
            {
                rect.Right = Math.Floor(rect.Right * multiple);
            }
            else if (rect.Mode_2 == 1)
            {
                rect.Right = Math.Floor(Width / 2 - 1 - (width / 2 - 1 - rect.Right) * multiple);
            }
            else if (rect.Mode_2 == 2)
            {
                rect.Right = Math.Floor(Width - 1 - (width - rect.Right - 1) * multiple);
            }
            rect.Top = Math.Floor(rect.Top * multiple);
            rect.Bottom = Math.Floor(rect.Bottom * multiple);
            return FindMultiColor((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, startColorArr[3], result, sim);
        }

        public static bool CompareColor(byte[] rgb, double similarity, int x, int y, int offset)
        {
            int offsetSize = offset == 0 ? 1 : 9;
            Point[] offsetPoint = new Point[]{
                new Point(x, y),
                new Point(x - 1, y - 1),
                new Point(x - 1, y),
                new Point(x - 1, y + 1),
                new Point(x, y - 1),
                new Point(x, y + 1),
                new Point(x + 1, y - 1),
                new Point(x + 1, y),
                new Point(x + 1, y + 1),
            };

            for (int j = 0; j < offsetSize; j++)
            {
                int _x = (int)offsetPoint[j].X;
                int _y = (int)offsetPoint[j].Y;
                if (_x >= 0 && _x < Width && _y >= 0 && _y < Height)
                {
                    byte[] GetRGB = GetPixel(_x, _y);
                    if (Math.Abs(GetRGB[0] - rgb[0]) <= similarity && Math.Abs(GetRGB[1] - rgb[1]) <= similarity && Math.Abs(GetRGB[2] - rgb[2]) <= similarity)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static CompareResult CompareColorEx(string colorString, int sim = 95, int x = 0, int y = 0)
        {
            int findX;
            int findY;

            int offset = Setting.Instance.IsOffset ? 1 : 0;
            if (sim == 0)
            {
                sim = Setting.Instance.DiySim;
            }

            double similarity = 255 - 255 * (sim / 100.0);
            colorString = colorString.Trim("\"".ToCharArray());
            string[] findColors = colorString.Split(',');
            if (findColors.Length != 0)
            {
                for (byte i = 0; i < findColors.Length; i++)
                {
                    string[] findColor = findColors[i].Split('|');
                    byte[] findRGB = { 0, 0, 0 };
                    findRGB[0] = Convert.ToByte(findColor[2].Substring(0, 2), 16);
                    findRGB[1] = Convert.ToByte(findColor[2].Substring(2, 2), 16);
                    findRGB[2] = Convert.ToByte(findColor[2].Substring(4, 2), 16);

                    findX = x + int.Parse(findColor[0]);
                    findY = y + int.Parse(findColor[1]);
                    if (findX < 0 || findY < 0 || findX > Width || findY > Height)
                    {
                        return new CompareResult(false, string.Format("坐标越界:  index = {0}, x = {1}, y = {2}, color = 0x{3}", i, findX, findY, findColor[2]));
                    }

                    if (!CompareColor(findRGB, similarity, findX, findY, offset))
                    {
                        return new CompareResult(false, string.Format("return false:  index = {0}, x = {1}, y = {2}, color = 0x{3}", i, findX, findY, findColor[2]));
                    }
                }
            }
            return new CompareResult(true);
        }

        public static Point FindMultiColor(int startX, int startY, int endX, int endY, string findcolorString, string compareColorString, int sim = 95)
        {
            startX = Math.Max(startX, 0);
            startY = Math.Max(startY, 0);
            endX = Math.Min(endX, Width - 1);
            endY = Math.Min(endY, Height - 1);

            if (sim == 0)
            {
                sim = Setting.Instance.DiySim;
            }

            double similarity = 255 - 255 * (sim / 100.0);
            string[] findColor = findcolorString.Split('-');
            byte findR = Convert.ToByte(findColor[0].Substring(0, 2), 16);
            byte findG = Convert.ToByte(findColor[0].Substring(2, 2), 16);
            byte findB = Convert.ToByte(findColor[0].Substring(4, 2), 16);

            for (int i = startY; i <= endY; i++)
            {
                int location = startX * FormatSize + RowStride * i;
                for (int j = startX; j <= endX; j++)
                {
                    if (Math.Abs(ScreenData[location + 2] - findR) <= similarity)
                    {
                        if (Math.Abs(ScreenData[location + 1] - findG) <= similarity)
                        {
                            if (Math.Abs(ScreenData[location] - findB) <= similarity)
                            {
                                var compareResult = CompareColorEx(compareColorString, sim, j, i);
                                if (compareResult.Result)
                                {
                                    return new Point(j, i);
                                }
                            }
                        }
                    }
                    location += FormatSize;
                }
            }
            return new Point(-1, -1);
        }
    }
}
