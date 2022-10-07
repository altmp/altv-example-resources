import * as alt from "alt-client"

type EventNames =
  "updatePlayersOnline" |
  "openChat" |
  "closeChat" |
  "addString" |
  "addMessage" |
  "setPlayerId" |
  "setWeaponsDisabled" |
  "focusChatInput" |
  "setStreamedEntities"

class View extends alt.WebView {
  public override emit(eventName: EventNames, ...args: unknown[]) {
    super.emit(eventName, ...args)
  }
}

export const view = new View("http://resource/html/index.html")
