﻿using UfoGame.Model.Data;

namespace UfoGame.Model;

public class MissionDeployment
{
    public readonly MissionDeploymentData Data;

    public int MinAgentsSendableOnMission => 1;

    public int MaxAgentsSendableOnMission => Math.Min(
        Data.TransportCapacity,
        _agents.AgentsSendableOnMissionCount);

    private readonly Agents _agents;

    public MissionDeployment(MissionDeploymentData data, Agents agents)
    {
        Data = data;
        _agents = agents;
    }
}