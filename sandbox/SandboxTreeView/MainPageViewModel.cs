using System.Collections.ObjectModel;
using System.Windows.Input;
using TreeView.Maui.Core;

namespace SandboxTreeView;
public class MainPageViewModel : BindableObject
{
    public ObservableCollection<TreeViewNode> Nodes { get; set; } = new();

    public ICommand RandomizeNamesCommand { get; set; }

    public ICommand SwitchIsLeafCommand { get; set; }
    public MainPageViewModel()
    {
        Nodes.Add(new TreeViewNode("A")
        {
            Children =
            {
                new TreeViewNode("A.1"),
                new TreeViewNode("A.2"),
            }
        });
        Nodes.Add(new TreeViewNode("B")
        {
            Children =
            {
                new TreeViewNode("B.1")
                {
                    Children =
                    {
                        new TreeViewNode("B.1.a"),
                        new TreeViewNode("B.1.b"),
                        new TreeViewNode("B.1.c"),
                        new TreeViewNode("B.1.d"),

                    }
                },
                new TreeViewNode("B.2"),
            }
        });
        Nodes.Add(new TreeViewNode("C"));
        Nodes.Add(new TreeViewNode("D"));

        RandomizeNamesCommand = new Command(() =>
        {
            foreach (var node in Nodes)
            {
                node.Name = node.Name + " " + new Random().Next(0, 100);
            }
        });

        SwitchIsLeafCommand = new Command(() =>
        {
            foreach (var node in Nodes)
            {
                node.IsLeaf = !node.IsLeaf;
            }
        });
    }
}
