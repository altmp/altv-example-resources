import * as alt from 'alt-client';
import * as game from 'natives';
import chat from 'chat';

let myTeam = null;

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

function loadWebView() {
  mainView = new alt.WebView('http://resource/client/html/index.html');
  mainView.on('viewLoaded', () => {
    alt.log('GangWar view loaded');
    alt.emitServer('viewLoaded');
    viewLoaded = true;
  });

  mainView.on('teamSelected', (teamId) => {
    alt.emitServer('teamSelected', teamId);
    alt.toggleGameControls(true);
    alt.showCursor(false);
  });
}

alt.on("connectionComplete", () => {
  loadIpls();
  alt.emitServer('authData', {
    discord: alt.Discord.currentUser,
    sc: alt.getLicenseHash()
  });
});

alt.onServer('youAreConnected', () => {
  chat.pushLine('Loading...');
  loadWebView();
});

let weaponBlip = null;
let vehicleBlip = null;
alt.onServer('updateTeam', (team) => {
  myTeam = team;
  if (weaponBlip) {
    weaponBlip.destroy();
  }

  weaponBlip = new alt.PointBlip(
    positions[myTeam].weapon.x, positions[myTeam].weapon.y, positions[myTeam].weapon.z
  );

  weaponBlip.sprite = 110;
  weaponBlip.name = 'Weapon provider';
  weaponBlip.alpha = 200;
  weaponBlip.shortRange = true;

  if (vehicleBlip) {
    vehicleBlip.destroy();
  }

  vehicleBlip = new alt.PointBlip(
    positions[myTeam].vehicle.x, positions[myTeam].vehicle.y, positions[myTeam].vehicle.z
  );

  vehicleBlip.sprite = 227;
  vehicleBlip.name = 'Vehicle provider';
  vehicleBlip.alpha = 200;
  vehicleBlip.shortRange = true;
});

const colors = {
  ballas: 'C400AB',
  families: '008000',
  vagos: 'FFBF00'
};

alt.onServer('applyAppearance', (team) => {
  const components = clothes[team];
  for (let c in components) {
    game.setPedComponentVariation(alt.Player.local.scriptID, parseInt(c), components[c].drawable, components[c].texture, 0);
  }
});

alt.onServer('updateTeamPoints', (info) => {
  let myTeamPoints = info[myTeam];
  if(viewLoaded) mainView.emit('setTeamPoints', myTeam, myTeamPoints);

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
  if(!viewLoaded) return;
  if (state == false) {
    mainView.emit('hideProgress');
  } else {
    mainView.emit('showProgress');
  }
});

alt.onServer('playerKill', (data) => {
  if(!viewLoaded) return;
  mainView.emit('registerKill', data);
});

alt.onServer('showTeamSelect', (teamsPopulation) => {
  if(!viewLoaded)return;
  mainView.emit('showTeamSelect', teamsPopulation);
  mainView.focus();
  alt.toggleGameControls(false);
  alt.showCursor(true);
});

alt.on('keydown', (key) => {
  if (key == 'E'.charCodeAt(0)) {
    alt.emitServer('action');
  }
});

let captureBlip = null;

alt.onServer('startCapture', (info) => {
  const { x1, x2, y1, y2 } = info;

  if (captureBlip != null) {
    captureBlip.destroy();
    captureBlip = null;
  }
  
  leadingTeam = null;
  lastLeadingTeam = null;
  captureBlip = new alt.AreaBlip((x1 + x2) / 2, (y1 + y2) / 2, 0, 200, 200);
  // game.SetBlipSprite(captureBlip, 84);
  captureBlip.color = 39;
  captureBlip.flashTimer = 500;
  captureBlip.flashInterval = 500;
  captureBlip.flashes = true;
  captureBlip.alpha = 125;
  captureBlip.heading = 0;
  captureBlip.name = 'Turf War';
  
  if(viewLoaded) {
    mainView.emit('setProgress', 0, 0, '#000000', '#000000');
  }
});

alt.onServer('stopCapture', () => {
  leadingTeam = null;
  lastLeadingTeam = null;
  if (captureBlip) {
    captureBlip.destroy();
    captureBlip = null;
  }

  if(viewLoaded) {
    mainView.emit('setProgress', 0, 0, '#000000', '#000000');
  }
});

alt.everyTick(() => {
  if (captureBlip) {
    if (leadingTeam && leadingTeam != lastLeadingTeam && leadingTeam in teamColors) {
      captureBlip.color = teamColors[leadingTeam].blipColor;
      lastLeadingTeam = leadingTeam;
    } else if (!leadingTeam) {
      captureBlip.color = 39;
      lastLeadingTeam = leadingTeam;
    }
  }
});

alt.onServer('showInfo', (text) => {
  game.beginTextCommandDisplayHelp('STRING');
  game.addTextComponentSubstringKeyboardDisplay(text);
  game.endTextCommandDisplayHelp(0, 0, 0, -1);
});

alt.onServer('updatePlayersOnline', (players) => {
  if(!viewLoaded)
    return;
  mainView.emit('updatePlayersOnline', players);
});

