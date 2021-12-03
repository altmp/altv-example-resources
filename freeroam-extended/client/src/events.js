import { playerData, setWeaponsUsage } from './helpers';

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

alt.on('keyup', key => {
    if (key === 113) { // F2
        playerData.areNametagsVisible = !playerData.areNametagsVisible;
    }
});