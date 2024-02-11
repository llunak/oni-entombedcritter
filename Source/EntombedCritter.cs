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

            public bool IsEntombed()
            {
                if(HasTag(GameTags.Creatures.Burrowed))
                    return false;
                if (smi.HasTag(GameTags.Creatures.Digger))
                    return false;
                return Grid.IsSolidCell(Grid.PosToCell(this));
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
