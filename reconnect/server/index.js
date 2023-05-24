import * as alt from 'alt-server';

if (alt.debug) {
    const RETRY_DELAY = 2500;
    const DEBUG_PORT = 9223;

    async function getLocalClientStatus() {
        try {
            const response = await fetch(`http://127.0.0.1:${DEBUG_PORT}/status`);
            return response.text();
        } catch (error) {
            return null;
        }
    }

    async function connectLocalClient() {
        const status = await getLocalClientStatus();
        if (status === null) return;

        if (status !== "MAIN_MENU" && status !== "IN_GAME") {
            setTimeout(() => connectLocalClient(), RETRY_DELAY);
            return;
        }

        try {
            await fetch(`http://127.0.0.1:${DEBUG_PORT}/reconnect`, {
                method: "POST",
                body: alt.getServerConfig().password ? alt.getServerConfig().password : "", // only needed when a password is set in the server.toml
            });
        } catch (error) {
            console.log(error);
        }
    }

    connectLocalClient();
}
