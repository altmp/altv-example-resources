import * as alt from "alt-client";
import * as game from "natives";

alt.onServer("freeroam:spawned", () => {
  game.setPedDefaultComponentVariation(alt.Player.local.scriptID);
});

alt.onServer("freeroam:switchInOutPlayer", (in_switch, instant_switch, switch_type) => {
  if (in_switch) {
    game.switchInPlayer(alt.Player.local.scriptID);
  } else {
    game.switchOutPlayer(alt.Player.local.scriptID, instant_switch, switch_type);
  }
});

// Source: https://github.com/Stuyk/altV-Open-Roleplay/blob/5ccdeb9e960a7e0fde758cc89c366ed2953cc639/resources/orp/client/systems/interiors.mjs
alt.onServer("freeroam:Interiors", () => {
  alt.loadDefaultIpls();
});

alt.onServer("freeroam:sendNotification", sendNotification);

function sendNotification(textColor, bgColor, message, blink) {
  game.setColourOfNextTextComponent(textColor);
  game.setNotificationBackgroundColor(bgColor);
  game.setNotificationTextEntry("STRING");
  game.addTextComponentSubstringPlayerName(message);
  game.drawNotification(blink, false);
}
