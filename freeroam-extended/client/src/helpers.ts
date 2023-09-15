/* eslint-disable @typescript-eslint/explicit-member-accessibility */
import * as native from "natives"
import * as alt from "alt-client"
import { playerData } from "./playerdata"

export const LOCAL_PLAYER = alt.Player.local
export const EMPTY_WEAPON_HASH = 0xA2719263

export function displayAdvancedNotification(
  message: string,
  title = "Title",
  subtitle = "subtitle",
  notifImage: string | null = null,
  iconType = 0,
  backgroundColor: number | null = null,
  durationMult = 1,
): number {
  native.beginTextCommandThefeedPost("STRING")
  native.addTextComponentSubstringPlayerName(message)
  if (backgroundColor != null)
    native.thefeedSetBackgroundColorForNextPost(backgroundColor)
  if (notifImage != null)
    native.endTextCommandThefeedPostMessagetextTu(notifImage, notifImage, false, iconType, title, subtitle, durationMult)
  return native.endTextCommandThefeedPostTicker(false, true)
}

export function setWeaponsUsage(state: boolean): void {
  native.playSoundFrontend(-1, "SIGN_DESTROYED", "HUD_AWARDS", true)

  if (state) {
    playerData.areWeaponsDisabled = false

    native.setCanPedSelectAllWeapons(LOCAL_PLAYER, true)
    displayAdvancedNotification("Have fun.", "Weapons Usage", "Activated", "CHAR_AMMUNATION", 1, 203, 1.5)
  }
  else {
    native.giveWeaponToPed(LOCAL_PLAYER, EMPTY_WEAPON_HASH, 0, false, true)
    playerData.areWeaponsDisabled = true

    native.setCanPedSelectAllWeapons(LOCAL_PLAYER, false)
    displayAdvancedNotification(
      "You can only use weapons in the LS Airport zone.",
      "Weapons Usage",
      "Deactivated",
      "CHAR_AMMUNATION",
      1, 31, 1.5,
    )
  }
}

export class DirectionVector {
  constructor(
    private readonly position: alt.IVector3,
    private readonly rotation: alt.IVector3,
  ) {}

  eulerToQuaternion(rotation: alt.IVector3): alt.IVector3 & { w: number } {
    const roll = rotation.x * (Math.PI / 180.0)
    const pitch = rotation.y * (Math.PI / 180.0)
    const yaw = rotation.z * (Math.PI / 180.0)

    const qx = Math.sin(roll / 2) * Math.cos(pitch / 2) * Math.cos(yaw / 2) - Math.cos(roll / 2) * Math.sin(pitch / 2) * Math.sin(yaw / 2)
    const qy = Math.cos(roll / 2) * Math.sin(pitch / 2) * Math.cos(yaw / 2) + Math.sin(roll / 2) * Math.cos(pitch / 2) * Math.sin(yaw / 2)
    const qz = Math.cos(roll / 2) * Math.cos(pitch / 2) * Math.sin(yaw / 2) - Math.sin(roll / 2) * Math.sin(pitch / 2) * Math.cos(yaw / 2)
    const qw = Math.cos(roll / 2) * Math.cos(pitch / 2) * Math.cos(yaw / 2) + Math.sin(roll / 2) * Math.sin(pitch / 2) * Math.sin(yaw / 2)

    return { x: qx, y: qy, z: qz, w: qw }
  }

  forwardVector(): alt.Vector3 {
    const quatRot = this.eulerToQuaternion(this.rotation)

    const fVectorX = 2 * (quatRot.x * quatRot.y - quatRot.w * quatRot.z)
    const fVectorY = 1 - 2 * (quatRot.x * quatRot.x + quatRot.z * quatRot.z)
    const fVectorZ = 2 * (quatRot.y * quatRot.z + quatRot.w * quatRot.x)

    return new alt.Vector3(fVectorX, fVectorY, fVectorZ)
  }

  forward(distance: number): alt.Vector3 {
    const forwardVector = this.forwardVector()

    return new alt.Vector3(
      this.position.x + forwardVector.x * distance,
      this.position.y + forwardVector.y * distance,
      this.position.z + forwardVector.z * distance,
    )
  }

  rightVector(): alt.Vector3 {
    const quatRot = this.eulerToQuaternion(this.rotation)

    const rVectorX = 1 - 2 * (quatRot.y * quatRot.y + quatRot.z * quatRot.z)
    const rVectorY = 2 * (quatRot.x * quatRot.y + quatRot.w * quatRot.z)
    const rVectorZ = 2 * (quatRot.x * quatRot.z - quatRot.w * quatRot.y)

    return new alt.Vector3(rVectorX, rVectorY, rVectorZ)
  }

  right(distance: number): alt.Vector3 {
    const rightVector = this.rightVector()

    return new alt.Vector3(
      this.position.x + rightVector.x * distance,
      this.position.y + rightVector.y * distance,
      this.position.z + rightVector.z * distance,
    )
  }

