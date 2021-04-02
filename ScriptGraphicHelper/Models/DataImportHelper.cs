using Avalonia;
using Avalonia.Media;
using ScriptGraphicHelper.Converters;
using System.Collections.ObjectModel;

namespace ScriptGraphicHelper.Models
{
    public static class DataImportHelper
    {
        public static ObservableCollection<ColorInfo> Import(string str)
        {
            str = str.Replace("\"", "");
            string[] strArray = str.Split(",");
            string[] info = strArray[0].Split('/');
            if (info.Length == 3)
            {
                return info[0] switch
                {
                    "dm" => DmFindStr(info, strArray),
                    "anjian" => AnjianFindStr(info, strArray),
                    "cd" => CdFindStr(info, strArray),
                    "autojs" => AutojsFindStr(info, str),
                    "ec" => EcFindStr(info, strArray),
                    _ => new ObservableCollection<ColorInfo>(),
                };
            }
            else if (str.IndexOf("none") != -1 || str.IndexOf("left") != -1 || str.IndexOf("center") != -1 || str.IndexOf("right") != -1)
            {
                return AnthorStr(str);
            }
            else if (info[0] == "autojs")
            {
                return AutojsCompareStr(str);
            }
            else if (info[0] == "anjian")
            {
                return AnjianCompareStr(str);
            }
            else
            {
                return CompareStr(str);
            }
        }

