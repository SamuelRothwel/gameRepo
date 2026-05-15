namespace coolbeats.scripts.logicScripts.AttachedLogic.units
{
    public partial class marine : unitControler
    {
        public marine()
        {
            unitKey = "marine";
            type = "attacker";
            radius = 30;
            detectionRadius = 150;
            maxHP = 50;
            traits.SetNumber("speed", 1);
            traits.SetNumber("attackRange", 0);
            traits.SetDescription("role", "attacker");
            QueueRedraw();
        }
    }
}