  upVector(): alt.Vector3 {
    const quatRot = this.eulerToQuaternion(this.rotation)

    const uVectorX = 2 * (quatRot.x * quatRot.z + quatRot.w * quatRot.y)
    const uVectorY = 2 * (quatRot.y * quatRot.z - quatRot.w * quatRot.x)
    const uVectorZ = 1 - 2 * (quatRot.x * quatRot.x + quatRot.y * quatRot.y)

    return new alt.Vector3(uVectorX, uVectorY, uVectorZ)
  }

  up(distance: number): alt.Vector3 {
    const upVector = this.upVector()

    return new alt.Vector3(
      this.position.x + upVector.x * distance,
      this.position.y + upVector.y * distance,
      this.position.z + upVector.z * distance,
    )
  }
}

export function drawDMZone(
  centerX: number,
  centerY: number,
  radius: number,
  count: number,
): void {
  const steps = 2 * Math.PI / count
  for (let i = 0; i < count; i++) {
    const blipX = radius * Math.cos(steps * i) + centerX
    const blipY = radius * Math.sin(steps * i) + centerY

    const blip = new alt.PointBlip(blipX, blipY, 0)
    blip.sprite = 310
    blip.shortRange = true
    native.setBlipHiddenOnLegend(blip.scriptID, true)
  }
}

let adminMessageEveryTick: number | null = null

export function mhint(head: string, msg: string, time = 5): void {
  const scaleform = native.requestScaleformMovie("MIDSIZED_MESSAGE")
  alt.setTimeout(() => {
    if (adminMessageEveryTick != null) {
      alt.clearEveryTick(adminMessageEveryTick)
      adminMessageEveryTick = null
    }

    native.playSoundFrontend(
      -1,
      "SIGN_DESTROYED",
      "HUD_AWARDS",
      true,
    )

    native.beginScaleformMovieMethod(scaleform, "SHOW_MIDSIZED_MESSAGE")
    native.beginTextCommandScaleformString("STRING")
    native.scaleformMovieMethodAddParamPlayerNameString(head)
    native.scaleformMovieMethodAddParamTextureNameString(msg)
    native.scaleformMovieMethodAddParamInt(100)
    native.scaleformMovieMethodAddParamBool(true)
    native.scaleformMovieMethodAddParamInt(100)
    native.endScaleformMovieMethod()

    adminMessageEveryTick = alt.everyTick(() => {
      native.drawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0)
    })

    alt.setTimeout(() => {
      if (adminMessageEveryTick == null) return

      alt.clearEveryTick(adminMessageEveryTick)
      adminMessageEveryTick = null
    }, time * 1000)
  }, 1000)
}

export async function tpToWaypoint(): Promise<void> {
  const point = getWaypoint()
  if (!point) {
    alt.log("no waypoint to tp")
    return
  }

  alt.FocusData.overrideFocus(point)
  const startPos = new alt.Vector3(point.x, point.y, 1500)
  let destPos = startPos
  let groundPos: alt.Vector3 | null = null

  try {
    await alt.Utils.waitFor(() => {
      destPos = destPos.sub(0, 0, 200.0)

      alt.FocusData.overrideFocus(destPos)

      if (destPos.z < -500)
        throw new Error("failed to get ground pos")

      groundPos = raycast(startPos, destPos)?.pos ?? null
      if (!groundPos) return false

      return true
    }, 3000)
  }
  catch {}

  if (!groundPos) {
    alt.logWarning("failed to get ground pos for waypoint, trying getGroundZ native...")

    alt.FocusData.overrideFocus(point)

    let foundZ: number | null = null
    try {
      await alt.Utils.waitFor(() => {
        const [found, z] = native.getGroundZAndNormalFor3dCoord(point.x, point.y, 9999)
        if (!found) return false

        foundZ = z
        return true
      }, 3000)
    }
    catch {}

    if (foundZ == null) {
      alt.logError("failed to get ground z for waypoint")
      groundPos = startPos
    }
    else
      groundPos = new alt.Vector3(point.x, point.y, foundZ)
  }

  alt.FocusData.clearFocus()

  if (!groundPos)
    throw new Error("no groundPos")

  groundPos = groundPos.add(0, 0, 2.0)

  alt.emitServer("tp_to_waypoint", ...groundPos.toArray())
}

function getWaypoint(sprite = 8): alt.Vector3 | null {
  const waypoint = native.getFirstBlipInfoId(sprite)

  if (native.doesBlipExist(waypoint))
    return native.getBlipInfoIdCoord(waypoint)

  return null
}

export function raycast(
  start: alt.Vector3,
  dest: alt.Vector3,
  flags = 99999,
): { pos: alt.Vector3; entity: number } | null {
  const ray = native.startExpensiveSynchronousShapeTestLosProbe(
    start.x,
    start.y,
    start.z,
    dest.x,
    dest.y,
    dest.z,
    flags,
    LOCAL_PLAYER,
    0,
  )

  const [, hit, pos,, entity] = native.getShapeTestResult(ray)
  return hit
    ? { pos, entity }
    : null
}

export function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(min, value), max)
}
