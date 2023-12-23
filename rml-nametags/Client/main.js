import * as alt from "alt-client";
import * as native from "natives";

// ---------------- Config ----------------

const showVehicleIds = true;
const showPlayerIds = true;
const showPlayerNames = true;
const checkLoS = true;
const dynamicSize = true;
const controlKey = 79;

// --------------- Prototype --------------

alt.RmlElement.prototype.shown = false;

// ---------------- Script ----------------

alt.loadRmlFont("/Client/arialbd.ttf", "arial", false, true);
const document = new alt.RmlDocument("/Client/index.rml");
const container = document.getElementByID("nametag-container");
const nameTags = new Map();
let tickHandle = undefined;

alt.on("gameEntityCreate", (entity) => {
    const rmlElement = document.createElement("button");
    rmlElement.entityType = entity.type;
    rmlElement.entityID = entity.remoteID;
    rmlElement.addClass("nametag");
    rmlElement.addClass("hide");

    if (entity instanceof alt.Player) {
        if (showPlayerIds && !showPlayerNames)
            rmlElement.innerRML = `ID: ${entity.remoteID}`;
        else if (showPlayerIds && showPlayerNames)
            rmlElement.innerRML = `ID: ${entity.remoteID} | Name: ${entity.name}`;
        else if (!showPlayerIds && showPlayerNames)
            rmlElement.innerRML = `Name: ${entity.name}`;
        else {
            rmlElement.destroy();
            return;
        }
    } else if (entity instanceof alt.Vehicle && showVehicleIds)
        rmlElement.innerRML = `ID: ${entity.remoteID}`;
    else {
        rmlElement.destroy();
        return;
    }

    nameTags.set(entity, rmlElement);
    container.appendChild(rmlElement);
    rmlElement.on("click", printCoordinates);

    if (tickHandle !== undefined) return;
    tickHandle = alt.everyTick(drawMarkers);
});

alt.on("gameEntityDestroy", (entity) => {
    const rmlElement = nameTags.get(entity);
    if (rmlElement === undefined) return;
    container.removeChild(rmlElement);
    rmlElement.destroy();
    nameTags.delete(entity);

    if (tickHandle === undefined || nameTags.size > 0) return;
    alt.clearEveryTick(tickHandle);
    tickHandle = undefined;
});

alt.on("keyup", (key) => {
    if (key !== controlKey) return;

    const currentState = alt.rmlControlsEnabled();
    if (currentState) {
        alt.toggleGameControls(true);
        alt.showCursor(false);
        alt.toggleRmlControls(false);
    } else {
        alt.toggleGameControls(false);
        alt.showCursor(true);
        alt.toggleRmlControls(true);
    }
});

function printCoordinates(rmlElement, eventArgs) {
    const entity = alt.BaseObject.getByID(rmlElement.entityType, rmlElement.entityID);
    alt.log("Entity Position", "X", entity.pos.x, "Y", entity.pos.y, "Z", entity.pos.z);
}

function drawMarkers() {
    nameTags.forEach((rmlElement, entity) => {
        const {x, y, z} = entity.pos;

        if (!native.isSphereVisible(x, y, z, 0.0099999998) || (checkLoS && !native.hasEntityClearLosToEntity(alt.Player.local, entity, 17))) {
            if (!rmlElement.shown) return;

            rmlElement.addClass("hide");
            rmlElement.shown = false;
        } else {
            if (!rmlElement.shown) {
                rmlElement.removeClass("hide");
                rmlElement.shown = true;
            }

            const {x: screenX, y: screenY} = alt.worldToScreen(x, y, z + 2);
            rmlElement.style["left"] = `${screenX}px`;
            rmlElement.style["top"] = `${screenY}px`;

            if (!dynamicSize) return;
            const fontSizeModificator = Math.min(entity.pos.distanceTo(alt.Player.local.pos) / 100, 1);
            const fontSize = (1 - fontSizeModificator) * 50;
            rmlElement.style["font-size"] = `${fontSize}dp`;
        }
    });
}
