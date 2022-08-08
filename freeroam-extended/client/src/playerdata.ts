class PlayerData {
  public onAreNametagsVisibleChange: ((value: boolean) => void) | null = null

  private _areNametagsVisible = true
  private _areWeaponsDisabled = true
  private _lastCommandTimestamp = Date.now() - 10000
  private _lastMessageTimestamp = Date.now() - 10000
  private _chatState = false
  private _commandTimestamp = 0

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
  }

  public get lastCommandTimestamp(): number {
    return this._lastCommandTimestamp
  }

  public set lastCommandTimestamp(value: number) {
    this._lastCommandTimestamp = value
  }

  public get lastMessageTimestamp(): number {
    return this._lastMessageTimestamp
  }

  public set lastMessageTimestamp(value: number) {
    this._lastMessageTimestamp = value
  }

  public get chatState(): boolean {
    return this._chatState
  }

  public set chatState(value: boolean) {
    this._chatState = value
  }

  public get commandTimestamp(): number {
    return this._commandTimestamp
  }

  public set commandTimestamp(value: number) {
    this._commandTimestamp = value
  }
}

export const playerData = new PlayerData()
