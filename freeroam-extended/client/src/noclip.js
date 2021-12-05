import * as native from 'natives';
import * as alt from 'alt-client';

import { DirectionVector } from "./helpers";

let tick = null;
let noclipCam = null;

export function toggleNoclip(state) {
    switch (state) {
        case false: {
            alt.clearEveryTick(tick);

            noclipCam = null;
            native.renderScriptCams(false, true, 500, true, false, 0);

            const position = native.getEntityCoords(alt.Player.local.scriptID, true);
            let [unk, ground] = native.getGroundZFor3dCoord(position.x, position.y, position.z, 0.0, false, false);
            native.setEntityCoordsNoOffset(alt.Player.local.scriptID, position.x, position.y, ground, false, false, false);

            break;
        }

        case true: {
            tick = alt.everyTick(handleTick);

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

function handleTick() {
    native.disableControlAction(0, 1, true);
    native.disableControlAction(0, 2, true);
    native.disableControlAction(0, 24, true);
    native.disableControlAction(0, 25, true);
    native.disableControlAction(0, 30, true);
    native.disableControlAction(0, 31, true);
    native.disableControlAction(0, 49, true);

    const pos = native.getCamCoord(noclipCam);
    const rot = native.getCamRot(noclipCam, 2);

    const dir = new DirectionVector(pos, rot);
    const fwd = dir.Forward(3.5);
    const sens = getSensitivity();

    native.setEntityCoords(alt.Player.local, fwd.x, fwd.y, fwd.z - 2.0, true, false, false, true);

    if (alt.gameControlsEnabled() === false)
        return;

    // 'W' and 'D'
    if (native.isDisabledControlPressed(0, 32) && native.isDisabledControlPressed(0, 30)) {
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
    else if (native.isDisabledControlPressed(0, 32) && native.isDisabledControlPressed(0, 34)) {
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
    else if (native.isDisabledControlPressed(0, 33) && native.isDisabledControlPressed(0, 30)) {
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
    else if (native.isDisabledControlPressed(0, 33) && native.isDisabledControlPressed(0, 34)) {
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

        if (native.isDisabledControlPressed(0, 32)) direction = dir.Forward(sens);
        if (native.isDisabledControlPressed(0, 33)) direction = dir.Forward(-sens);
        if (native.isDisabledControlPressed(0, 34)) direction = dir.Right(-sens);
        if (native.isDisabledControlPressed(0, 30)) direction = dir.Right(sens);

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
    if (native.isDisabledControlPressed(0, 21)) {
        // 'E' Key
        if (native.isDisabledControlPressed(0, 38)) {
            sens *= 5;
        }

        return sens *= 5;
    }

    // Left Ctrl
    if (native.isDisabledControlPressed(0, 36)) {
        return sens = 0.035;
    }

    return sens;
};