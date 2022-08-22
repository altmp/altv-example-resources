import * as alt from "alt-client"
import * as chat from "./chat"

let state = alt.isGameFocused()

const gameFocusChange = () => {
  chat.onGameFocusChange(state)
}

setInterval(() => {
  const current = alt.isGameFocused()
  if (state === current) return
  state = current
  gameFocusChange()
}, 500)
