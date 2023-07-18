# Discord Auth

Change the application id in the `client/startup.js` file to change the logo, title, etc.

## Installation

Ensure this resource is loaded in your `resources` array in the `server.toml`.

Inside of the `server.toml` make sure to also add these imports.

```toml
[js-module]
global-webcrypto = true
network-imports = true
```

## Usage

After authenticating through the Discord Desktop Client you can listen to streamSyncedMeta changes to further authenticate.

```ts
alt.on('streamSyncedMetaChange', (entity, key, value) => {
    if (!(entity instanceof alt.Player)) {
        return;
    }

    if (key !== 'authenticated') {
        return;
    }

    // Discord Name
    const name = player.getStreamSyncedMeta('name');

    // Discord Identifier
    const id = player.getStreamSyncedMeta('discord');

    console.log(name);
    console.log(id);
});
```
