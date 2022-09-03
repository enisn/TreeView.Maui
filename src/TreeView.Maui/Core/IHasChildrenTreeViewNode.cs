using System.Collections.Specialized;

namespace TreeView.Maui.Core;

public interface IHasChildrenTreeViewNode : ITreeViewNode
{
    IList<IHasChildrenTreeViewNode> Children { get; }
    bool IsLeaf { get; set; }
}
