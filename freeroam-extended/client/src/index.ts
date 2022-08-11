import { LOCAL_PLAYER } from "./helpers"
import * as native from "natives"
import * as alt from "alt-client"
import "./events"
import "./chat"
import "./noclip"
import { playerData } from "./playerdata"
import { view } from "./view"
import { playerNametags } from "./nametags"
import { ConfigFlag, StatName, WatermarkPosition } from "altv-enums"
import { ATTACK_CONTROLS } from "./const"

alt.setConfigFlag(ConfigFlag.DisableAutoWeaponSwap, true)
alt.setConfigFlag(ConfigFlag.DisableIdleCamera, true)
alt.setStat(StatName.Stamina, 100)
alt.setWatermarkPosition(WatermarkPosition.TopCenter)

view.emit("setPlayerId", LOCAL_PLAYER.id)

setInterval(() => {
  if (!playerData.areNametagsVisible) return
  view.emit("updatePlayersOnline", alt.Player.all.length)
}, 1000)

playerData.onAreNametagsVisibleChange = (value) => {
  playerNametags.enable(value)
}

playerData.onAreWeaponsDisabledChange = (value) => {
  view.emit("setWeaponsDisabled", value)
}

alt.everyTick(() => {
  if (playerData.areWeaponsDisabled) {
    native.setCanPedEquipAllWeapons(LOCAL_PLAYER, false)
    native.disablePlayerFiring(LOCAL_PLAYER, true)

    for (const control of ATTACK_CONTROLS)
      native.disableControlAction(0, control, true)
  }
  else
    native.setCanPedEquipAllWeapons(LOCAL_PLAYER, true)
})
