using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using STRINGS;

namespace EntombedCritter
{
    public class EntombedCritterMonitor : GameStateMachine<EntombedCritterMonitor, EntombedCritterMonitor.Instance, IStateMachineTarget, EntombedCritterMonitor.Def>
    {
        public class Def : BaseDef
        {
        }

        public new class Instance : GameInstance
        {
            public Instance(IStateMachineTarget master, Def def)
                : base(master, def)
            {
            }

            delegate bool IsValidDigCellDelegate(DiggerMonitor.Instance instance, int cell, object arg);
            private static readonly IsValidDigCellDelegate isValidDigCellDelegate
                = AccessTools.MethodDelegate<IsValidDigCellDelegate>(
                    AccessTools.Method(typeof(DiggerMonitor.Instance), "IsValidDigCell"));

            public bool IsEntombed()
            {
                int cell = Grid.PosToCell(this);
                if( !Grid.IsSolidCell(cell))
                    return false;
                if(HasTag(GameTags.Creatures.Burrowed))
                {
                    // Hatches can unburrow even from built tiles, but there must be room above.
                    if( gameObject.GetSMI<BurrowMonitor.Instance>().EmergeIsClear())
                        return false;
                }
                if (smi.HasTag(GameTags.Creatures.Digger))
                {
                    // Shove voles can get out of almost everything.
                    if( isValidDigCellDelegate(gameObject.GetSMI<DiggerMonitor.Instance>(), Grid.PosToCell(this), null))
                        return false;
                }
                return true;
            }
        }

#pragma warning disable CS0649
        private State normal;

        private State entombed;
#pragma warning restore CS0649

        public override void InitializeStates(out BaseState default_state)
        {
            default_state = normal;
            normal.Transition(entombed, (Instance smi) => smi.IsEntombed(), UpdateRate.SIM_4000ms);
            entombed.Transition(normal, (Instance smi) => !smi.IsEntombed(), UpdateRate.SIM_4000ms)
                .ToggleNotification((Instance smi) => new Notification(STRINGS.ENTOMBEDCRITTER.NOTIFICATION_NAME,
                    NotificationType.BadMinor,
                    (List<Notification> notifications, object data)
                        => string.Concat(STRINGS.ENTOMBEDCRITTER.NOTIFICATION_TOOLTIP,
                            notifications.ReduceMessages(countNames: false)), expires: false));
        }
    }

    [HarmonyPatch(typeof(EntityTemplates))]
    public class EntityTemplates_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(AddCreatureBrain))]
        public static void AddCreatureBrain(GameObject prefab, ChoreTable.Builder chore_table, Tag species, string symbol_prefix)
        {
            // This should check !HasTag(GameTags.Robot), but at least for rovers that is set only after
            // this function is called :(. So try to get based on the species, which is hackish, but seems to work.
            if( species.Name.EndsWith("Species"))
                prefab.AddOrGetDef<EntombedCritterMonitor.Def>();
        }
    }
}
