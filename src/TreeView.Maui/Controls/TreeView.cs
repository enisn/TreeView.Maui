using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.Specialized;
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
                        _root.Children.Insert(e.NewStartingIndex, new TreeViewNodeView(item as IHasChildrenTreeViewNode, ItemTemplate, ArrowTheme));
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
                _root.Children.Add(new TreeViewNodeView(node, ItemTemplate, ArrowTheme));
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
                extendButton.RotateTo(0);

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
            else
            {
                extendButton.RotateTo(-90);
            }
        };

        var content = ItemTemplate.CreateContent() as View;

        sl.Children.Add(new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Children =
            {
                extendButton,
                content
            }
        });

        foreach (var child in node.Children)
        {
            slChildrens.Children.Add(new TreeViewNodeView(child, ItemTemplate, theme));
        }

        sl.Children.Add(slChildrens);

        if (Node.Children is INotifyCollectionChanged ovservableCollection)
        {
            ovservableCollection.CollectionChanged += Children_CollectionChanged;
        }
    }

    private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems)
            {
                slChildrens.Children.Insert(e.NewStartingIndex, new TreeViewNodeView(item as IHasChildrenTreeViewNode, ItemTemplate, ArrowTheme));
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
