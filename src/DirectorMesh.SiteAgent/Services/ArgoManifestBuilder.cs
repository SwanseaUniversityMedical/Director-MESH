namespace DirectorMesh.SiteAgent.Services;

public static class ArgoManifestBuilder
{
    public static string BuildDataPlaneApp(
        string siteName, string dataPlaneName, string repoUrl, string manifestPath) =>
$"""
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: director-mesh-{Slug(siteName)}-{Slug(dataPlaneName)}
  namespace: argocd
spec:
  project: default
  source:
    repoURL: {repoUrl}
    targetRevision: HEAD
    path: {manifestPath}
  destination:
    server: https://kubernetes.default.svc
    namespace: director-mesh-{Slug(siteName)}
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
""";

    public static string BuildAppOfApps(string siteName, string repoUrl, string appsPath) =>
$"""
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: director-mesh-{Slug(siteName)}
  namespace: argocd
spec:
  project: default
  source:
    repoURL: {repoUrl}
    targetRevision: HEAD
    path: {appsPath}/{Slug(siteName)}
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
""";

    private static string Slug(string name) =>
        new string(name.ToLower().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
}
