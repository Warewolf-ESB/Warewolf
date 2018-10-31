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
var FromEventObservable = /** @class */ (function (_super) {
    __extends(FromEventObservable, _super);
    function FromEventObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    /* tslint:enable:max-line-length */
    FromEventObservable.create = function (target, eventName, options, selector) {
        return rxjs_1.fromEvent(target, eventName, options, selector);
    };
    return FromEventObservable;
}(rxjs_1.Observable));
exports.FromEventObservable = FromEventObservable;
//# sourceMappingURL=FromEventObservable.js.map