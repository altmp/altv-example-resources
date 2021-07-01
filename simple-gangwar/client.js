import * as alt from 'alt-client';
import * as game from 'natives';
import chat from 'chat';

let myTeam = null;

alt.emitServer('authData', {
  discord: alt.discordInfo(),
  sc: alt.getLicenseHash()
});

const weapons = [
  "WEAPON_KNIFE", "WEAPON_BAT", "WEAPON_BOTTLE", "WEAPON_WRENCH",
  "WEAPON_PISTOL", "WEAPON_HEAVYPISTOL", "WEAPON_REVOLVER",
  "WEAPON_MICROSMG", "WEAPON_SMG", "WEAPON_COMBATPDW",
  "WEAPON_ASSAULTRIFLE", "WEAPON_CARBINERIFLE",
  "WEAPON_PUMPSHOTGUN"
];

function giveWeapons() {
  let ped = game.playerPedId()

  for (const weapon of weapons) {
    game.giveWeaponToPed(ped, game.getHashKey(weapon), 9999, false, false)
  }
}

const clothes = {
  families: {
    1: {
      drawable: 51,
      texture: 5
    },
    2: {
      drawable: 8,
      texture: 1
    },
    3: {
      drawable: 1,
      texture: 0
    },
    4: {
      drawable: 15,
      texture: 13
    },
    6: {
      drawable: 9,
      texture: 4
    },
    8: {
      drawable: 0,
      texture: 240
    },
    11: {
      drawable: 14,
      texture: 6
    }
  },
  ballas: {
    1: {
      drawable: 51,
      texture: 6
    },
    2: {
      drawable: 10,
      texture: 4
    },
    3: {
      drawable: 5,
      texture: 0
    },
    4: {
      drawable: 88,
      texture: 23
    },
    6: {
      drawable: 9,
      texture: 3
    },
    8: {
      drawable: 0,
      texture: 240
    },
    11: {
      drawable: 17,
      texture: 3
    }
  },
  vagos: {
    1: {
      drawable: 51,
      texture: 8
    },
    2: {
      drawable: 10,
      texture: 3
    },
    3: {
      drawable: 5,
      texture: 0
    },
    4: {
      drawable: 88,
      texture: 19
    },
    6: {
      drawable: 9,
      texture: 11
    },
    8: {
      drawable: 0,
      texture: 240
    },
    11: {
      drawable: 17,
      texture: 2
    }
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

let leadingTeam = null;
let lastLeadingTeam = null;

const teamColors = {
  ballas: {
    rgba: { r: 196, g: 0, b: 171, a: 150 },
    hex: 'C400AB',
    blipColor: 83
  },
  families: {
    rgba: { r: 0, g: 127, b: 0, a: 150 },
    hex: '008000',
    blipColor: 52
  },
  vagos: {
    rgba: { r: 255, g: 191, b: 0, a: 150 },
    hex: 'FFBF00',
    blipColor: 81
  }
};

let mainView = null;
let viewLoaded = false;

function loadWebView()
{
  mainView = new alt.WebView('http://resources/ls-gangwar/client/html/index.html');
  mainView.on('viewLoaded', () => {
    alt.log('GangWar view loaded');
    alt.emitServer('viewLoaded');
    viewLoaded = true;
  });

  mainView.on('teamSelected', (teamId) => {
    alt.emitServer('teamSelected', teamId);
    alt.showCursor(false);
  });
}

alt.onServer('youAreConnected', () => {
  chat.pushLine('Loading...');
  loadWebView();
});

let weaponBlip = null;
let vehicleBlip = null;
alt.onServer('updateTeam', (team) => {
  myTeam = team;
  if (weaponBlip) {
    game.removeBlip(weaponBlip);
  }
  weaponBlip = game.addBlipForCoord(positions[myTeam].weapon.x, positions[myTeam].weapon.y, positions[myTeam].weapon.z);
  game.setBlipSprite(weaponBlip, 110);
  game.beginTextCommandSetBlipName('STRING');
  game.addTextComponentSubstringPlayerName('Weapon provider');
  game.endTextCommandSetBlipName(weaponBlip);

  if (vehicleBlip) {
    game.removeBlip(vehicleBlip);
  }
  vehicleBlip = game.addBlipForCoord(positions[myTeam].vehicle.x, positions[myTeam].vehicle.y, positions[myTeam].vehicle.z);
  game.setBlipSprite(vehicleBlip, 227);
  game.beginTextCommandSetBlipName('STRING');
  game.addTextComponentSubstringPlayerName('Vehicle provider');
  game.endTextCommandSetBlipName(vehicleBlip);
});

const colors = {
  ballas: 'C400AB',
  families: '008000',
  vagos: 'FFBF00'
};

alt.onServer('applyAppearance', (team) => {
  game.setPedDefaultComponentVariation(game.playerPedId());
  const components = clothes[team];
  for (let c in components) {
    game.setPedComponentVariation(game.playerPedId(), c, components[c].drawable, components[c].texture, 0);
  }
});

alt.onServer('updateTeamPoints', (info) => {
  let myTeamPoints = info[myTeam];
  if(viewLoaded)
    mainView.emit('setTeamPoints', myTeam, myTeamPoints);

  const teamsArray = [];
  for (let t in info) {
    teamsArray.push({
      team: t,
      scores: info[t]
    });
  }
  teamsArray.sort((a, b) => {
    return a.scores < b.scores ? 1 : -1;
  });
  if(teamsArray[0].scores == 0)
    leadingTeam = null;
  else
    leadingTeam = teamsArray[0].team;

  const rightTeam = teamsArray[0].team == myTeam ? teamsArray[1] : teamsArray[0];

  const progressLeft = myTeamPoints / 1000;
  const progressRight = rightTeam.scores / 1000;
  const colorLeft = colors[myTeam];
  const colorRight = colors[rightTeam.team];

  if(viewLoaded)
    mainView.emit('setProgress', progressLeft, progressRight, '#' + colorLeft, '#' + colorRight);
});

alt.onServer('captureStateChanged', (state) => {
  if(!viewLoaded)
    return;

  if (state == false) {
    mainView.emit('hideProgress');
  } else {
    mainView.emit('showProgress');
  }
});

alt.onServer('playerKill', (data) => {
  if(!viewLoaded)
    return;

  mainView.emit('registerKill', data);
});

alt.onServer('showTeamSelect', (teamsPopulation) => {
  if(!viewLoaded)
    return;

  alt.log(JSON.stringify(teamsPopulation, null, 4));
  mainView.emit('showTeamSelect', teamsPopulation);
  mainView.focus();
  alt.showCursor(true);
});

alt.on('keydown', (key) => {
  if (key == 'E'.charCodeAt(0)) {
    alt.emitServer('action');
  }
});

alt.onServer('setintoveh', veh => {
  game.setPedIntoVehicle(game.playerPedId(), veh.scriptID, -1);
});

alt.onServer('giveAllWeapons', () => {
  giveWeapons();
});

let captureBlip = null;

alt.onServer('startCapture', (info) => {
  const { x1, x2, y1, y2 } = info;

  if (captureBlip != null) {
    game.removeBlip(captureBlip);
    captureBlip = null;
  }
  
  leadingTeam = null;
  lastLeadingTeam = null;
  captureBlip = game.addBlipForArea((x1 + x2) / 2, (y1 + y2) / 2, 0, 200, 200);
  // game.SetBlipSprite(captureBlip, 84);
  game.setBlipColour(captureBlip, 39);
  game.setBlipFlashTimer(captureBlip, 500);
  game.setBlipFlashInterval(captureBlip, 500);
  game.setBlipFlashes(captureBlip, true);
  game.setBlipAlpha(captureBlip, 125);
  game.setBlipRotation(captureBlip, 0)
  game.beginTextCommandSetBlipName('STRING');
  game.addTextComponentSubstringPlayerName('Turf War');
  game.endTextCommandSetBlipName(captureBlip);
  
  if(viewLoaded)
    mainView.emit('setProgress', 0, 0, '#000000', '#000000');
});

alt.onServer('stopCapture', () => {
  leadingTeam = null;
  lastLeadingTeam = null;
  if (captureBlip) {
    game.removeBlip(captureBlip);
    captureBlip = null;
  }

  if(viewLoaded)
    mainView.emit('setProgress', 0, 0, '#000000', '#000000');
});

alt.on('update', () => {
  if (captureBlip) {
    if (leadingTeam && leadingTeam != lastLeadingTeam && leadingTeam in teamColors) {
      game.setBlipColour(captureBlip, teamColors[leadingTeam].blipColor);
      lastLeadingTeam = leadingTeam;
    } else if (!leadingTeam) {
      game.setBlipColour(captureBlip, 39);
      lastLeadingTeam = leadingTeam;
    }
  }
});

alt.onServer('showInfo', (text) => {
  game.beginTextCommandDisplayHelp('STRING');
  game.addTextComponentScaleform(text);
  game.endTextCommandDisplayHelp(0, 0, 0, -1);
});

alt.onServer('updatePlayersOnline', (players) => {
  if(!viewLoaded)
    return;
  mainView.emit('updatePlayersOnline', players);
});

game.requestIpl('chop_props');
game.requestIpl('FIBlobby');
game.removeIpl('FIBlobbyfake');
game.requestIpl('FBI_colPLUG');
game.requestIpl('FBI_repair');
game.requestIpl('v_tunnel_hole');
game.requestIpl('TrevorsMP');
game.requestIpl('TrevorsTrailer');
game.requestIpl('TrevorsTrailerTidy');
game.removeIpl('farm_burnt');
game.removeIpl('farm_burnt_lod');
game.removeIpl('farm_burnt_props');
game.removeIpl('farmint_cap');
game.removeIpl('farmint_cap_lod');
game.requestIpl('farm');
game.requestIpl('farmint');
game.requestIpl('farm_lod');
game.requestIpl('farm_props');
game.requestIpl('facelobby');
game.removeIpl('CS1_02_cf_offmission');
game.requestIpl('CS1_02_cf_onmission1');
game.requestIpl('CS1_02_cf_onmission2');
game.requestIpl('CS1_02_cf_onmission3');
game.requestIpl('CS1_02_cf_onmission4');
game.requestIpl('v_rockclub');
game.requestIpl('v_janitor');
game.removeIpl('hei_bi_hw1_13_door');
game.requestIpl('bkr_bi_hw1_13_int');
game.requestIpl('ufo');
game.requestIpl('ufo_lod');
game.requestIpl('ufo_eye');
game.removeIpl('v_carshowroom');
game.removeIpl('shutter_open');
game.removeIpl('shutter_closed');
game.removeIpl('shr_int');
game.requestIpl('csr_afterMission');
game.requestIpl('v_carshowroom');
game.requestIpl('shr_int');
game.requestIpl('shutter_closed');
game.requestIpl('smboat');
game.requestIpl('smboat_distantlights');
game.requestIpl('smboat_lod');
game.requestIpl('smboat_lodlights');
game.requestIpl('cargoship');
game.requestIpl('railing_start');
game.removeIpl('sp1_10_fake_interior');
game.removeIpl('sp1_10_fake_interior_lod');
game.requestIpl('sp1_10_real_interior');
game.requestIpl('sp1_10_real_interior_lod');
game.removeIpl('id2_14_during_door');
game.removeIpl('id2_14_during1');
game.removeIpl('id2_14_during2');
game.removeIpl('id2_14_on_fire');
game.removeIpl('id2_14_post_no_int');
game.removeIpl('id2_14_pre_no_int');
game.removeIpl('id2_14_during_door');
game.requestIpl('id2_14_during1');
game.removeIpl('Coroner_Int_off');
game.requestIpl('coronertrash');
game.requestIpl('Coroner_Int_on');
game.removeIpl('bh1_16_refurb');
game.removeIpl('jewel2fake');
game.removeIpl('bh1_16_doors_shut');
game.requestIpl('refit_unload');
game.requestIpl('post_hiest_unload');
game.requestIpl('Carwash_with_spinners');
game.requestIpl('KT_CarWash');
game.requestIpl('ferris_finale_Anim');
game.removeIpl('ch1_02_closed');
game.requestIpl('ch1_02_open');
game.requestIpl('AP1_04_TriAf01');
game.requestIpl('CS2_06_TriAf02');
game.requestIpl('CS4_04_TriAf03');
game.removeIpl('scafstartimap');
game.requestIpl('scafendimap');
game.removeIpl('DT1_05_HC_REMOVE');
game.requestIpl('DT1_05_HC_REQ');
game.requestIpl('DT1_05_REQUEST');
game.requestIpl('FINBANK');
game.removeIpl('DT1_03_Shutter');
game.removeIpl('DT1_03_Gr_Closed');
game.requestIpl('golfflags');
game.requestIpl('airfield');
game.requestIpl('v_garages');
game.requestIpl('v_foundry');
game.requestIpl('hei_yacht_heist');
game.requestIpl('hei_yacht_heist_Bar');
game.requestIpl('hei_yacht_heist_Bedrm');
game.requestIpl('hei_yacht_heist_Bridge');
game.requestIpl('hei_yacht_heist_DistantLights');
game.requestIpl('hei_yacht_heist_enginrm');
game.requestIpl('hei_yacht_heist_LODLights');
game.requestIpl('hei_yacht_heist_Lounge');
game.requestIpl('hei_carrier');
game.requestIpl('hei_Carrier_int1');
game.requestIpl('hei_Carrier_int2');
game.requestIpl('hei_Carrier_int3');
game.requestIpl('hei_Carrier_int4');
game.requestIpl('hei_Carrier_int5');
game.requestIpl('hei_Carrier_int6');
game.requestIpl('hei_carrier_LODLights');
game.requestIpl('bkr_bi_id1_23_door');
game.requestIpl('lr_cs6_08_grave_closed');
game.requestIpl('hei_sm_16_interior_v_bahama_milo_');
game.requestIpl('CS3_07_MPGates');
game.requestIpl('cs5_4_trains');
game.requestIpl('v_lesters');
game.requestIpl('v_trevors');
game.requestIpl('v_michael');
game.requestIpl('v_comedy');
game.requestIpl('v_cinema');
game.requestIpl('V_Sweat');
game.requestIpl('V_35_Fireman');
game.requestIpl('redCarpet');
game.requestIpl('triathlon2_VBprops');
game.requestIpl('jetstegameurnel');
game.requestIpl('Jetsteal_ipl_grp1');
game.requestIpl('v_hospital');
game.removeIpl('RC12B_Default');
game.removeIpl('RC12B_Fixed');
game.requestIpl('RC12B_Destroyed');
game.requestIpl('RC12B_HospitalInterior');
game.requestIpl('canyonriver01');
