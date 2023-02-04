using MusicParty.MusicApi;

namespace MusicParty;

public static class Extensions
{
    public static bool TryGetMusicApi(this IEnumerable<IMusicApi> apis, string name, out IMusicApi? api)
    {
        api = apis.FirstOrDefault(n => n.ServiceName == name);
        return api is not null;
    }

    public static bool TryGetMusicApiServiceBinding(this User user, string serviceName, out string? identifier)
    {
        return user.MusicApiServiceBindings.TryGetValue(serviceName, out identifier);
    }

    public static dynamic BuildResponseMessageWithCode(this string message, int code)
    {
        return new { Code = code, Message = message };
    }
}