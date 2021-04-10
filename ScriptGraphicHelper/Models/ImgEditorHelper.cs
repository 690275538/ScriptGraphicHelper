using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models
{
    public class ImgEditorHelper
    {
        public WriteableBitmap DrawBitmap { get; private set; }
        private int Width;
        private int Height;
        private int RowStride;
        private List<byte> Data = new();

        public ImgEditorHelper(Range range, byte[] data)
        {
            Width = (int)(range.Right - range.Left + 1);
            Height = (int)(range.Bottom - range.Top + 1);
            Data = data.ToList();
            DrawBitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            var drawBmpData = DrawBitmap.Lock();
            unsafe
            {
                var ptr = (byte*)drawBmpData.Address;
                for (int j = 0; j < Height; j++)
                {
                    int k = j * drawBmpData.RowBytes;
                    for (int i = 0; i < Width; i++, k += 4)
                    {
                        ptr[k] = data[k];
                        ptr[k + 1] = data[k + 1];
                        ptr[k + 2] = data[k + 2];
                        ptr[k + 3] = 255;
                    }
                }
            }
            RowStride = drawBmpData.RowBytes;
            drawBmpData.Dispose();
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                var drawBmpData = DrawBitmap.Lock();
                unsafe
                {
                    var ptr = (byte*)drawBmpData.Address;
                    int k = y * drawBmpData.RowBytes * x * 4;
                    ptr[k] = color.B;
                    ptr[k + 1] = color.G;
                    ptr[k + 2] = color.R;
                    ptr[k + 3] = 255;
                }
                drawBmpData.Dispose();
            }
        }

        public byte[] GetPixel(int x, int y)
        {
            var result = new byte[3];
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                var drawBmpData = DrawBitmap.Lock();
                unsafe
                {
                    var ptr = (byte*)drawBmpData.Address;
                    int k = y * drawBmpData.RowBytes * x * 4;
                    result[2] = ptr[k];
                    result[1] = ptr[k + 1];
                    result[0] = ptr[k + 2];
                }
                drawBmpData.Dispose();
            }
            return result;
        }

        public async void SetPixels(Color src, Color dest, int offset, bool reverse)
        {
            await Task.Run(() =>
            {
                if (reverse)
                {

                    byte srcR = src.R; byte srcG = src.G; byte srcB = src.B;
                    byte destR = dest.R; byte destG = dest.G; byte destB = dest.B;
                    int similarity = (int)(255 - 255 * (offset / 100.0));

                    int step = 0;
                    var drawBmpData = DrawBitmap.Lock();
                    unsafe
                    {
                        var ptr = (byte*)drawBmpData.Address;
                        for (int i = 0; i < Height; i++)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                if (Math.Abs(ptr[step] - srcB) > similarity && Math.Abs(ptr[step + 1] - srcG) > similarity && Math.Abs(ptr[step + 2] - srcR) > similarity)
                                {
                                    ptr[step] = destB;
                                    ptr[step + 1] = destG;
                                    ptr[step + 2] = destR;
                                }
                                step += 4;
                            }
                        }
                    }
                    drawBmpData.Dispose();
                }
                else
                {
                    byte srcR = src.R; byte srcG = src.G; byte srcB = src.B;
                    byte destR = dest.R; byte destG = dest.G; byte destB = dest.B;
                    int similarity = (int)(255 - 255 * ((100 - offset) / 100.0));

                    int step = 0;
                    var drawBmpData = DrawBitmap.Lock();
                    unsafe
                    {
                        var ptr = (byte*)drawBmpData.Address;
                        for (int i = 0; i < Height; i++)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                if (Math.Abs(ptr[step] - srcB) <= similarity && Math.Abs(ptr[step + 1] - srcG) <= similarity && Math.Abs(ptr[step + 2] - srcR) <= similarity)
                                {
                                    ptr[step] = destB;
                                    ptr[step + 1] = destG;
                                    ptr[step + 2] = destR;
                                }
                                step += 4;
                            }
                        }
                    }
                    drawBmpData.Dispose();
                }
            });
        }

        public WriteableBitmap CutImg()
        {
            var drawBmpData = DrawBitmap.Lock();
            unsafe
            {
                var ptr = (byte*)drawBmpData.Address;
                int site = 0;
                byte[] ltColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                site = (Width - 1) * 4;
                byte[] rtColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                site = (Height - 1) * RowStride;
                byte[] lbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };
                site = (Height - 1) * RowStride + (Width - 1) * 4;
                byte[] rbColor = new byte[] { ptr[site], ptr[site + 1], ptr[site + 2] };


                if (ltColor.SequenceEqual(rtColor) && ltColor.SequenceEqual(lbColor) && ltColor.SequenceEqual(rbColor))
                {
                    Range range = new(0, 0, Width - 1, Height - 1);
                    for (int i = 0; i < Height; i++)
                    {
                        int num = 0;
                        int location = i * RowStride;
                        for (int j = 0; j < Width; j++, location += 4)
                        {
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                        }
                        if (num != Width)
                        {
                            range.Top = i > 0 ? i - 1 : 0;
                            break;
                        }
                    }

                    for (int i = Height - 1; i >= 0; i--)
                    {
                        int num = 0;
                        int location = i * RowStride;
                        for (int j = 0; j < Width; j++, location += 4)
                        {
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                        }
                        if (num != Width)
                        {
                            range.Bottom = i < Height - 1 ? i + 1 : Height - 1;
                            break;
                        }
                    }

                    for (int i = 0; i < Width; i++)
                    {
                        int num = 0;
                        for (int j = 0; j < Height; j++)
                        {
                            int location = j * RowStride + i * 4;
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                            location += 4;
                        }
                        if (num != Height)
                        {
                            range.Left = i > 0 ? i - 1 : 0;
                            break;
                        }
                    }

                    for (int i = Width - 1; i >= 0; i--)
                    {
                        int num = 0;
                        for (int j = 0; j < Height; j++)
                        {
                            int location = j * RowStride + i * 4;
                            if (ptr[location] == ltColor[0] && ptr[location + 1] == ltColor[1] && ptr[location + 2] == ltColor[2])
                            {
                                num++;
                            }
                            location += 4;
                        }
                        if (num != Height)
                        {
                            range.Right = i < Width - 1 ? i + 1 : Width - 1;
                            break;
                        }
                    }
                    drawBmpData.Dispose();
                    return CutImg(range);
                }
                return DrawBitmap;
            }
        }

        public WriteableBitmap CutImg(Range range)
        {
            int left = (int)range.Left;
            int top = (int)range.Top;
            int right = (int)range.Right;
            int bottom = (int)range.Bottom;
            Width = right - left + 1;
            Height = bottom - top + 1;
            var bitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            var bmpData = bitmap.Lock();
            var drawBmpData = DrawBitmap.Lock();
            unsafe
            {
                var bmpPtr = (byte*)bmpData.Address;
                var rawBmpPtr = (byte*)drawBmpData.Address;

                int step = 0;
                for (int j = top; j <= bottom; j++)
                {
                    int k = j * drawBmpData.RowBytes + left * 4;
                    for (int i = left; i <= right; i++, k += 4, step += 4)
                    {
                        bmpPtr[step] = rawBmpPtr[k];
                        bmpPtr[step + 1] = rawBmpPtr[k + 1];
                        bmpPtr[step + 2] = rawBmpPtr[k + 2];
                        bmpPtr[step + 3] = 255;
                    }
                }
            }

            Data.Clear();
            byte[] data = new byte[Width * Height * 4];
            Marshal.Copy(bmpData.Address, data, 0, data.Length);
            Data = data.ToList();

            RowStride = bmpData.RowBytes;
            drawBmpData.Dispose();
            bmpData.Dispose();
            DrawBitmap = bitmap;
            return DrawBitmap;
        }

        public WriteableBitmap ResetImg()
        {
            DrawBitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            var drawBmpData = DrawBitmap.Lock();
            unsafe
            {
                var ptr = (byte*)drawBmpData.Address;
                for (int j = 0; j < Height; j++)
                {
                    int k = j * drawBmpData.RowBytes;
                    for (int i = 0; i < Width; i++, k += 4)
                    {
                        ptr[k] = Data[k];
                        ptr[k + 1] = Data[k + 1];
                        ptr[k + 2] = Data[k + 2];
                        ptr[k + 3] = 255;
                    }
                }
            }
            RowStride = drawBmpData.RowBytes;
            drawBmpData.Dispose();
            return DrawBitmap;
        }

    }
}
