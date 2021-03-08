namespace ScriptGraphicHelper.Models
{
    public class Setting
    {
        public double Width { get; set; } = 1450;
        public double Height { get; set; } = 850;
        public int SimSelectedIndex { get; set; } = 0;
        public int FormatSelectedIndex { get; set; } = 0;
        public bool Addrange { get; set; } = false;
        public string DmRegcode { get; set; } = string.Empty;
        public string YsPath { get; set; } = string.Empty;
        public string XyPath { get; set; } = string.Empty;
        public string Ldpath3 { get; set; } = string.Empty;
        public string Ldpath4 { get; set; } = string.Empty;
        public string Ldpath64 { get; set; } = string.Empty;
    }

    public static class PubSetting
    {
        public static Setting Setting = new();
    }
}
