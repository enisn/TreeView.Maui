using System.Collections.ObjectModel;
using System.Windows.Input;
using TreeView.Maui.Core;

namespace SandboxTreeView;
public class FileSystemCustomTemplatePageViewModel : BindableObject
{
    private bool isBusy;
    public bool IsBusy { get => isBusy; set { isBusy = value; OnPropertyChanged(); } }

    private ObservableCollection<MyTreeViewNode> nodes;

    public ObservableCollection<MyTreeViewNode> Nodes { get => nodes; set { nodes = value; OnPropertyChanged(); } }

    public ICommand RemoveFirstItemCommand { get; }

    public ICommand AddCurrentDateCommand { get; }

    public FileSystemCustomTemplatePageViewModel()
    {
        InitializeNodes();
        RemoveFirstItemCommand = new Command(() => Nodes.RemoveAt(0));
        AddCurrentDateCommand = new Command(() => Nodes.Insert(0, new MyTreeViewNode
        {
            Name = DateTime.Now.ToLongDateString(),
            IsDirectory = false,
            IsLeaf = true
        }));
    }

    async void InitializeNodes()
    {
        IsBusy = true;

        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            path = "C:\\";
        }

        Nodes = new ObservableCollection<MyTreeViewNode>(
            GetContent(path));
        IsBusy = false;
    }

    IEnumerable<MyTreeViewNode> GetContent(string dir)
    {
        var directories = Directory.GetDirectories(dir);
        foreach (string d in directories)
        {
            yield return new MyTreeViewNode
            {
                Name = d.Split(Path.DirectorySeparatorChar).LastOrDefault(),
                Value = d,
                IsDirectory = true,
                GetChildren = (node) => GetContent(node.Value.ToString())
                //Children = DirSearch(d).ToList()
            };
        }
        var files = Directory.GetFiles(dir);

        foreach (string f in files)
        {
            var node = new MyTreeViewNode
            {
                Name = f.Split(Path.DirectorySeparatorChar).LastOrDefault(),
                Value = f,
                IsDirectory = false,
            };
            yield return node;
        }
    }

    public class MyTreeViewNode : TreeViewNode
    {
        public bool IsDirectory { get; set; }

        private bool isSelected;

        public bool IsSelected { get => isSelected; set { isSelected = value; OnPropertyChanged(); } }

        public ICommand ToggleCommand { get; protected set; }

        public MyTreeViewNode()
        {
            ToggleCommand = new Command(() => IsSelected = !IsSelected);
        }
    }
}
