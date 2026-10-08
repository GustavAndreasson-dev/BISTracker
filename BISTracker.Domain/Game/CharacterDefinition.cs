namespace BISTracker.Domain;

public enum GameVersion { Classic, Forever }
public enum CharacterClass { Druid, Hunter, Mage, Paladin, Priest, Rogue, Shaman, Warlock, Warrior }
public sealed record Specialization(string Id, string Name);

public static class CharacterDefinition
{
    private static readonly IReadOnlyDictionary<CharacterClass, IReadOnlyList<Specialization>> Specs =
        new Dictionary<CharacterClass, IReadOnlyList<Specialization>>
        {
            [CharacterClass.Druid] = Define("balance", "Balance", "feral", "Feral", "restoration", "Restoration"),
            [CharacterClass.Hunter] = Define("beast-mastery", "Beast Mastery", "marksmanship", "Marksmanship", "survival", "Survival"),
            [CharacterClass.Mage] = Define("arcane", "Arcane", "fire", "Fire", "frost", "Frost"),
            [CharacterClass.Paladin] = Define("holy", "Holy", "protection", "Protection", "retribution", "Retribution"),
            [CharacterClass.Priest] = Define("discipline", "Discipline", "holy", "Holy", "shadow", "Shadow"),
            [CharacterClass.Rogue] = Define("assassination", "Assassination", "combat", "Combat", "subtlety", "Subtlety"),
            [CharacterClass.Shaman] = Define("elemental", "Elemental", "enhancement", "Enhancement", "restoration", "Restoration"),
            [CharacterClass.Warlock] = Define("affliction", "Affliction", "demonology", "Demonology", "destruction", "Destruction"),
            [CharacterClass.Warrior] = Define("arms", "Arms", "fury", "Fury", "protection", "Protection")
        };

    public static IReadOnlyList<Specialization> Specializations(CharacterClass characterClass) =>
        Specs.TryGetValue(characterClass, out var specs) ? specs : throw new ArgumentException("Unknown class.", nameof(characterClass));

    public static Specialization Specialization(CharacterClass characterClass, string id) =>
        Specializations(characterClass).SingleOrDefault(spec => spec.Id == id)
            ?? throw new ArgumentException("The specialization does not belong to this class.", nameof(id));

    public static string VersionName(GameVersion version) => version switch
    {
        GameVersion.Classic => "WoW Classic (Vanilla)",
        GameVersion.Forever => "WoW Forever",
        _ => throw new ArgumentException("Unknown game version.", nameof(version))
    };

    private static IReadOnlyList<Specialization> Define(params string[] pairs) =>
        Array.AsReadOnly(Enumerable.Range(0, pairs.Length / 2).Select(index => new Specialization(pairs[index * 2], pairs[index * 2 + 1])).ToArray());
}
