using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OxceTests;

public record Commendations(IEnumerable<Commendation> CommendationData) : IEnumerable<Commendation>
{
    public static Commendations FromRulesetFileYaml(YamlMapping commendationsYaml)
    {
        var commendationsNodesLines =
            new YamlBlockSequence(commendationsYaml.Lines("commendations")).NodesLines();
        var soldierBonuses = commendationsNodesLines.Select(
            lines => Commendation.FromCommendationYaml(new YamlMapping(lines)));
        return new Commendations(soldierBonuses);
    }

    public IEnumerator<Commendation> GetEnumerator() => CommendationData.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}