function loadIpls() {
  alt.requestIpl('chop_props');
  alt.requestIpl('FIBlobby');
  alt.removeIpl('FIBlobbyfake');
  alt.requestIpl('FBI_colPLUG');
  alt.requestIpl('FBI_repair');
  alt.requestIpl('v_tunnel_hole');
  alt.requestIpl('TrevorsMP');
  alt.requestIpl('TrevorsTrailer');
  alt.requestIpl('TrevorsTrailerTidy');
  alt.removeIpl('farm_burnt');
  alt.removeIpl('farm_burnt_lod');
  alt.removeIpl('farm_burnt_props');
  alt.removeIpl('farmint_cap');
  alt.removeIpl('farmint_cap_lod');
  alt.requestIpl('farm');
  alt.requestIpl('farmint');
  alt.requestIpl('farm_lod');
  alt.requestIpl('farm_props');
  alt.requestIpl('facelobby');
  alt.removeIpl('CS1_02_cf_offmission');
  alt.requestIpl('CS1_02_cf_onmission1');
  alt.requestIpl('CS1_02_cf_onmission2');
  alt.requestIpl('CS1_02_cf_onmission3');
  alt.requestIpl('CS1_02_cf_onmission4');
  alt.requestIpl('v_rockclub');
  alt.requestIpl('v_janitor');
  alt.removeIpl('hei_bi_hw1_13_door');
  alt.requestIpl('bkr_bi_hw1_13_int');
  alt.requestIpl('ufo');
  alt.requestIpl('ufo_lod');
  alt.requestIpl('ufo_eye');
  alt.removeIpl('v_carshowroom');
  alt.removeIpl('shutter_open');
  alt.removeIpl('shutter_closed');
  alt.removeIpl('shr_int');
  alt.requestIpl('csr_afterMission');
  alt.requestIpl('v_carshowroom');
  alt.requestIpl('shr_int');
  alt.requestIpl('shutter_closed');
  alt.requestIpl('smboat');
  alt.requestIpl('smboat_distantlights');
  alt.requestIpl('smboat_lod');
  alt.requestIpl('smboat_lodlights');
  alt.requestIpl('cargoship');
  alt.requestIpl('railing_start');
  alt.removeIpl('sp1_10_fake_interior');
  alt.removeIpl('sp1_10_fake_interior_lod');
  alt.requestIpl('sp1_10_real_interior');
  alt.requestIpl('sp1_10_real_interior_lod');
  alt.removeIpl('id2_14_during_door');
  alt.removeIpl('id2_14_during1');
  alt.removeIpl('id2_14_during2');
  alt.removeIpl('id2_14_on_fire');
  alt.removeIpl('id2_14_post_no_int');
  alt.removeIpl('id2_14_pre_no_int');
  alt.removeIpl('id2_14_during_door');
  alt.requestIpl('id2_14_during1');
  alt.removeIpl('Coroner_Int_off');
  alt.requestIpl('coronertrash');
  alt.requestIpl('Coroner_Int_on');
  alt.removeIpl('bh1_16_refurb');
  alt.removeIpl('jewel2fake');
  alt.removeIpl('bh1_16_doors_shut');
  alt.requestIpl('refit_unload');
  alt.requestIpl('post_hiest_unload');
  alt.requestIpl('Carwash_with_spinners');
  alt.requestIpl('KT_CarWash');
  alt.requestIpl('ferris_finale_Anim');
  alt.removeIpl('ch1_02_closed');
  alt.requestIpl('ch1_02_open');
  alt.requestIpl('AP1_04_TriAf01');
  alt.requestIpl('CS2_06_TriAf02');
  alt.requestIpl('CS4_04_TriAf03');
  alt.removeIpl('scafstartimap');
  alt.requestIpl('scafendimap');
  alt.removeIpl('DT1_05_HC_REMOVE');
  alt.requestIpl('DT1_05_HC_REQ');
  alt.requestIpl('DT1_05_REQUEST');
  alt.requestIpl('FINBANK');
  alt.removeIpl('DT1_03_Shutter');
  alt.removeIpl('DT1_03_Gr_Closed');
  alt.requestIpl('golfflags');
  alt.requestIpl('airfield');
  alt.requestIpl('v_garages');
  alt.requestIpl('v_foundry');
  alt.requestIpl('hei_yacht_heist');
  alt.requestIpl('hei_yacht_heist_Bar');
  alt.requestIpl('hei_yacht_heist_Bedrm');
  alt.requestIpl('hei_yacht_heist_Bridge');
  alt.requestIpl('hei_yacht_heist_DistantLights');
  alt.requestIpl('hei_yacht_heist_enginrm');
  alt.requestIpl('hei_yacht_heist_LODLights');
  alt.requestIpl('hei_yacht_heist_Lounge');
  alt.requestIpl('hei_carrier');
  alt.requestIpl('hei_Carrier_int1');
  alt.requestIpl('hei_Carrier_int2');
  alt.requestIpl('hei_Carrier_int3');
  alt.requestIpl('hei_Carrier_int4');
  alt.requestIpl('hei_Carrier_int5');
  alt.requestIpl('hei_Carrier_int6');
  alt.requestIpl('hei_carrier_LODLights');
  alt.requestIpl('bkr_bi_id1_23_door');
  alt.requestIpl('lr_cs6_08_grave_closed');
  alt.requestIpl('hei_sm_16_interior_v_bahama_milo_');
  alt.requestIpl('CS3_07_MPGates');
  alt.requestIpl('cs5_4_trains');
  alt.requestIpl('v_lesters');
  alt.requestIpl('v_trevors');
  alt.requestIpl('v_michael');
  alt.requestIpl('v_comedy');
  alt.requestIpl('v_cinema');
  alt.requestIpl('V_Sweat');
  alt.requestIpl('V_35_Fireman');
  alt.requestIpl('redCarpet');
  alt.requestIpl('triathlon2_VBprops');
  alt.requestIpl('jetstealturnel');
  alt.requestIpl('Jetsteal_ipl_grp1');
  alt.requestIpl('v_hospital');
  alt.removeIpl('RC12B_Default');
  alt.removeIpl('RC12B_Fixed');
  alt.requestIpl('RC12B_Destroyed');
  alt.requestIpl('RC12B_HospitalInterior');
  alt.requestIpl('canyonriver01');
}

