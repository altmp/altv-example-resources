import * as native from "natives"
import * as alt from "alt-client"
import { drawDMZone, setWeaponsUsage, mhint, tpToWaypoint, LOCAL_PLAYER } from "./helpers"
import * as chat from "./chat"
import { toggleNoclip } from "./noclip"
import { playerData } from "./playerdata"
import { view } from "./view"
import { KeyCode, Permission, PermissionState } from "altv-enums"

alt.on("connectionComplete", () => {
  setTimeout(() => {
    // We assume that we are not in the airport if areWeaponsDisabled is on true when it triggers
    if (playerData.areWeaponsDisabled)
      setWeaponsUsage(false)
  }, 1000)
})

alt.onServer("airport_state", setWeaponsUsage)

alt.onServer("chat:message", chat.pushMessage)

alt.onServer("noclip", toggleNoclip)

alt.onServer("set_chat_state", (state: boolean) => {
  playerData.chatState = state
})

alt.onServer("draw_dmzone", (
  centerX: number,
  centerY: number,
  radius: number,
  count: number,
) => {
  drawDMZone(centerX, centerY, radius, count)
})

alt.onServer("announce", (header: string, body: string, time: number) => {
  mhint(header, body, time)
})

alt.on("keyup", (key) => {
  if (!chat.chatData.loaded) return

  switch (key) {
    case KeyCode.F2: {
      playerData.areNametagsVisible = !playerData.areNametagsVisible
      native.displayRadar(playerData.areNametagsVisible)
      native.displayHud(playerData.areNametagsVisible)
      chat.toggleChat()
      break
    }

    case KeyCode.Enter:
    case KeyCode.T: {
      if (alt.isKeyDown(KeyCode.Ctrl)) {
        tpToWaypoint().catch(e => {
          alt.logError(e?.stack ?? e)
        })
        break
      }

      if (!chat.chatData.opened && alt.gameControlsEnabled()) {
        chat.chatData.opened = true
        view.emit("openChat", false)
        view.focus()
        alt.toggleGameControls(false)
        alt.emit("Client:HUD:setCefStatus", true) // for what its here?
      }
      break
    }

    case KeyCode.Escape: { // Escape
      if (chat.chatData.opened) {
        chat.chatData.opened = false
        view.emit("closeChat")
        view.unfocus()
        alt.toggleGameControls(true)
        alt.emit("Client:HUD:setCefStatus", false) // for what its here?
      }
      break
    }
  }
})

alt.onServer("get_pos", () => {
  const state = alt.getPermissionState(Permission.ClipboardAccess)

  if (state !== PermissionState.Allowed) {
    alt.log("get_pos clipboard access is not allowed, state:", state)
    return
  }

  alt.copyToClipboard(LOCAL_PLAYER.pos
    .toArray()
    .map(v => v.toFixed(2))
    .join(" "),
  )

  chat.pushLine("{5eff64}Your position is copied to clipboard!")
})

// TODO: use alt.Utils.EveryTick
let espTick = 0
alt.onServer("esp", (state: boolean) => {
  chat.pushLine(`esp ${state}`)

  if (espTick) alt.clearEveryTick(espTick)
  espTick = 0

  // if (!state) return

  // TODO: add abstract 3d nametags for players and vehicles
  // espTick = alt.everyTick(() => {
  //   for (const veh of alt.Vehicle.streamedIn) {

  //   }
  // })
})
