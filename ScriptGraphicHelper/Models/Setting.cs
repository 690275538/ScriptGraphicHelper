namespace ScriptGraphicHelper.Models
{
    public class Setting
    {
        public static Setting Instance { get; set; } = new();

        public double Width { get; set; } = 1720;
        public double Height { get; set; } = 880;
        public int SimSelectedIndex { get; set; } = 0;
        public int FormatSelectedIndex { get; set; } = 0;
        public bool AddRange { get; set; } = false;
        public bool AddInfo { get; set; } = false;
        public int RangeTolerance { get; set; } = 50;
        public int DiySim { get; set; } = 95;
        public bool IsOffset { get; set; } = false;
        public string DmRegcode { get; set; } = string.Empty;
        public string YsPath { get; set; } = string.Empty;
        public string XyPath { get; set; } = string.Empty;
        public string Ldpath3 { get; set; } = string.Empty;
        public string Ldpath4 { get; set; } = string.Empty;
        public string Ldpath64 { get; set; } = string.Empty;
        public GetColorInfosConfig GetColorInfosConfig { get; set; } = new GetColorInfosConfig();

    }

    public class GetColorInfosConfig
    {
        public int ModeSelectedIndex { get; set; } = 0;
        public int Threshold { get; set; } = 12;
        public int Size { get; set; } = -1;
    }
}
