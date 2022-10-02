using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TreeView.Maui.Core;

public class TreeViewNode : BindableObject, ILazyLoadTreeViewNode, ISelectableNode
{
    private bool? _isLeaf;
    private string name;
    private bool isExtended;
    private object value;
    private SelectionState selection;

    public TreeViewNode()
    {
        Initialize();
    }

    public TreeViewNode(string name, object value = null, bool isExtended = false, IList<IHasChildrenTreeViewNode> children = null) : this()
    {
        Name = name;
        Value = value;
        IsExtended = isExtended;

        if (children != null)
        {
            Children = children;
        }

        Initialize();
    }

    public virtual string Name { get => name; set => SetProperty(ref name, value); }
    public virtual bool IsExtended { get => isExtended; set => SetProperty(ref isExtended, value); }
    public virtual object Value { get => value; set => SetProperty(ref this.value, value); }
    public virtual IHasChildrenTreeViewNode Parent { get; internal set; }
    public virtual IList<IHasChildrenTreeViewNode> Children { get; protected set; } = new ObservableCollection<IHasChildrenTreeViewNode>();
    public virtual Func<ITreeViewNode, IEnumerable<IHasChildrenTreeViewNode>> GetChildren { get; set; }
    public virtual bool IsLeaf { get => _isLeaf ?? !Children.Any() && GetChildren == null; set => SetProperty(ref _isLeaf, value); }
    public SelectionState Selection { get => GetSelectionState(); set => SetProperty(ref selection, value, doAfter: OnSelectionStateChanged); }
    
    public ICommand SelectCommand { get; set; }

    protected virtual SelectionState GetSelectionState()
    {
        if (Children == null || Children.Count == 0)
        {
            return selection;
        }

        var firstChild = Children.FirstOrDefault(x => x is ISelectableNode) as ISelectableNode;

        if (firstChild == null)
        {
            return selection;
        }

        foreach (ISelectableNode child in Children.Where(x =>x is ISelectableNode))
        {
            if (firstChild.Selection != child.Selection)
            {
                return SelectionState.PartiallySelected;
            }
        }

        return firstChild.Selection;
    }
    
    protected virtual void Initialize()
    {
        SelectCommand = new Command(() =>
        {
            Selection = Selection == SelectionState.Selected ? SelectionState.Unselected : SelectionState.Selected;
        });
        
        if (Children is INotifyCollectionChanged observable)
        {
            observable.CollectionChanged += Children_CollectionChanged;
        }

        if (Children != null)
        {
            foreach (var child in Children)
            {
                if (child is TreeViewNode node)
                {
                    node.Parent = this;
                }
            }
        }
    }

    private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems)
            {
                if (item is TreeViewNode nodeWithChildren)
                {
                    nodeWithChildren.Parent = this;
                }
            }
        }
    }
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

    protected virtual void OnSelectionStateChanged(SelectionState state)
    {
        if (IsLeaf && state == SelectionState.PartiallySelected)
        {
            return;
        }

        if (Children == null || Children.Count == 0)
        {
            LoadChildren();
        }

        foreach (var child in Children)
        {
            if (child is ISelectableNode selectable)
            {
                selectable.Selection = state;
            }
        }

        if (this.Parent is TreeViewNode node)
        {
            node.NotifyChildSelectionChanged();
        }
    }

    protected virtual void NotifyChildSelectionChanged()
    {
        OnPropertyChanged(nameof(Selection));
    }

    public void LoadChildren()
    {
        if (GetChildren == null)
        {
            return;
        }
        
        foreach (var child in GetChildren(this))
        {
            Children.Add(child);
        }
    }
}
