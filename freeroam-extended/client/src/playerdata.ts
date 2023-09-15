class PlayerData {
  public onAreNametagsVisibleChange: ((value: boolean) => void) | null = null
  public onAreWeaponsDisabledChange: ((value: boolean) => void) | null = null

  private _areNametagsVisible = true
  private _areWeaponsDisabled = true
  private _chatState = false

  public get areNametagsVisible(): boolean {
    return this._areNametagsVisible
  }

  public set areNametagsVisible(value: boolean) {
    this._areNametagsVisible = value
    this.onAreNametagsVisibleChange?.(value)
  }

  public get areWeaponsDisabled(): boolean {
    return this._areWeaponsDisabled
  }

  public set areWeaponsDisabled(value: boolean) {
    this._areWeaponsDisabled = value
    this.onAreWeaponsDisabledChange?.(value)
  }

  public get chatState(): boolean {
    return this._chatState
  }

  public set chatState(value: boolean) {
    this._chatState = value
  }
}

export const playerData = new PlayerData()
