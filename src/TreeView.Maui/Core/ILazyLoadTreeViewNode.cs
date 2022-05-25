namespace TreeView.Maui.Core;

public interface ILazyLoadTreeViewNode : IHasChildrenTreeViewNode
{
    Func<ITreeViewNode, IEnumerable<IHasChildrenTreeViewNode>> GetChildren { get; }
}
