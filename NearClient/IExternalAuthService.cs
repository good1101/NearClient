namespace NearClient
{
    public interface IExternalAuthService
    {
        bool OpenUrl(string url);
    }
}