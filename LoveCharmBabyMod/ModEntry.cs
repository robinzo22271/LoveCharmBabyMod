using StardewModdingAPI;
using StardewModdingAPI.Events;
using HarmonyLib;
using StardewValley;

namespace LoveCharmBabyMod
{
    public class ModEntry : Mod
    {
        private static bool forceBabyTonight = false;
        private const int LoveCharmItemID = 787; // Magic Rock Candy (can change later)

        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.dayUpdate)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ForceBabyPostfix))
            );
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            var item = Game1.player.CurrentItem;
            if (item != null && item.ParentSheetIndex == LoveCharmItemID)
            {
                forceBabyTonight = true;
                Game1.showGlobalMessage("The Love Charm glows warmly...");
                Game1.player.removeItemFromInventory(item); // Remove item
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            forceBabyTonight = false;
        }

        public static void ForceBabyPostfix(NPC __instance)
        {
            if (!forceBabyTonight) return;

            var player = Game1.player;
            if (__instance.isMarried() &&
                !__instance.isRoommate() &&
                player.spouse == __instance.Name &&
                player.getChildrenCount() < 2 &&
                player.houseUpgradeLevel >= 2)
            {
                Game1.player.currentLocation.createQuestionDialogue(
                    $"Would you like to have a child with {__instance.displayName}?",
                    Game1.currentLocation.createYesNoResponses(),
                    "babyQuestion"
                );
                forceBabyTonight = false;
            }
        }
    }
}
