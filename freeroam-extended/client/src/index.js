import { LOCAL_PLAYER, playerData, distance } from './helpers';

import * as native from 'natives';
import * as alt from 'alt-client';

import './events';
import './chat';

alt.setConfigFlag('DISABLE_AUTO_WEAPON_SWAP', true);
alt.setConfigFlag('DISABLE_IDLE_CAMERA', true);
alt.setStat('STAMINA', 100);

alt.everyTick(() => {
    // native.drawSphere(-1216.839599609375, -2832.514404296875, 13.9296875, 800, 0, 0, 255, 100);
    if (playerData.areNametagsVisible) {
        processNametags();
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

function drawText3d(text, x, y, z, scale, r, g, b, a, outline, offset, lagcomp, lagcompEntity) {
    // If lagcomp is enabled and the lagcomp entity is in a vehicle.
    if (lagcomp === true && lagcompEntity.vehicle !== null) {
        const vector = native.getEntityVelocity(lagcompEntity.vehicle);
        const frameTime = native.getFrameTime();

        native.setDrawOrigin(x + (vector.x * frameTime), y + (vector.y * frameTime), z + (vector.z * frameTime), 0);
    } else native.setDrawOrigin(x, y, z, 0);
    
    native.setTextFont(4);
    native.setTextProportional(false);
    native.setTextScale(scale, scale);
    native.setTextColour(r, g, b, a);
    native.setTextDropshadow(0, 0, 0, 0, 255);
    native.setTextEdge(2, 0, 0, 0, 150);
    native.setTextDropShadow();
    native.setTextCentre(true);

    if (outline) native.setTextOutline();

    native.beginTextCommandDisplayText("CELL_EMAIL_BCON");

    text.match(/.{1,99}/g).forEach(textBlock => {
        native.addTextComponentSubstringPlayerName(textBlock);
    });

    native.endTextCommandDisplayText(0.0, offset, 0.0);
    native.clearDrawOrigin();
};