namespace ScriptGraphicHelper.Models
{
    public class Range
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public int Mode_1 { get; set; }
        public int Mode_2 { get; set; }


        public Range(double left, double top, double right, double bottom, int mode_1 = -1, int mode_2 = -1)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Mode_1 = mode_1;
            this.Mode_2 = mode_2;
        }
        public string ToString(int mode = 0)
        {

            if (mode == 1)
            {
                var width = this.Right - this.Left;
                var height = this.Bottom - this.Top;
                return string.Format("{0},{1},{2},{3}", this.Left.ToString(), this.Top.ToString(), width.ToString(), height.ToString());
            }
            else if (mode == 2)
            {
                var mode_1 = this.Mode_1 == 0 ? "left" : this.Mode_1 == 1 ? "center" : this.Mode_1 == 2 ? "right" : "none";
                var mode_2 = this.Mode_2 == 0 ? "left" : this.Mode_2 == 1 ? "center" : this.Mode_2 == 2 ? "right" : "none";

                if (this.Mode_1 == this.Mode_2)
                {
                    return string.Format("{0},{1},{2},{3},{4}", this.Left.ToString(), this.Top.ToString(), this.Right.ToString(), this.Bottom.ToString(), mode_1);
                }
                else
                {
                    return string.Format("{0},{1},{2},{3},{4},{5}", this.Left.ToString(), this.Top.ToString(), mode_1, this.Right.ToString(), this.Bottom.ToString(), mode_2);
                }
            }
            else
            {
                return string.Format("{0},{1},{2},{3}", this.Left.ToString(), this.Top.ToString(), this.Right.ToString(), this.Bottom.ToString());
            }
        }
    }
}
