import * as native from 'natives';
import * as alt from 'alt-client';

export const LOCAL_PLAYER = alt.Player.local;
export const EMPTY_WEAPON_HASH = 0xA2719263;

export const playerData = {
    areWeaponsDisabled: true,
    areNametagsVisible: true,
    lastCommandTimestamp: Date.now() - 10000,
    lastMessageTimestamp: Date.now() - 10000,
    chatState: false
}

export function distance(vector1, vector2) {
    return Math.sqrt(
      Math.pow(vector1.x - vector2.x, 2) +
        Math.pow(vector1.y - vector2.y, 2) +
        Math.pow(vector1.z - vector2.z, 2)
    );
}

export function displayAdvancedNotification(message, title = "Title", subtitle = "subtitle", notifImage = null, iconType = 0, backgroundColor = null, durationMult = 1) {
    native.beginTextCommandThefeedPost('STRING')
    native.addTextComponentSubstringPlayerName(message)
    if (backgroundColor != null) native.thefeedSetNextPostBackgroundColor(backgroundColor)
    if (notifImage != null) native.endTextCommandThefeedPostMessagetextTu(notifImage, notifImage, false, iconType, title, subtitle, durationMult)
    return native.endTextCommandThefeedPostTicker(false, true)
}

export function setWeaponsUsage(state) {
    native.playSoundFrontend(-1, 'SIGN_DESTROYED', 'HUD_AWARDS', 1);

    if (state) {
        playerData.areWeaponsDisabled = false;

        native.setCanPedEquipAllWeapons(LOCAL_PLAYER, true);
        displayAdvancedNotification('Have fun.', 'Weapons Usage', 'Activated', 'CHAR_AMMUNATION', 1, 203, 1.5);
    }
    else {
        native.giveWeaponToPed(LOCAL_PLAYER, EMPTY_WEAPON_HASH, 0, false, true);
        playerData.areWeaponsDisabled = true;
    
        native.setCanPedEquipAllWeapons(LOCAL_PLAYER, false);
        displayAdvancedNotification('You can only use weapons in the LS Airport zone.', 'Weapons Usage', 'Deactivated', 'CHAR_AMMUNATION', 1, 31, 1.5);
    }
}

export function drawText3d(text, x, y, z, scale, r, g, b, a, outline, offset, lagcomp, lagcompEntity) {
    // If lagcomp is enabled and the lagcomp entity is in a vehicle.
    if (lagcomp === true && lagcompEntity.vehicle !== null) {
        const vector = native.getEntityVelocity(lagcompEntity.vehicle);
        const frameTime = native.getFrameTime();

        native.setDrawOrigin(x + (vector.x * frameTime), y + (vector.y * frameTime), z + (vector.z * frameTime), 0);
    } else native.setDrawOrigin(x, y, z, 0);
    
    native.setTextFont(4);
    native.setTextProportional(false);
    native.setTextScale(scale, scale);
    native.setTextColour(r, g, b, a);
    native.setTextDropshadow(0, 0, 0, 0, 255);
    native.setTextEdge(2, 0, 0, 0, 150);
    native.setTextDropShadow();
    native.setTextCentre(true);

    if (outline) native.setTextOutline();

    native.beginTextCommandDisplayText("CELL_EMAIL_BCON");

    text.match(/.{1,99}/g).forEach(textBlock => {
        native.addTextComponentSubstringPlayerName(textBlock);
    });

    native.endTextCommandDisplayText(0.0, offset, 0.0);
    native.clearDrawOrigin();
};

export class DirectionVector {
	position;
	rotation;

	constructor(position, rotation) {
        this.position = position;
        this.rotation = rotation;
	}

	euler_to_quaternion(rotation) {
		const roll = rotation.x * (Math.PI / 180.0);
		const pitch = rotation.y * (Math.PI / 180.0);
		const yaw = rotation.z * (Math.PI / 180.0);

		const qx = Math.sin(roll / 2) * Math.cos(pitch / 2) * Math.cos(yaw / 2) - Math.cos(roll / 2) * Math.sin(pitch / 2) * Math.sin(yaw / 2)
		const qy = Math.cos(roll / 2) * Math.sin(pitch / 2) * Math.cos(yaw / 2) + Math.sin(roll / 2) * Math.cos(pitch / 2) * Math.sin(yaw / 2)
		const qz = Math.cos(roll / 2) * Math.cos(pitch / 2) * Math.sin(yaw / 2) - Math.sin(roll / 2) * Math.sin(pitch / 2) * Math.cos(yaw / 2)
        const qw = Math.cos(roll / 2) * Math.cos(pitch / 2) * Math.cos(yaw / 2) + Math.sin(roll / 2) * Math.sin(pitch / 2) * Math.sin(yaw / 2)
        
		return { x: qx, y: qy, z: qz, w: qw };
	}

