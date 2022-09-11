namespace TreeView.Maui;
public class GenericTriggerAction<T> : TriggerAction<T>
    where T : BindableObject
{
    private readonly Action<T> action;

    public GenericTriggerAction(Action<T> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        this.action = action;
    }
    protected override void Invoke(T sender)
    {
        action(sender);
    }
}
