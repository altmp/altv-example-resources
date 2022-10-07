import * as alt from "alt-client"
import { playerData } from "./playerdata"
import { view } from "./view"

export const chatData = {
  loaded: false,
  opened: false,
}

interface IBufferItem {
  name: string | null
  text: string
}

const buffer: IBufferItem[] = []

export function toggleChat(): void {
  view.isVisible = !view.isVisible
}

function addMessage(name: string | null, text: string) {
  if (name)
    view.emit("addMessage", name, text)
  else
    view.emit("addString", text)
}

export function pushMessage(name: string | null, text: string): void {
  if (!chatData.loaded)
    buffer.push({ name, text })
  else
    addMessage(name, text)
}

export function pushLine(text: string): void {
  pushMessage(null, text)
}

alt.on("windowFocusChange", (state) => {
  if (!state) return
  if (!chatData.opened) return

  alt.nextTick(() => {
    view.emit("focusChatInput")
  })
})

view.on("chatloaded", () => {
  for (const msg of buffer)
    addMessage(msg.name, msg.text)

  chatData.loaded = true
})

view.on("chatmessage", (text: string) => {
  if (playerData.chatState)
    alt.emitServer("chat:message", text)

  alt.toggleGameControls(true)
  view.unfocus()

  // Timeout to avoid collision with Enter key
  setTimeout(() => {
    chatData.opened = false
  }, 200)
})

pushLine("<b>alt:V Multiplayer has started</b>")
