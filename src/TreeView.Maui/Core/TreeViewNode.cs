using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TreeView.Maui.Core;

public class TreeViewNode : BindableObject, ILazyLoadTreeViewNode
{
    private bool? _isLeaf;
    private IList<IHasChildrenTreeViewNode> children = new ObservableCollection<IHasChildrenTreeViewNode>();
    private string name;
    private bool isExtended;

    public TreeViewNode()
    {
    }

    public TreeViewNode(string name, object value = null, bool isExtended = false, IList<IHasChildrenTreeViewNode> children = null)
    {
        Name = name;
        Value = value;
        IsExtended = isExtended;

        if (children != null)
        {
            Children = children;
        }
    }

    public virtual string Name { get => name; set { name = value; OnPropertyChanged(); } }
    public virtual bool IsExtended { get => isExtended; set { isExtended = value; OnPropertyChanged(); } }
    public virtual object Value { get; set; }
    public virtual IList<IHasChildrenTreeViewNode> Children { get => children; set { children = value; OnChildrenSet(); OnPropertyChanged(); } }
    public virtual Func<ITreeViewNode, IEnumerable<IHasChildrenTreeViewNode>> GetChildren { get; set; }
    public virtual bool IsLeaf { get => _isLeaf ?? !Children.Any() && GetChildren == null; set => _isLeaf = value; }

    protected virtual void OnChildrenSet()
    {
        if (Children is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsLeaf));
            };
        }

        OnPropertyChanged(nameof(IsLeaf));
    }
}
