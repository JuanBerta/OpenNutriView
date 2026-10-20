using Eco.Core.Plugins.Interfaces;
using Eco.Core.Plugins;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.IO;
namespace OpenNutriView
{
    public class OpenNutriViewPlugin :
      Singleton<OpenNutriViewPlugin>,
      IModKitPlugin,
      IServerPlugin,
      ISaveablePlugin,
      IModInit
    {
        private readonly OpenNutriViewData data;

        public OpenNutriViewPlugin()
        {
            try
            {
                data = StorageManager.LoadOrCreate<OpenNutriViewData>("OpenNutriView", null);
            }
            catch
            {
                // 2.2.6: back up corrupt storage files (rename to .corrupt.<timestamp>) instead of
                // deleting them, so a single bad save cannot wipe every player's configuration.
                foreach (string file in StorageManager.GetFiles("OpenNutriView"))
                {
                    try
                    {
                        if (File.Exists(file))
                            File.Move(file, file + ".corrupt." + System.DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                    }
                    catch { }
                }
                data = StorageManager.LoadOrCreate<OpenNutriViewData>("OpenNutriView", null);
            }
        }

        public string GetCategory() => Localizer.DoStr("Mods");

        public string GetStatus() => string.Empty;

        public void SaveAll() => Singleton<StorageManager>.Obj.MarkDirty(data);

        public static ModRegistration Register() => new()
        {
            ModName = "OpenNutriView",
            ModDescription = "Enhances stomach and food item tooltips to help you eat the best food for a balanced and nutritious stomach content.",
            ModDisplayName = "OpenNutriView",
        };
    }
}
