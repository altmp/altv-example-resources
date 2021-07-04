let playersOnline = null;

window.addEventListener("load", () => {
  playersOnline = document.querySelector(".players-online");
});

alt.on("updatePlayersOnline", (players) => {
  playersOnline.innerText = players + " players";
});
