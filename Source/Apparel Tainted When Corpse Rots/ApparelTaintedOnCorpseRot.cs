using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ApparelTaintedOnCorpseRot
{
    [StaticConstructorOnStartup]
    public static class ApparelTaintedOnCorpseRot
    {
        static ApparelTaintedOnCorpseRot()
        {
            var harmony = new Harmony("Inglix.ApparelTaintedOnCorpseRot");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker))]
    [HarmonyPatch(nameof(Pawn_ApparelTracker.Notify_PawnKilled))]
    class PatchPawnKilled
    {
        static bool Prefix(Pawn_ApparelTracker __instance, DamageInfo? dinfo)
        {
            if (dinfo == null || !dinfo.Value.Def.ExternalViolenceFor(__instance.pawn)) return false;
            for (var i = 0; i < __instance.GetDirectlyHeldThings().Count; i++)
            {
                if (!__instance.GetDirectlyHeldThings()[i].def.useHitPoints) continue;
                var num = Mathf.RoundToInt(__instance.GetDirectlyHeldThings()[i].HitPoints * Rand.Range(0.15f, 0.4f));
                __instance.GetDirectlyHeldThings()[i].TakeDamage(new DamageInfo(dinfo.Value.Def, num));
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Corpse))]
    [HarmonyPatch(nameof(Corpse.RotStageChanged))]
    class PatchRotStateChanged
    {
        static void Postfix(Corpse __instance)
        {
            var rot = __instance.TryGetComp<CompRottable>();
            if (rot == null || rot.Stage == RotStage.Fresh) return;
            var corpseOwner = __instance.InnerPawn;
            if (corpseOwner?.apparel == null) return;
            var corpseApparel = (ThingOwner<Apparel>)corpseOwner.apparel.GetDirectlyHeldThings();
            foreach (var t in corpseApparel)
            {
                t.Notify_PawnKilled();
            }
        }
    }
}