# Check KIND
kubectl cluster-info
kubectl get nodes

# Install ArgoCD
kubectl create namespace argocd

kubectl apply -n argocd --server-side --force-conflicts `
  -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Wait for it
kubectl get pods -n argocd -w

# Get admin password
$encoded = kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}"
[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($encoded))

# Expose UI
kubectl port-forward svc/argocd-server -n argocd 8088:443