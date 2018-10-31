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
var ForkJoinObservable = /** @class */ (function (_super) {
    __extends(ForkJoinObservable, _super);
    function ForkJoinObservable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    /* tslint:enable:max-line-length */
    ForkJoinObservable.create = function () {
        var sources = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            sources[_i] = arguments[_i];
        }
        return rxjs_1.forkJoin.apply(void 0, sources);
    };
    return ForkJoinObservable;
}(rxjs_1.Observable));
exports.ForkJoinObservable = ForkJoinObservable;
//# sourceMappingURL=ForkJoinObservable.js.map