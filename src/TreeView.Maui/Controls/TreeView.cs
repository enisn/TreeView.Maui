using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using TreeView.Maui.Core;

namespace TreeView.Maui.Controls;

public partial class TreeView : ContentView
{
    private StackLayout _root = new StackLayout { Spacing = 0 };

    public TreeView()
    {
        Content = _root;
    }

    protected virtual void OnItemsSourceSetting(IEnumerable oldValue, IEnumerable newValue)
    {
        if (oldValue is INotifyCollectionChanged oldItemsSource)
        {
            oldItemsSource.CollectionChanged -= OnItemsSourceChanged;
        }

        if (newValue is INotifyCollectionChanged newItemsSource)
        {
            newItemsSource.CollectionChanged += OnItemsSourceChanged;
        }
    }

    protected virtual void OnItemsSourceSet()
    {
        Render();
    }

    private protected virtual void OnItemsSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    foreach (var item in e.NewItems)
                    {
                        var nodeView = new TreeViewNodeView(item as IHasChildrenTreeViewNode, ItemTemplate, ArrowTheme);
                        nodeView.SetBinding(TreeViewNodeView.SelectionColorProperty,
                            new Binding(nameof(TreeView.SelectionColor), source: this));

                        _root.Children.Insert(e.NewStartingIndex, nodeView);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                {
                    foreach (var item in e.OldItems)
                    {
                        _root.Children.Remove(_root.Children.FirstOrDefault(x => (x as View).BindingContext == item));
                    }
                }
                break;
            default:
                Render();
                break;
        }
    }

    protected virtual void OnItemTemplateChanged()
    {
        // TODO: Some optimizations
        // Eventually
        Render();
    }

    void Render()
    {
        _root.Children.Clear();

        if (ItemsSource == null)
        {
            return;
        }

        foreach (var item in ItemsSource)
        {
            if (item is IHasChildrenTreeViewNode node)
            {
                var nodeView = new TreeViewNodeView(node, ItemTemplate, ArrowTheme);
                nodeView.SetBinding(TreeViewNodeView.SelectionColorProperty,
                    new Binding(nameof(TreeView.SelectionColor), source: this));
                _root.Children.Add(nodeView);

            }
        }
    }

    protected virtual void OnArrowThemeChanged()
    {
        foreach (TreeViewNodeView treeViewNodeView in _root.Children.Where(x => x is TreeViewNodeView))
        {
            treeViewNodeView.UpdateArrowTheme(ArrowTheme);
        }
    }
}

public class TreeViewNodeView : ContentView
{
    protected ImageButton extendButton;
    protected StackLayout slChildrens;
    protected IHasChildrenTreeViewNode Node { get; }
    protected DataTemplate ItemTemplate { get; }
    protected NodeArrowTheme ArrowTheme { get; }
    public Color SelectionColor { get => (Color)GetValue(SelectionColorProperty); set => SetValue(SelectionColorProperty, value); }

    public static readonly BindableProperty SelectionColorProperty =
        BindableProperty.Create(nameof(SelectionColor), typeof(Color), typeof(TreeViewNodeView), defaultValue: Colors.Purple);
    public TreeViewNodeView(IHasChildrenTreeViewNode node, DataTemplate itemTemplate, NodeArrowTheme theme)
    {
        var sl = new StackLayout { Spacing = 0 };
        BindingContext = Node = node;
        ItemTemplate = itemTemplate;
        ArrowTheme = theme;
        Content = sl;

        slChildrens = new StackLayout { IsVisible = node.IsExtended, Margin = new Thickness(10, 0, 0, 0), Spacing = 0 };

        extendButton = new ImageButton
        {
            Source = GetArrowSource(theme),
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
            Opacity = node.IsLeaf ? 0 : 1, // Using opacity instead isvisible to keep alignment
            Rotation = node.IsExtended ? 0 : -90,
            HeightRequest = 30,
            WidthRequest = 30,
            CornerRadius = 15
        };

        extendButton.Triggers.Add(new DataTrigger(typeof(ImageButton))
        {
            Binding = new Binding(nameof(Node.IsLeaf)),
            Value = true,
            Setters = { new Setter { Property = ImageButton.OpacityProperty, Value = 0 } }
        });

        extendButton.Triggers.Add(new DataTrigger(typeof(ImageButton))
        {
            Binding = new Binding(nameof(Node.IsLeaf)),
            Value = false,
            Setters = { new Setter { Property = ImageButton.OpacityProperty, Value = 1 } }
        });

        extendButton.Triggers.Add(new DataTrigger(typeof(ImageButton))
        {
            Binding = new Binding(nameof(Node.IsExtended)),
            Value = true,
            EnterActions =
            {
                new GenericTriggerAction<ImageButton>((sender) =>
                {
                    sender.RotateTo(0);
                })
            },
            ExitActions =
            {
                new GenericTriggerAction<ImageButton>((sender) =>
                {
                    sender.RotateTo(-90);
                })
            }
        });

        extendButton.Clicked += (s, e) =>
        {
            node.IsExtended = !node.IsExtended;
            slChildrens.IsVisible = node.IsExtended;

            if (node.IsExtended)
            {
                if (node is ILazyLoadTreeViewNode lazyNode && lazyNode.GetChildren != null && !lazyNode.Children.Any())
                {
                    var lazyChildren = lazyNode.GetChildren(lazyNode);
                    foreach (var child in lazyChildren)
                    {
                        lazyNode.Children.Add(child);
                    }

                    if (!lazyNode.Children.Any())
                    {
                        extendButton.Opacity = 0;
                        lazyNode.IsLeaf = true;
                    }
                }
            }
        };

        var content = ItemTemplate.CreateContent() as View;

        var nodeLine = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Children =
            {
                extendButton,
                content
            }
        };

