import * as alt from 'alt-server';
import { Events } from '../shared/events.js';

/** @type {{ [id: number]: number }} */
let kickPlayerIn = {};

/**
 * Handle Authentication
 * @param {alt.Player} player
 */
function handleAuthenticate(player) {
    Object.keys(kickPlayerIn).forEach((id) => {
        if (kickPlayerIn[id] > Date.now()) {
            return;
        }

        const somePlayer = alt.Player.all.find((x) => x.id === parseInt(id));
        if (!somePlayer) {
            delete kickPlayerIn[id];
            return;
        }

        somePlayer.kick('Failed to Login');
    });

    kickPlayerIn[player.id] = Date.now() + 60000 * 3;
    player.emitRaw(Events.toClient.authenticate);
}

/**
 * Finish Authentication
 *
 * @param {alt.Player} player
 * @param {string} bearerToken
 * @return {*} 
 */
async function handleFinishAuthenticate(player, bearerToken) {
    if (typeof bearerToken === 'undefined') {
        player.kick('Open Discord, and Rejoin the Server');
        return;
    }

    /** @type {Response} */
    const request = await fetch('https://discordapp.com/api/users/@me', {
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            Authorization: `Bearer ${bearerToken}`,
        },
    }).catch((err) => {
        console.log(err);
        return undefined;
    });

    if (!request || request.status !== 200) {
        player.kick('Open Discord, and Rejoin the Server');
        return;
    }

    /** @type {undefined | { id: string, username: string, discriminator: string, avatar: string, verified: boolean, email: string }} */
    const data = await request.json();
    if (!data) {
        player.kick('Failed to obtain discord name or discriminator.');
        return;
    }

    // Setup General Player Information
    const name = `${data.username}#${data.discriminator}`;
    player.setStreamSyncedMeta('authenticated', true);
    player.setStreamSyncedMeta('name', name);
    player.setStreamSyncedMeta('discord', data.id);
    console.log(`${name} has Authenticated!`);
}

alt.on('playerConnect', handleAuthenticate);
alt.onClient(Events.toServer.finishAuthenticate, handleFinishAuthenticate);



