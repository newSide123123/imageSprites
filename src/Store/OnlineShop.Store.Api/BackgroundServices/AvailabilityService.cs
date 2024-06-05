namespace OnlineShop.Store.Api.BackgroundServices;

public class AvailabilityService
{
    private bool _broken;

    public bool IsBroken()
    {
        return _broken;
    }

    public void Break()
    {
        _broken = true;
    }
}