import * as native from "natives"
import * as alt from "alt-client"
import { drawDMZone, setWeaponsUsage, mhint, tpToWaypoint } from "./helpers"
import { pushMessage, chatData, toggleChat } from "./chat"
import { toggleNoclip } from "./noclip"
import { KeyCode } from "./keycodes"
import { playerData } from "./playerdata"
import { view } from "./view"

alt.on("connectionComplete", () => {
  setTimeout(() => {
    // We assume that we are not in the airport if areWeaponsDisabled is on true when it triggers
    if (playerData.areWeaponsDisabled)
      setWeaponsUsage(false)
  }, 1000)
})

alt.onServer("airport_state", setWeaponsUsage)

alt.onServer("chat:message", pushMessage)

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
  if (!chatData.loaded) return

  switch (key) {
    case KeyCode.F2: {
      playerData.areNametagsVisible = !playerData.areNametagsVisible
      native.displayRadar(playerData.areNametagsVisible)
      native.displayHud(playerData.areNametagsVisible)
      toggleChat()
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

      if (!chatData.opened && alt.gameControlsEnabled()) {
        chatData.opened = true
        view.emit("openChat", false)
        view.focus()
        alt.toggleGameControls(false)
        alt.emit("Client:HUD:setCefStatus", true) // for what its here?
      }
      break
    }

    case KeyCode.Escape: { // Escape
      if (chatData.opened) {
        chatData.opened = false
        view.emit("closeChat")
        view.unfocus()
        alt.toggleGameControls(true)
        alt.emit("Client:HUD:setCefStatus", false) // for what its here?
      }
      break
    }

    default:
      break
  }
})
