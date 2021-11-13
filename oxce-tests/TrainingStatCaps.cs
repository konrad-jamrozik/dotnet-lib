using System.Collections.Generic;

namespace OxceTests
{
    public record TrainingStatCaps(
        int TU,
        int Stamina,
        int Health,
        int Bravery,
        int Reactions,
        int Firing,
        int Throwing,
        int Strength,
        int PsiStrength,
        int PsiSkill,
        int Melee,
        int Mana)
    {
        // @formatter:off
        private static readonly TrainingStatCaps SoldierTrainingStatCaps  = new TrainingStatCaps(60, 70, 35, 100, 60, 65, 55, 45, 100, 50, 60, 0);
        private static readonly TrainingStatCaps HybridTrainingStatCaps   = new TrainingStatCaps(60, 60, 35, 100, 70, 75, 60, 35, 90, 60, 75, 0);
        private static readonly TrainingStatCaps DogTrainingStatCaps      = new TrainingStatCaps(70, 90, 30, 20, 60, 0, 0, 1, 40, 0, 90, 0);
        private static readonly TrainingStatCaps RatTrainingStatCaps      = new TrainingStatCaps(65, 100, 18, 30, 30, 0, 0, 2, 45, 0, 60, 0);
        private static readonly TrainingStatCaps MuggleAITrainingStatCaps = new TrainingStatCaps(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        // @formatter:on

        public static readonly Dictionary<string, TrainingStatCaps> MapByType =
            new()
            {
                { "STR_SOLDIER", SoldierTrainingStatCaps },
                { "STR_HYBRID", HybridTrainingStatCaps },
                { "STR_DOG", DogTrainingStatCaps },
                { "STR_RAT", RatTrainingStatCaps },
                { "STR_MUGGLE_AI", MuggleAITrainingStatCaps },
            };
    }
}