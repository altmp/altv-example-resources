# Vehicle Addon

This shows how to add a modded vehicle to your server through our modding system.

Vehicle mods must **always be loaded** before your actual code base.

The vehicle name is `karby` if you wish to spawn it.

## Spawning

With JavaScript

```js
const vehicle = new alt.Vehicle('karby', 0, 0, 72, 0, 0, 0);
vehicle.customPrimaryColor = new alt.RGBA(251, 231, 239);
```

## Credits

https://www.gta5-mods.com/vehicles/karby-addon-replace-unlocked-z3d