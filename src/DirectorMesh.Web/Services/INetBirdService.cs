namespace DirectorMesh.Web.Services;

public interface INetBirdService
{
    Task<string?> CreatePeerGroupAsync(string groupName, CancellationToken ct = default);
    Task<bool> CreatePolicyAsync(string name, string sourceGroupId, string destinationGroupId, string protocol, int port, CancellationToken ct = default);
    Task<bool> DeletePolicyAsync(string policyId, CancellationToken ct = default);
    Task<string?> CreateSetupKeyAsync(string name, CancellationToken ct = default);
}
