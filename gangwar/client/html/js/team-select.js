let teamSelectLayer = null;

window.addEventListener("load", () => {
  teamSelectLayer = document.querySelector(".team-select-layer");
});

function showTeamSelect(population) {
  teamSelectLayer.style.display = "flex";

  const teamPlayersSpawn = document.querySelectorAll(".team-select span");

  for (let tp = 0; tp < teamPlayersSpawn.length; ++tp) {
    switch (tp) {
      case 0:
        teamPlayersSpawn[tp].innerHTML = population["families"] + " players";
        break;
      case 1:
        teamPlayersSpawn[tp].innerHTML = population["ballas"] + " players";
        break;
      case 2:
        teamPlayersSpawn[tp].innerHTML = population["vagos"] + " players";
        break;
    }
  }
}

function hideTeamSelect() {
  teamSelectLayer.style.display = "none";
}

function selectTeam(teamId) {
  alt.emit("teamSelected", teamId);
  hideTeamSelect();
}

alt.on("showTeamSelect", (population) => {
  console.log(population);
  showTeamSelect(population);
});
