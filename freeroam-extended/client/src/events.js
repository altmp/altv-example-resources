import { drawDMZone, playerData, setWeaponsUsage, mhint } from './helpers';
import { pushMessage, chatData, view, toggleChat } from './chat';
import { toggleNoclip } from './noclip';

import * as native from 'natives';
import * as alt from 'alt-client';

alt.on('connectionComplete', () => {
    setTimeout(() => {
        // We assume that we are not in the airport if areWeaponsDisabled is on true when it triggers
        if (playerData.areWeaponsDisabled) {
            setWeaponsUsage(false);
        }
    }, 1000);
});

alt.onServer('airport_state', setWeaponsUsage);

alt.onServer("chat:message", pushMessage);

alt.onServer('set_last_command', () => {
    playerData.commandTimestamp = Date.now();
});

alt.onServer('noclip', toggleNoclip);

alt.onServer('set_chat_state', state => {
    playerData.chatState = state;
});

alt.onServer('draw_dmzone', (center_x, center_y, radius, count) => {
    drawDMZone(center_x, center_y, radius, count);
});

alt.onServer('announce', (header, body, time) => {
    mhint(header, body, time);
});

alt.on('keyup', (key) => {
    if (!chatData.loaded) return;
    
    switch (key) {
        case 113: { // F2
            playerData.areNametagsVisible = !playerData.areNametagsVisible;
            native.displayRadar(playerData.areNametagsVisible);
            native.displayHud(playerData.areNametagsVisible);
            toggleChat();
            break;
        }

        case 0xD:     // Enter
        case 0x54: { // T
            if (!chatData.opened && alt.gameControlsEnabled()) {
                chatData.opened = true;
                view.emit('openChat', false);
                view.focus();
                alt.toggleGameControls(false);
                alt.emit("Client:HUD:setCefStatus", true);
            }
            break;
        }
        
        case 0x1B: { // Escape
            if (chatData.opened) {
                chatData.opened = false;
                view.emit('closeChat');
                view.unfocus();
                alt.toggleGameControls(true);
                alt.emit("Client:HUD:setCefStatus", false);
            }
            break;
        }
    
        default:
            break;
    }
});