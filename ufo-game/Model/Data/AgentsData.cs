using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class AgentsData
{
    [JsonInclude] public int NextAgentId;
    [JsonInclude] public List<AgentData> Data { get; private set; } = new List<AgentData>();

    public AgentsData()
        => Reset();

    public void Reset()
    {
        NextAgentId = 0;
        Data = new List<AgentData>();
    }

    public List<AgentData> AddNewRandomAgents(int agentsToAdd, int currentTime)
    {
        List<AgentData> addedAgentsData = Enumerable.Range(NextAgentId, agentsToAdd)
            .ToList()
            .Select(
                id => 
                    new AgentData(
                        id,
                        AgentNames.RandomName(),
                        currentTime)).ToList();
        Data.AddRange(addedAgentsData);
        NextAgentId += agentsToAdd;
        return addedAgentsData;
    }
}