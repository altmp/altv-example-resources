import { LOCAL_PLAYER } from "./helpers"

import * as native from "natives"
import * as alt from "alt-client"

import "./events"
import "./chat"
import "./noclip"
import { playerData } from "./playerdata"
import { view } from "./view"
import { playerNametags } from "./nametags"

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
  playerNametags.enable(value)
}

alt.everyTick(() => {
  if (playerData.areWeaponsDisabled) {
    native.setCanPedEquipAllWeapons(LOCAL_PLAYER, false)
    native.disablePlayerFiring(LOCAL_PLAYER, true)
  }
  else
    native.setCanPedEquipAllWeapons(LOCAL_PLAYER, true)
})
