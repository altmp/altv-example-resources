import * as native from "natives"
import * as alt from "alt-client"
import { DirectionVector } from "./helpers"

let tick: number | null = null
let noclipCam: number | null = null

let oldPlayerPos: alt.Vector3 | null = null

export function toggleNoclip(state: boolean): void {
  switch (state) {
    case false: {
      if (tick == null) return

      alt.clearEveryTick(tick)

      noclipCam = null
      native.renderScriptCams(false, true, 500, true, false, 0)

      const pos = alt.FocusData.focusOverridePos

      const [, ground] = native.getGroundZFor3dCoord(...pos.toArray(), 0.0, false, false)

      alt.emitServer("tp_to_coords", pos.x, pos.y, ground + 1.0)

      break
    }

    case true: {
      const gameplayCamPos = native.getGameplayCamCoord()
      const gameplayCamRot = native.getGameplayCamRot(2)

      noclipCam = native.createCamWithParams(
        "DEFAULT_SCRIPTED_CAMERA",
        gameplayCamPos.x, gameplayCamPos.y, gameplayCamPos.z,
        0.0, 0.0, gameplayCamRot.z, native.getGameplayCamFov(), false, 2,
      )

      tick = alt.everyTick(handleTick.bind(null, noclipCam))

      native.setCamActiveWithInterp(noclipCam, native.getRenderingCam(), 500, 0, 0)
      native.renderScriptCams(true, true, 500, true, false, 0)

      break
    }
  }
}

function handleTick(noclipCam: number) {
  native.disableControlAction(0, 1, true)
  native.disableControlAction(0, 2, true)
  native.disableControlAction(0, 24, true)
  native.disableControlAction(0, 25, true)
  native.disableControlAction(0, 30, true)
  native.disableControlAction(0, 31, true)
  native.disableControlAction(0, 49, true)

  const currentPlayerPos = alt.Player.local.pos
  oldPlayerPos ??= currentPlayerPos
  let pos: alt.Vector3

  if (oldPlayerPos.distanceToSquared(currentPlayerPos) > 10.0) {
    oldPlayerPos = currentPlayerPos
    pos = currentPlayerPos
    native.setCamCoord(noclipCam, ...pos.toArray())
  }
  else
    pos = native.getCamCoord(noclipCam)

  const rot = native.getCamRot(noclipCam, 2)

  const dir = new DirectionVector(pos, rot)
  const fwd = dir.forward(3.5)
  const sens = getSensitivity()

  alt.FocusData.overrideFocus(fwd)

  if (alt.gameControlsEnabled() === false)
    return

  // 'W' and 'D'
  if (native.isDisabledControlPressed(0, 32) && native.isDisabledControlPressed(0, 30)) {
    const forward = dir.forward(sens)
    const right = dir.right(sens)

    const finishedPos = {
      x: (forward.x + right.x) / 2,
      y: (forward.y + right.y) / 2,
      z: (forward.z + right.z) / 2,
    }

    native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z)
  }

  // 'W' and 'A'
  else if (native.isDisabledControlPressed(0, 32) && native.isDisabledControlPressed(0, 34)) {
    const forward = dir.forward(sens)
    const left = dir.right(-sens)

    const finishedPos = {
      x: (forward.x + left.x) / 2,
      y: (forward.y + left.y) / 2,
      z: (forward.z + left.z) / 2,
    }

    native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z)
  }

  // 'S' and 'D'
  else if (native.isDisabledControlPressed(0, 33) && native.isDisabledControlPressed(0, 30)) {
    const back = dir.forward(-sens)
    const right = dir.right(sens)

    const finishedPos = {
      x: (back.x + right.x) / 2,
      y: (back.y + right.y) / 2,
      z: (back.z + right.z) / 2,
    }

    native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z)
  }

  // 'S' and 'A'
  else if (native.isDisabledControlPressed(0, 33) && native.isDisabledControlPressed(0, 34)) {
    const back = dir.forward(-sens)
    const left = dir.right(-sens)

    const finishedPos = {
      x: (back.x + left.x) / 2,
      y: (back.y + left.y) / 2,
      z: (back.z + left.z) / 2,
    }

    native.setCamCoord(noclipCam, finishedPos.x, finishedPos.y, finishedPos.z)
  }

  else {
    let direction = null

    if (native.isDisabledControlPressed(0, 32)) direction = dir.forward(sens)
    if (native.isDisabledControlPressed(0, 33)) direction = dir.forward(-sens)
    if (native.isDisabledControlPressed(0, 34)) direction = dir.right(-sens)
    if (native.isDisabledControlPressed(0, 30)) direction = dir.right(sens)

    if (direction !== null)
      native.setCamCoord(noclipCam, direction.x, direction.y, direction.z)
  }

  processCameraRotation(noclipCam)
}

// Noclip functions
function processCameraRotation(noclipCam: number) {
  const camRot = native.getCamRot(noclipCam, 2)

  const mouseX = native.getDisabledControlNormal(1, 1)
  const mouseY = native.getDisabledControlNormal(1, 2)

  const mouseSens = native.getProfileSetting(13)

  const finalRot = {
    x: camRot.x - mouseY * mouseSens,
    y: camRot.y,
    z: camRot.z - mouseX * mouseSens,
  }

  if (finalRot.x >= 89) finalRot.x = 89
  if (finalRot.x <= -89) finalRot.x = -89

  native.setCamRot(noclipCam, finalRot.x, finalRot.y, finalRot.z, 2)
}

function getSensitivity() {
  let sens = 0.15

  // Left Shift
  if (native.isDisabledControlPressed(0, 21)) {
    // 'E' Key
    if (native.isDisabledControlPressed(0, 38))
      sens *= 5

    return sens * 5
  }

  // Left Ctrl
  if (native.isDisabledControlPressed(0, 36))
    return 0.035

  return sens
}
