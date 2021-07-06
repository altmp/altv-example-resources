import * as alt from "alt-server";
import * as chat from "chat";
import fs from "fs";
import { resolve, dirname } from "path";

const __dirname = dirname(decodeURI(new URL(import.meta.url).pathname)).replace(/^\/([A-Za-z]):\//, "$1:/");

let blacklistData = {};
if (fs.existsSync(__dirname + "/blacklist.json")) {
  blacklistData = JSON.parse(fs.readFileSync(__dirname + "/blacklist.json").toString());
}

function addToBlacklist(info) {
  blacklistData[info] = true;
  fs.writeFileSync(__dirname + "/blacklist.json", JSON.stringify(blacklistData, null, 4));
}

const vehicles = {
  ballas: ["chino2", "buccaneer2", "buccaneer", "faction"],
  families: ["faction2", "blade", "gauntlet", "impaler"],
  vagos: ["ellie", "chino", "dukes", "impaler"],
};

const weapons = {
  WEAPON_KNIFE: "Knife",
  WEAPON_BAT: "Bat",
  WEAPON_BOTTLE: "Bottle",
  WEAPON_WRENCH: "Wrench",
  WEAPON_PISTOL: "Pistol",
  WEAPON_HEAVYPISTOL: "Heavy pistol",
  WEAPON_REVOLVER: "Revolver",
  WEAPON_MICROSMG: "Micro-SMG",
  WEAPON_SMG: "SMG",
  WEAPON_COMBATPDW: "Combat PDW",
  WEAPON_ASSAULTRIFLE: "Assault Rifle",
  WEAPON_CARBINERIFLE: "Carbin Rifle",
  WEAPON_PUMPSHOTGUN: "Pump Shotgun",
  WEAPON_GRENADE: "Grenade",
  WEAPON_RAMMED_BY_CAR: "Jumped out of car",
  WEAPON_RUN_OVER_BY_CAR: "Run over by car",
  WEAPON_FALL: "Fall",
  WEAPON_DROWNING: "Drowning",
  WEAPON_DROWNING_IN_VEHICLE: "Drowning",
  WEAPON_EXPLOSION: "Explosion",
  WEAPON_FIRE: "Fired",
  WEAPON_BLEEDING: "Bleeding",
  WEAPON_BARBED_WIRE: "Barbed wire",
  WEAPON_EXHAUSTION: "Exhaustion",
  WEAPON_ELECTRIC_FENCE: "Electric fence",
};

const weaponHashes = {};

for (let w in weapons) {
  weaponHashes[alt.hash(w)] = weapons[w];
}

const availableWeapons = ["WEAPON_KNIFE", "WEAPON_BAT", "WEAPON_BOTTLE", "WEAPON_WRENCH", "WEAPON_PISTOL", "WEAPON_HEAVYPISTOL", "WEAPON_REVOLVER", "WEAPON_MICROSMG", "WEAPON_SMG", "WEAPON_COMBATPDW", "WEAPON_ASSAULTRIFLE", "WEAPON_CARBINERIFLE", "WEAPON_PUMPSHOTGUN"];

function giveWeapons(player) {
  for (const weapon of availableWeapons) {
    player.giveWeapon(alt.hash(weapon), 9999, true);
  }
}

const colors = {
  ballas: {
    rgba: { r: 196, g: 0, b: 171, a: 150 },
    hex: "C400AB",
  },
  families: {
    rgba: { r: 0, g: 127, b: 0, a: 150 },
    hex: "008000",
  },
  vagos: {
    rgba: { r: 255, g: 191, b: 0, a: 150 },
    hex: "FFBF00",
  },
};

const positions = {
  vagos: {
    spawns: [
      { x: 334.6681, y: -2052.6726, z: 20.8212 },
      { x: 341.789, y: -2051.3669, z: 21.3267 },
      { x: 345.7582, y: -2044.6812, z: 21.63 },
      { x: 342.3955, y: -2040.356, z: 21.5626 },
      { x: 351.2835, y: -2043.2043, z: 22.0007 },
    ],
    weapon: { x: 359.5912, y: -2060.611, z: 21.4952 },
    vehicle: { x: 330.9758, y: -2036.6241, z: 20.9897 },
  },
  ballas: {
    spawns: [
      { x: 88.6285, y: -1959.389, z: 20.737 },
      { x: 109.3054, y: -1955.8022, z: 20.737 },
      { x: 117.7318, y: -1947.7583, z: 20.72 },
      { x: 118.9186, y: -1934.2681, z: 20.7707 },
      { x: 105.7318, y: -1923.4154, z: 20.737 },
    ],
    weapon: { x: 84.989, y: -1958.6241, z: 21.1076 },
    vehicle: { x: 105.7186, y: -1941.5867, z: 20.7875 },
  },
  families: {
    spawns: [
      { x: -196.4439, y: -1607.0505, z: 34.1494 },
      { x: -174.356, y: -1609.978, z: 33.7281 },
      { x: -175.0681, y: -1623.1647, z: 33.5596 },
      { x: -191.1692, y: -1641.4813, z: 33.408 },
      { x: -183.5736, y: -1587.5999, z: 34.8234 },
    ],
    weapon: { x: -210.7648, y: -1606.8132, z: 34.8571 },
    vehicle: { x: -183.5736, y: -1587.5999, z: 34.8234 },
  },
};

const checkpoints = {
  ballas: {
    vehicle: null,
    weapon: null,
  },
  families: {
    vehicle: null,
    weapon: null,
  },
  vagos: {
    vehicle: null,
    weapon: null,
  },
};

for (let i in positions) {
  checkpoints[i].vehicle = new alt.Checkpoint(45, positions[i].vehicle.x, positions[i].vehicle.y, positions[i].vehicle.z - 1.1, 5, 1, colors[i].rgba.r, colors[i].rgba.g, colors[i].rgba.b, 255);
  checkpoints[i].weapon = new alt.Checkpoint(45, positions[i].weapon.x, positions[i].weapon.y, positions[i].weapon.z - 1.1, 1, 1, colors[i].rgba.r, colors[i].rgba.g, colors[i].rgba.b, 255);
}

const currentTurfPoints = {
  ballas: 0,
  families: 0,
  vagos: 0,
};

class Turf {
  constructor(x1, y1, x2, y2) {
    this.x1 = x1;
    this.y1 = y1;
    this.x2 = x2;
    this.y2 = y2;
  }

  between(min, p, max) {
    let result = false;

    if (min < max) {
      if (p > min && p < max) result = true;
    }

    if (min > max) {
      if (p > max && p < min) result = true;
    }

    if (p == min || p == max) result = true;
    return result;
  }

  contains(x, y) {
    let result = false;
    if (this.between(this.x1, x, this.x2) && this.between(this.y1, y, this.y2)) result = true;
    return result;
  }
}

let turfs = [];
let currentTurf = null;

const xStartTurf = -404.1889;
const yStartTurf = -1221.2967;

for (let i = 0; i < 5; ++i) {
  for (let j = 0; j < 5; ++j) {
    const x1 = xStartTurf + 200 * i;
    const y1 = yStartTurf - 200 * j;
    turfs.push(new Turf(x1, y1, x1 + 200, y1 - 200));
  }
}

function startCapture() {
  currentTurfPoints.ballas = 0;
  currentTurfPoints.families = 0;
  currentTurfPoints.vagos = 0;

  currentTurf = turfs[Math.round(Math.random() * (turfs.length - 1))];
  alt.emitAllClients("captureStateChanged", true);
  alt.emitAllClients("startCapture", {
    x1: currentTurf.x1,
    y1: currentTurf.y1,
    x2: currentTurf.x2,
    y2: currentTurf.y2,
  });
  alt.emitAllClients("updateTeamPoints", currentTurfPoints);
}

function stopCapture() {
  currentTurfPoints.ballas = 0;
  currentTurfPoints.families = 0;
  currentTurfPoints.vagos = 0;
  currentTurf = null;
  alt.emitAllClients("captureStateChanged", false);
  alt.emitAllClients("stopCapture");
}

alt.setInterval(() => {
  let allPlayers = alt.Player.all;
  if (allPlayers.length > 0) {
    if (currentTurf == null) {
      startCapture();
    } else {
      for (let p of allPlayers) {
        if (!p.valid) continue;
        const pTeam = p.getMeta("team");
        if (!pTeam) continue;
        if (currentTurf.contains(p.pos.x, p.pos.y)) {
          currentTurfPoints[pTeam]++;
          if (currentTurfPoints[pTeam] >= 1000) {
            chat.broadcast(`{${colors[pTeam].hex}} ${pTeam} {FFFFFF}got this turf. Next capture started`);
            stopCapture();
            return;
          }
        }
      }
      alt.emitAllClients("updateTeamPoints", currentTurfPoints);
    }
  } else if (currentTurf != null) {
    stopCapture();
  }
}, 1000);

function getTeamsPopulation() {
  const population = {
    ballas: 0,
    families: 0,
    vagos: 0,
  };
  for (let p of alt.Player.all) {
    const team = p.getMeta("team");
    if (team) {
      population[team]++;
    }
  }
  return population;
}

function broadcastTeamsPopulation() {
  for (let p of alt.Player.all) {
    if (p.getMeta("selectingTeam")) {
      alt.emitClient(p, "showTeamSelect", getTeamsPopulation());
    }
  }
}

function broadcastPlayersOnline(add) {
  if (add !== undefined) alt.emitAllClients("updatePlayersOnline", alt.Player.all.length + add);
  else alt.emitAllClients("updatePlayersOnline", alt.Player.all.length);
}

alt.onClient("authData", (player, data) => {
  const licenseHash = data.sc;
  player.setMeta("licenseHash", licenseHash);
  let dsInfo = null;
  if (data.discord && data.discord.id) {
    dsInfo = data.discord.id;
    player.setMeta("discordId", dsInfo);
  }
  if (licenseHash in blacklistData || (dsInfo !== null && dsInfo in blacklistData)) {
    chat.broadcast(`{5555AA}${player.name} {FFFFFF}kicked (Blacklisted)`);
    player.kick();
  }
});

chat.registerCmd("kick", (player, args) => {
  if (player.getMeta("admin")) {
    if (args.length > 0) {
      let players = alt.Player.all.filter((p) => p.name === args.join(" "));
      if (players.length != 0) {
        for (const p of players) {
          chat.send(p, `{FF0000}You was kicked from the server`);
          chat.broadcast(`{5555AA}${p.name} {FFFFFF}kicked`);
          p.kick("You was kicked from the server");
        }
      } else {
        for (const p of players) {
          if (p.name.startsWith(args.join(" "))) {
            chat.send(p, `{FF0000}You was kicked from the server`);
            chat.broadcast(`{5555AA}${p.name} {FFFFFF}kicked`);
            p.kick("You was kicked from the server");
          }
        }
      }
    }
  } else {
    chat.send(player, "{FF00FF}You don`t have enough permissions to use this command");
  }
});

chat.registerCmd("ban", (player, args) => {
  if (player.getMeta("admin")) {
    if (args.length > 0) {
      let players = alt.Player.all.filter((p) => p.name === args.join(" "));

      if (players.length != 0) {
        for (const p of players) {
          chat.send(p, `{FF0000}You was banned from the server`);
          chat.broadcast(`{5555AA}${p.name} {FFFFFF}banned`);
          addToBlacklist(p.getMeta("licenseHash"));
          const discordId = p.getMeta("discordId");
          if (discordId) {
            addToBlacklist(discordId);
          }
          p.kick("You was banned from the server");
        }
      } else {
        for (const p of players) {
          if (p.name.startsWith(args.join(" "))) {
            chat.send(p, `{FF0000}You was banned from the server`);
            chat.broadcast(`{5555AA}${p.name} {FFFFFF}banned`);
            addToBlacklist(p.getMeta("licenseHash"));
            const discordId = p.getMeta("discordId");
            if (discordId) {
              addToBlacklist(discordId);
            }
            p.kick("You was banned from the server");
          }
        }
      }
    }
  } else {
    chat.send(player, "{FF00FF}You don`t have enough permissions to use this command");
  }
});

alt.on("playerConnect", (player) => {
  player.setMeta("selectingTeam", true);
  player.setMeta("checkpoint", 0);
  player.setMeta("vehicle", null);
  player.setMeta("canSpawnVehicle", 0);
  player.setMeta("warns", 0);

  broadcastPlayersOnline();

  chat.broadcast(`{5555AA}${player.name} {FFFFFF}connected`);
  alt.log(`${player.name} connected`);

  alt.emitClient(player, "youAreConnected");
});

alt.onClient("viewLoaded", (player) => {
  alt.log("View loaded for " + player.name);
  alt.emitClient(player, "showTeamSelect", getTeamsPopulation());
  alt.emitClient(player, "updatePlayersOnline", alt.Player.all.length);
});

alt.on("playerDisconnect", (player) => {
  const veh = player.getMeta("vehicle");
  if (veh) {
    veh.destroy();
  }

  player.setMeta("selectingTeam", false);

  broadcastTeamsPopulation();
  broadcastPlayersOnline(-1);

  chat.broadcast(`{5555AA}${player.name} {FFFFFF}disconnected`);
  alt.log(`${player.name} disconnected`);
});

alt.onClient("teamSelected", (player, teamId) => {
  let team = "families";
  if (teamId == 2) team = "ballas";
  else if (teamId == 3) team = "vagos";

  player.setMeta("team", team);
  player.setMeta("selectingTeam", false);
  broadcastTeamsPopulation();

  chat.broadcast(`{5555AA}${player.name} {FFFFFF}joined {${colors[team].hex}}${team}`);

  alt.log(player.name + " joined " + team);
  const nextSpawns = positions[team].spawns;

  const spawn = nextSpawns[Math.round(Math.random() * (nextSpawns.length - 1))];
  console.log("Spawning in " + JSON.stringify(spawn));
  player.model = "mp_m_freemode_01";
  player.spawn(spawn.x, spawn.y, spawn.z, 0);
  alt.emitClient(player, "applyAppearance", team);
  alt.emitClient(player, "updateTeam", team);

  if (currentTurf != null) {
    alt.emitAllClients("captureStateChanged", true);
    alt.emitAllClients("startCapture", {
      x1: Math.min(currentTurf.x1, currentTurf.x2),
      y1: Math.min(currentTurf.y1, currentTurf.y2),
      x2: Math.max(currentTurf.x1, currentTurf.x2),
      y2: Math.max(currentTurf.y1, currentTurf.y2),
    });
    alt.emitAllClients("updateTeamPoints", currentTurfPoints);
  }
});

alt.onClient("action", (player) => {
  const cp = player.getMeta("checkpoint");
  if (cp == 1) {
    const nextTimeSpawn = player.getMeta("canSpawnVehicle");
    if (nextTimeSpawn > Date.now()) return;

    const pTeam = player.getMeta("team");
    const pos = player.pos;
    let curVeh = player.getMeta("vehicle");
    if (curVeh) {
      curVeh.destroy();
      curVeh = null;
    }

    const nextModel = vehicles[pTeam][Math.round(Math.random() * (vehicles[pTeam].length - 1))];
    const vehColor = colors[pTeam].rgba;
    curVeh = new alt.Vehicle(nextModel, pos.x + 2, pos.y, pos.z, 0, 0, 0);
    curVeh.customPrimaryColor = { r: vehColor.r, g: vehColor.g, b: vehColor.b };
    curVeh.customSecondaryColor = { r: vehColor.r, g: vehColor.g, b: vehColor.b };
    player.setMeta("vehicle", curVeh);
    player.setMeta("canSpawnVehicle", Date.now() + 400);
  } else if (cp == 2) {
    giveWeapons(player);
  }
});

alt.on("entityEnterColshape", (colshape, entity) => {
  if (entity instanceof alt.Player) {
    const pTeam = entity.getMeta("team");
    if (colshape == checkpoints[pTeam].vehicle) {
      entity.setMeta("checkpoint", 1);
      alt.emitClient(entity, "showInfo", "~INPUT_PICKUP~ to get car");
    } else if (colshape == checkpoints[pTeam].weapon) {
      entity.setMeta("checkpoint", 2);
      alt.emitClient(entity, "showInfo", "~INPUT_PICKUP~ to get weapons and ammo");
    }
  }
});

alt.on("entityLeaveColshape", (colshape, entity) => {
  if (entity instanceof alt.Player) {
    entity.setMeta("checkpoint", 0);
  }
});

alt.on("playerDeath", (player, killer, weapon) => {
  let weaponName = "Killed";
  if (weapon in weaponHashes) weaponName = weaponHashes[weapon];

  if (player == killer && weaponName == "Killed") weaponName = "Suicided";
  else if (weaponName == "Killed") console.log("Unknown death reason: " + weapon.toString(16));

  const team = player.getMeta("team");
  if (!killer) killer = player;

  const nextSpawns = positions[team].spawns;
  const spawnPos = nextSpawns[Math.round(Math.random() * (nextSpawns.length - 1))];
  player.spawn(spawnPos.x, spawnPos.y, spawnPos.z, 5000);

  if (killer) {
    const killerTeam = killer.getMeta("team");
    alt.emitAllClients("playerKill", { killerName: killer.name, killerGang: killerTeam, victimName: player.name, victimGang: team, weapon: weaponName });

    if (currentTurf != null && killer != player && team != killerTeam) {
      if (currentTurf.contains(player.pos.x, player.pos.y)) {
        currentTurfPoints[killerTeam] += 50;
        if (currentTurfPoints[killerTeam] >= 1000) {
          chat.broadcast(`{${colors[killerTeam].hex}} ${killerTeam} {FFFFFF}got this turf. Next capture started`);
          stopCapture();
        }
      }
    } else if (currentTurf != null && team == killerTeam) {
      if (currentTurf.contains(player.pos.x, player.pos.y)) {
        if (currentTurfPoints[killerTeam] > 50) currentTurfPoints[killerTeam] -= 50;
        else currentTurfPoints[killerTeam] = 0;
      }
    }

    if (team == killerTeam && player != killer) {
      let warns = killer.getMeta("warns");
      if (warns == 2) {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}kicked for team killing`);
        killer.kick();
      } else {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}warned [${warns + 1}/3] for team killing`);
        killer.setMeta("warns", warns + 1);
      }
    } else if (player != killer && weaponName == weapons.WEAPON_RUN_OVER_BY_CAR) {
      let warns = killer.getMeta("warns");
      if (warns == 2) {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}kicked for vehicle kill`);
        killer.kick();
      } else {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}warned [${warns + 1}/3] for vehicle kill`);
        killer.setMeta("warns", warns + 1);
      }
    }
  }
});

function getDistanceBetweenPoints(pos1, pos2) {
  const dX = pos1.x - pos2.x;
  const dY = pos1.y - pos2.y;
  const dZ = pos1.z - pos2.z;
  return Math.sqrt(dX * dX + dY * dY + dZ * dZ);
}

alt.setInterval(() => {
  for (let p of alt.Player.all) {
    if (!p.valid) continue;
    const lastPos = p.getMeta("lastPos");
    if (lastPos) {
      if (getDistanceBetweenPoints(lastPos, p.pos) <= 1) {
        chat.broadcast(`${p.name} {FFFFFF}was kicked for being AFK`);
        p.kick();
      } else {
        p.setMeta("lastPos", p.pos);
      }
    } else {
      p.setMeta("lastPos", p.pos);
    }
  }
}, 240000);
