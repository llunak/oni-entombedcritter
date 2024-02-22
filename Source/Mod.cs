using HarmonyLib;
using PeterHan.PLib.Database;

namespace EntombedCritter
{
    public class Mod : KMod.UserMod2
    {

        public override void OnLoad( Harmony harmony )
        {
            base.OnLoad( harmony );
            LocString.CreateLocStringKeys( typeof( STRINGS.ENTOMBEDCRITTER ));
            new PLocalization().Register();
        }
    }
}
