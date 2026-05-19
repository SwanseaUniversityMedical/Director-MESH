namespace DirectorMesh.Web.Services;

// Rewrites outbound backchannel HTTP requests from the public Keycloak base URL
// to an internal one (e.g. http://keycloak:8080) so the web container can reach
// Keycloak inside Docker while the browser still redirects to localhost:8080.
// Only active when Keycloak:InternalAuthority is set (docker-compose sets this).
internal sealed class KeycloakBackchannelHandler(string publicBase, string internalBase)
    : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is { } uri && uri.ToString().StartsWith(publicBase, StringComparison.OrdinalIgnoreCase))
            request.RequestUri = new Uri(internalBase + uri.ToString()[publicBase.Length..]);

        return base.SendAsync(request, cancellationToken);
    }
}
