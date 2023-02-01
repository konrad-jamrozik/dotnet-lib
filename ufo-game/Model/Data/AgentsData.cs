using System.Text.Json.Serialization;
using UfoGame.Infra;

namespace UfoGame.Model.Data;

public class AgentsData : IPersistable
{
    [JsonInclude] public List<AgentData> Data { get; private set; } = new List<AgentData>();

    public AgentsData()
        => Reset();

    public void Reset()
    {
        Data = new List<AgentData>();
    }

    public List<AgentData> AddNewRandomAgents(int agentsToAdd, int currentTime, RandomGen randomGen)
    {
        List<AgentData> addedAgentsData = Enumerable.Range(start: NextAgentId, count: agentsToAdd)
            .ToList()
            .Select(
                id => 
                    new AgentData(
                        id,
                        AgentNames.RandomName(randomGen),
                        currentTime)).ToList();
        Data.AddRange(addedAgentsData);
        return addedAgentsData;
    }

    private int NextAgentId => Data.Count;
}