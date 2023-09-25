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
  "setStreamedEntities" |
  "setVoiceConnectionState"

class View extends alt.WebView {
  public override emit(eventName: EventNames, ...args: unknown[]) {
    super.emit(eventName, ...args)
  }
}

const locale = alt.getLocale();

let viewUrl = "http://resource/html/index.html"
if (locale === "de") {
  viewUrl = "http://resource/html/index.de.html"
}
export const view = new View(viewUrl)
