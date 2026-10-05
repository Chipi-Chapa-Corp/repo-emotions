namespace RepoEmoteWheel;

// After an interruption, require a release before accepting another hold.
internal sealed class HoldState
{
    internal bool Active { get; private set; }
    private bool blocked;

    internal void Tick(bool allowed, bool held)
    {
        if (!held) blocked = false;
        else if (!allowed) blocked = true;
        Active = allowed && held && !blocked;
    }

    internal void Cancel(bool held)
    {
        Active = false;
        blocked = held;
    }
}
