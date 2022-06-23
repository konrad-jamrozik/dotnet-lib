using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record SoldierWeaponClassDecorations(
    // Order of fields taken from commendations ruleset file.
    int Monster,
    int Wrestler,
    int Slasher,
    int Rocket,
    int Trooper,
    int Sniper,
    int Shotgun,
    int Gunslinger,
    int Assaulter,
    int Cannoneer,
    int Bombardier,
    int Warrior,
    int Technician,
    int Traditionalist,
    int Incapacitator,
    int JungleMower,
    int Gunner,
    int Purifier,
    int Grenadier,
    int Tasemaster,
    int Suppressor,
    int Sorcerer)
{
    public static SoldierWeaponClassDecorations FromDiary(Diary diary)
        => new SoldierWeaponClassDecorations(
            diary.Decoration("STR_MEDAL_MONSTER_NAME"),
            diary.Decoration("STR_MEDAL_WRESTLER_NAME"),
            diary.Decoration("STR_MEDAL_SLASHER_NAME"),
            diary.Decoration("STR_MEDAL_ROCKET_SCIENTIST_NAME"),
            diary.Decoration("STR_MEDAL_TROOPER_NAME"),
            diary.Decoration("STR_MEDAL_SNIPER_NAME"),
            diary.Decoration("STR_MEDAL_SHOTGUN_SURGEON_NAME"),
            diary.Decoration("STR_MEDAL_GUNSLINGER_NAME"),
            diary.Decoration("STR_MEDAL_ASSAULTER_NAME"),
            diary.Decoration("STR_MEDAL_CANNONEER_NAME"),
            diary.Decoration("STR_MEDAL_BOMBARDIER_NAME"),
            diary.Decoration("STR_MEDAL_WARRIOR_NAME"),
            diary.Decoration("STR_MEDAL_TECHNICIAN_NAME"),
            diary.Decoration("STR_MEDAL_TRADITIONALIST_NAME"),
            diary.Decoration("STR_MEDAL_INCAPACITATOR_NAME"),
            diary.Decoration("STR_MEDAL_JUNGLE_MOWER_NAME"),
            diary.Decoration("STR_MEDAL_GUNNER_NAME"),
            diary.Decoration("STR_MEDAL_PURIFIER_NAME"),
            diary.Decoration("STR_MEDAL_GRENADIER_NAME"),
            diary.Decoration("STR_MEDAL_TASEMASTER_NAME"),
            diary.Decoration("STR_MEDAL_SUPPRESSOR_NAME"),
            diary.Decoration("STR_MEDAL_SORCERER_NAME"));

    public static IEnumerable<string> CsvHeaders() => new[]
    {
        "Monster",
        "Wrestler",
        "Slasher",
        "Rocket",
        "Trooper",
        "Sniper",
        "Shotgun",
        "Gunslinger",
        "Assaulter",
        "Cannoneer",
        "Bombardier",
        "Warrior",
        "Technician",
        "Traditionalist",
        "Incapacitator",
        "JungleMower",
        "Gunner",
        "Purifier",
        "Grenadier",
        "Tasemaster",
        "Suppressor",
        "Sorcerer"
    };

    public IEnumerable<(string Name, object Value)> AsKeyValueTuples()
    {
        var propertyInfos = typeof(SoldierWeaponClassDecorations).GetProperties();
        return propertyInfos.Select(p => (p.Name, p.GetValue(this)));
    }
}