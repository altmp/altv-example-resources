import * as alt from "alt-client";
import { playerData } from './helpers';

export const chatData = {
    loaded: false,
    opened: false
}

export const view = new alt.WebView("http://resource/client/src/html/index.html");
const buffer = [];

function addMessage(name, text) {
    if (name) {
        view.emit("addMessage", name, text);
    } else {
        view.emit("addString", text);
    }
}

export function pushMessage(name, text) {
    if (!chatData.loaded) {
        buffer.push({ name, text });
    } else {
        addMessage(name, text);
    }
}

export function pushLine(text) {
    pushMessage(null, text);
}

view.on("chatloaded", () => {
    for (const msg of buffer) {
        addMessage(msg.name, msg.text);
    }

    chatData.loaded = true;
});

view.on("chatmessage", (text) => {
    // alt.emitServer("chat:message", text);

    console.log(playerData.chatState);
    if (text.startsWith('/') && (Date.now() - playerData.lastCommandTimestamp) / 1000 > 10) {
        alt.emitServer("chat:message", text);
    }
    else if (playerData.chatState && ((Date.now() - playerData.lastMessageTimestamp) / 1000 > 10)) {
        alt.emitServer("chat:message", text);
        playerData.lastMessageTimestamp = Date.now();
    }

    alt.toggleGameControls(true);
    view.unfocus();

    // Timeout to avoid collision with Enter key
    setTimeout(() => {
        chatData.opened = false;
    }, 150);
});

pushLine("<b>alt:V Multiplayer has started</b>");
