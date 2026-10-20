using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Shared.Logging;
using System;
using System.Collections.Generic;

namespace OpenNutriView
{
    [TooltipLibrary]
    public static class FoodItemExtension
    {
        // 2.1.3: the tooltips intentionally stay uncached for fresh data, but the underlying
        // world store/container scan is expensive; a short TTL cache keeps tooltips lively
        // while preventing a full scan on every single hover. Keys: (ownerId, foodTypeId).
        private static readonly object CacheLock = new();
        private static readonly Dictionary<(int OwnerId, int FoodTypeId), (DateTime At, LocString Value)> Cache = new();
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(5);

        [NewTooltip(CacheAs.Disabled, overrideType: typeof(Stomach))]
        public static LocString StomachTooltip(this Stomach stomach)
        {
            try
            {
                var ownerId = stomach.Owner.Id;
                lock (CacheLock)
                {
                    if (Cache.TryGetValue((ownerId, 0), out var hit) && DateTime.UtcNow - hit.At < CacheTtl)
                        return hit.Value;
                }
                var value = OpenNutriView.NextFood.Stomach(stomach);
                lock (CacheLock)
                    Cache[(ownerId, 0)] = (DateTime.UtcNow, value);
                return value;
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"[OpenNutriView] Failed to generate stomach tooltip for {stomach.Owner.Name}. See following exception:"));
                Log.WriteException(e);
                return LocString.Empty;
            }
        }

        [NewTooltip(CacheAs.Disabled, overrideType: typeof(FoodItem))]
        public static LocString FoodTasteTooltip(this FoodItem type, User user)
        {
            try
            {
                // 2.2.4: unified exception guard with the stomach path so a nutrition
                // computation failure cannot poison every food tooltip on the server.
                var key = (user.Id, type.TypeID);
                lock (CacheLock)
                {
                    if (Cache.TryGetValue(key, out var hit) && DateTime.UtcNow - hit.At < CacheTtl)
                        return hit.Value;
                }
                var value = OpenNutriView.NextFood.FoodItem(user.Player, type);
                lock (CacheLock)
                    Cache[key] = (DateTime.UtcNow, value);
                return value;
            }
            catch (Exception e)
            {
                Log.WriteError(Localizer.Do($"[OpenNutriView] Failed to generate food tooltip for {type.DisplayName} (user {user.Name}). See following exception:"));
                Log.WriteException(e);
                return LocString.Empty;
            }
        }
    }
}
