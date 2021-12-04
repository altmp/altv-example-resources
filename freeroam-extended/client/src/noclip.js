import * as native from 'natives';
import * as alt from 'alt-client';

import { DirectionVector } from "./helpers";

let tick = null;
let tickId = null;
let noclipCam = null;

export function toggleNoclip(state) {
    switch (state) {
        case false: {
            alt.clearEveryTick(tickId);

            noclipCam = null;
            native.renderScriptCams(false, true, 500, true, false, 0);

            break;
        }

        case true: {
            tickId = alt.everyTick(handleTick);

            const gameplayCamPos = native.getGameplayCamCoord();
            const gameplayCamRot = native.getGameplayCamRot(2);
            
            noclipCam = native.createCamWithParams(
                "DEFAULT_SCRIPTED_CAMERA", 
                gameplayCamPos.x, gameplayCamPos.y, gameplayCamPos.z,
                0.0, 0.0, gameplayCamRot.z, native.getGameplayCamFov(), false, 2, 
            );
            
            native.setCamActiveWithInterp(noclipCam, native.getRenderingCam(), 500, 0, 0);
            native.renderScriptCams(true, true, 500, true, false, 0);

            break;
        }
    }
};

// We do this so that we can freeze the player with disableAllControlActions &
// be able to access all other keys, like 'ESC' & etc..
const keys = [];

alt.on("keydown", (key) => { keys.push(key) });
alt.on("keyup", (key) => { keys.splice(keys.indexOf(key), 1) });

function handleTick() {
    // native.disableAllControlActions(0);
    // native.disableAllControlActions(1);
    // native.disableAllControlActions(2);

    native.disableControlAction(0, 1, true);
    native.disableControlAction(0, 2, true);
    native.disableControlAction(0, 24, true);
    native.disableControlAction(0, 25, true);
    native.disableControlAction(0, 30, true);
    native.disableControlAction(0, 31, true);
    native.disableControlAction(0, 49, true);

    if (alt.isConsoleOpen())
        return;

    const pos = native.getCamCoord(noclipCam);
    const rot = native.getCamRot(noclipCam, 2);

    const dir = new DirectionVector(pos, rot);
    const fwd = dir.Forward(3.5);
    const sens = getSensitivity();

    native.setEntityCoords(alt.Player.local, fwd.x, fwd.y, fwd.z - 2.0, true, false, false, true);

    // 'W' and 'D'
    if (keys.includes(87) && keys.includes(68)) {
        const forward = dir.Forward(sens);
        const right = dir.Right(sens);

        const finishedPos = {
            x: (forward.x + right.x) / 2,
            y: (forward.y + right.y) / 2,
            z: (forward.z + right.z) / 2
        };

        native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z);
    }
    
    // 'W' and 'A'
    else if (keys.includes(87) && keys.includes(65)) {
        const forward = dir.Forward(sens);
        const left = dir.Right(-sens);

        const finishedPos = {
            x: (forward.x + left.x) / 2,
            y: (forward.y + left.y) / 2,
            z: (forward.z + left.z) / 2
        };

        native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z);
    }

    // 'S' and 'D'
    else if (keys.includes(83) && keys.includes(68)) {
        const back = dir.Forward(-sens);
        const right = dir.Right(sens);

        const finishedPos = {
            x: (back.x + right.x) / 2,
            y: (back.y + right.y) / 2,
            z: (back.z + right.z) / 2
        };

        native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z);
    }

    // 'S' and 'A'
    else if (keys.includes(83) && keys.includes(65)) {
        const back = dir.Forward(-sens);
        const left = dir.Right(-sens);

        const finishedPos = {
            x: (back.x + left.x) / 2,
            y: (back.y + left.y) / 2,
            z: (back.z + left.z) / 2
        };

        native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z);
    }

    else {
        let direction = null;

        if (keys.includes(87)) direction = dir.Forward(sens);
        if (keys.includes(83)) direction = dir.Forward(-sens);
        if (keys.includes(65)) direction = dir.Right(-sens);
        if (keys.includes(68)) direction = dir.Right(sens);

        if (direction !== null) {
            native.setCamCoord(noclipCam, direction.x, direction.y, direction.z);
        }
    }

    processCameraRotation();
};

// Noclip functions
function processCameraRotation() {
    const camRot = native.getCamRot(noclipCam, 2);

    const mouseX = native.getDisabledControlNormal(1, 1);
    const mouseY = native.getDisabledControlNormal(1, 2);
    
    const mouseSens = native.getProfileSetting(13);

    let finalRot = {
        x: camRot.x - mouseY * mouseSens,
        y: camRot.y,
        z: camRot.z - mouseX * mouseSens
    };

    if (finalRot.x >= 89) finalRot.x = 89;
    if (finalRot.x <= -89) finalRot.x = -89;

    native.setCamRot(noclipCam, finalRot.x, finalRot.y, finalRot.z, 2);
};

function getSensitivity() {
    let sens = 0.15;

    // Left Shift
    if (keys.includes(16)) {
        // 'E' Key
        if (keys.includes(69)) {
            sens *= 5;
        }

        return sens *= 5;
    }

    // Left Ctrl
    if (keys.includes(17)) {
        return sens = 0.035;
    }

    return sens;
};