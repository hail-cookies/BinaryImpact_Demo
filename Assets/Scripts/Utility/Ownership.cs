public class Ownership<T>
{
    public delegate bool ClaimHandler(T obj);
    ClaimHandler _callBack;
    uint nextKey = 1;
    uint currentKey = 1;
    public T This { get; private set; }
    public bool Claimed { get => currentKey > 0; }

    public Ownership(T reference)
    {
        This = reference;
    }

    public bool HasAccess(uint key) => key == currentKey && key > 0;

    public bool Claim(ClaimHandler callBack, out uint key)
    {
        key = 0;

        if (_callBack != null)
        {
            if (_callBack.Invoke(This))
            {
                key = currentKey = nextKey++;
                _callBack = callBack;
                return true;
            }
        }
        else
        {
            key = currentKey = nextKey++;
            _callBack = callBack;
            return true;
        }

        return false;
    }

    public bool Release(uint key)
    {
        if (key == currentKey && key > 0)
        {
            _callBack = null;
            currentKey = 0;
            return true;
        }

        return false;
    }
}