        private static ObservableCollection<ColorInfo> CompareStr(string str)
        {
            if (str.IndexOf("{{") != -1)
            {
                return CdCompareStr(str);
            }
            var colorInfos = new ObservableCollection<ColorInfo>();
            str = str.Replace("0x", "");
            var strArray = str.Split(",");
            foreach (var item in strArray)
            {
                var arr = item.Split("|");
                if (arr.Length == 3)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = new Point(int.Parse(arr[0]), int.Parse(arr[1])),
                        Color = Color.Parse("#" + arr[2]),
                    };

                    colorInfos.Add(colorInfo);
                }
            }
            return colorInfos;
        }

        private static ObservableCollection<ColorInfo> CdCompareStr(string str)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            str = str.Replace("0x", "#");
            var strArray = str.Split("},");
            foreach (var item in strArray)
            {
                var arr = item.Replace("{", "").Replace("}", "").Split(",");
                if (arr.Length == 3)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = new Point(int.Parse(arr[0]), int.Parse(arr[1])),
                        Color = Color.Parse(arr[2]),
                    };
                    colorInfos.Add(colorInfo);
                }
            }
            return colorInfos;
        }

        private static ObservableCollection<ColorInfo> AnjianCompareStr(string str)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            str = str.Replace("0x", "");
            var strArray = str.Split(",");
            foreach (var item in strArray)
            {
                var arr = item.Split("|");
                if (arr.Length == 3)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = new Point(int.Parse(arr[0]), int.Parse(arr[1])),
                        Color = Color.Parse("#" + arr[2][4] + arr[2][5] + arr[2][2] + arr[2][3] + arr[2][0] + arr[2][1]),
                    };
                    colorInfos.Add(colorInfo);
                }
            }
            return colorInfos;
        }

        private static ObservableCollection<ColorInfo> AutojsCompareStr(string str)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            var strArray = str.Split(",");
            var startPoint = new Point(int.Parse(strArray[1]), int.Parse(strArray[2]));
            var startColor = Color.Parse(strArray[3]);

            colorInfos.Add(new ColorInfo
            {
                Index = 0,
                Point = startPoint,
                Color = startColor
            });
            int startIndex = str.IndexOf("[[");
            int endIndex = str.IndexOf("]]", startIndex);
            string[] array = str.Substring(startIndex, endIndex - startIndex).Replace("[", "").Split("],");
            foreach (var item in array)
            {
                var arr = item.Split(",");
                var colorInfo = new ColorInfo
                {
                    Index = colorInfos.Count,
                    Point = new Point(startPoint.X + int.Parse(arr[0]), startPoint.Y + int.Parse(arr[1])),
                    Color = Color.Parse(arr[2])
                };
                colorInfos.Add(colorInfo);
            }
            return colorInfos;
        }

        private static ObservableCollection<ColorInfo> EcFindStr(string[] info, string[] strArray)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            var startPoint = new Point(int.Parse(info[1]), int.Parse(info[2]));
            for (int i = 1; i < strArray.Length; i++)
            {
                string item = strArray[i];
                string[] array = item.Split("|");
                if (item.Length == 8)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = startPoint,
                        Color = Color.Parse("#" + item.Replace("0x", ""))
                    };
                    colorInfos.Add(colorInfo);
                }
                else if (array.Length == 3)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = new Point(startPoint.X + int.Parse(array[0]), startPoint.Y + int.Parse(array[1])),
                        Color = Color.Parse("#" + array[2].Replace("0x", ""))
                    };
                    colorInfos.Add(colorInfo);
                }
            }
            return colorInfos;
        }

        private static ObservableCollection<ColorInfo> AutojsFindStr(string[] info, string str)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            var startPoint = new Point(int.Parse(info[1]), int.Parse(info[2]));

            var startColor = Color.Parse(str.Split(",")[1]);
            colorInfos.Add(new ColorInfo
            {
                Index = 0,
                Point = startPoint,
                Color = startColor
            });
            int startIndex = str.IndexOf("[[");
            int endIndex = str.IndexOf("]]", startIndex);
            string[] array = str.Substring(startIndex, endIndex - startIndex).Replace("[", "").Split("],");
            foreach (var item in array)
            {
                var arr = item.Split(",");
                var colorInfo = new ColorInfo
                {
                    Index = colorInfos.Count,
                    Point = new Point(startPoint.X + int.Parse(arr[0]), startPoint.Y + int.Parse(arr[1])),
                    Color = Color.Parse(arr[2])
                };
                colorInfos.Add(colorInfo);
            }
            return colorInfos;
        }

        private static ObservableCollection<ColorInfo> CdFindStr(string[] info, string[] strArray)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            var startPoint = new Point(int.Parse(info[1]), int.Parse(info[2]));
            for (int i = 1; i < strArray.Length; i++)
            {
                string item = strArray[i];
                string[] array = item.Split("|");
                if (item.Length == 8)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = startPoint,
                        Color = Color.Parse("#" + item.Replace("0x", ""))
                    };
                    colorInfos.Add(colorInfo);
                }
                else if (array.Length == 3)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = new Point(startPoint.X + int.Parse(array[0]), startPoint.Y + int.Parse(array[1])),
                        Color = Color.Parse("#" + array[2].Replace("0x", ""))
                    };
                    colorInfos.Add(colorInfo);
                }
            }
            return colorInfos;
        }

        private static ObservableCollection<ColorInfo> AnjianFindStr(string[] info, string[] strArray)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            var startPoint = new Point(int.Parse(info[1]), int.Parse(info[2]));
            for (int i = 1; i < strArray.Length; i++)
            {
                string item = strArray[i];
                string[] array = item.Split("|");
                if (item.Length == 6)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = startPoint,
                        Color = Color.Parse("#" + item[4] + item[5] + item[2] + item[3] + item[0] + item[1])
                    };
                    colorInfos.Add(colorInfo);
                }
                else if (array.Length == 3)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = new Point(startPoint.X + int.Parse(array[0]), startPoint.Y + int.Parse(array[1])),
                        Color = Color.Parse("#" + array[2][4] + array[2][5] + array[2][2] + array[2][3] + array[2][0] + array[2][1])
                    };
                    colorInfos.Add(colorInfo);
                }
            }
            return colorInfos;
        }

        public static ObservableCollection<ColorInfo> DmFindStr(string[] info, string[] strArray)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            var startPoint = new Point(int.Parse(info[1]), int.Parse(info[2]));
            for (int i = 1; i < strArray.Length; i++)
            {
                string item = strArray[i];
                string[] array = item.Split("|");
                if (item.Length == 6)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Point = startPoint,
                        Color = Color.Parse("#" + item)
                    };
                    colorInfos.Add(colorInfo);
                }
                else if (array.Length == 3)
                {
                    var colorInfo = new ColorInfo
                    {
                        Index = colorInfos.Count,
                        Anchor = AnchorType.None,
                        Point = new Point(startPoint.X + int.Parse(array[0]), startPoint.Y + int.Parse(array[1])),
                        Color = Color.Parse("#" + array[2])
                    };
                    colorInfos.Add(colorInfo);
                }
            }
            return colorInfos;
        }

        public static ObservableCollection<ColorInfo> AnthorStr(string str)
        {
            var colorInfos = new ObservableCollection<ColorInfo>();
            var strArray = str.Split(",");
            var width = int.Parse(strArray[0].Trim().Trim('['));
            var height = int.Parse(strArray[1].Trim());
            ColorInfo.Width = width;
            ColorInfo.Height = height;

            int startIndex = str.IndexOf("[[");
            int endIndex = str.IndexOf("]]", startIndex);
            string[] array = str.Substring(startIndex, endIndex - startIndex).Replace("\r\n", "").Replace("[", "").Split("],");
            foreach (var item in array)
            {
                var arr = item.Split(",");
                var colorInfo = new ColorInfo();
                switch (arr[0])
                {
                    case "left": colorInfo.Anchor = AnchorType.Left; break;
                    case "center": colorInfo.Anchor = AnchorType.Center; break;
                    case "right": colorInfo.Anchor = AnchorType.Right; break;
                    default: colorInfo.Anchor = AnchorType.None; break;
                }
                colorInfo.Index = colorInfos.Count;
                colorInfo.Point = new Point(int.Parse(arr[1]), int.Parse(arr[2]));
                colorInfo.Color = Color.Parse(arr[3].Replace("0x", "#"));
                colorInfos.Add(colorInfo);
            }
            return colorInfos;
        }

    }
}
