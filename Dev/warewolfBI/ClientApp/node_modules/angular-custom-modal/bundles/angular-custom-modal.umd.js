(function (global, factory) {
	typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports, require('@angular/core'), require('@angular/common')) :
	typeof define === 'function' && define.amd ? define(['exports', '@angular/core', '@angular/common'], factory) :
	(factory((global['angular-custom-modal'] = {}),global.ng.core,global.ng.common));
}(this, (function (exports,core,common) { 'use strict';

/* tslint:disable:component-selector */
var ModalComponent = (function () {
    /**
     * @param {?} elementRef
     * @param {?} changeDetectorRef
     */
    function ModalComponent(elementRef, changeDetectorRef) {
        this.elementRef = elementRef;
        this.changeDetectorRef = changeDetectorRef;
        this.closeOnOutsideClick = true;
        this.visible = false;
        this.visibleAnimate = false;
    }
    /**
     * @return {?}
     */
    ModalComponent.prototype.ngOnDestroy = function () {
        // Prevent modal from not executing its closing actions if the user navigated away (for example,
        // through a link).
        this.close();
    };
    /**
     * @return {?}
     */
    ModalComponent.prototype.open = function () {
        var _this = this;
        document.body.classList.add('modal-open');
        this.visible = true;
        setTimeout(function () {
            _this.visibleAnimate = true;
        });
    };
    /**
     * @return {?}
     */
    ModalComponent.prototype.close = function () {
        var _this = this;
        document.body.classList.remove('modal-open');
        this.visibleAnimate = false;
        setTimeout(function () {
            _this.visible = false;
            _this.changeDetectorRef.markForCheck();
        }, 200);
    };
    /**
     * @param {?} event
     * @return {?}
     */
    ModalComponent.prototype.onContainerClicked = function (event) {
        if (((event.target)).classList.contains('modal') && this.isTopMost() && this.closeOnOutsideClick) {
            this.close();
        }
    };
    /**
     * @param {?} event
     * @return {?}
     */
    ModalComponent.prototype.onKeyDownHandler = function (event) {
        // If ESC key and TOP MOST modal, close it.
        if (event.key === 'Escape' && this.isTopMost()) {
            this.close();
        }
    };
    /**
     * Returns true if this modal is the top most modal.
     * @return {?}
     */
    ModalComponent.prototype.isTopMost = function () {
        return !this.elementRef.nativeElement.querySelector(':scope modal > .modal');
    };
    return ModalComponent;
}());
ModalComponent.decorators = [
    { type: core.Component, args: [{
                selector: 'modal',
                template: "\n    <div \n      class=\"modal fade\"\n      role=\"dialog\"\n      tabindex=\"-1\"\n      [class.in]=\"visibleAnimate\"\n      *ngIf=\"visible\">\n      <div class=\"modal-dialog\">\n        <div class=\"modal-content\">\n          <div class=\"modal-header\">\n            <ng-container *ngTemplateOutlet=\"header\"></ng-container>\n            <button class=\"close\" data-dismiss=\"modal\" type=\"button\" aria-label=\"Close\" (click)=\"close()\">\u00D7</button>\n          </div>\n          <div class=\"modal-body\">\n            <ng-container *ngTemplateOutlet=\"body\"></ng-container>\n          </div>\n          <div class=\"modal-footer\">\n            <ng-container *ngTemplateOutlet=\"footer\"></ng-container>\n          </div>\n        </div>\n      </div>\n    </div>\n  ",
                styles: ["\n    /**\n     * A more specific selector overwrites bootstrap display properties, but they still enable users\n     * to overwite them on their apps.\n     */\n    /deep/ modal .modal {\n      display: -webkit-box;\n      display: -ms-flexbox;\n      display: flex;\n      -webkit-box-flex: 1;\n          -ms-flex: 1;\n              flex: 1;\n      -webkit-box-align: center;\n          -ms-flex-align: center;\n              align-items: center;\n      -webkit-box-pack: center;\n          -ms-flex-pack: center;\n              justify-content: center; }\n\n    /deep/ .modal {\n      position: fixed;\n      top: 0;\n      left: 0;\n      width: 100%;\n      min-height: 100%;\n      background-color: rgba(0, 0, 0, 0.15);\n      z-index: 42; }\n\n    /deep/ .modal.in {\n      opacity: 1; }\n  "],
            },] },
];
/**
 * @nocollapse
 */
ModalComponent.ctorParameters = function () { return [
    { type: core.ElementRef, },
    { type: core.ChangeDetectorRef, },
]; };
ModalComponent.propDecorators = {
    'header': [{ type: core.ContentChild, args: ['modalHeader',] },],
    'body': [{ type: core.ContentChild, args: ['modalBody',] },],
    'footer': [{ type: core.ContentChild, args: ['modalFooter',] },],
    'closeOnOutsideClick': [{ type: core.Input },],
    'onContainerClicked': [{ type: core.HostListener, args: ['click', ['$event'],] },],
    'onKeyDownHandler': [{ type: core.HostListener, args: ['document:keydown', ['$event'],] },],
};
var ModalModule = (function () {
    function ModalModule() {
    }
    return ModalModule;
}());
ModalModule.decorators = [
    { type: core.NgModule, args: [{
                imports: [
                    common.CommonModule,
                ],
                exports: [ModalComponent],
                declarations: [ModalComponent],
                providers: [],
            },] },
];
/**
 * @nocollapse
 */
ModalModule.ctorParameters = function () { return []; };

exports.ModalComponent = ModalComponent;
exports.ModalModule = ModalModule;

Object.defineProperty(exports, '__esModule', { value: true });

})));
//# sourceMappingURL=angular-custom-modal.umd.js.map
