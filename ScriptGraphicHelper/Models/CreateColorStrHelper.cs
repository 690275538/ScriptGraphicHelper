using Avalonia;
using Newtonsoft.Json;
using ScriptGraphicHelper.Converters;
using System.Collections.ObjectModel;
using System.IO;

namespace ScriptGraphicHelper.Models
{
    public enum FormatMode
    {
        compareStr = 0,
        dmFindStr = 1,
        anjianFindStr = 2,
        anjianCompareStr = 3,
        cdFindStr = 4,
        cdCompareStr = 5,
        autojsFindStr = 6,
        autojsCompareStr = 7,
        ecFindStr = 8,
        ecCompareStr = 9,
        diyFindStr = 10,
        diyCompareStr = 11,
        anchorsFindStr = 12,
        anchorsCompareStr = 13,
        anchorsFindStr4Test = 14,
        anchorsCompareStr4Test = 15,
        findStr4Test = 16

    };
    public static class CreateColorStrHelper
    {
        public static string Create(FormatMode mode, ObservableCollection<ColorInfo> colorInfos, Range rect = null)
        {
            return mode switch
            {
                FormatMode.compareStr => CompareStr(colorInfos),
                FormatMode.dmFindStr => DmFindStr(colorInfos, rect),
                FormatMode.anjianFindStr => AnjianFindStr(colorInfos, rect),
                FormatMode.anjianCompareStr => AnjianCompareStr(colorInfos),
                FormatMode.cdFindStr => CdFindStr(colorInfos, rect),
                FormatMode.cdCompareStr => CdCompareStr(colorInfos),
                FormatMode.autojsFindStr => AutojsFindStr(colorInfos, rect),
                FormatMode.autojsCompareStr => AutojsCompareStr(colorInfos, rect),
                FormatMode.ecFindStr => EcFindStr(colorInfos, rect),
                FormatMode.ecCompareStr => EcCompareStr(colorInfos, rect),
                FormatMode.diyFindStr => DiyFindStr(colorInfos, rect),
                FormatMode.diyCompareStr => DiyCompareStr(colorInfos),
                FormatMode.anchorsCompareStr => AnchorsCompareStr(colorInfos),
                FormatMode.anchorsFindStr => AnchorsFindStr(colorInfos, rect),
                FormatMode.anchorsCompareStr4Test => AnchorsCompareStr4Test(colorInfos),
                FormatMode.anchorsFindStr4Test => AnchorsCompareStr4Test(colorInfos),
                FormatMode.findStr4Test => FindStr4Test(colorInfos, rect),
                _ => CompareStr(colorInfos),
            };
        }

        public static DiyFormat GetDiyFormat()
        {
            StreamReader sr = File.OpenText(System.AppDomain.CurrentDomain.BaseDirectory + @"Assets\diyFormat.json");
            string result = sr.ReadToEnd();
            sr.Close();
            return JsonConvert.DeserializeObject<DiyFormat>(result);
        }

