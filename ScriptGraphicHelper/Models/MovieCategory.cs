using System.Collections.ObjectModel;


namespace ScriptGraphicHelper.Models
{
    public class MovieCategory
    {
        public int Hwnd { get; set; }

        public string Title { get; set; }

        public string ClassName { get; set; }

        public string Info { get; set; }
        public ObservableCollection<MovieCategory> Movies { get; set; } = new ObservableCollection<MovieCategory>();

        public MovieCategory(int hwnd, string title, string className, params MovieCategory[] movies)
        {
            Hwnd = hwnd;
            Title = title;
            ClassName = className;
            Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
            Movies = new ObservableCollection<MovieCategory>(movies);
        }
        public MovieCategory(int hwnd, string title, string className)
        {
            Hwnd = hwnd;
            Title = title;
            ClassName = className;
            Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
        }
        public MovieCategory()
        {
            Hwnd = -1;
            Title = "";
            ClassName = "";
            Info = string.Format("[{0}][{1}][{2}]", Hwnd, Title, ClassName);
            Movies = new ObservableCollection<MovieCategory>();
        }
    }
}
