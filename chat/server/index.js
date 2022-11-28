import * as alt from "alt-server";
import { CHAT_MESSAGE_EVENT } from "../shared";

let cmdHandlers = {};
let mutedPlayers = new Map();

function invokeCmd(player, cmd, args) {
  cmd = cmd.toLowerCase();
  const callback = cmdHandlers[cmd];

  if (callback) {
    callback(player, args);
  } else {
    send(player, `{FF0000} Unknown command /${cmd}`);
  }
}

alt.onClient(CHAT_MESSAGE_EVENT, (player, msg) => {
  if (msg[0] === "/") {
    msg = msg.trim().slice(1);

    if (msg.length > 0) {
      alt.log("[chat:cmd] " + player.name + ": /" + msg);

      let args = msg.split(" ");
      let cmd = args.shift();

      invokeCmd(player, cmd, args);
    }
  } else {
    if (mutedPlayers.has(player) && mutedPlayers[player]) {
      send(player, "{FF0000} You are currently muted.");
      return;
    }

    msg = msg.trim();

    if (msg.length > 0) {
      alt.log("[chat:msg] " + player.name + ": " + msg);

      alt.emitAllClients(CHAT_MESSAGE_EVENT, player.name, msg.replace(/</g, "&lt;").replace(/'/g, "&#39").replace(/"/g, "&#34"));
    }
  }
});

export function send(player, msg) {
  if (!player) {
    alt.logError("[chat.send] player parameter should not be null, use chat.broadcast instead.");
    return;
  }

  alt.emitClient(player, CHAT_MESSAGE_EVENT, null, msg);
}

export function broadcast(msg) {
  alt.emitAllClients(CHAT_MESSAGE_EVENT, null, msg);
}

export function registerCmd(cmd, callback) {
  cmd = cmd.toLowerCase();

  if (cmdHandlers[cmd] !== undefined) {
    alt.logError(`Failed to register command /${cmd}, already registered`);
  } else {
    cmdHandlers[cmd] = callback;
  }
}

export function mutePlayer(player, state) {
  mutedPlayers.set(player, state);
}

// Used in an onConnect function to add functions to the player entity for a separate resource.
export function setupPlayer(player) {
  player.sendMessage = (msg) => {
    send(player, msg);
  };

  player.mute = (state) => {
    mutePlayer(player, state);
  };
}

// Arbitrary events to call.
alt.on("sendChatMessage", (player, msg) => {
  alt.logWarning("Usage of chat events is deprecated use export functions instead");
  send(player, msg);
});

alt.on("broadcastMessage", (msg) => {
  alt.logWarning("Usage of chat events is deprecated use export functions instead");
  broadcast(msg);
});
