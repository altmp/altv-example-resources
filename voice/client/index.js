import * as alt from "alt-client";

alt.on("keyup", (keycode) => {
  switch (keycode) {
    case 112: // Key: F1
      alt.emitServer("voice:rangeChanged");
      break;
  }
});
