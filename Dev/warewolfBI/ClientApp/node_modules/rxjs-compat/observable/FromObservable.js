"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var FromObservable = /** @class */ (function (_super) {
    __extends(FromObservable, _super);
    function FromObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    FromObservable.create = function (ish, scheduler) {
        return rxjs_1.from(ish, scheduler);
    };
    return FromObservable;
}(rxjs_1.Observable));
exports.FromObservable = FromObservable;
//# sourceMappingURL=FromObservable.js.map