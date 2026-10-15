namespace SmartWarehouse.Web.Simulation;

public interface ISimulationRunner
{
    Task<bool> QueueAsync(string flow, CancellationToken cancellationToken = default);
    Task InjectFaultAsync(CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}

public sealed record SimulationUpdate(string Flow, string State, int Percent, string Message, string? Epc, string? TaskNumber, DateTimeOffset UpdatedAt);

