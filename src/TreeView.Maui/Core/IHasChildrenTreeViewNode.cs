using System.Collections.Specialized;

namespace TreeView.Maui.Core;

public interface IHasChildrenTreeViewNode : ITreeViewNode
{
    IHasChildrenTreeViewNode Parent { get; }
    IList<IHasChildrenTreeViewNode> Children { get; }
    bool IsLeaf { get; set; }
}
