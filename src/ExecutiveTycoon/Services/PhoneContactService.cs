namespace ExecutiveTycoon.Services;

public sealed class PhoneContactService
{
    private readonly Logger _logger;

    public PhoneContactService(Logger logger)
    {
        _logger = logger;
    }

    public bool TryRegisterExecutiveBrokerContact()
    {
        _logger.Info("Phone contact integration not guaranteed on all SHVDN3 stacks; using LemonUI broker fallback for V1.");
        return false;
    }
}
