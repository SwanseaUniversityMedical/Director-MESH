using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DirectorMesh.Web.Models.Domain;

namespace DirectorMesh.Web.Services;

public class ConfigurationGeneratorService(
    ILogger<ConfigurationGeneratorService> logger,
    IConfiguration configuration) : IConfigurationGeneratorService
{
    public Task<SiteConfiguration> GenerateAsync(Site site, DataPlane dataPlane, IEnumerable<AclRule> dataPlaneRules, CancellationToken ct = default)
    {
        var rules = dataPlaneRules.ToList();
        var dataPlaneServices = site.Services.Where(s => s.DataPlaneId == dataPlane.Id).ToList();
        var managementUrl = configuration["NetBird:ManagementUrl"] ?? "https://api.netbird.io";
        var signalUrl = configuration["NetBird:SignalUrl"] ?? "https://signal.netbird.io";
        var namespaceName = $"dm-{Slug(site.Name)}-{Slug(dataPlane.Name)}";
 
        var configObj = new
        {
            siteId = site.Id,
            siteName = site.Name,
            dataPlaneId = dataPlane.Id,
            dataPlane = dataPlane.Name,
            k8sNamespace = namespaceName,
            generatedAt = DateTime.UtcNow,
            services = dataPlaneServices.Select(s => new
            {
                s.Id,
                s.Name,
                direction = s.Direction.ToString(),
                protocol = s.Protocol.ToString(),
                s.Port,
                s.NetBirdPeerName,
                s.NetBirdDnsName,
                s.NetBirdIpAddress,
                s.NetBirdSetupKey,
                s.LocalEndpoint,
                s.RemoteNetBirdDnsName,
                s.IsEnabled
            }),
            aclRules = rules.Select(r => new
            {
                r.Id,
                r.Name,
                sourceSite = r.SourceService.Site.Name,
                sourceService = r.SourceService.Name,
                destinationSite = r.DestinationService.Site.Name,
                destinationService = r.DestinationService.Name,
                action = r.Action.ToString(),
                protocol = r.DestinationService.Protocol.ToString(),
                port = r.DestinationService.Port
            })
        };

        var json = JsonSerializer.Serialize(configObj, new JsonSerializerOptions { WriteIndented = true });
        var contentHash = ComputeHash(json);
        var k8s = GenerateK8sManifests(dataPlaneServices, namespaceName, managementUrl, signalUrl);

        var config = new SiteConfiguration
        {
            SiteId = site.Id,
            DataPlaneId = dataPlane.Id,
            Status = ConfigurationStatus.Draft,
            ApprovalToken = Guid.NewGuid().ToString("N"),
            ContentHash = contentHash,
            ConfigurationJson = json,
            HaProxyConfig = null,
            K8sManifestYaml = k8s,
            CreatedAt = DateTime.UtcNow
        };

        logger.LogInformation("Generated configuration for site {SiteId} data plane {DataPlaneId} hash {Hash}",
            site.Id, dataPlane.Id, contentHash[..8]);
        return Task.FromResult(config);
    }

    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static string GenerateK8sManifests(IEnumerable<NetworkService> services, string namespaceName, string managementUrl, string signalUrl)
    {
        var sb = new StringBuilder();

        sb.AppendLine("---");
        sb.AppendLine("apiVersion: v1");
        sb.AppendLine("kind: Namespace");
        sb.AppendLine("metadata:");
        sb.AppendLine($"  name: {namespaceName}");
        sb.AppendLine();

        foreach (var svc in services.Where(s => s.IsEnabled))
        {
            var slug = Slug(svc.Name);
            AppendServiceManifests(sb, svc, slug, namespaceName, managementUrl, signalUrl);
        }

        return sb.ToString();
    }

    private static void AppendServiceManifests(StringBuilder sb, NetworkService svc, string slug,
        string namespaceName, string managementUrl, string signalUrl)
    {
        sb.AppendLine("---");
        sb.AppendLine("apiVersion: v1");
        sb.AppendLine("kind: Secret");
        sb.AppendLine("metadata:");
        sb.AppendLine($"  name: netbird-{slug}");
        sb.AppendLine($"  namespace: {namespaceName}");
        sb.AppendLine("type: Opaque");
        sb.AppendLine("stringData:");
        sb.AppendLine($"  setup-key: \"{svc.NetBirdSetupKey ?? "REPLACE_WITH_SETUP_KEY"}\"");
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine("apiVersion: v1");
        sb.AppendLine("kind: ConfigMap");
        sb.AppendLine("metadata:");
        sb.AppendLine($"  name: haproxy-{slug}");
        sb.AppendLine($"  namespace: {namespaceName}");
        sb.AppendLine("data:");
        sb.AppendLine("  haproxy.cfg: |");
        AppendHaProxyConfig(sb, svc);
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine("apiVersion: apps/v1");
        sb.AppendLine("kind: Deployment");
        sb.AppendLine("metadata:");
        sb.AppendLine($"  name: {slug}");
        sb.AppendLine($"  namespace: {namespaceName}");
        sb.AppendLine("  labels:");
        sb.AppendLine($"    app: {slug}");
        sb.AppendLine("spec:");
        sb.AppendLine("  replicas: 1");
        sb.AppendLine("  selector:");
        sb.AppendLine("    matchLabels:");
        sb.AppendLine($"      app: {slug}");
        sb.AppendLine("  template:");
        sb.AppendLine("    metadata:");
        sb.AppendLine("      labels:");
        sb.AppendLine($"        app: {slug}");
        sb.AppendLine("    spec:");
        sb.AppendLine("      hostNetwork: true");
        sb.AppendLine("      dnsPolicy: ClusterFirstWithHostNet");
        sb.AppendLine("      containers:");
        sb.AppendLine("      - name: netbird");
        sb.AppendLine("        image: netbirdio/netbird:latest");
        sb.AppendLine("        securityContext:");
        sb.AppendLine("          privileged: true");
        sb.AppendLine("        env:");
        sb.AppendLine("        - name: NB_SETUP_KEY");
        sb.AppendLine("          valueFrom:");
        sb.AppendLine("            secretKeyRef:");
        sb.AppendLine($"              name: netbird-{slug}");
        sb.AppendLine("              key: setup-key");
        sb.AppendLine("        - name: NB_MANAGEMENT_URL");
        sb.AppendLine($"          value: \"{managementUrl}\"");
        sb.AppendLine("        - name: NB_SIGNAL_URL");
        sb.AppendLine($"          value: \"{signalUrl}\"");
        sb.AppendLine("        - name: NB_HOSTNAME");
        sb.AppendLine($"          value: \"{svc.NetBirdPeerName ?? slug}\"");
        sb.AppendLine("      - name: haproxy");
        sb.AppendLine("        image: haproxy:latest");
        sb.AppendLine("        ports:");
        sb.AppendLine($"        - containerPort: {svc.Port}");
        sb.AppendLine("        volumeMounts:");
        sb.AppendLine("        - name: haproxy-config");
        sb.AppendLine("          mountPath: /usr/local/etc/haproxy");
        sb.AppendLine("      volumes:");
        sb.AppendLine("      - name: haproxy-config");
        sb.AppendLine("        configMap:");
        sb.AppendLine($"          name: haproxy-{slug}");
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine("apiVersion: v1");
        sb.AppendLine("kind: Service");
        sb.AppendLine("metadata:");
        sb.AppendLine($"  name: {slug}");
        sb.AppendLine($"  namespace: {namespaceName}");
        sb.AppendLine("spec:");
        sb.AppendLine("  selector:");
        sb.AppendLine($"    app: {slug}");
        sb.AppendLine("  ports:");
        sb.AppendLine($"  - port: {svc.Port}");
        sb.AppendLine($"    targetPort: {svc.Port}");
        sb.AppendLine($"    name: port-{svc.Port}");
        sb.AppendLine();
    }

    private static void AppendHaProxyConfig(StringBuilder sb, NetworkService svc)
    {
        const string indent = "    ";
        sb.AppendLine($"{indent}global");
        sb.AppendLine($"{indent}  log stdout format raw local0");
        sb.AppendLine($"{indent}defaults");
        sb.AppendLine($"{indent}  log global");
        sb.AppendLine($"{indent}  mode tcp");
        sb.AppendLine($"{indent}  timeout connect 10s");
        sb.AppendLine($"{indent}  timeout client 1h");
        sb.AppendLine($"{indent}  timeout server 1h");
        sb.AppendLine($"{indent}frontend frontend");
        sb.AppendLine($"{indent}  bind 0.0.0.0:{svc.Port}");
        sb.AppendLine($"{indent}  default_backend backend");
        sb.AppendLine($"{indent}backend backend");
        sb.AppendLine($"{indent}  mode tcp");
        sb.AppendLine($"{indent}  option tcp-check");

        if (svc.Direction == ServiceDirection.Inbound)
        {
            var target = svc.LocalEndpoint ?? $"localhost:{svc.Port}";
            sb.AppendLine($"{indent}  server local {target} check");
        }
        else
        {
            var remote = svc.RemoteNetBirdDnsName ?? "UNCONFIGURED";
            sb.AppendLine($"{indent}  server remote {remote}:{svc.Port} check");
        }
    }

    private static string Slug(string name) =>
        new string(name.ToLower().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
}
