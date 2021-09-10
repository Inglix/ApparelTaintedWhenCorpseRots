using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (dinfo != null && dinfo.Value.Def.ExternalViolenceFor(__instance.pawn))
            {
                for (int i = 0; i < __instance.GetDirectlyHeldThings().Count; i++)
                {
                    if (__instance.GetDirectlyHeldThings()[i].def.useHitPoints)
                    {
                        int num = Mathf.RoundToInt((float)__instance.GetDirectlyHeldThings()[i].HitPoints * Rand.Range(0.15f, 0.4f));
                        __instance.GetDirectlyHeldThings()[i].TakeDamage(new DamageInfo(dinfo.Value.Def, (float)num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
                    }
                }
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
            Pawn corpseOwner = __instance.InnerPawn;
            if (corpseOwner.apparel != null) 
            {
                ThingOwner<Apparel> corpseApparel = (ThingOwner<Apparel>)corpseOwner.apparel.GetDirectlyHeldThings();
                for (int i = 0; i < corpseApparel.Count; i++)
                {
                    corpseApparel[i].Notify_PawnKilled();
                }
            }

        }
    }
}
