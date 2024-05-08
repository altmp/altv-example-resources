import * as alt from "alt-client";
import { CHAT_MESSAGE_EVENT } from "../shared/index.js";

let opened = false;

const view = new alt.WebView("http://resource/client/html/index.html");

function addMessage(name, text) {
  if (name) {
    view.emit("addMessage", name, text);
  } else {
    view.emit("addString", text);
  }
}

view.on("chatmessage", (text) => {
  alt.emitServer(CHAT_MESSAGE_EVENT, text);

  opened = false;
  alt.toggleGameControls(true);
  view.unfocus();
});

export function pushLine(text) {
  addMessage(null, text);
}

alt.onServer(CHAT_MESSAGE_EVENT, addMessage);

alt.on("keyup", (key) => {
  if (!opened && key === 0x54 && alt.gameControlsEnabled()) {
    opened = true;
    view.emit("openChat", false);
    alt.toggleGameControls(false);
    view.focus();
  } else if (!opened && key === 0xbf && alt.gameControlsEnabled()) {
    opened = true;
    view.emit("openChat", true);
    alt.toggleGameControls(false);
    view.focus();
  } else if (opened && key == 0x1b) {
    opened = false;
    view.emit("closeChat");
    alt.toggleGameControls(true);
    view.unfocus();
  }
});

pushLine("<b>alt:V Multiplayer has started</b>");
