using System.Collections.ObjectModel;


namespace ScriptGraphicHelper.Models
{
    public class MoveCategory
    {
        public int Hwnd { get; set; }

        public string Title { get; set; }

        public string ClassName { get; set; }

        public string Info { get; set; }
        public ObservableCollection<MoveCategory> Movies { get; set; } = new ObservableCollection<MoveCategory>();

        public MoveCategory(int hwnd, string title, string className, params MoveCategory[] movies)
        {
            Hwnd = hwnd;
            Title = title;
            ClassName = className;
            Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
            Movies = new ObservableCollection<MoveCategory>(movies);
        }
        public MoveCategory(int hwnd, string title, string className)
        {
            Hwnd = hwnd;
            Title = title;
            ClassName = className;
            Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
        }
        public MoveCategory()
        {
            Hwnd = -1;
            Title = "";
            ClassName = "";
            Info = string.Format("[{0}][{1}][{2}]", Hwnd, Title, ClassName);
            Movies = new ObservableCollection<MoveCategory>();
        }
    }
}
