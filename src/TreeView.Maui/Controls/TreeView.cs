using System.Collections;
using System.Collections.Specialized;
using TreeView.Maui.Core;

namespace TreeView.Maui.Controls;

public class TreeView : ContentView
{
    private StackLayout _root = new StackLayout { Spacing = 0 };

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(TreeView), null, propertyChanging: (b, o, n) => (b as TreeView).OnItemsSourceSetting(o as IEnumerable, n as IEnumerable), propertyChanged: (b,o,v) => (b as TreeView).OnItemsSourceSet() );

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(TreeView), new DataTemplate(typeof(DefaultTreeViewNodeView)), propertyChanged: (b, o, n) => (b as TreeView).OnItemTemplateChanged());

    public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public DataTemplate ItemTemplate { get => (DataTemplate)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

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
                        _root.Children.Insert(e.NewStartingIndex, new TreeViewNodeView(item as IHasChildrenTreeViewNode, ItemTemplate));
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
                _root.Children.Add(new TreeViewNodeView(node, ItemTemplate));
            }
        }
    }
}

public class TreeViewNodeView : ContentView
{
    public TreeViewNodeView(IHasChildrenTreeViewNode node, DataTemplate itemTemplate)
    {
        var sl = new StackLayout {  Spacing = 0 };
        BindingContext = node;
        Content = sl;

        var slChildrens = new StackLayout { IsVisible = node.IsExtended, Margin = new Thickness(10, 0, 0, 0), Spacing = 0 };

        var extendButton = new ImageButton
        {
            Source = Application.Current.RequestedTheme == AppTheme.Dark ? "down_light.png" : "down_dark.png",
            VerticalOptions = LayoutOptions.Center,
            Opacity = node.IsLeaf ? 0 : 1,
            Rotation = node.IsExtended ? 0 : -90,
            HeightRequest = 30,
            WidthRequest = 30,
        };

        extendButton.Clicked += (s, e) =>
        {
            node.IsExtended = !node.IsExtended;
            slChildrens.IsVisible = node.IsExtended;

            if (node.IsExtended)
            {
                extendButton.RotateTo(0);

                if (node is ILazyLoadTreeViewNode lazyNode && lazyNode.GetChildren != null && !lazyNode.Children.Any())
                {
                    foreach (var child in lazyNode.GetChildren(lazyNode))
                    {
                        lazyNode.Children.Add(child);
                        slChildrens.Add(new TreeViewNodeView(child, itemTemplate));
                    }

                    if(!lazyNode.Children.Any())
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

        var content = itemTemplate.CreateContent() as View;

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
            slChildrens.Children.Add(new TreeViewNodeView(child, itemTemplate));
        }

        sl.Children.Add(slChildrens);
    }
}
