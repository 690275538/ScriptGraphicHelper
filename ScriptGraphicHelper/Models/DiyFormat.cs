﻿namespace ScriptGraphicHelper.Models
{
    public class DiyFormat
    {
        //public int ColorMode { get; set; } = 0;//0:字符串找色 1:字符串比色 2:数组找色 3:数组比色
        //public bool StartPoint { get; set; } = false;
        //public string ParentSplit { get; set; } = string.Empty;
        //public string ChildSplit { get; set; } = string.Empty;
        //public bool BGR { get; set; } = false;
        //public string ColorPrefix { get; set; } = string.Empty;
        //public int Range { get; set; } = -1;
        //public int Point { get; set; } = -1;
        //public string Prefix { get; set; } = string.Empty;
        //public string Suffix { get; set; } = string.Empty;

        public string FirstColorFormat { get; set; } = string.Empty;
        public string FollowColorFormat { get; set; } = string.Empty;
        public string FindStrFormat { get; set; } = string.Empty;
        public string CompareStrFormat { get; set; } = string.Empty;
        public bool IsBgr { get; set; } = false;


    }
}