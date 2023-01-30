using System.Net;
using System.Text;
using CTFServer.Models.Internal;
using CTFServer.Services.Interface;
using CTFServer.Utils;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Options;
using NPOI.SS.Formula.Functions;

namespace CTFServer.Services;

public class K8sService : IContainerService
{
    private const string Namespace = "gzctf-challenges";
    private const string NetworkPolicy = "gzctf-policy";

    private readonly ILogger<K8sService> logger;
    private readonly Kubernetes kubernetesClient;
    private readonly string hostIP;
    private readonly string publicEntry;
    private readonly string? AuthSecretName;

    public K8sService(IOptions<RegistryConfig> _registry, IOptions<ContainerProvider> _provider, ILogger<K8sService> _logger)
    {
        logger = _logger;
        publicEntry = _provider.Value.PublicEntry;

        if (!File.Exists("k8sconfig.yaml"))
        {
            LogHelper.SystemLog(logger, "Unable to load K8s config file, please ensure that /app/k8sconfig.yaml is mounted");
            throw new FileNotFoundException("k8sconfig.yaml");
        }

        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile("k8sconfig.yaml");

        hostIP = config.Host[(config.Host.LastIndexOf('/') + 1)..config.Host.LastIndexOf(':')];

        kubernetesClient = new Kubernetes(config);

        var registry = _registry.Value;
        var withAuth = !string.IsNullOrWhiteSpace(registry.ServerAddress)
            && !string.IsNullOrWhiteSpace(registry.UserName)
            && !string.IsNullOrWhiteSpace(registry.Password);

        if (withAuth)
        {
            var padding = Codec.StrMD5($"{registry.UserName}@{registry.Password}@{registry.ServerAddress}");
            AuthSecretName = $"{registry.UserName}-{padding}";
        }

        InitK8s(withAuth, registry);

        logger.SystemLog($"K8s service started ({config.Host})", TaskStatus.Success, LogLevel.Debug);
    }

