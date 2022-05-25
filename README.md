# TreeView.Maui
 A simple treeview control for MAUI

## Showcase

![](art/windows-demo.gif)

![](art/android-demo.gif)


## Getting Started

- Install `TreeView.Maui` from NuGet. _(coming soon)_

- Create your nodes with `TreeViewNode` class.

    ```csharp
    public class MainPageViewModel : BindableObject
    {
        public ObservableCollection<TreeViewNode> Nodes { get; set; } = new();

        public MainPageViewModel()
        {
            Nodes.Add(new TreeViewNode("A")
            {
                Children =
                {
                    new TreeViewNode("A.1"),
                    new TreeViewNode("A.2"),
                }
            });
            Nodes.Add(new TreeViewNode("B")
            {
                Children =
                {
                    new TreeViewNode("B.1")
                    {
                        Children =
                        {
                            new TreeViewNode("B.1.a"),
                            new TreeViewNode("B.1.b"),
                            new TreeViewNode("B.1.c"),
                            new TreeViewNode("B.1.d"),

                        }
                    },
                    new TreeViewNode("B.2"),
                }
            });
            Nodes.Add(new TreeViewNode("C"));
            Nodes.Add(new TreeViewNode("D"));
        }
    }
    ```

- Use `TreeView` in XAML page and bind the noes as `ItemsSource`

    ```xml
        <t:TreeView ItemsSource="{Binding Nodes}">
        </t:TreeView>
    ```

### Customizations

- Using a custom item template

    ```xml
    <t:TreeView ItemsSource="{Binding Nodes}">
        <t:TreeView.ItemTemplate>
            <DataTemplate>
                <HorizontalStackLayout>
                    <Image Source="folder.png"/>
                    <Label Text="{Binding Name}">
                    <Image source="check.png" IsVisible="{Binding IsSelected}">
                </HorizontalStackLayout>
            </DataTemplate>
        </t:TreeView.ItemTemplate>
    </t:TreeView>
    ```