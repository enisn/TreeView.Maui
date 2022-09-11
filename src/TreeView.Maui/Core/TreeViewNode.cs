using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace TreeView.Maui.Core;

public class TreeViewNode : BindableObject, ILazyLoadTreeViewNode
{
    private bool? _isLeaf;
    private string name;
    private bool isExtended;
    private object value;

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

    public virtual string Name { get => name; set => SetProperty(ref name, value); }
    public virtual bool IsExtended { get => isExtended; set => SetProperty(ref isExtended, value); }
    public virtual object Value { get => value; set => SetProperty(ref this.value, value); }
    public virtual IList<IHasChildrenTreeViewNode> Children { get; set; } = new ObservableCollection<IHasChildrenTreeViewNode>();
    public virtual Func<ITreeViewNode, IEnumerable<IHasChildrenTreeViewNode>> GetChildren { get; set; }
    public virtual bool IsLeaf { get => _isLeaf ?? !Children.Any() && GetChildren == null; set => SetProperty(ref _isLeaf, value); }

    protected virtual void SetProperty<T>(ref T field, T value, Action<T> doAfter = null, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
        doAfter?.Invoke(value);
    }
}
