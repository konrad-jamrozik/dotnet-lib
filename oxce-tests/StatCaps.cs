using System.Collections.Generic;

namespace OxceTests
{
    public record StatCaps(
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
        private static readonly StatCaps SoldierStatCaps  = new StatCaps(65, 90, 45, 100, 60, 90, 70, 50, 100, 50, 90, 100);
        private static readonly StatCaps HybridStatCaps   = new StatCaps(65, 70, 40, 100, 70, 90, 70, 40, 90, 90, 90, 80);
        private static readonly StatCaps DogStatCaps      = new StatCaps(100, 130, 50, 40, 110, 10, 10, 2, 40, 0, 140, 220);
        private static readonly StatCaps RatStatCaps      = new StatCaps(65, 100, 18, 30, 80, 0, 0, 2, 45, 0, 60, 80);
        private static readonly StatCaps MuggleAIStatCaps = new StatCaps(75, 90, 15, 0, 50, 100, 80, 0, 0, 0, 80, 100);
        // @formatter:on

        public static readonly Dictionary<string, StatCaps> MapByType =
            new()
            {
                { "STR_SOLDIER", SoldierStatCaps },
                { "STR_HYBRID", HybridStatCaps },
                { "STR_DOG", DogStatCaps },
                { "STR_RAT", RatStatCaps },
                { "STR_MUGGLE_AI", MuggleAIStatCaps },
            };
    }
}