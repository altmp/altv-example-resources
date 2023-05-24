# Addon Object

This is an example object addon of the alt:V Logo.

Object mods must be loaded before your code base.

## Spawning the Object

```js
async function spawnLogo() {
    const modelHash = alt.hash("tw_altv_logo");
    await alt.Utils.requestModel(modelHash);
    native.createObject(modelHash, 0, 0, 76, false, false, false);
}

spawnLogo();
```