import * as native from 'natives';
import * as alt from 'alt-client';

export const LOCAL_PLAYER = alt.Player.local;
export const EMPTY_WEAPON_HASH = 0xA2719263;

export const playerData = {
    areWeaponsDisabled: true,
    areNametagsVisible: true,
    lastCommandTimestamp: Date.now() - 10000,
    lastMessageTimestamp: Date.now() - 10000,
    chatState: false
}

export function distance(vector1, vector2) {
    return Math.sqrt(
      Math.pow(vector1.x - vector2.x, 2) +
        Math.pow(vector1.y - vector2.y, 2) +
        Math.pow(vector1.z - vector2.z, 2)
    );
}

export function displayAdvancedNotification(message, title = "Title", subtitle = "subtitle", notifImage = null, iconType = 0, backgroundColor = null, durationMult = 1) {
    native.beginTextCommandThefeedPost('STRING')
    native.addTextComponentSubstringPlayerName(message)
    if (backgroundColor != null) native.thefeedSetNextPostBackgroundColor(backgroundColor)
    if (notifImage != null) native.endTextCommandThefeedPostMessagetextTu(notifImage, notifImage, false, iconType, title, subtitle, durationMult)
    return native.endTextCommandThefeedPostTicker(false, true)
}

export function setWeaponsUsage(state) {
    native.playSoundFrontend(-1, 'SIGN_DESTROYED', 'HUD_AWARDS', 1);

    if (state) {
        playerData.areWeaponsDisabled = false;

        native.setCanPedEquipAllWeapons(LOCAL_PLAYER, true);
        displayAdvancedNotification('Have fun.', 'Weapons Usage', 'Activated', 'CHAR_AMMUNATION', 1, 203, 1.5);
    }
    else {
        native.giveWeaponToPed(LOCAL_PLAYER, EMPTY_WEAPON_HASH, 0, false, true);
        playerData.areWeaponsDisabled = true;
    
        native.setCanPedEquipAllWeapons(LOCAL_PLAYER, false);
        displayAdvancedNotification('You can only use weapons in the LS Airport zone.', 'Weapons Usage', 'Deactivated', 'CHAR_AMMUNATION', 1, 31, 1.5);
    }
}