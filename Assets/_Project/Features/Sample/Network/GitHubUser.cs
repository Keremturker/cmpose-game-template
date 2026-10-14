using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Kturker.UnityBase.Features.Sample.Network
{
    // GET users/<login>, only the fields the card shows. Newtonsoft fills it by reflection, so IL2CPP must keep it.
    [Preserve]
    public sealed class GitHubUser
    {
        [JsonProperty("login")] public string Login;
        [JsonProperty("name")] public string Name;
        [JsonProperty("avatar_url")] public string AvatarUrl;
    }
}
