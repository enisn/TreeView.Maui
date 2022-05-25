using Microsoft.Maui.Devices;
using System.Collections.ObjectModel;
using TreeView.Maui.Core;

namespace SandboxTreeView;
public class FileSystemPageViewModel : BindableObject
{
    private bool isBusy;
    private ObservableCollection<TreeViewNode> nodes;

    public ObservableCollection<TreeViewNode> Nodes { get => nodes; set { nodes = value; OnPropertyChanged(); } }

    public bool IsBusy { get => isBusy; set { isBusy = value; OnPropertyChanged(); } }

    public FileSystemPageViewModel()
    {
        InitializeNodes();
    }

    async void InitializeNodes()
    {
        IsBusy = true;

        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            path = "C:\\";
        }

        Nodes = new ObservableCollection<TreeViewNode>(
            GetContent(path));
        IsBusy = false;
    }

    IEnumerable<TreeViewNode> GetContent(string dir)
    {
        var directories = Directory.GetDirectories(dir);
        foreach (string d in directories)
        {
            yield return new TreeViewNode
            {
                Name = d.Split(Path.DirectorySeparatorChar).LastOrDefault(),
                Value = d,
                GetChildren = (node) => GetContent(node.Value.ToString())
                //Children = DirSearch(d).ToList()
            };
        }
        var files = Directory.GetFiles(dir);

        foreach (string f in files)
        {
            var node = new TreeViewNode
            {
                Name = f.Split(Path.DirectorySeparatorChar).LastOrDefault(),
                Value = f,
            };
            yield return node;
        }
    }
}
