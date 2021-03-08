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
        ajFindStr = 2,
        ajCompareStr = 3,
        cdFindStr = 4,
        cdCompareStr = 5,
        autojsFindStr = 6,
        ecFindStr = 7,
        diyFindStr = 8,
        diyCompareStr = 9,
        anchorsFindStr = 10,
        anchorsCompareStr = 11,
        anchorsFindStrTest = 12,
        anchorsCompareStrTest = 13,

    };
    public static class CreateColorStrHelper
    {
        public static bool IsAddRange { get; set; } = false;
        public static string Create(FormatMode mode, ObservableCollection<ColorInfo> colorInfos, Range rect = null)
        {
            IsAddRange = PubSetting.Setting.Addrange;
            return mode switch
            {
                FormatMode.compareStr => CompareStr(colorInfos),
                FormatMode.dmFindStr => DmFindStr(colorInfos, rect),
                FormatMode.ajFindStr => AjFindStr(colorInfos, rect),
                FormatMode.ajCompareStr => AjCompareStr(colorInfos),
                FormatMode.cdFindStr => CdFindStr(colorInfos, rect),
                FormatMode.cdCompareStr => CdCompareStr(colorInfos),
                FormatMode.autojsFindStr => AutojsFindStr(colorInfos, rect),
                FormatMode.ecFindStr => EcFindStr(colorInfos, rect),
                FormatMode.diyFindStr => DiyFindStr(colorInfos, rect),
                FormatMode.diyCompareStr => DiyCompareStr(colorInfos),
                FormatMode.anchorsCompareStr => AnchorsCompareStr(colorInfos),
                FormatMode.anchorsFindStr => AnchorsFindStr(colorInfos, rect),
                FormatMode.anchorsCompareStrTest => AnchorsCompareStrTest(colorInfos),
                FormatMode.anchorsFindStrTest => AnchorsCompareStrTest(colorInfos),
                _ => CompareStr(colorInfos),
            };
        }
        public static DiyFormat GetDiyFormat()
        {
            StreamReader sr = File.OpenText(System.AppDomain.CurrentDomain.BaseDirectory + "diyFormat.json");
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

                    string endstr = res[(res.IndexOf("{color}") + 7)..];
                    res = res.Substring(0, res.IndexOf(color) + 6) + endstr;

                    colorStr += res + ",";

                }
            }
            colorStr = colorStr.Trim(',');
            string result = diyFormat.CompareStrFormat;

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

                        string endstr = res[(res.IndexOf("{color}") + 7)..];
                        res = res.Substring(0, res.IndexOf(color) + 6) + endstr;
                        colorStr[0] += res;

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

                        string endstr = res[(res.IndexOf("{color}") + 7)..];
                        res = res.Substring(0, res.IndexOf(color) + 6) + endstr;
                        colorStr[1] += res + ",";
                    }
                }
            }
            colorStr[1] = colorStr[1].Trim(',');
            string result = diyFormat.FindStrFormat;
            if (result.IndexOf("{range}") != -1)
            {
                result = result.Replace("{range}", rect.ToString());
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
            if (IsAddRange)
            {
                result = rect.ToString() + ",";
            }
            bool isInit = false;
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.Point;
                        result += "\"" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - startPoint.X;
                        double OffsetY = colorInfo.Point.Y - startPoint.Y;
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
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        startPoint = colorInfo.Point;
                        result += "0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + ",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - startPoint.X;
                        double OffsetY = colorInfo.Point.Y - startPoint.Y;
                        result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (IsAddRange)
            {
                result += "\",90," + rect.ToString();
            }
            else
            {
                result += "\"";
            }
            return result;
        }
        public static string AjFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            string result = string.Empty;
            if (IsAddRange)
            {
                result = rect.ToString() + ",";
            }
            bool isInit = false;
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.Point;
                        result += "\"" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - startPoint.X;
                        double OffsetY = colorInfo.Point.Y - startPoint.Y;
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
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        startPoint = colorInfo.Point;
                        result = "\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",[";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - startPoint.X;
                        double OffsetY = colorInfo.Point.Y - startPoint.Y;
                        result += "[" + OffsetX.ToString() + "," + OffsetY.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                            colorInfo.Color.B.ToString("x2") + "\"],";
                    }
                }
            }
            result = result.Trim(',');
            if (IsAddRange)
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
            Point startPoint = new Point();
            foreach (ColorInfo colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (result == string.Empty)
                    {
                        startPoint = colorInfo.Point;
                        result += "\"0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        double OffsetX = colorInfo.Point.X - startPoint.X;
                        double OffsetY = colorInfo.Point.Y - startPoint.Y;
                        result += OffsetX.ToString() + "|" + OffsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.B.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            if (IsAddRange)
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
        public static string AjCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            string result = "\"";
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
            if (IsAddRange)
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
        public static string AnchorsCompareStrTest(ObservableCollection<ColorInfo> colorInfos)
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
    }
}
