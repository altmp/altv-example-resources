# alt:V Chat

Simple chat system with user interface.

### Installation

You can start by adding the chat resource in its own folder called 'chat'.

```
altVServerFolder/
└── resources/
    ├── chat/
    |   ├── index.js
    |   ├── client.js
    |   ├── resource.toml
    |   └── html/
    └── your_resource/
        ├── your_resource_main.js
        ├── your_resource_client.js
        └── your_resource.toml
        └── package.json
```

**This is for YOUR resource that you want to implement the chat resource into.**
resource.toml

```toml
type = 'js'
main = 'your_resource_main.js'
client-main = 'your_resource_client.js'
client-files = []
deps = [
    'chat'
]
```

package.json

```json
{
    "type": "module"
}
```

### General Usage

**Serverside**

```
import * as chat from 'chat';

// Uses the chat resource to register a command.
// Sends a chat message to the player with their position information.
chat.registerCmd('pos', (player, args) => {
    chat.send(player, `X: ${player.pos.x}, Y: ${player.pos.y}, Z: ${player.pos.z}`);

    // Sends to all players.
    chat.broadcast(`${player.name} is located at: ${player.pos.x}, Y: ${player.pos.y}, Z: ${player.pos.z}`);
});
```
