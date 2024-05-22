﻿using System.Collections.Concurrent;

namespace SupportCenter.SignalRHub;
public static class SignalRConnectionsDB
{
    public static ConcurrentDictionary<string, string> ConnectionIdByUser { get; } = new ConcurrentDictionary<string, string>();
    public static ConcurrentDictionary<string, string> AllConnections { get; } = new ConcurrentDictionary<string, string>();

}