	ForwardVector() {
        const quatRot = this.euler_to_quaternion(this.rotation);
        
		const fVectorX = 2 * (quatRot.x * quatRot.y - quatRot.w * quatRot.z);
		const fVectorY = 1 - 2 * (quatRot.x * quatRot.x + quatRot.z * quatRot.z);
		const fVectorZ = 2 * (quatRot.y * quatRot.z + quatRot.w * quatRot.x);

		return new alt.Vector3(fVectorX, fVectorY, fVectorZ);
	}

	Forward(distance) {
        const forwardVector = this.ForwardVector();
        
		return new alt.Vector3(
			this.position.x + forwardVector.x * distance,
			this.position.y + forwardVector.y * distance,
            this.position.z + forwardVector.z * distance
        );
	}

	RightVector() {
        const quatRot = this.euler_to_quaternion(this.rotation);
        
		const rVectorX = 1 - 2 * (quatRot.y * quatRot.y + quatRot.z * quatRot.z);
		const rVectorY = 2 * (quatRot.x * quatRot.y + quatRot.w * quatRot.z);
		const rVectorZ = 2 * (quatRot.x * quatRot.z - quatRot.w * quatRot.y);

		return new alt.Vector3(rVectorX, rVectorY, rVectorZ);
	}

	Right(distance) {
        const rightVector = this.RightVector();
        
		return new alt.Vector3(
			this.position.x + rightVector.x * distance,
			this.position.y + rightVector.y * distance,
            this.position.z + rightVector.z * distance
        );
	}

	UpVector() {
        const quatRot = this.euler_to_quaternion(this.rotation);
        
		const uVectorX = 2 * (quatRot.x * quatRot.z + quatRot.w * quatRot.y);
		const uVectorY = 2 * (quatRot.y * quatRot.z - quatRot.w * quatRot.x);
		const uVectorZ = 1 - 2 * (quatRot.x * quatRot.x + quatRot.y * quatRot.y);

		return new alt.Vector3(uVectorX, uVectorY, uVectorZ);
	}

	Up(distance) {
        const upVector = this.UpVector();
        
		return new alt.Vector3(
			this.position.x + upVector.x * distance,
			this.position.y + upVector.y * distance,
            this.position.z + upVector.z * distance
        );
	}
}

export function drawDMZone(center_x, center_y, radius, count)
{
    let steps = 2 * Math.PI / count;
    for(let i = 0; i < count; i++)
    {
        let blip_x = radius * Math.cos(steps * i) + center_x;
        let blip_y = radius * Math.sin(steps * i) + center_y;

        let blip = native.addBlipForCoord(blip_x, blip_y, 0);
        native.setBlipSprite(blip, 310);
        native.setBlipHiddenOnLegend(blip, true);
        native.setBlipAsShortRange(blip, true);
    }
}

export function drawText2D(
    text,
    pos,
    scale,
    color,
    alignment = 0,
    padding = 0
) {
    if (scale > 2) {
        scale = 2;
    }

    native.beginTextCommandDisplayText('STRING');
    native.addTextComponentSubstringPlayerName(text);
    native.setTextFont(4);
    native.setTextScale(1, scale);
    native.setTextColour(color.r, color.g, color.b, color.a);
    native.setTextOutline();
    native.setTextDropShadow();
    if (alignment !== null) {
        native.setTextWrap(padding, 1 - padding);
        native.setTextJustification(alignment);
    }

    native.endTextCommandDisplayText(pos.x, pos.y, 0);
}

let adminMessageEveryTick = null;

export function mhint(head, msg, time = 5000) {
    let scaleform = native.requestScaleformMovie("MIDSIZED_MESSAGE");
    alt.setTimeout(() => {
        if (adminMessageEveryTick != null) {
            alt.clearEveryTick(adminMessageEveryTick);
            adminMessageEveryTick = null;
        }

        native.playSoundFrontend(-1, 'SIGN_DESTROYED', 'HUD_AWARDS', 1);

        native.beginScaleformMovieMethod(scaleform, "SHOW_MIDSIZED_MESSAGE");
        native.beginTextCommandScaleformString("STRING");
        native.scaleformMovieMethodAddParamPlayerNameString(head);
        native.scaleformMovieMethodAddParamTextureNameString(msg);
        native.scaleformMovieMethodAddParamInt(100);
        native.scaleformMovieMethodAddParamBool(true);
        native.scaleformMovieMethodAddParamInt(100);
        native.endScaleformMovieMethod();
        
        adminMessageEveryTick = alt.everyTick(() => {
            native.drawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);
        });

        alt.setTimeout(() => {
            alt.clearEveryTick(adminMessageEveryTick);
            adminMessageEveryTick = null;
        }, time * 1000);
    }, 1000);
}