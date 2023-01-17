<picture>
  <source media="(prefers-color-scheme: dark)" srcset="assets/banner.dark.svg">
  <img alt="Banner" src="assets/banner.light.svg">
</picture>

# GZ::CTF

[![publish](https://github.com/GZTimeWalker/GZCTF/actions/workflows/ci.yml/badge.svg)](https://github.com/GZTimeWalker/GZCTF/actions/workflows/ci.yml)
![version](https://img.shields.io/github/v/release/GZTimeWalker/GZCTF?include_prereleases&label=version)
![license](https://img.shields.io/github/license/GZTimeWalker/GZCTF?color=FF5531)
[![Telegram Group](https://img.shields.io/endpoint?color=blue&url=https%3A%2F%2Ftg.sumanjay.workers.dev%2Fgzctf)](https://telegram.dog/gzctf)
[![DeepSource](https://deepsource.io/gh/GZTimeWalker/GZCTF.svg/?label=active+issues&show_trend=true&token=NSWORRXijp9nyrThqJTk-S7O)](https://deepsource.io/gh/GZTimeWalker/GZCTF/?ref=repository-badge)

GZ::CTF is an open source CTF platform based on ASP.NET Core.

Features 🛠️

- Create highly customizable challenges
  - Static Attachment, Dynamic Attachment, Static Container, Dynamic Container
    - Static Attachment: Shared attachment, any added flag can be submitted
    - Dynamic Attachment: Requires at least one flag and attachment per team, distributed according to team
    - Static Container: Shared container template, no flag is issued, and any added flag can be submitted
    - Dynamic Container: Flags are generated and issued by container environment variables, each team has a unique flag
  - Dynamic Scoring System
    - Formula:
      $$f(S, r, d, x) = \left \lfloor S \times \left[r  + ( 1- r) \times \exp\left( \dfrac{1 - x}{d} \right) \right] \right \rfloor $$
      where $S$ is the original score, $r$ is the minimum score ratio, $d$ is the difficulty coefficient, and $x$ is the number of submissions. The first three parameters can be customized to implement most of the dynamic scoring requirements.

    - Three blood reward: Awards 5%, 3%, 1% of the current question score for the first, second and third blood respectively
  - New challenges can be added at any time during the competition
  - Cheat detection support for dynamic flags, optional flag termplates, leet flag functionality
- Dynamic container distribution based on Docker or K8s
- Grouped team score timeline, grouped score board
- Real-time competition notifications, event and flag submission monitoring using signalR.
- SMTP registration emails, protection against malicious registration using Google ReCaptchav3.
- User bans, three-level user permission management
-  Optional team review, invitation code, registration email domain whitelist
- In-platform writeup collection and viewing
- Real-time event monitoring, download scoreboards, submissions, and writeups.
- Competition monitoring, keeping track of submissions and important events.
- In-app global settings
- And more...

## Demo 🗿

![](assets/demo-1.png)
![](assets/demo-2.png)
![](assets/demo-3.png)
![](assets/demo-4.png)
![](assets/demo-5.png)
![](assets/demo-6.png)
![](assets/demo-7.png)
![](assets/demo-8.png)
![](assets/demo-9.png)

## Deployment 🚀

The application has been compiled and packaged into a Docker image, which can be obtained in the following way:

```bash
docker pull gztime/gzctf:latest
# or
docker pull ghcr.io/gztimewalker/gzctf/gzctf:latest
```

You can also use the `docker-compose.yml` file in the `scripts` directory for configuration.

Please see the [GZCTF-Challenges](https://github.com/GZTimeWalker/GZCTF-Challenges) repository for challenge configuration and examples.

### `appsettings.json` Configuration

When `ContainerProvider` is `Docker`:

- To use local docker, set Uri to empty and mount `/var/run/docker.sock` in the container
- To use external docker, set Uri to your docker daemon address

When `ContainerProvider` is `K8s`:

- Place the cluster connection configuration in the `k8sconfig.yaml` file and mount it to the `/app` directory

```json5
{
  AllowedHosts: "*",
  ConnectionStrings: {
    Database: "Host=db:5432;Database=gzctf;Username=postgres;Password=<Database Password>",
    // redis is optional
    //"RedisCache": "cache:6379,password=<Redis Password>"
  },
  Logging: {
    LogLevel: {
      Default: "Information",
      Microsoft: "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
    },
  },
  EmailConfig: {
    SendMailAddress: "a@a.com",
    UserName: "",
    Password: "",
    Smtp: {
      Host: "localhost",
      Port: 587,
    },
  },
  XorKey: "<Random Key Str>",
  ContainerProvider: {
    Type: "Docker", // or "Kubernetes"
    PublicEntry: "ctf.example.com", // or "xxx.xxx.xxx.xxx"
    DockerConfig: {
      // optional
      SwarmMode: false,
      Uri: "unix:///var/run/docker.sock",
    },
  },
  RequestLogging: false,
  DisableRateLimit: false,
  RegistryConfig: {
    UserName: "",
    Password: "",
    ServerAddress: "",
  },
  GoogleRecaptcha: {
    VerifyAPIAddress: "https://www.recaptcha.net/recaptcha/api/siteverify",
    Sitekey: "",
    Secretkey: "",
    RecaptchaThreshold: "0.5",
  },
}
```

### Initial Administrator Account

In production environments, there are no user with administrator privileges by default. The initial administrator password must be set using the `GZCTF_ADMIN_PASSWORD` environment variable when the application is first started, and then logged in using the `Admin` account.

You can also manually change the database entry to set the current registered user as an administrator. Once you have registerd and logged in, enter the selected database and execute the following SQL statement:

```sql
UPDATE "AspNetUsers" SET "Role"=3 WHERE "UserName"='some_user_name';
```

### Exposed Ports Range Configuration

The following methods are based on experience and may vary depending on your environment. If they don't work properly, please refer to the official documentation for your environment.

- Docker deployment:

  - `sudo nano /etc/sysctl.conf`
  - Add the following entry to specify `ip_local_port_range`:

    ```
    net.ipv4.ip_local_port_range = 20000 50000
    ```

  - Run `sudo sysctl -p` to apply the settings
  - Restart the docker service

- K3s deployment:

  - `sudo nano /etc/systemd/system/k3s.service`
  - Edit the `ExecStart` setting, specifying `service-node-port-range`

    ```
    ExecStart=/usr/local/bin/k3s \
        server \
        --kube-apiserver-arg service-node-port-range=20000-50000
    ```

  - `sudo systemctl daemon-reload`
  - `sudo systemctl restart k3s`

- K8s and Docker Swarm Deployment:

  - The author has not yet attempted this. If you have done so successfully, please consider submitting a PR to add your knowledge.

### Q&A

- **Q: Does the "static container" challenge type mean that all participants share one container?**

  No. Static containers, like dynamic containers, are seperate for each participating team. 
  
  However, the contents of each static container are the same, and no dynamic flag will be passed down. Only static flags, whic hare hard-coded in the container, can be used as verification indicators, hence the "static" name.

- **Q: How does the current team logic work?**

  A user can join multiple teams, each team member must register separately for each competition, a user can participate in multiple competitions simultaneously with different teams, and choose which team to participate in when registering.

  Registrations can be withdrawn when waiting for review or rejected. Once approved or banned, registration cannot be withdrawn, the team cannot be changed, and the team will be locked. 

  After competing, teams will stay locked until there are no other competitions in progress. At this time, the team will be unlocked automatically, allowing members to join and leave again.

- **Q: What deployment forms does the platform support?**

  The following deployment forms are supported:

  - K8s Cluster Deployment:

    GZCTF, database, and challenge containers are all in the same K8s cluster, using namespaces for isolation

  - Docker + K8s Separated Deployment

    GZCTF and database are in a single Docker instance, challenge containers in a separate K8s cluster

  - Docker Standalone Deployment:

    GZCTF, database, and challenge containers are all in the same Docker instance

  - Docker Separated Deployment:

    GZCTF, database, and challenge containers in the same Docker, and use a remote Docker/Swarm for challenges (not recommended)

  - Docker Swarm Cluster Deployment:

    GZCTF, database, and challenge containers are all in the same Docker Swarm cluster (not recommended)

- **Q: Which deployment method is recommended?**

  For users with multiple machine clusters and deployment requirements, it is recommended to use K3s as a distribution of K8s, which can provide all the features required and is easy to install and deploy.

  In general, the most straightforward way to deploy is using Docker and K3s separately. You can set up the GZCTF platform by simply running `docker-compose` - for K3s, you only need to install it, export the kubeconfig, and mount it to GZCTF.

  For deploying within a K3s cluster, you'll need to provide two PVCs (for database and attachments), a ConfigMap (for storing `appsettings.json`), and a Secret (for storing `k8sconfig.yaml` pointing to your own cluster). You can deploy the database and Redis as needed, but if you're only running a single GZCTF instance, you don't need to deploy Redis.

- **Q: What should I pay attention to when deploying multiple instances of GZCTF?**
  
  GZCTF supports multiple instances running at the same time, but requires the same database instance, Redis instance (required for multiple instances), and shared storage (such as NFS) as PVCs. The database instance and shared storage are used to ensure data consistency, and the Redis instance is used for backend data cache synchronization and SignalR Scale-Out broadcast. 

  In order to ensure the normal operation of SignalR based on websocket, Sticky Session needs to be configured in the load balancer.

- **Q: Is there a more detailed deployment tutorial?**

  I'm writing it, it will be released when v1.0.0 is released.

## About i18n 🌐

 For the time being, no multi-language adaptation is considered.

## Contributors 👋

<a href="https://github.com/GZTimeWalker/GZCTF/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=GZTimeWalker/GZCTF" />
</a>

## CTFs using GZCTF 🏆

Various organizers have chosen GZCTF and successfully completed the competition, their trust, support and timely feedback are the first driving force for GZCTF to continuously improve.

- **Tsinghua University Network Security Technology Challenge THUCTF 2022**
- **Zhejiang University ZJUCTF 2022**
- **Southeastern University 3rd University Student Network Security Challenge**
- **Gansu University of Political Science and Law DIDCTF 2022**
- **Shandong University of Science and Technology 1st Network Security Practice Competition woodpecker**
- **Northwestern Polytechnical University NPUCTF 2022**
- **SkyNICO Network Space Security Tournament (Xiamen University of Technology, Fujian Normal University, Qilu University of Technology)**

_The ranking is not in order,feel free to submit a PR to add your CTF._

## Special Thanks  ❤️‍🔥

Thanks to the organizers of THUCTF 2022 NanoApe for their sponsorship and Aliyun public network concurrency testing, which helped to verify the service stability of GZCTF single instance under 1,000 concurrent, three-minute 1.34 million requests pressure.

## Stars ✨

[![Stargazers over time](https://starchart.cc/GZTimeWalker/GZCTF.svg)](https://starchart.cc/GZTimeWalker/GZCTF)
