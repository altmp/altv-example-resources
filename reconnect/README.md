# Reconnect

Make sure this resource is loaded last.

## Installation

Ensure this resource is loaded in your `resources` array in the `server.toml`.

Inside of the `server.toml` make sure to also add these imports.

```toml
[js-module]
global-webcrypto = true
network-imports = true
```
