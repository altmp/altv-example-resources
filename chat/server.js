import alt from 'alt-server';

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

alt.onClient('chat:message', (player, msg) => {
    if (msg[0] === '/') {
        msg = msg.trim().slice(1);

        if (msg.length > 0) {
            alt.log('[chat:cmd] ' + player.name + ': /' + msg);

            let args = msg.split(' ');
            let cmd = args.shift();

            invokeCmd(player, cmd, args);
        }
    } else {
        if (mutedPlayers.has(player) && mutedPlayers[player]) {
            send(player, '{FF0000} You are currently muted.');
            return;
        }

        msg = msg.trim();

        if (msg.length > 0) {
            alt.log('[chat:msg] ' + player.name + ': ' + msg);

            alt.emitClient(null, 'chat:message', player.name, msg.replace(/</g, '&lt;').replace(/'/g, '&#39').replace(/"/g, '&#34'));
        }
    }
});

export function send(player, msg) {
    alt.emitClient(player, 'chat:message', null, msg);
}

export function broadcast(msg) {
    send(null, msg);
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

// Used in an onConnect function to add functions to the player entity for a seperate resource.
export function setupPlayer(player) {
    player.sendMessage = (msg) => {
        send(player, msg);
    }

    player.mute = (state) => {
        mutePlayer(player, state);
    }
}

// Arbitrary events to call.
alt.on('sendChatMessage', (player, msg) => {
    alt.logWarning('Usage of chat events is deprecated use export functions instead');
    send(player, msg);
});

alt.on('broadcastMessage', (msg) => {
    alt.logWarning('Usage of chat events is deprecated use export functions instead');
    send(null, msg);
});
