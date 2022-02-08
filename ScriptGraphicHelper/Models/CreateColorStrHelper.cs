using Avalonia;
using Newtonsoft.Json;
using ScriptGraphicHelper.Converters;
using ScriptGraphicHelper.Engine;
using ScriptGraphicHelper.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ScriptGraphicHelper.Models
{
    public enum FormatMode
    {
        CmpStr = 0,
        DMFindStr = 1,
        AnjianFindStr = 2,
        AnjianCmpStr = 3,
        AutojsFindStr = 4,
        AutojsCmpStr = 5,
        DiyFindStr = 6,
        DiyCmpStr = 7,
        AnchorsFindStr = 8,
        AnchorsCmpStr = 9,
        ATFindStr = 10,
        ATCmpStr = 11,
        ATAnchorsFindStr = 12,
        ATAnchorsCmpStr = 13,
        CDFindStr = 14,
        CDCmpStr = 15,
        ECFindStr = 16,
        ECCmpStr = 17,
        AnchorsFindStr4Test = 18,
        AnchorsCmpStr4Test = 19,
        FindStr4Test = 20

    };
    public static class CreateColorStrHelper
    {
        public static string Create(FormatMode mode, ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            try
            {
                return mode switch
                {
                    FormatMode.CmpStr => CompareStr(colorInfos),
                    FormatMode.DMFindStr => DmFindStr(colorInfos, rect),
                    FormatMode.AnjianFindStr => AnjianFindStr(colorInfos, rect),
                    FormatMode.AnjianCmpStr => AnjianCompareStr(colorInfos),
                    FormatMode.CDFindStr => CdFindStr(colorInfos, rect),
                    FormatMode.CDCmpStr => CdCompareStr(colorInfos),
                    FormatMode.AutojsFindStr => AutojsFindStr(colorInfos, rect),
                    FormatMode.AutojsCmpStr => AutojsCompareStr(colorInfos),
                    FormatMode.ECFindStr => EcFindStr(colorInfos, rect),
                    FormatMode.ECCmpStr => EcCompareStr(colorInfos),
                    FormatMode.DiyFindStr => DiyFindStr(colorInfos, rect),
                    FormatMode.DiyCmpStr => DiyCompareStr(colorInfos),
                    FormatMode.AnchorsCmpStr => AnchorsCompareStr(colorInfos),
                    FormatMode.AnchorsFindStr => AnchorsFindStr(colorInfos, rect),
                    FormatMode.ATCmpStr => AstatorCompareStr(colorInfos),
                    FormatMode.ATFindStr => AstatorFindStr(colorInfos, rect),
                    FormatMode.ATAnchorsCmpStr => AstatorAnchorsCompareStr(colorInfos),
                    FormatMode.ATAnchorsFindStr => AstatorAnchorsFindStr(colorInfos, rect),
                    FormatMode.AnchorsCmpStr4Test => AnchorsCompareStr4Test(colorInfos),
                    FormatMode.AnchorsFindStr4Test => AnchorsCompareStr4Test(colorInfos),
                    FormatMode.FindStr4Test => FindStr4Test(colorInfos),
                    _ => CompareStr(colorInfos),
                };
            }
            catch (Exception e)
            {
                MessageBox.ShowAsync(e.ToString());
                return string.Empty;
            }
        }
        public static string Create(FormatMode mode, ObservableCollection<ColorInfo> colorInfos)
        {
            Range rect = new(0, 0, 0, 0);
            return mode switch
            {
                FormatMode.CmpStr => CompareStr(colorInfos),
                FormatMode.DMFindStr => DmFindStr(colorInfos, rect),
                FormatMode.AnjianFindStr => AnjianFindStr(colorInfos, rect),
                FormatMode.AnjianCmpStr => AnjianCompareStr(colorInfos),
                FormatMode.CDFindStr => CdFindStr(colorInfos, rect),
                FormatMode.CDCmpStr => CdCompareStr(colorInfos),
                FormatMode.AutojsFindStr => AutojsFindStr(colorInfos, rect),
                FormatMode.AutojsCmpStr => AutojsCompareStr(colorInfos),
                FormatMode.ECFindStr => EcFindStr(colorInfos, rect),
                FormatMode.ECCmpStr => EcCompareStr(colorInfos),
                FormatMode.DiyFindStr => DiyFindStr(colorInfos, rect),
                FormatMode.DiyCmpStr => DiyCompareStr(colorInfos),
                FormatMode.AnchorsCmpStr => AnchorsCompareStr(colorInfos),
                FormatMode.AnchorsFindStr => AnchorsFindStr(colorInfos, rect),
                FormatMode.AnchorsCmpStr4Test => AnchorsCompareStr4Test(colorInfos),
                FormatMode.AnchorsFindStr4Test => AnchorsCompareStr4Test(colorInfos),
                FormatMode.FindStr4Test => FindStr4Test(colorInfos),
                _ => CompareStr(colorInfos),
            };
        }

        private static string DiyCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            if (Setting.Instance.DiyFormatMode == "script")
            {
                return DiyCompareStr_Script(colorInfos);
            }
            return DiyCompareStr_Json(colorInfos);
        }

        private static string DiyFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            if (Setting.Instance.DiyFormatMode == "script")
            {
                return DiyFindStr_Script(colorInfos, rect);
            }
            return DiyFindStr_Json(colorInfos, rect);
        }

        private static string DiyCompareStr_Script(ObservableCollection<ColorInfo> colorInfos)
        {
            var engine = new ScriptEngine();
            engine.LoadScript(AppDomain.CurrentDomain.BaseDirectory + @"assets/diyFormat.csx");

            var compile = engine.Compile();
            if (!compile.Success)
            {
                var errorMessage = new List<string>();
                foreach (var msg in compile.Diagnostics)
                {
                    errorMessage.Add(msg.ToString());
                }
                MessageBox.ShowAsync("编译失败:\r\n" + string.Join("\r\n", errorMessage.ToArray()));
                return string.Empty;
            }

            var result = engine.Execute("CreateColorStrHelper.DiyFormat", "CreateCmpColor", new object[] { colorInfos.ToList() });
            engine.UnExecute();
            return result;
        }

        private static string DiyFindStr_Script(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var engine = new ScriptEngine();
            engine.LoadScript(AppDomain.CurrentDomain.BaseDirectory + @"assets/diyFormat.csx");

            var compile = engine.Compile();
            if (!compile.Success)
            {
                var errorMessage = new List<string>();
                foreach (var msg in compile.Diagnostics)
                {
                    errorMessage.Add(msg.ToString());
                }
                MessageBox.ShowAsync("编译失败:\r\n" + string.Join("\r\n", errorMessage.ToArray()));
                return string.Empty;
            }

            var result = engine.Execute("CreateColorStrHelper.DiyFormat", "CreateFindColor", new object[] { colorInfos.ToList(), rect });
            engine.UnExecute();
            return result;

        }


        private static DiyFormat GetDiyFormatJson()
        {
            var sr = File.OpenText(System.AppDomain.CurrentDomain.BaseDirectory + @"assets/diyFormat.json");
            var result = sr.ReadToEnd();
            sr.Close();

            DiyFormat format;

            format = JsonConvert.DeserializeObject<DiyFormat>(result);

            if (format == null)
            {
                MessageBox.ShowAsync("自定义格式错误!");
            }

            return format ?? new DiyFormat();
        }

        private static string DiyCompareStr_Json(ObservableCollection<ColorInfo> colorInfos)
        {
            var diyFormat = GetDiyFormatJson();
            var colorStr = string.Empty;
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    var res = diyFormat.FollowColorFormat;

                    var color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                    if (diyFormat.IsBgr)
                    {
                        color = colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2");
                    }

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
            var result = diyFormat.CompareStrFormat;
            if (result.IndexOf("{ImportInfo}") != -1)
            {
                var info = diyFormat.ImportInfo;
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

        private static string DiyFindStr_Json(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var diyFormat = GetDiyFormatJson();
            var isInit = false;
            Point startPoint = new();
            var colorStr = new string[2];
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!isInit)
                    {
                        isInit = true;
                        startPoint = colorInfo.Point;
                        var res = diyFormat.FirstColorFormat;

                        var color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                        if (diyFormat.IsBgr)
                        {
                            color = colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2");
                        }

                        if (res.IndexOf("{color}") != -1)
                        {
                            res = res.Replace("{color}", color);
                        }
                        colorStr[0] = res;
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - startPoint.X;
                        var offsetY = colorInfo.Point.Y - startPoint.Y;

                        var res = diyFormat.FollowColorFormat;

                        var color = colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2");
                        if (diyFormat.IsBgr)
                        {
                            color = colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.R.ToString("x2");
                        }

                        if (res.IndexOf("{x}") != -1)
                        {
                            res = res.Replace("{x}", offsetX.ToString());
                        }
                        if (res.IndexOf("{y}") != -1)
                        {
                            res = res.Replace("{y}", offsetY.ToString());
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
            var result = diyFormat.FindStrFormat;
            if (result.IndexOf("{ImportInfo}") != -1)
            {
                var info = diyFormat.ImportInfo;
                if (info != string.Empty && info != "")
                {
                    info = string.Format("\"{0}/{1}/{2}\"", info, startPoint.X, startPoint.Y);
                }
                result = result.Replace("{ImportInfo}", info);
            }
            if (result.IndexOf("{range}") != -1)
            {
                var range = diyFormat.RangeFormat;
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

        private static string DmFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;

            var inited = false;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
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
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
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
            var result = string.Empty;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
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
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
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

        private static string AnjianFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            var inited = false;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
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
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|" + colorInfo.Color.B.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                        colorInfo.Color.R.ToString("x2") + ",";
                    }
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AutojsFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
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
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += "[" + offsetX.ToString() + "," + offsetY.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
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

        private static string EcFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
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
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|0x" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
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

        private static string CompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
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

        private static string AnjianCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = "\"";
            if (Setting.Instance.AddInfo)
            {
                result += string.Format("{0}\",", "anjian");
            }
            foreach (var colorInfo in colorInfos)
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

        private static string CdCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = "{";
            foreach (var colorInfo in colorInfos)
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

        private static string AutojsCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = string.Empty;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
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
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += "[" + offsetX.ToString() + "," + offsetY.ToString() + ",\"#" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
                            colorInfo.Color.B.ToString("x2") + "\"],";
                    }
                }
            }
            result = result.Trim(',');
            result += "]";
            return result;
        }

        private static string EcCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
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

        private static string AnchorsCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = "[" + ColorInfo.Width.ToString() + "," + ColorInfo.Height.ToString() + ",\r\n[";
            foreach (var colorInfo in colorInfos)
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

        private static string AnchorsFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var result = "[" + ColorInfo.Width.ToString() + "," + ColorInfo.Height.ToString();
            if (Setting.Instance.AddRange)
            {
                result += string.Format(",\r\n[{0}],\r\n[\r\n", rect.ToString(2));
            }
            else
            {
                result += ",\r\n[\r\n";
            }
            foreach (var colorInfo in colorInfos)
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
            result += "\r\n]\r\n]";
            return result;
        }

        private static string AstatorCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AstatorAnchorsCompareStr(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = $"\"{ColorInfo.Width},{ColorInfo.Height},";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Anchor.ToString().ToLower()}|{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }
        private static string AstatorFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            if (Setting.Instance.AddRange)
            {
                result += $"\"{rect.ToString()}\",";
            }
            result += "\"";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AstatorAnchorsFindStr(ObservableCollection<ColorInfo> colorInfos, Range rect)
        {
            var result = string.Empty;
            if (Setting.Instance.AddRange)
            {
                result += $"\"{rect.ToString(2)}\",";
            }

            result += $"\"{ColorInfo.Width},{ColorInfo.Height},";
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    result += $"{colorInfo.Anchor.ToString().ToLower()}|{colorInfo.Point.X}|{colorInfo.Point.Y}|{colorInfo.Color.ToHexString()},";
                }
            }
            result = result.Trim(',');
            result += "\"";
            return result;
        }

        private static string AnchorsCompareStr4Test(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = "\"";
            foreach (var colorInfo in colorInfos)
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


        private static string FindStr4Test(ObservableCollection<ColorInfo> colorInfos)
        {
            var result = string.Empty;

            var inited = false;
            Point firstPoint = new();
            foreach (var colorInfo in colorInfos)
            {
                if (colorInfo.IsChecked)
                {
                    if (!inited)
                    {
                        inited = true;
                        firstPoint = colorInfo.Point;
                        result += "\"" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") + colorInfo.Color.B.ToString("x2") + "\",\"";
                    }
                    else
                    {
                        var offsetX = colorInfo.Point.X - firstPoint.X;
                        var offsetY = colorInfo.Point.Y - firstPoint.Y;
                        result += offsetX.ToString() + "|" + offsetY.ToString() + "|" + colorInfo.Color.R.ToString("x2") + colorInfo.Color.G.ToString("x2") +
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
