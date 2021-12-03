import { LOCAL_PLAYER, playerData, distance } from './helpers';

import * as native from 'natives';
import * as alt from 'alt-client';

import './events';

alt.setConfigFlag('DISABLE_AUTO_WEAPON_SWAP', true);
alt.setConfigFlag('DISABLE_IDLE_CAMERA', true);
alt.setStat('STAMINA', 100);

alt.everyTick(() => {
    native.drawSphere(-1216.839599609375, -2832.514404296875, 13.9296875, 800, 0, 0, 255, 100);

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

    for (let player of alt.Player.streamedIn) {
        if (player.valid && distance(LOCAL_PLAYER.pos, player.pos) <= 25) {
            renderNametags(player);
        }
    }
}

function renderNametags(player) {
    native.requestPedVisibilityTracking(player);
    if (!native.isTrackedPedVisible(player)) return;

    const entity = player.vehicle ? player.vehicle : player;
    const velocity = native.getEntityVelocity(entity);
    const frameTime = native.getFrameTime();

    const lagFixX = player.pos.x + (velocity.x * frameTime);
    const lagFixY = player.pos.y + (velocity.y * frameTime);
    const lagFixZ = player.pos.z + (velocity.z * frameTime);

    if (player === LOCAL_PLAYER && playerData.areWeaponsDisabled) {
        native.setDrawOrigin(lagFixX + 0.025, lagFixY, lagFixZ + 1.1, 0);
        native.beginTextCommandDisplayText("STRING");
        native.setTextFont(4);
        native.setTextCentre(true);
        native.setTextScale(0.35, 0.35);
        native.setTextProportional(true);
        native.setTextColour(255, 255, 255, 255);
        native.addTextComponentSubstringPlayerName('~o~Weapons disabled');
        native.endTextCommandDisplayText(0, 0, 0);
    }

    native.setDrawOrigin(lagFixX + 0.025, lagFixY, lagFixZ + 1, 0);
    native.beginTextCommandDisplayText("STRING");
    native.setTextFont(4);
    native.setTextCentre(true);
    native.setTextScale(0.4, 0.4);
    native.setTextProportional(true);
    native.setTextColour(255, 255, 255, 255);
    native.addTextComponentSubstringPlayerName(`${player.name} #${player.id} | ~g~${player.health - 100} / 100`);
    native.endTextCommandDisplayText(0, 0, 0);
}
