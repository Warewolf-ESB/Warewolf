export abstract class AbstractDashboardCard {
  constructor(private _name: string,
    private _routerLink: string,
    private _iconClass: string,
    private _col: string,
    private _chart: string,
    private _row: string,
    private _color: string) {
  }

  get name(): string {
    return this._name;
  }

  get routerLink(): string {
    return this._routerLink;
  }

  get iconClass(): string {
    return this._iconClass;
  }

  get chart(): string {
    return this._chart;
  }
  get col(): string {
    return this._col;
  }

  get row(): string {
    return this._row;
  }

  get color(): string {
    return this._color;
  }
}
