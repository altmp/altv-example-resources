import * as alt from 'alt-server';
import chat from 'chat';
import fs from 'fs';
import { resolve, dirname } from 'path';

const __dirname = dirname(decodeURI(new URL(import.meta.url).pathname)).replace(/^\/([A-Za-z]):\//, '$1:/');

let blacklistData = {};
if(fs.existsSync(__dirname + '/blacklist.json'))
{
  blacklistData = JSON.parse(fs.readFileSync(__dirname + '/blacklist.json').toString());
}

function addToBlacklist(info) {
  blacklistData[info] = true;
  fs.writeFileSync(__dirname + '/blacklist.json', JSON.stringify(blacklistData, null, 4));
}

const vehicles = {
  ballas: [
    'chino2',
    'buccaneer2',
    'buccaneer',
    'faction'
  ],
  families: [
    'faction2',
    'blade',
    'gauntlet',
    'impaler'
  ],
  vagos: [
    'ellie',
    'chino',
    'dukes',
    'impaler'
  ]
};

function jhash(key){
  var keyLowered = key.toLowerCase();
  var length = keyLowered.length;
  var hash, i;

  for (hash = i = 0; i < length; i++){
      hash += keyLowered.charCodeAt(i);
      hash += (hash << 10);
      hash ^= (hash >>> 6);
  }

  hash += (hash << 3);
  hash ^= (hash >>> 11);
  hash += (hash << 15);

  return convertToUnsigned(hash);
}

function convertToUnsigned(value){
  return (value >>> 0);
}

const weapons = {
  WEAPON_KNIFE: 'Knife',
  WEAPON_BAT: 'Bat',
  WEAPON_BOTTLE: 'Bottle',
  WEAPON_WRENCH: 'Wrench',
  WEAPON_PISTOL: 'Pistol',
  WEAPON_HEAVYPISTOL: 'Heavy pistol',
  WEAPON_REVOLVER: 'Revolver',
  WEAPON_MICROSMG: 'Micro-SMG',
  WEAPON_SMG: 'SMG',
  WEAPON_COMBATPDW: 'Combat PDW',
  WEAPON_ASSAULTRIFLE: 'Assault Rifle',
  WEAPON_CARBINERIFLE: 'Carbin Rifle',
  WEAPON_PUMPSHOTGUN: 'Pump Shotgun',
  WEAPON_GRENADE: 'Grenade',
  WEAPON_RAMMED_BY_CAR: 'Jumped out of car',
  WEAPON_RUN_OVER_BY_CAR: 'Run over by car',
  WEAPON_FALL: 'Fall',
  WEAPON_DROWNING: 'Drowning',
  WEAPON_DROWNING_IN_VEHICLE: 'Drowning',
  WEAPON_EXPLOSION: 'Explosion',
  WEAPON_FIRE: 'Fired',
  WEAPON_BLEEDING: 'Bleeding',
  WEAPON_BARBED_WIRE: 'Barbed wire',
  WEAPON_EXHAUSTION: 'Exhaustion',
  WEAPON_ELECTRIC_FENCE: 'Electric fence'
};

const weaponHashes = {};

for(let w in weapons) {
  weaponHashes[jhash(w)] = weapons[w];
}

const colors = {
  ballas: {
    rgba: { r: 196, g: 0, b: 171, a: 150 },
    hex: 'C400AB'
  },
  families: {
    rgba: { r: 0, g: 127, b: 0, a: 150 },
    hex: '008000'
  },
  vagos: {
    rgba: { r: 255, g: 191, b: 0, a: 150 },
    hex: 'FFBF00'
  }
};

const positions = {
  vagos: {
    spawns: [
      { x: 334.6681, y: -2052.6726, z: 20.8212 },
      { x: 341.7890, y: -2051.3669, z: 21.3267 },
      { x: 345.7582, y: -2044.6812, z: 21.6300 },
      { x: 342.3955, y: -2040.3560, z: 21.5626 },
      { x: 351.2835, y: -2043.2043, z: 22.0007 }
    ],
    weapon: { x: 359.5912, y: -2060.6110, z: 21.4952 },
    vehicle: { x: 330.9758, y: -2036.6241, z: 20.9897 }
  },
  ballas: {
    spawns: [
      { x: 88.6285, y: -1959.3890, z: 20.7370 },
      { x: 109.3054, y: -1955.8022, z: 20.7370 },
      { x: 117.7318, y: -1947.7583, z: 20.7200 },
      { x: 118.9186, y: -1934.2681, z: 20.7707 },
      { x: 105.7318, y: -1923.4154, z: 20.7370 }
    ],
    weapon: { x: 84.9890, y: -1958.6241, z: 21.1076 },
    vehicle: { x: 105.7186, y: -1941.5867, z: 20.7875 }
  },
  families: {
    spawns: [
      { x: -196.4439, y: -1607.0505, z: 34.1494 },
      { x: -174.3560, y: -1609.9780, z: 33.7281 },
      { x: -175.0681, y: -1623.1647, z: 33.5596 },
      { x: -191.1692, y: -1641.4813, z: 33.4080 },
      { x: -183.5736, y: -1587.5999, z: 34.8234 }
    ],
    weapon: { x: -210.7648, y: -1606.8132, z: 34.8571 },
    vehicle: { x: -183.5736, y: -1587.5999, z: 34.8234 }
  }
};

const checkpoints = {
  ballas: {
    vehicle: null,
    weapon: null
  },
  families: {
    vehicle: null,
    weapon: null
  },
  vagos: {
    vehicle: null,
    weapon: null
  }
};

for(let i in positions) {
  checkpoints[i].vehicle = alt.createCheckpoint(45, positions[i].vehicle.x, positions[i].vehicle.y, positions[i].vehicle.z - 1.1, 5, 1, colors[i].rgba.r, colors[i].rgba.g, colors[i].rgba.b, 255);
  checkpoints[i].weapon = alt.createCheckpoint(45, positions[i].weapon.x, positions[i].weapon.y, positions[i].weapon.z - 1.1, 1, 1, colors[i].rgba.r, colors[i].rgba.g, colors[i].rgba.b, 255);
}

const currentTurfPoints = {
  ballas: 0,
  families: 0,
  vagos: 0
};

class Turf {
  constructor(x1, y1, x2, y2) {
    this.x1 = x1;
    this.y1 = y1;
    this.x2 = x2;
    this.y2 = y2;
  }

  between(min, p, max){
    let result = false;
  
    if (min < max){
      if (p > min && p < max)
        result = true;
    }
  
    if ( min > max ){
      if (p > max && p < min)
        result = true
    }
  
    if (p == min || p == max)
      result = true;
    return result;
  }
  
  contains(x, y){
    let result = false;
    if (this.between(this.x1, x, this.x2) && this.between(this.y1, y, this.y2))
      result = true;
    return result;
  }
}

let turfs = [];
let currentTurf = null;

const xStartTurf = -404.1889;
const yStartTurf = -1221.2967;

for(let i = 0; i < 5; ++i) {
  for(let j = 0; j < 5; ++j) {
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
  alt.emitClient(null, 'captureStateChanged', true);
  alt.emitClient(null, 'startCapture', {
    x1: currentTurf.x1, y1: currentTurf.y1, x2: currentTurf.x2, y2: currentTurf.y2
  });
  alt.emitClient(null, 'updateTeamPoints', currentTurfPoints);
}

function stopCapture() {
  currentTurfPoints.ballas = 0;
  currentTurfPoints.families = 0;
  currentTurfPoints.vagos = 0;
  currentTurf = null;
  alt.emitClient(null, 'captureStateChanged', false);
  alt.emitClient(null, 'stopCapture');
}

setInterval(() => {
  if(alt.players.length > 0) {
    if(currentTurf == null) {
      startCapture();
    }
    else {
      for(let p of alt.players) {
        const pTeam = p.getMeta('team');
        if(!pTeam)
          continue;

        if(currentTurf.contains(p.pos.x, p.pos.y)) {
          currentTurfPoints[pTeam]++;
          if(currentTurfPoints[pTeam] >= 1000) {
            chat.broadcast(`{${colors[pTeam].hex}} ${pTeam} {FFFFFF}got this turf. Next capture started`);
            stopCapture();
            return;
          }
        }
      }
      alt.emitClient(null, 'updateTeamPoints', currentTurfPoints);
    }
  }
  else if(currentTurf != null) {
    stopCapture();
  }
}, 1000);

function getTeamsPopulation() {
  const population = {
    ballas: 0,
    families: 0,
    vagos: 0
  };
  for(let p of alt.players) {
    const team = p.getMeta('team');
    if(team) {
      population[team]++;
    }
  }
  return population;
}

function broadcastTeamsPopulation() {
  for(let p of alt.players) {
    if(p.getMeta('selectingTeam')) {
      alt.emitClient(p, 'showTeamSelect', getTeamsPopulation());
    }
  }
}

function broadcastPlayersOnline(add) {
  if(add !== undefined)
    alt.emitClient(null, 'updatePlayersOnline', alt.players.length + add);
  else
    alt.emitClient(null, 'updatePlayersOnline', alt.players.length);
}

alt.onClient('authData', (player, data) => {

  const licenseHash = data.sc;
  player.setMeta('licenseHash', licenseHash);
  let dsInfo = null;
  if(data.discord && data.discord.id) {
    dsInfo = data.discord.id;
    player.setMeta('discordId', dsInfo);
  }
  if(licenseHash in blacklistData || (dsInfo !== null && dsInfo in blacklistData)) {
    chat.broadcast(`{5555AA}${player.name} {FFFFFF}kicked (Blacklisted)`);
    player.kick();
  }
});

chat.registerCmd('kick', (player, args) => {
  if (player.getMeta('admin')) {
    if (args.length > 0) {
      let players = alt.getPlayersByName(args.join(' '));

      if(players.length != 0) {
        for (const p of players) {
          chat.send(p, `{FF0000}You was kicked from the server`);
          chat.broadcast(`{5555AA}${p.name} {FFFFFF}kicked`);
          p.kick();
        }
      }
      else {
        for(const p of players) {
          if(p.name.startsWith(args.join(' '))) {
            chat.send(p, `{FF0000}You was kicked from the server`);
            chat.broadcast(`{5555AA}${p.name} {FFFFFF}kicked`);
            p.kick();
          }
        }
      }
    }
  } else {
    chat.send(player, '{FF00FF}You don`t have enough permissions to use this command');
  }
});

chat.registerCmd('ban', (player, args) => {
  if (player.getMeta('admin')) {
    if (args.length > 0) {
      let players = alt.getPlayersByName(args.join(' '));

      if(players.length != 0) {
        for (const p of players) {
          chat.send(p, `{FF0000}You was banned from the server`);
          chat.broadcast(`{5555AA}${p.name} {FFFFFF}banned`);
          addToBlacklist(p.getMeta('licenseHash'));
          const discordId = p.getMeta('discordId');
          if(discordId) {
            addToBlacklist(discordId);
          }
          p.kick();
        }
      }
      else {
        for(const p of players) {
          if(p.name.startsWith(args.join(' '))) {
            chat.send(p, `{FF0000}You was banned from the server`);
            chat.broadcast(`{5555AA}${p.name} {FFFFFF}banned`);
            addToBlacklist(p.getMeta('licenseHash'));
            const discordId = p.getMeta('discordId');
            if(discordId) {
              addToBlacklist(discordId);
            }
            p.kick();
          }
        }
      }
    }
  } else {
    chat.send(player, '{FF00FF}You don`t have enough permissions to use this command');
  }
});

alt.on('playerConnect', (player) => {
  player.setMeta('selectingTeam', true);
  player.setMeta('checkpoint', 0);
  player.setMeta('vehicle', null);
  player.setMeta('canSpawnVehicle', 0);
  player.setMeta('warns', 0);

  broadcastPlayersOnline();

  chat.broadcast(`{5555AA}${player.name} {FFFFFF}connected`);
  alt.log(`${player.name} connected`);

  alt.emitClient(player, 'youAreConnected');
});

alt.onClient('viewLoaded', (player) => {
  alt.log('View loaded for ' + player.name);
  alt.emitClient(player, 'showTeamSelect', getTeamsPopulation());
  alt.emitClient(player, 'updatePlayersOnline', alt.players.length);
});

alt.on('playerDisconnect', (player) => {
  const veh = player.getMeta('vehicle');
  if(veh) {
    alt.removeEntity(veh);
  }

  player.setMeta('selectingTeam', false);

  broadcastTeamsPopulation();
  broadcastPlayersOnline(-1);

  chat.broadcast(`{5555AA}${player.name} {FFFFFF}disconnected`);
  alt.log(`${player.name} disconnected`);
})

alt.onClient('teamSelected', (player, teamId) => {
  let team = 'families';
  if(teamId == 2)
    team = 'ballas';
  else if(teamId == 3)
    team = 'vagos';
  
  player.setMeta('team', team);
  player.setMeta('selectingTeam', false);
  broadcastTeamsPopulation();

  chat.broadcast(`{5555AA}${player.name} {FFFFFF}joined {${colors[team].hex}}${team}`);

  alt.log(player.name + ' joined ' + team);
  const nextSpawns = positions[team].spawns;

  const spawn = nextSpawns[Math.round(Math.random() * (nextSpawns.length - 1))];
  console.log('Spawning in ' + JSON.stringify(spawn));

  player.spawn(spawn.x, spawn.y, spawn.z, 0);
  alt.emitClient(player, 'applyAppearance', team);
  alt.emitClient(player, 'updateTeam', team);

  if(currentTurf != null) {
    alt.emitClient(null, 'captureStateChanged', true);
    alt.emitClient(null, 'startCapture', {
      x1: Math.min(currentTurf.x1, currentTurf.x2), y1: Math.min(currentTurf.y1, currentTurf.y2), x2: Math.max(currentTurf.x1, currentTurf.x2), y2: Math.max(currentTurf.y1, currentTurf.y2)
    });
    alt.emitClient(null, 'updateTeamPoints', currentTurfPoints);
  }
});

alt.onClient('action', (player) => {
  const cp = player.getMeta('checkpoint');
  if(cp == 1) {
    const nextTimeSpawn = player.getMeta('canSpawnVehicle');
    if(nextTimeSpawn > Date.now())
      return;

    const pTeam = player.getMeta('team');
    const pos = player.pos;
    let curVeh = player.getMeta('vehicle');
    if(curVeh) {
      alt.removeEntity(curVeh);
      curVeh = null;
    }

    const nextModel = vehicles[pTeam][Math.round(Math.random() * (vehicles[pTeam].length - 1))];
    const vehColor = colors[pTeam].rgba;
    curVeh = alt.createVehicle(nextModel, pos.x, pos.y, pos.z, 0, 0, 0);
    curVeh.customPrimaryColor = { r: vehColor.r, g: vehColor.g, b: vehColor.b };
    curVeh.customSecondaryColor = { r: vehColor.r, g: vehColor.g, b: vehColor.b };
    
    setTimeout(((player) => {
      alt.emitClient(player, 'setintoveh', curVeh);
      player.setMeta('intoVehTimeout', null);
    }).bind(null, player), 200);

    player.setMeta('vehicle', curVeh);
    player.setMeta('canSpawnVehicle', Date.now() + 400);
  }
  else if(cp == 2) {
    alt.emitClient(player, 'giveAllWeapons');
  }
});

alt.on('entityEnterCheckpoint', (cp, entity) => {
  if (entity instanceof alt.Player) {
    const pTeam = entity.getMeta('team');
    if(cp == checkpoints[pTeam].vehicle) {
      entity.setMeta('checkpoint', 1);
      alt.emitClient(entity, 'showInfo', '~INPUT_PICKUP~ to get car');
    }
    else if(cp == checkpoints[pTeam].weapon) {
      entity.setMeta('checkpoint', 2);
      alt.emitClient(entity, 'showInfo', '~INPUT_PICKUP~ to get weapons and ammo');
    }
  }
});

alt.on('entityLeaveCheckpoint', (cp, entity) => {
  if (entity instanceof alt.Player) {
    entity.setMeta('checkpoint', 0);
  }
});

alt.on('playerDead', (player, killer, weapon) => {
  let weaponName = 'Killed';
  if(weapon in weaponHashes)
    weaponName = weaponHashes[weapon];

  if(player == killer && weaponName == 'Killed')
    weaponName = 'Suicided';
  else if(weaponName == 'Killed')
    console.log('Unknown death reason: ' + weapon.toString(16));

  const team = player.getMeta('team');
  if(!killer)
    killer = player;

  const nextSpawns = positions[team].spawns;
  const spawnPos = nextSpawns[Math.round(Math.random() * (nextSpawns.length - 1))];
  player.spawn(spawnPos.x, spawnPos.y, spawnPos.z, 5000);

  if(killer) {
    const killerTeam = killer.getMeta('team');
    alt.emitClient(null, 'playerKill', {killerName: killer.name, killerGang: killerTeam, victimName: player.name, victimGang: team, weapon: weaponName});
  
    if(currentTurf != null && killer != player && team != killerTeam) {
      if(currentTurf.contains(player.pos.x, player.pos.y)) {
        currentTurfPoints[killerTeam] += 50;
        if(currentTurfPoints[killerTeam] >= 1000) {
          chat.broadcast(`{${colors[killerTeam].hex}} ${killerTeam} {FFFFFF}got this turf. Next capture started`);
          stopCapture();
        }
      }
    }
    else if(currentTurf != null && team == killerTeam) {
      if(currentTurf.contains(player.pos.x, player.pos.y)) {
        if(currentTurfPoints[killerTeam] > 50)
          currentTurfPoints[killerTeam] -= 50;
        else
          currentTurfPoints[killerTeam] = 0;
      }
    }

    if(team == killerTeam && player != killer) {
      let warns = killer.getMeta('warns');
      if(warns == 2) {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}kicked for team killing`);
        killer.kick();
      }
      else {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}warned [${warns + 1}/3] for team killing`);
        killer.setMeta('warns', (warns + 1));
      }
    }
    else if(player != killer && weaponName == weapons.WEAPON_RUN_OVER_BY_CAR) {
      let warns = killer.getMeta('warns');
      if(warns == 2) {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}kicked for vehicle kill`);
        killer.kick();
      }
      else {
        chat.broadcast(`{5555AA}${killer.name} {AA0000}warned [${warns + 1}/3] for vehicle kill`);
        killer.setMeta('warns', (warns + 1));
      }
    }
  }
});

setInterval(() => {
  for(let p of alt.players) {
    const lastPos = p.getMeta('lastPos');
    if(lastPos) {
      if(lastPos.x == p.pos.x && lastPos.y == p.pos.y && lastPos.z == p.pos.z) {
        chat.broadcast(`${p.name} {FFFFFF}was kicked for being AFK`);
        p.kick();
      }
    }
    else {
      p.setMeta('lastPos', p.pos);
    }
  }
}, 60000);
