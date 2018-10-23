export class AbstractDashboardCard {
    constructor(_name, _routerLink, _iconClass, _col, _chart, _row, _color) {
        this._name = _name;
        this._routerLink = _routerLink;
        this._iconClass = _iconClass;
        this._col = _col;
        this._chart = _chart;
        this._row = _row;
        this._color = _color;
    }
    get name() {
        return this._name;
    }
    get routerLink() {
        return this._routerLink;
    }
    get iconClass() {
        return this._iconClass;
    }
    get chart() {
        return this._chart;
    }
    get col() {
        return this._col;
    }
    get row() {
        return this._row;
    }
    get color() {
        return this._color;
    }
}
//# sourceMappingURL=abstract-dashboard-card.js.map