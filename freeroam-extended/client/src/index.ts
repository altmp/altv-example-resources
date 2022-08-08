import { LOCAL_PLAYER, drawText3d, drawText2D } from "./helpers"

import * as native from "natives"
import * as alt from "alt-client"

import "./events"
import "./chat"
import "./noclip"
import { playerData } from "./playerdata"
import { view } from "./view"

alt.setConfigFlag("DISABLE_AUTO_WEAPON_SWAP", true)
alt.setConfigFlag("DISABLE_IDLE_CAMERA", true)
alt.setStat("stamina", 100)
alt.setWatermarkPosition(3) // top center

setInterval(() => {
  if (!playerData.areNametagsVisible) return
  view.emit("updatePlayersOnline", alt.Player.all.length)
}, 1000)

playerData.onAreNametagsVisibleChange = (value) => {
  if (!value) view.emit("updatePlayersOnline", null)
}

alt.everyTick(() => {
  if (playerData.areNametagsVisible)
    processNametags()

  if (playerData.areWeaponsDisabled) {
    native.setCanPedEquipAllWeapons(LOCAL_PLAYER, false)
    native.disablePlayerFiring(LOCAL_PLAYER, true)
  }
  else
    native.setCanPedEquipAllWeapons(LOCAL_PLAYER, true)
})

function processNametags() {
  renderNametags(LOCAL_PLAYER)

  const streamedIn = alt.Player.streamedIn
  for (let i = 0, len = streamedIn.length; i < len; i++) {
    const player = streamedIn[i]
    if (player?.valid && LOCAL_PLAYER.pos.distanceTo(player.pos) <= 25)
      renderNametags(player)
  }
}

function renderNametags(player: alt.Player) {
  native.requestPedVisibilityTracking(player)
  if (!native.isTrackedPedVisible(player) && alt.Player.local.vehicle === null) return

  const distance = alt.Player.local.pos.distanceTo(player.pos)
  const pos = native.getPedBoneCoords(player.scriptID, 31086, 0.0, 0.0, 0.0)
  const scale = 0.35 - distance * 0.01

  let nametagText = `~n~~w~<font color='#FFFFFF'>${player.name} #${player.id} | ~g~${player.health - 100} / 100</font>`

  if (player === LOCAL_PLAYER && playerData.areWeaponsDisabled)
    nametagText = "~o~Weapons Disabled" + nametagText

  drawText3d(
    nametagText,
    pos.x, pos.y, (pos.z + -(scale)) + 1.0,
    scale, 255, 255, 255, 255, true, 0.038 * (-scale), true, player,
  )
}
