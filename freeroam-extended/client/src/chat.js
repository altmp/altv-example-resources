import * as alt from "alt-client";
import { playerData } from './helpers';

let buffer = [];

let loaded = false;
let opened = false;

const view = new alt.WebView("http://resource/client/src/html/index.html");

function addMessage(name, text) {
    if (name) {
        view.emit("addMessage", name, text);
    } else {
        view.emit("addString", text);
    }
}

view.on("chatloaded", () => {
    for (const msg of buffer) {
        addMessage(msg.name, msg.text);
    }

    loaded = true;
});

view.on("chatmessage", (text) => {
    if (text.startsWith('/') && (Date.now() - playerData.lastCommandTimestamp) / 1000 > 10) {
        console.log('commands :D');
        alt.emitServer("chat:message", text);
    }
    else if (playerData.chatState && ((Date.now() - playerData.lastMessageTimestamp) / 1000 > 10)) {
        console.log('message :D');
        alt.emitServer("chat:message", text);
        playerData.lastMessageTimestamp = Date.now();
    }

    alt.toggleGameControls(true);
    view.unfocus();

    // Timeout to avoid collision with Enter key
    setTimeout(() => {
        opened = false;
    }, 150);
});

export function pushMessage(name, text) {
    if (!loaded) {
        buffer.push({ name, text });
    } else {
        addMessage(name, text);
    }
}

export function pushLine(text) {
    pushMessage(null, text);
}

alt.onServer("chat:message", pushMessage);

alt.onServer('set_last_command', () => {
    playerData.commandTimestamp = Date.now();
});

alt.onServer('set_chat_state', state => {
    playerData.chatState = state;
});

alt.on('keyup', (key) => {
    if (!loaded) return;
    
    switch (key) {
        case 0xD:     // Enter
        case 0x54: { // T
            if (!opened && alt.gameControlsEnabled()) {
                console.log('triggered again');
                opened = true;
                view.emit('openChat', false);
                view.focus();
                alt.toggleGameControls(false);
                alt.emit("Client:HUD:setCefStatus", true);
            }
            break;
        }
        
        case 0x1B: { // Escape
            if (opened) {
                opened = false;
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

pushLine("<b>alt:V Multiplayer has started</b>");
