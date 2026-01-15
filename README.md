# HookHubNet

## Short Description

HookHubNet is a tunneling system that enables secure forwarding of TCP traffic through WebSocket connections. It consists of a central hub that manages connections from multiple hooks, each forwarding traffic to backend services.

## Key Features

- WebSocket-based tunneling for TCP traffic
- Dynamic port assignment for hooks
- Concurrent tunnel management
- REST API for hook management
- Configurable backend targets

## Functionality Overview

The system operates by having hooks connect to a central hub via WebSockets. Each hook is assigned a unique TCP port on the hub. Incoming TCP connections to these ports are tunneled through the WebSocket to the hook, which then forwards the traffic to the configured backend service. This allows exposing local services securely without direct network exposure.

## Technology Stack

- **Programming Language**: C#
- **Frameworks**: .NET 10, ASP.NET Core
- **Libraries**: System.Net.WebSockets, System.Net.Sockets
- **External Services**: None

## Architecture

### High-level Architecture Description

HookHubNet follows a client-server architecture with a hub-and-spoke model:

- **Hub**: An ASP.NET Core web application that accepts WebSocket connections from hooks and manages TCP listeners.
- **Hooks**: Console applications that connect to the hub via WebSockets and forward traffic to backend services.
- **Tunnels**: Bidirectional data streams between TCP clients and backend services via WebSockets.

### Component Responsibilities

- **HookHubNet.Hub**: Handles WebSocket upgrades, assigns ports, manages tunnel registry, and forwards data.
- **HookHubNet.Hook**: Establishes WebSocket connection to hub, receives tunnel commands, and forwards TCP traffic to backends.
- **HookHubNet.Common**: Shared data transfer objects, models, and protocol utilities.

### Authentication / Authorization Flow

No authentication or authorization is currently implemented. Connections are accepted based on hook ID query parameter.

## Solution / Repository Organization

- `HookHubNet.sln`: Solution file containing all projects.
- `HookHubNet.Common/`: Shared library with DTOs, models, and protocol handling.
- `HookHubNet.Hub/`: ASP.NET Core web application for the central hub.
- `HookHubNet.Hook/`: Console application for hook clients.
- `HookHubNet.Proxy/`: Legacy project (deprecated).

## Getting Started

### Prerequisites

- .NET 10 SDK
- Access to network ports for TCP listeners

### Clone Repository

```bash
git clone <repository-url>
cd hookhubnet
```

### Configuration

Configuration is managed via `appsettings.json` files in each project:

- **HookHubNet.Hub**: Standard ASP.NET Core configuration.
- **HookHubNet.Hook**: Configure `HubUrl` and `Hooks` array with hook IDs, target hosts, and ports.

Example `appsettings.json` for HookHubNet.Hook:

```json
{
  "HubUrl": "ws://localhost:5201/hookhubnet",
  "Hooks": [
    {
      "HookId": "webhook",
      "TargetHost": "localhost",
      "TargetPort": 8080
    }
  ]
}
```

Environment variables can override settings.

### Build Instructions

```bash
dotnet build HookHubNet.sln
```

### Run Instructions

1. Start the hub:
   ```bash
   dotnet run --project HookHubNet.Hub
   ```

2. Start a hook:
   ```bash
   dotnet run --project HookHubNet.Hook
   ```

The hub runs on the configured port (default 5000), and hooks connect via WebSocket.

## Usage Examples

### API Examples

- **Get all hooks**:
  ```bash
  curl http://localhost:5000/hook/getall
  ```

- **Get hook info**:
  ```bash
  curl http://localhost:5000/hook/get/webhook
  ```

- **Remove hook**:
  ```bash
  curl http://localhost:5000/hook/remove/webhook
  ```

### Web Usage Examples

Hooks connect via WebSocket:
```
ws://localhost:5000/hookhubnet?id=webhook
```

Once connected, TCP traffic to the assigned port is forwarded to the hook's backend.

## Test Cases

No automated tests are currently implemented. Manual testing involves:

- Starting the hub and verifying WebSocket acceptance.
- Connecting a hook and checking port assignment.
- Sending TCP traffic to the assigned port and verifying forwarding to the backend.

Example test scenario: Use `telnet` or `nc` to connect to the hub's assigned port and observe data forwarding.

## Security Considerations

- WebSocket connections are unencrypted; use HTTPS/WSS in production.
- No authentication; implement API keys or certificates for production use.
- Ensure proper firewall rules to restrict access to hub ports.
- Validate hook IDs to prevent unauthorized connections.

## Deployment Notes

Deploy the hub as a standard ASP.NET Core application using IIS, Nginx, or Docker. Hooks can run as console applications or services. Ensure network connectivity between hub and hooks. Use reverse proxies for SSL termination.

## Roadmap / Future Improvements

- Implement authentication and authorization.
- Add encryption for WebSocket traffic.
- Support for UDP tunneling.
- Web-based management interface.
- Comprehensive test suite.

## License

This project is licensed under GPL v3.

## Maintainer

Manuel Rodríguez Camacho

## Contribution Guidelines

1. Fork the repository.
2. Create a feature branch.
3. Make changes with clear commit messages.
4. Submit a pull request with description of changes.
5. Ensure code follows .NET coding standards.

## Troubleshooting

- **Hook connection fails**: Check hub URL and network connectivity.
- **Port conflicts**: Ensure assigned ports are available.
- **Data not forwarding**: Verify backend service is running and accessible.
- **WebSocket errors**: Check for firewall blocking WebSocket traffic.

## Additional Notes

This is a development version; production deployment requires security hardening.