import * as alt from 'alt-client';
import * as native from 'natives';
import { Events } from '../shared/events.js';

// Discord APP ID is a public facing identifier for your application.
// Change this value at any time by making a new Discord Application.
const DISCORD_APP_ID = '1090747667317010532';
/** @type {number} */
let interval

async function handleAuthentication() {
    /** @type {string} */
    let bearerToken;

    alt.addGxtText('warning_error', 'Login with Discord');
    alt.addGxtText('warning_text', ' Tab Out, and check your Discord Application');
    alt.addGxtText('warning_text2', 'Declining will immediately kick you.');

    interval = alt.setInterval(() => {
        native.setWarningMessageWithHeader(
            'warning_error',
            'warning_text',
            0,
            'warning_text2',
            false,
            -1,
            null,
            null,
            true,
            Number(0)
        );
    }, 0);

    try {
        bearerToken = await alt.Discord.requestOAuth2Token(DISCORD_APP_ID);
    } catch (e) { }

    alt.clearInterval(interval);
    alt.emitServerRaw(Events.toServer.finishAuthenticate, bearerToken);
}

alt.onServer(Events.toClient.authenticate, handleAuthentication);
