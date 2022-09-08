using System.Collections.ObjectModel;

namespace TreeView.Maui.Core;

public class TreeViewNode : BindableObject, ILazyLoadTreeViewNode
{
    private bool? _isLeaf;
    public TreeViewNode()
    {
    }

    public TreeViewNode(string name, object value = null, bool isExtended = false, IList<IHasChildrenTreeViewNode> children = null)
    {
        Name = name;
        Value = value;
        IsExtended = isExtended;

        if(children != null)
        {
            Children = children;
        }
    }

    public virtual string Name { get; set; }
    public virtual bool IsExtended { get; set; }
    public virtual object Value { get; set; }
    public virtual IList<IHasChildrenTreeViewNode> Children { get; set; } = new ObservableCollection<IHasChildrenTreeViewNode>();
    public virtual Func<ITreeViewNode, IEnumerable<IHasChildrenTreeViewNode>> GetChildren { get; set; }
    public virtual bool IsLeaf { get => _isLeaf ?? !Children.Any() && GetChildren == null; set => _isLeaf = value; }
}
