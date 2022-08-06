import * as alt from "alt-client"
import { playerData } from "./helpers"

export const chatData = {
  loaded: false,
  opened: false,
}

export const view = new alt.WebView("http://resource/html/index.html")

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

view.on("chatloaded", () => {
  for (const msg of buffer)
    addMessage(msg.name, msg.text)

  chatData.loaded = true
})

view.on("chatmessage", (text: string) => {
  // alt.emitServer("chat:message", text);

  if (text.startsWith("/") && (Date.now() - playerData.lastCommandTimestamp) / 1000 > 10)
    alt.emitServer("chat:message", text)

  // Activate this if we want a cd on the messages
  // else if (playerData.chatState && ((Date.now() - playerData.lastMessageTimestamp) / 1000 > 10)) {
  else if (playerData.chatState) {
    alt.emitServer("chat:message", text)
    playerData.lastMessageTimestamp = Date.now()
  }

  alt.toggleGameControls(true)
  view.unfocus()

  // Timeout to avoid collision with Enter key
  setTimeout(() => {
    chatData.opened = false
  }, 200)
})

pushLine("<b>alt:V Multiplayer has started</b>")
