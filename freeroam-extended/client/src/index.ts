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
alt.setConfigFlag(ConfigFlag.DisableSPEnterVehicleClipset, false)
alt.setConfigFlag(ConfigFlag.DisablePedPropKnockOff, true)
alt.setConfigFlag(ConfigFlag.ForceRenderSnow, true)
alt.setStat(StatName.Stamina, 100)
alt.setWatermarkPosition(WatermarkPosition.TopCenter)
alt.loadDefaultIpls()
alt.setMsPerGameMinute(60000)

const weatherConfig = { weathers: [1, 1, 4, 4, 2, 4, 2, 1, 0, 3, 4, 1, 1, 0, 1, 0, 4, 1, 4, 4, 2, 0, 3, 4, 2, 5, 8, 4, 0, 2, 1, 4, 4, 1, 1, 1, 0, 0, 3, 1, 0, 1, 1, 1, 4, 0, 0, 0, 1, 0, 1, 0, 3, 4, 1, 2, 2, 2, 1, 2, 4, 1, 4, 2, 5, 8, 4, 2, 4, 0, 1, 2, 2, 0, 3, 1, 1, 4, 2, 4, 4, 0, 1, 2, 1, 1, 2, 4, 1, 1, 4, 1, 0, 4, 0, 4, 4, 0, 1, 1, 2, 1, 2, 4, 0, 3, 1, 1, 2, 4, 0, 1, 1, 0, 2, 4, 0, 4, 0, 3, 0, 4, 2, 5, 8, 4, 4, 1, 1, 2, 2, 2, 0, 3, 0, 4, 1, 2, 1, 1, 2, 4, 0, 0, 0, 4, 2, 4, 0, 1, 2, 4, 1, 1, 2, 5, 8, 6, 7, 8, 4, 0, 2, 4, 0, 3, 1, 4, 2, 1, 0, 1, 1, 1, 2, 2, 2, 0, 0, 4, 4, 2, 1, 0, 3, 2, 4, 4, 1, 2, 1, 4, 0, 0, 1, 1, 2, 5, 8, 6, 7, 8, 4, 1, 0, 3, 0, 0, 4, 0, 2, 0, 0, 0, 4, 2, 1, 1, 2, 0, 0, 4, 1, 2, 4, 1, 0, 3, 2, 1, 1, 0, 0, 1, 4, 0, 4, 1, 0, 4, 2, 2, 5, 8, 4, 0, 0, 4, 1, 1, 2, 0, 2, 0, 4], multipliers: [2, 4, 2, 3, 4, 2, 4, 4, 3, 4, 3, 3, 3, 2, 3, 3, 3, 4, 2, 2, 4, 3, 2, 4, 3, 3, 4, 4, 4, 3, 4, 4, 4, 2, 4, 3, 2, 2, 2, 2, 2, 3, 4, 4, 2, 4, 4, 2, 4, 4, 2, 4, 3, 2, 2, 4, 2, 3, 4, 2, 3, 4, 3, 3, 4, 2, 2, 4, 3, 3, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 3, 2, 3, 2, 2, 2, 4, 4, 3, 4, 2, 3, 4, 3, 4, 3, 2, 3, 2, 3, 4, 3, 2, 2, 3, 4, 3, 3, 3, 4, 2, 3, 2, 2, 3, 4, 2, 2, 3, 3, 2, 4, 3, 2, 2, 3, 4, 2, 2, 4, 3, 4, 2, 2, 3, 3, 4, 4, 3, 3, 2, 4, 4, 3, 4, 3, 3, 4, 3, 2, 3, 2, 3, 3, 3, 3, 3, 2, 2, 3, 2, 4, 2, 2, 3, 4, 2, 4, 4, 2, 2, 4, 3, 4, 4, 3, 3, 4, 4, 2, 3, 2, 3, 4, 2, 4, 2, 3, 3, 4, 4, 4, 3, 4, 4, 2, 2, 2, 3, 4, 2, 2, 4, 4, 3, 4, 2, 2, 4, 4, 4, 2, 3, 3, 4, 3, 3, 3, 4, 2, 2, 4, 4, 3, 4, 4, 3, 2, 3, 3, 2, 4, 4, 3, 4, 3, 4, 2, 3, 3, 2, 2, 3, 3, 4, 3, 2, 2, 2, 3, 2, 2, 2, 4, 4] }

alt.setWeatherCycle(weatherConfig.weathers, weatherConfig.multipliers)
alt.setWeatherSyncActive(true)

view.emit("setPlayerId", LOCAL_PLAYER.remoteID)

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
    native.setCanPedSelectAllWeapons(LOCAL_PLAYER, false)
    native.disablePlayerFiring(LOCAL_PLAYER, true)

    for (const control of ATTACK_CONTROLS)
      native.disableControlAction(0, control, true)
  }
  else
    native.setCanPedSelectAllWeapons(LOCAL_PLAYER, true)
})

alt.setInterval(() => {
  const players = alt.Player.streamedIn.length + 1 // local player always in the stream
  const vehicles = alt.Vehicle.streamedIn.length

  view.emit("setStreamedEntities", players, vehicles)
}, 500)
