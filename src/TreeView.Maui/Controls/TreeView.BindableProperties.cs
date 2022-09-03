using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeView.Maui.Controls;
public partial class TreeView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(TreeView), null, propertyChanging: (b, o, n) => (b as TreeView).OnItemsSourceSetting(o as IEnumerable, n as IEnumerable), propertyChanged: (b, o, v) => (b as TreeView).OnItemsSourceSet());
    public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(TreeView), new DataTemplate(typeof(DefaultTreeViewNodeView)), propertyChanged: (b, o, n) => (b as TreeView).OnItemTemplateChanged());
    public DataTemplate ItemTemplate { get => (DataTemplate)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    public NodeArrowTheme ArrowTheme { get => (NodeArrowTheme)GetValue(ArrowThemeProperty); set => SetValue(ArrowThemeProperty, value); }

    public static readonly BindableProperty ArrowThemeProperty =
        BindableProperty.Create(nameof(ArrowTheme), typeof(NodeArrowTheme), typeof(TreeView), defaultValue: NodeArrowTheme.Default,
            propertyChanged: (bo, ov, nv) => (bo as TreeView).OnArrowThemeChanged());
}