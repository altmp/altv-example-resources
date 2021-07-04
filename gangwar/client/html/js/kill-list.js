let killList = null;

function deleteOldLists() {
  const now = Date.now();
  for (let k of killList.children) {
    if (k.addedAt + 10000 <= now && k.addedAt + 10200 > now) {
      k.classList.add("kill-list-item-delete");
    } else if (k.addedAt + 10200 <= now) {
      killList.removeChild(k);
    }
  }
}

window.addEventListener("load", () => {
  killList = document.querySelector(".kill-list");
  setInterval(deleteOldLists, 50);
});

function registerKill(killerName, killerGang, victimName, victimGang, weapon) {
  const killListItem = document.createElement("div");
  killListItem.className = "kill-list-item";
  killListItem.addedAt = Date.now();

  const killListKiller = document.createElement("div");
  killListKiller.classList.add("kill-list-killer");
  killListKiller.classList.add(killerGang);
  killListKiller.innerText = killerName;

  const killListWeapon = document.createElement("div");
  killListWeapon.className = "kill-list-weapon";
  killListWeapon.innerText = `(${weapon})`;

  const killListVictim = document.createElement("div");
  killListVictim.classList.add("kill-list-victim");
  killListVictim.classList.add(victimGang);
  killListVictim.innerText = victimName;

  killListItem.appendChild(killListKiller);
  killListItem.appendChild(killListWeapon);
  killListItem.appendChild(killListVictim);

  killList.appendChild(killListItem);
}

alt.on("registerKill", (data) => {
  registerKill(data.killerName, data.killerGang, data.victimName, data.victimGang, data.weapon);
});