        public static string DiyCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            DiyFormat diyFormat = GetDiyFormat();
            string colorStr = string.Empty;
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    string res = diyFormat.FollowColorFormat;
                    string color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                    if (res.IndexOf("{x}") != -1)
                    {
                        res = res.Replace("{x}", colorInfo.Point.X.ToString());
                    }
                    if (res.IndexOf("{y}") != -1)
                    {
                        res = res.Replace("{y}", colorInfo.Point.Y.ToString());
                    }
                    if (res.IndexOf("{color}") != -1)
                    {
                        res = res.Replace("{color}", color);
                    }
                    colorStr += res + ",";
                }
            }
            colorStr = colorStr.Trim(',');
            string result = diyFormat.CompareStrFormat;
            if (result.IndexOf("{ImportInfo}") != -1)
            {
                string info = diyFormat.ImportInfo;
                if (info != string.Empty && info != "")
                {
                    info = string.Format("\"{0}\"", info);
                }
                result = result.Replace("{ImportInfo}", info);
            }
            if (result.IndexOf("{colorStr}") != -1)
            {
                result = result.Replace("{colorStr}", colorStr);
            }
            return result;
        }

        public static string DiyFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            DiyFormat diyFormat = GetDiyFormat();
            bool isInit = false;
            Point startPoint = new Point();
            string[] colorStr = new string[2];
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.Point;
                        string res = diyFormat.FirstColorFormat;
                        string color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                        if (res.IndexOf("{color}") != -1)
                        {
                            res = res.Replace("{color}", color);
                        }
                        colorStr[0] = res;
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - startPoint.X;
                        double OffsetY = colorInfo.Point.Y - startPoint.Y;

                        string res = diyFormat.FollowColorFormat;
                        string color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                        if (res.IndexOf("{x}") != -1)
                        {
                            res = res.Replace("{x}", OffsetX.ToString());
                        }
                        if (res.IndexOf("{y}") != -1)
                        {
                            res = res.Replace("{y}", OffsetY.ToString());
                        }
                        if (res.IndexOf("{color}") != -1)
                        {
                            res = res.Replace("{color}", color);
                        }
                        colorStr[1] += res + ",";
                    }
                }
            }
            colorStr[1] = colorStr[1].Trim(',') ?? string.Empty;
            string result = diyFormat.FindStrFormat;
            if (result.IndexOf("{ImportInfo}") != -1)
            {
                string info = diyFormat.ImportInfo;
                if (info != string.Empty && info != "")
                {
                    info = string.Format("\"{0}/{1}/{2}\"", info, startPoint.X, startPoint.Y);
                }
                result = result.Replace("{ImportInfo}", info);
            }
            if (result.IndexOf("{range}") != -1)
            {
                string range = diyFormat.RangeFormat;
                if (range.IndexOf("{startX}") != -1)
                {
                    range = range.Replace("{startX}", rect.Left.ToString());
                }
                if (range.IndexOf("{startY}") != -1)
                {
                    range = range.Replace("{startY}", rect.Top.ToString());
                }
                if (range.IndexOf("{endX}") != -1)
                {
                    range = range.Replace("{endX}", rect.Right.ToString());
                }
                if (range.IndexOf("{endY}") != -1)
                {
                    range = range.Replace("{endY}", rect.Bottom.ToString());
                }
                if (range.IndexOf("{width}") != -1)
                {
                    range = range.Replace("{width}", (rect.Right - rect.Left).ToString());
                }
                if (range.IndexOf("{height}") != -1)
                {
                    range = range.Replace("{height}", (rect.Bottom - rect.Top).ToString());
                }
                result = result.Replace("{range}", range);
            }
            if (result.IndexOf("{firstColorStr}") != -1)
            {
                result = result.Replace("{firstColorStr}", colorStr[0]);
            }
            if (result.IndexOf("{followColorStr}") != -1)
            {
                result = result.Replace("{followColorStr}", colorStr[1]);
            }
            return result;
        }

        public static string DmFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;

            bool inited = false;
            Point firstPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!inited)
                    {
                        inited = true;
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "dm", firstPoint.X, firstPoint.Y);
                        }
                        if (Setting.Instance.AddRange)
                        {
                            result += rect.ToString() + ",";
                        }
                        result += "\"" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - firstPoint.X;
                        double OffsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";

                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        public static string CdFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            Point firstPoint = new();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "cd", firstPoint.X, firstPoint.Y);
                        }
                        result += "0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - firstPoint.X;
                        double OffsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (Setting.Instance.AddRange)
            {
                result += "\",90," + rect.ToString();
            }
            else
            {
                result += "\"";
            }
            return result;
        }

        public static string AnjianFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            bool inited = false;
            Point firstPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!inited)
                    {
                        inited = true;
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "anjian", firstPoint.X, firstPoint.Y);
                        }
                        if (Setting.Instance.AddRange)
                        {
                            result += rect.ToString() + ",";
                        }
                        result += "\"" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - firstPoint.X;
                        double OffsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.R.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        public static string AutojsFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            Point firstPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "autojs", firstPoint.X, firstPoint.Y);
                        }
                        result += "\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",[";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - firstPoint.X;
                        double OffsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += "[" + OffsetX.ToString() + "," + OffsetY.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                            colorInfo.Color.B.ToString("x2") + "\"],";
                    }
                }
            }
            result = result.Trim(',');
            if (Setting.Instance.AddRange)
            {
                result += "],{region:[" + rect.ToString(1) + "],threshold:[26]}";
            }
            else
            {
                result += "]";
            }
            return result;
        }

        public static string EcFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            Point firstPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}/{1}/{2}\",", "ec", firstPoint.X, firstPoint.Y);
                        }
                        result += "\"0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - firstPoint.X;
                        double OffsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (Setting.Instance.AddRange)
            {
                result += "\",0.9," + rect.ToString();
            }
            else
            {
                result += "\"";
            }

            return result;
        }

        public static string CompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "\"";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        public static string AnjianCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "\"";
            if (Setting.Instance.AddInfo)
            {
                result += string.Format("{0}\",", "anjian");
            }
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        public static string CdCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "{";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += "{" + colorInfo.Point.X.ToString() + "," + colorInfo.Point.Y.ToString() + ",0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "},";
                }
            }
            result = result.Trim(',');
            result += "}";
            return result;
        }

        private static string AutojsCompareStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            Point firstPoint = new();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddInfo)
                        {
                            result += string.Format("\"{0}\",", "autojs");
                        }
                        result += firstPoint.X.ToString() + "," + firstPoint.Y.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",[";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - firstPoint.X;
                        double OffsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += "[" + OffsetX.ToString() + "," + OffsetY.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                            colorInfo.Color.B.ToString("x2") + "\"],";
                    }
                }
            }
            result = result.Trim(',');
            result += "]";
            return result;
        }

        private static string EcCompareStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = "\"";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        public static string AnchorsCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "[" + ColorInfo.Width.ToString() + "," + ColorInfo.Height.ToString() + ",\r\n[";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.Anchor == AnchorType.Left)
                        result += "[left,";
                    else if (colorInfo.Anchor == AnchorType.Center)
                        result += "[center,";
                    else if (colorInfo.Anchor == AnchorType.Right)
                        result += "[right,";
                    else
                        result += "[none,";

                    result += colorInfo.Point.X.ToString() + "," + colorInfo.Point.Y.ToString() + ",0x" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "],\r\n";
                }
            }
            result = result.Trim(",\r\n".ToCharArray());
            result += "]\r\n]";
            return result;
        }

        public static string AnchorsFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = "[" + ColorInfo.Width.ToString() + "," + ColorInfo.Height.ToString();
            if (Setting.Instance.AddRange)
            {
                result += string.Format(",\r\n[{0}],\r\n[", rect.ToString(2));
            }
            else
            {
                result += ",\r\n[";
            }
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (colorInfo.Anchor == AnchorType.Left)
                        result += "[left,";
                    else if (colorInfo.Anchor == AnchorType.Center)
                        result += "[center,";
                    else if (colorInfo.Anchor == AnchorType.Right)
                        result += "[right,";
                    else
                        result += "[none,";

                    result += colorInfo.Point.X.ToString() + "," + colorInfo.Point.Y.ToString() + ",0x" + colorInfo.Color.R.ToString("x2") +
                    colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "],\r\n";
                }
            }
            result = result.Trim(",\r\n".ToCharArray());
            result += "]\r\n]";
            return result;
        }

        public static string AnchorsCompareStr4Test(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "\"";
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += colorInfo.Anchor.ToString() + "|" + colorInfo.Point.X.ToString() + "|" + colorInfo.Point.Y.ToString() + "|" + colorInfo.Color.R.ToString("x2") +
                        colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }


        public static string FindStr4Test(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;

            bool inited = false;
            Point firstPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!inited)
                    {
                        inited = true;
                        firstPoint = colorInfo.Point;
                        if (Setting.Instance.AddRange)
                        {
                            result += rect.ToString() + ",";
                        }
                        result += "\"" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - firstPoint.X;
                        double OffsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";

                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }
    }
}
