import { LOCAL_PLAYER, playerData, distance, drawText3d, drawText2D, mhint } from './helpers';

import * as native from 'natives';
import * as alt from 'alt-client';

import './events';
import './chat';
import './noclip';

alt.setConfigFlag('DISABLE_AUTO_WEAPON_SWAP', true);
alt.setConfigFlag('DISABLE_IDLE_CAMERA', true);
alt.setStat('STAMINA', 100);

alt.everyTick(() => {
    // workaround for flickering GTA BUG
    native.drawRect(0, 0, 0, 0, 0, 0, 0, 0, 0);

    drawText2D('alt:V Public Stress Test - 5.12.2021', { x: 0.5, y: 0.0125 }, 0.35, { r: 255, g: 255, b: 255, a: 125 }, 0);

    // native.drawSphere(-1216.839599609375, -2832.514404296875, 13.9296875, 800, 0, 0, 255, 100);
    if (playerData.areNametagsVisible) {
        processNametags();

        drawText2D('F2: toggle HUD', { x: 0.825, y: 0.025 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('T / Enter: open chat', { x: 0.825, y: 0.05 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/tp <1 to 22>', { x: 0.825, y: 0.075 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/model <model> (change ped model)', { x: 0.825, y: 0.1 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/veh <model> (spawn vehicle)', { x: 0.825, y: 0.125 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/clearvehicles (clear your vehicles)', { x: 0.825, y: 0.15 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/tune <index> <value> (tune vehicle)', { x: 0.825, y: 0.175 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/weapons (give yourself weapons)', { x: 0.825, y: 0.2 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/addcomponent <name> (add weapon component)', { x: 0.825, y: 0.225 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/removecomponent <name> (remove weapon component)', { x: 0.825, y: 0.25 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
        drawText2D('/dm (toggles respawning in death match zone)', { x: 0.825, y: 0.275 }, 0.35, { r: 255, g: 255, b: 255, a: 255 }, 1);
    }

    if (playerData.areWeaponsDisabled) {
        native.setCanPedEquipAllWeapons(LOCAL_PLAYER, false);
        native.disablePlayerFiring(LOCAL_PLAYER, true);
    }
    else {
        native.setCanPedEquipAllWeapons(LOCAL_PLAYER, true);
    }
});

function processNametags() {
    renderNametags(LOCAL_PLAYER);

    const streamedIn = alt.Player.streamedIn;
    for (let i = 0, len = streamedIn.length; i < len; i++) {
        let player = streamedIn[i];
        if (player.valid && distance(LOCAL_PLAYER.pos, player.pos) <= 25) {
            renderNametags(player);
        }
    }
}

function renderNametags(player) {
    native.requestPedVisibilityTracking(player);
    if (!native.isTrackedPedVisible(player) && alt.Player.local.vehicle === null) return;

    const distance = alt.Player.local.pos.distanceTo(player.pos);
    const pos = native.getPedBoneCoords(player.scriptID, 31086, 0.0, 0.0, 0.0);
    const scale = 0.35 - distance * 0.01;

    let nametagText = `~n~~w~<font color='#FFFFFF'>${player.name} #${player.id} | ~g~${player.health - 100} / 100</font>`;

    if (player === LOCAL_PLAYER && playerData.areWeaponsDisabled) {
        nametagText = "~o~Weapons Disabled" + nametagText;
    }

    drawText3d(
        nametagText,
        pos.x, pos.y, (pos.z + -(scale)) + 1.0,
        scale, 255, 255, 255, 255, true, 0.038 * (-scale), true, player
    );
}