    public async Task<Container?> CreateContainerAsync(ContainerConfig config, CancellationToken token = default)
    {
        // first, get only the image name
        var imageName = $"{config.Image.Split("/").LastOrDefault()?.Split(":").FirstOrDefault()}";
        // use uuid avoid conflict
        var name = $"{imageName}-{Guid.NewGuid().ToString("N")[..16]}"
            .Replace('_', '-'); // ensure name is available

        var pod = new V1Pod("v1", "Pod")
        {
            Metadata = new V1ObjectMeta()
            {
                Name = name,
                NamespaceProperty = Namespace,
                Labels = new Dictionary<string, string>()
                {
                    ["ctf.gzti.me/ResourceId"] = name,
                    ["ctf.gzti.me/ImageId"] = imageName,
                    ["ctf.gzti.me/TeamId"] = config.TeamId,
                    ["ctf.gzti.me/UserId"] = config.UserId,
                }
            },
            Spec = new V1PodSpec()
            {
                ImagePullSecrets = AuthSecretName is null ?
                    Array.Empty<V1LocalObjectReference>() :
                    new List<V1LocalObjectReference>() { new() { Name = AuthSecretName } },
                DnsPolicy = "None",
                DnsConfig = new()
                {
                    Nameservers = new[] { "8.8.8.8", "223.5.5.5", "114.114.114.114" },
                },
                Containers = new[]
                {
                    new V1Container()
                    {
                        Name = name,
                        Image = config.Image,
                        ImagePullPolicy = "Always",
                        SecurityContext = new() { Privileged = config.PrivilegedContainer },
                        Env = config.Flag is null ? new List<V1EnvVar>() : new[]
                        {
                            new V1EnvVar("GZCTF_FLAG", config.Flag)
                        },
                        Ports = new[] { new V1ContainerPort(config.ExposedPort) },
                        Resources = new V1ResourceRequirements()
                        {
                            Limits = new Dictionary<string, ResourceQuantity>()
                            {
                                ["cpu"] = new ResourceQuantity($"{config.CPUCount}"),
                                ["memory"] = new ResourceQuantity($"{config.MemoryLimit}Mi"),
                                ["ephemeral-storage"] = new ResourceQuantity($"{config.StorageLimit}Mi")
                            },
                            Requests = new Dictionary<string, ResourceQuantity>()
                            {
                                ["cpu"] = new ResourceQuantity("100m"),
                                ["memory"] = new ResourceQuantity("32Mi"),
                                ["ephemeral-storage"] = new ResourceQuantity("128Mi")
                            }
                        }
                    }
                },
                RestartPolicy = "Never"
            }
        };

        try
        {
            pod = await kubernetesClient.CreateNamespacedPodAsync(pod, Namespace, cancellationToken: token);
        }
        catch (HttpOperationException e)
        {
            logger.SystemLog($"Container {name} creation failed, status: {e.Response.StatusCode.ToString()}", TaskStatus.Fail, LogLevel.Warning);
            logger.SystemLog($"Container {name} creation failed, response: {e.Response.Content}", TaskStatus.Fail, LogLevel.Error);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create container");
            return null;
        }

        if (pod is null)
        {
            logger.SystemLog($"Failed to create container instance {config.Image.Split("/").LastOrDefault()}", TaskStatus.Fail, LogLevel.Warning);
            return null;
        }

        var container = new Container()
        {
            ContainerId = name,
            Image = config.Image,
            Port = config.ExposedPort,
            IsProxy = true,
        };

        var service = new V1Service("v1", "Service")
        {
            Metadata = new V1ObjectMeta()
            {
                Name = name,
                NamespaceProperty = Namespace,
                Labels = new Dictionary<string, string>() { ["ctf.gzti.me/ResourceId"] = name }
            },
            Spec = new V1ServiceSpec()
            {
                Type = config.ExposedPort == 80 ? "ClusterIP" : "NodePort",
                Ports = new[]
                {
                    new V1ServicePort(config.ExposedPort, targetPort: config.ExposedPort)
                },
                Selector = new Dictionary<string, string>()
                {
                    ["ctf.gzti.me/ResourceId"] = name
                }
            }
        };

        try
        {
            service = await kubernetesClient.CoreV1.CreateNamespacedServiceAsync(service, Namespace, cancellationToken: token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create service");
            return null;
        }

        if (config.ExposedPort == 80)
        {
            await SetupIngressAsync(containerName: name, targetPort: config.ExposedPort, cancellationToken: token);
        }
        else
        {
            container.PublicPort = service.Spec.Ports[0].NodePort;
        }
        

        container.IP = hostIP;
        container.PublicIP = publicEntry;
        container.StartedAt = DateTimeOffset.UtcNow;

        return container;
    }

    public async Task DestroyContainerAsync(Container container, CancellationToken token = default)
    {
        try
        {
            if (container.Port == 80) {
                await kubernetesClient.NetworkingV1.DeleteNamespacedIngressAsync(container.ContainerId, Namespace, cancellationToken: token);
            }
            await kubernetesClient.CoreV1.DeleteNamespacedServiceAsync(container.ContainerId, Namespace, cancellationToken: token);
            await kubernetesClient.CoreV1.DeleteNamespacedPodAsync(container.ContainerId, Namespace, cancellationToken: token);
        }
        catch (HttpOperationException e)
        {
            if (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                container.Status = ContainerStatus.Destroyed;
                return;
            }
            logger.SystemLog($"Container {container.ContainerId} deletion failed, status: {e.Response.StatusCode.ToString()}", TaskStatus.Fail, LogLevel.Warning);
            logger.SystemLog($"Container {container.ContainerId} deletion failed, response: {e.Response.Content}", TaskStatus.Fail, LogLevel.Error);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete container");
            return;
        }

        container.Status = ContainerStatus.Destroyed;
    }

    private async Task SetupIngressAsync(string containerName, int targetPort, CancellationToken cancellationToken = default)
    {
        var backend_service = new V1IngressServiceBackend
        {
            Name = containerName,
            Port = new V1ServiceBackendPort
            {
                Number = targetPort
            }
        };

        var http_rule = new V1HTTPIngressRuleValue
        {
            Paths = new List<V1HTTPIngressPath>
                {
                    new V1HTTPIngressPath
                    {
                        Path = "/",
                        PathType = "Prefix",
                        Backend = new V1IngressBackend
                        {
                            Service = backend_service
                        }
                    }
                }
        };

        var ingress = new V1Ingress
        {
            Metadata = new V1ObjectMeta
            {
                Name = containerName,
                NamespaceProperty = Namespace,
            },
            Spec = new V1IngressSpec
            {
                Rules = new List<V1IngressRule>
                {
                    new V1IngressRule
                    {
                        Host = $"{containerName}.{publicEntry}",
                        Http = http_rule
                    }
                }
            }
        };

        await kubernetesClient.NetworkingV1.CreateNamespacedIngressAsync(ingress, Namespace, cancellationToken: cancellationToken);
    }

    private void InitK8s(bool withAuth, RegistryConfig? registry)
    {
        if (kubernetesClient.CoreV1.ListNamespace().Items.All(ns => ns.Metadata.Name != Namespace))
            kubernetesClient.CoreV1.CreateNamespace(new() { Metadata = new() { Name = Namespace } });

        if (kubernetesClient.NetworkingV1.ListNamespacedNetworkPolicy(Namespace).Items.All(np => np.Metadata.Name != NetworkPolicy))
        {
            kubernetesClient.NetworkingV1.CreateNamespacedNetworkPolicy(new()
            {
                Metadata = new() { Name = NetworkPolicy },
                Spec = new()
                {
                    PodSelector = new(),
                    PolicyTypes = new[] { "Egress" },
                    Egress = new[]
                    {
                        new V1NetworkPolicyEgressRule()
                        {
                            To = new[]
                            {
                                new V1NetworkPolicyPeer() { IpBlock = new() { Cidr = "0.0.0.0/0", Except = new[] { "10.0.0.0/8" } } },
                            }
                        }
                    }
                }
            }, Namespace);
        }

        if (withAuth && registry is not null)
        {
            var auth = Codec.Base64.Encode($"{registry.UserName}:{registry.Password}");
            var dockerjson = $"{{\"auths\":{{\"{registry.ServerAddress}\":{{\"auth\":\"{auth}\"," +
                $"\"username\":\"{registry.UserName}\",\"password\":\"{registry.Password}\"}}}}}}";
            var dockerjsonBytes = Encoding.ASCII.GetBytes(dockerjson);
            var secret = new V1Secret()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = AuthSecretName,
                    NamespaceProperty = Namespace,
                },
                Data = new Dictionary<string, byte[]>() { [".dockerconfigjson"] = dockerjsonBytes },
                Type = "kubernetes.io/dockerconfigjson"
            };

            try
            {
                kubernetesClient.CoreV1.ReplaceNamespacedSecret(secret, AuthSecretName, Namespace);
            }
            catch
            {
                kubernetesClient.CoreV1.CreateNamespacedSecret(secret, Namespace);
            }
        }
    }
}
