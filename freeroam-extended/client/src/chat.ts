import * as alt from "alt-client"
import { playerData } from "./playerdata"
import { view } from "./view"

export const chatData = {
  opened: false,
}

export function toggleChat(): void {
  view.isVisible = !view.isVisible
}

function addMessage(name: string | null, text: string) {
  if (name)
    view.emit("addMessage", name, text)
  else
    view.emit("addString", text)
}

export function pushLine(text: string): void {
  addMessage(null, text)
}

alt.on("windowFocusChange", (state) => {
  if (!state) return
  if (!chatData.opened) return

  alt.nextTick(() => {
    view.emit("focusChatInput")
  })
})

view.on("chatmessage", (text: string) => {
  alt.emitServer("chat:message", text)

  alt.toggleGameControls(true)
  view.unfocus()

  // Timeout to avoid collision with Enter key
  setTimeout(() => {
    chatData.opened = false
  }, 200)
})

pushLine("alt:V Multiplayer has started")
