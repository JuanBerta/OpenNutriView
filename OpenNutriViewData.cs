using Eco.Core.Serialization;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;

namespace OpenNutriView
{
    [Serialized]
    public class OpenNutriViewData : Singleton<OpenNutriViewData>, IStorage, ISerializable
    {
        [Serialized]
        public ThreadSafeDictionary<int, ONVUserConfig> UserConfiguration = new();

        public IPersistent StorageHandle { get; set; } = null!;

        public ONVUserConfig GetConfig(User user)
        {
            // 2.2.5: GetOrAdd semantics without the redundant TryRemove (the key was just
            // proven missing, so removing it again can never succeed and only wastes a lock).
            if (!UserConfiguration.TryGetValue(user.Id, out ONVUserConfig config))
            {
                config = new ONVUserConfig();
                UserConfiguration.Add(user.Id, config);
            }
            return config;
        }
    }
}
