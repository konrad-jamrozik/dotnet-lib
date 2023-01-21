﻿using System.Text.Json.Serialization;
using UfoGame.Model.Data;

namespace UfoGame.Model;

// kja rename to MissionDeployment
public class MissionPrep
{
    [JsonInclude]
    public readonly MissionPrepData Data;

    public int MinAgentsSendableOnMission => 1;

    public int MaxAgentsSendableOnMission => Math.Min(
        Data.TransportCapacity,
        _staff.Data.AgentsSendableOnMissionCount);

    private readonly Staff _staff;

    public MissionPrep(MissionPrepData data, Staff staff)
    {
        Data = data;
        _staff = staff;
    }
}