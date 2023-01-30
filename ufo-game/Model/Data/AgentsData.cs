using System.Text.Json.Serialization;
using UfoGame.Infra;

namespace UfoGame.Model.Data;

public class AgentsData : IPersistable
{
    // kja NextAgentId can be derived, doesn't have to be saved
    [JsonInclude] public int NextAgentId { get; private set; }
    [JsonInclude] public List<AgentData> Data { get; private set; } = new List<AgentData>();

    public AgentsData()
        => Reset();

    public void Reset()
    {
        NextAgentId = 0;
        Data = new List<AgentData>();
    }

    public List<AgentData> AddNewRandomAgents(int agentsToAdd, int currentTime, RandomGen randomGen)
    {
        List<AgentData> addedAgentsData = Enumerable.Range(NextAgentId, agentsToAdd)
            .ToList()
            .Select(
                id => 
                    new AgentData(
                        id,
                        AgentNames.RandomName(randomGen),
                        currentTime)).ToList();
        Data.AddRange(addedAgentsData);
        NextAgentId += agentsToAdd;
        return addedAgentsData;
    }
}