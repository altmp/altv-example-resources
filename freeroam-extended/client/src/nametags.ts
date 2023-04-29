import * as alt from "alt-client"
import * as native from "natives"
import { clamp, LOCAL_PLAYER } from "./helpers"
import type { RGBAArray } from "./types"

export class PlayerNametags {
  private readonly fontStyle = 0
  private readonly fontSize = 0.35
  private readonly defaultAlpha = 215
  private readonly barsConfig = {
    maxWidth: 0.10,
    maxHeight: 0.015,
    xOffset: 0,
    yOffset: 0.035,
    bgColor: [0, 0, 0, 100],
    healthColorHigh: [80, 185, 80, 180],
    healthColorMiddle: [255, 208, 69, 180],
    healthColorLow: [143, 44, 44, 180],
    armourColor: [222, 222, 222, 150],
    borderColor: [0, 0, 0, 50],
  } as const

  // head bone
  private readonly drawFromBoneId = 12844
  private readonly drawBonePosOffset = 0.5

  private drawRange: number
  private readonly handlers = [this.draw.bind(this)]
  private tick = 0

  constructor({ drawRange = 15 }: { drawRange?: number }) {
    this.drawRange = drawRange
    this.enable(true)
  }

  public enable(toggle: boolean): void {
    if (this.tick) alt.clearEveryTick(this.tick)

    if (toggle)
      this.tick = alt.everyTick(this.everyTickHandler.bind(this))
    else
      this.tick = 0
  }

  private everyTickHandler(): void {
    const camCoord = alt.getCamPos()

    for (const player of alt.Player.streamedIn) {
      const dist = player.pos.distanceTo(camCoord)
      if (dist > this.drawRange) continue

      if (!native.isEntityOnScreen(player)) continue
      if (!native.hasEntityClearLosToEntity(LOCAL_PLAYER, player, 17)) continue

      this.drawPlayerTick(player, dist)
    }
  }

  private drawPlayerTick(player: alt.Player, dist: number): void {
    const pos = {
      ...native.getPedBoneCoords(player, this.drawFromBoneId, 0, 0, 0),
    }
    pos.z += this.drawBonePosOffset

    const scale = 1 - (0.8 * dist) / this.drawRange

    const velocityEntity = player.vehicle ?? player
    const velocityVector = native.getEntityVelocity(velocityEntity)
    const frameTime = native.getFrameTime()

    native.setDrawOrigin(
      pos.x + velocityVector.x * frameTime,
      pos.y + velocityVector.y * frameTime,
      pos.z + velocityVector.z * frameTime,
      false,
    )

    for (const handler of this.handlers) {
      handler(
        player,
        scale,
      )
    }

    native.clearDrawOrigin()
  }

  private draw(player: alt.Player, scale: number): void {
    const fullName = `${player.name} <font family="3">~b~#${player.id}</font>`
    scale *= this.fontSize

    const yOffset = 0 - (player.armour > 0 ? 0.0015 : 0)

    native.beginTextCommandDisplayText("STRING")
    native.setTextFont(this.fontStyle)
    native.setTextScale(scale, scale)
    native.setTextProportional(true)
    native.setTextCentre(true)
    native.setTextColour(255, 255, 255, this.defaultAlpha)
    native.setTextOutline()
    native.addTextComponentSubstringPlayerName(fullName)
    native.endTextCommandDisplayText(0, yOffset, 0)

    this.drawBars(player, scale, yOffset)
  }

  private drawBars(player: alt.Player, scale: number, _yOffset: number): void {
    const {
      maxWidth,
      maxHeight,
      healthColorHigh,
      healthColorMiddle,
      healthColorLow,
      armourColor,
      borderColor,
      xOffset,
      yOffset,
      bgColor,
    } = this.barsConfig

    let {
      health,
      armour,
    } = player

    health -= 100
    health = clamp(health, 0, 100)

    const width = maxWidth * scale
    const y = yOffset * scale * 2.5 - _yOffset
    const healthWidth = (width * (health / 100))
    const healthXOffset = (width - healthWidth) / 2
    const armourWidth = (width * (armour / 100))
    const armourXOffset = (width - armourWidth) / 2
    const containerWidth = width
    const containerHeight = maxHeight * scale

    // background health
    this.drawRect(xOffset, y,
      containerWidth,
      containerHeight,
      ...bgColor,
    )

    // border health
    this.drawRect(xOffset, y,
      containerWidth + 0.0045,
      containerHeight + 0.0065,
      ...borderColor,
    )

    let healthColor: RGBAArray

    if (health > 50)
      healthColor = healthColorHigh
    else if (health < 20)
      healthColor = healthColorLow
    else
      healthColor = healthColorMiddle

    // health
    this.drawRect(
      xOffset - healthXOffset,
      y,
      healthWidth,
      containerHeight,
      ...healthColor,
    )

    if (armour > 0) {
      armour = clamp(armour, 0, 100)

      const _y = y + 0.04 * scale

      // background armour
      this.drawRect(xOffset, _y,
        containerWidth,
        containerHeight,
        ...bgColor,
      )

      // border armour
      this.drawRect(xOffset, _y,
        containerWidth + 0.0045,
        containerHeight + 0.0065,
        ...borderColor,
      )

      // armour
      this.drawRect(
        xOffset - armourXOffset,
        _y,
        armourWidth,
        containerHeight,
        ...armourColor,
      )
    }
  }

  private drawRect(x: number, y: number, width: number, height: number, r: number, g: number, b: number, a: number): void {
    native.drawRect(x, y, width, height, r, g, b, a, false)
  }
}

export const playerNametags = new PlayerNametags({
  drawRange: 25,
})
