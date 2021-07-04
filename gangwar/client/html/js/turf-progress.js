let turfProgress = null;
let teamPoints = null;
let turfProgressLayer = null;

window.addEventListener("load", () => {
  turfProgress = document.querySelector(".turf-progress");
  teamPoints = document.querySelector(".team-points");
  turfProgressLayer = document.querySelector(".turf-progress-layer");
});

function generateProgressGradient(progress, color, side) {
  return `linear-gradient(to ${side}, ${color}99, ${color}99 ${progress}%, transparent ${progress}%, transparent 100%)`;
}

function setProgress(progressLeft, progressRight, colorLeft, colorRight) {
  const leftLine = turfProgress.children[0];
  const rightLine = turfProgress.children[1];

  let rightLineGrad = generateProgressGradient(progressLeft * 100, colorLeft, "right");
  let leftLineGrad = generateProgressGradient(progressRight * 100, colorRight, "left");
  leftLine.style.background = leftLineGrad;
  rightLine.style.background = rightLineGrad;
}

function setTeamPoints(team, points) {
  teamPoints.className = "team-points " + team;
  teamPoints.innerText = `${points} / 1000`;
}

function hideProgress() {
  turfProgressLayer.style.display = "none";
}

function showProgress() {
  turfProgressLayer.style.display = "flex";
}

alt.on("setProgress", (progressLeft, progressRight, colorLeft, colorRight) => {
  setProgress(progressLeft, progressRight, colorLeft, colorRight);
});

alt.on("setTeamPoints", (team, points) => {
  setTeamPoints(team, points);
});

alt.on("hideProgress", () => {
  hideProgress();
});

alt.on("showProgress", () => {
  showProgress();
});
