using Eco.Core.Controller;
using Eco.Core.Utils;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.PropertyHandling;
using Eco.Gameplay.Utils;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Utils;
using PropertyChanged;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System;

namespace OpenNutriView
{
    [Serialized]
    public class ONVUserConfig
    {
        [Serialized] public float MinimumNutrients { get; set; } = 0;
        [Serialized] public int RecommendationCount { get; set; } = 3;
        [Serialized] public float MaxCostPer1000Calories { get; set; } = float.PositiveInfinity;
        [Serialized] public float MaxShopDistance { get; set; } = float.PositiveInfinity;
        [Serialized] public bool SortByCheapestPer1000Calories { get; set; } = false;   // 2.1.1
        [Serialized] public ThreadSafeList<int> IgnoredCurrencies = new();
        [Serialized] public ThreadSafeList<int> IgnoredFoodTypeIds = new();            // 2.1.2 food blacklist

        public ONVUserConfigUI ToUI()
        {
            ONVUserConfigUI ui = new()
            {
                MaxCostPerThousandCalories = MaxCostPer1000Calories,
                MaxShopDistance = MaxShopDistance,
                MinimumNutrients = MinimumNutrients,
                RecommendationCount = RecommendationCount,
                SortByCheapestPer1000Calories = SortByCheapestPer1000Calories,
            };
            var idToCurrency = new Dictionary<int, Currency>();
            CurrencyManager.Currencies.ForEach(c => idToCurrency.Add(c.Id, c));
            IgnoredCurrencies.Select(id => idToCurrency.GetOrDefault(id)).Where(c => c != null).ForEach(c => ui.IgnoredCurrencies.Add(c!));
            ui.IgnoredFoods = new ControllerList<Item>(ui, nameof(ui.IgnoredFoods));
            IgnoredFoodTypeIds.Select(id => Item.Get(id)).Where(i => i != null).ForEach(i => ui.IgnoredFoods.Add(i!));
            return ui;
        }

        public void UpdateFromUI(ONVUserConfigUI config)
        {
            // 2.2.3: sanitize user-entered numbers - NaN / negative values would silently
            // disable every filter (or filter everything out); treat them as "no limit".
            MaxCostPer1000Calories = SanitizePositive(config.MaxCostPerThousandCalories);
            MinimumNutrients = SanitizePositive(config.MinimumNutrients);
            RecommendationCount = Math.Clamp(config.RecommendationCount, 1, 10);
            MaxShopDistance = SanitizePositive(config.MaxShopDistance);
            SortByCheapestPer1000Calories = config.SortByCheapestPer1000Calories;
            IgnoredCurrencies.Clear();
            IgnoredCurrencies.AddRange(config.IgnoredCurrencies.Select(c => c.Id));
            IgnoredFoodTypeIds.Clear();
            IgnoredFoodTypeIds.AddRange(config.IgnoredFoods.Select(i => i.TypeID));
        }

        private static float SanitizePositive(float value)
            => float.IsNaN(value) || value < 0 ? float.PositiveInfinity : value;
    }

    public class ONVUserConfigUI : IController, INotifyPropertyChanged, Eco.Core.PropertyHandling.INotifyPropertyChangedInvoker, IHasClientControlledContainers
    {
        [Eco] public float MinimumNutrients { get; set; } = 0;

        [Eco, Range(1, 10)] public int RecommendationCount { get; set; } = 3;

        [Eco] public float MaxCostPerThousandCalories { get; set; } = float.PositiveInfinity;

        [Eco] public float MaxShopDistance { get; set; } = float.PositiveInfinity;
        [Eco, AllowEmpty] public ControllerList<Currency> IgnoredCurrencies { get; set; }

        // 2.1.1: toggle whether the first recommendation list prefers the cheapest food per 1000 kcal.
        [Eco] public bool SortByCheapestPer1000Calories { get; set; } = false;

        // 2.1.2: food blacklist - foods selected here are excluded from tooltip recommendations.
        [Eco, AllowEmpty] public ControllerList<Item> IgnoredFoods { get; set; }

        public ONVUserConfigUI()
        {
            IgnoredCurrencies = new ControllerList<Currency>(this, nameof(IgnoredCurrencies));
            IgnoredFoods = new ControllerList<Item>(this, nameof(IgnoredFoods));
        }


        #region IController
        public event PropertyChangedEventHandler? PropertyChanged;
        int controllerID;
        [DoNotNotify] public ref int ControllerID => ref controllerID;

        public void InvokePropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, eventArgs);
        }

        protected void OnPropertyChanged(string propertyName, object before, object after) => PropertyChangedNotificationInterceptor.Intercept(this, propertyName, before, after);
        #endregion
    }
}