        AddSelectionTriggers(nodeLine);

        sl.Children.Add(nodeLine);

        foreach (var child in node.Children)
        {
            var nodeView = new TreeViewNodeView(child, ItemTemplate, theme);
            nodeView.SetBinding(TreeViewNodeView.SelectionColorProperty,
                    new Binding(nameof(TreeView.SelectionColor), source: this));
            slChildrens.Children.Add(nodeView);
        }

        sl.Children.Add(slChildrens);

        if (Node.Children is INotifyCollectionChanged ovservableCollection)
        {
            ovservableCollection.CollectionChanged += Children_CollectionChanged;
        }
    }

    private void AddSelectionTriggers(View nodeLine)
    {
        nodeLine.Triggers.Add(new DataTrigger(typeof(View))
        {
            Binding = new Binding("Selection"),
            Value = SelectionState.Selected,
            Setters =
            {
                new Setter
                {
                    Property = View.BackgroundColorProperty,
                    Value = SelectionColor.WithAlpha(.6f)
                }
            }
        });

        nodeLine.Triggers.Add(new DataTrigger(typeof(View))
        {
            Binding = new Binding("Selection"),
            Value = SelectionState.PartiallySelected,
            Setters =
            {
                new Setter
                {
                    Property = View.BackgroundColorProperty,
                    Value = SelectionColor.WithAlpha(.4f)
                }
            }
        });

        nodeLine.Triggers.Add(new DataTrigger(typeof(View))
        {
            Binding = new Binding("Selection"),
            Value = SelectionState.Unselected,
            Setters =
            {
                new Setter
                {
                    Property = View.BackgroundColorProperty,
                    Value = Colors.Transparent
                }
            }
        });
    }

    private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems)
            {
                var nodeView = new TreeViewNodeView(item as IHasChildrenTreeViewNode, ItemTemplate, ArrowTheme);
                nodeView.SetBinding(TreeViewNodeView.SelectionColorProperty,
                    new Binding(nameof(TreeView.SelectionColor), source: this));
                slChildrens.Children.Insert(e.NewStartingIndex, nodeView);
            }
        }

        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems)
            {
                slChildrens.Children.Remove(slChildrens.Children.FirstOrDefault(x => (x as View).BindingContext == item));
            }
        }
    }

    public void UpdateArrowTheme(NodeArrowTheme theme)
    {
        extendButton.Source = GetArrowSource(theme);

        if (slChildrens.Any())
        {
            foreach (var child in slChildrens.Children)
            {
                if (child is TreeViewNodeView treeViewNodeView)
                {
                    treeViewNodeView.UpdateArrowTheme(theme);
                }
            }
        }
    }

    protected virtual ImageSource GetArrowSource(NodeArrowTheme theme)
    {
        if (theme == NodeArrowTheme.Default)
        {
            return GetImageSource(Application.Current.RequestedTheme == AppTheme.Dark ? "down_light.png" : "down_dark.png");
        }
        else
        {
            return theme == NodeArrowTheme.Light ? GetImageSource("down_light.png") : GetImageSource("down_dark.png");
        }
    }

    protected ImageSource GetImageSource(string fileName)
    {
        return
            ImageSource.FromResource("TreeView.Maui.Resources." + fileName, GetType().Assembly);
    }
}
public enum NodeArrowTheme
{
    Default,
    Light,
    Dark
}
