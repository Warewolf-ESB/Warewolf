/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
import { Injectable } from '@angular/core';
import * as i0 from "@angular/core";
/**
 * Configuration service for the NgbPopover directive.
 * You can inject this service, typically in your root component, and customize the values of its properties in
 * order to provide default values for all the popovers used in the application.
 */
export class NgbPopoverConfig {
    constructor() {
        this.autoClose = true;
        this.placement = 'top';
        this.triggers = 'click';
        this.disablePopover = false;
    }
}
NgbPopoverConfig.decorators = [
    { type: Injectable, args: [{ providedIn: 'root' },] },
];
/** @nocollapse */ NgbPopoverConfig.ngInjectableDef = i0.defineInjectable({ factory: function NgbPopoverConfig_Factory() { return new NgbPopoverConfig(); }, token: NgbPopoverConfig, providedIn: "root" });
if (false) {
    /** @type {?} */
    NgbPopoverConfig.prototype.autoClose;
    /** @type {?} */
    NgbPopoverConfig.prototype.placement;
    /** @type {?} */
    NgbPopoverConfig.prototype.triggers;
    /** @type {?} */
    NgbPopoverConfig.prototype.container;
    /** @type {?} */
    NgbPopoverConfig.prototype.disablePopover;
    /** @type {?} */
    NgbPopoverConfig.prototype.popoverClass;
}

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicG9wb3Zlci1jb25maWcuanMiLCJzb3VyY2VSb290Ijoibmc6Ly9AbmctYm9vdHN0cmFwL25nLWJvb3RzdHJhcC8iLCJzb3VyY2VzIjpbInBvcG92ZXIvcG9wb3Zlci1jb25maWcudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7OztBQUFBLE9BQU8sRUFBQyxVQUFVLEVBQUMsTUFBTSxlQUFlLENBQUM7Ozs7Ozs7QUFTekMsTUFBTTs7eUJBQ3dDLElBQUk7eUJBQ3BCLEtBQUs7d0JBQ3RCLE9BQU87OEJBRUQsS0FBSzs7OztZQU52QixVQUFVLFNBQUMsRUFBQyxVQUFVLEVBQUUsTUFBTSxFQUFDIiwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0IHtJbmplY3RhYmxlfSBmcm9tICdAYW5ndWxhci9jb3JlJztcbmltcG9ydCB7UGxhY2VtZW50QXJyYXl9IGZyb20gJy4uL3V0aWwvcG9zaXRpb25pbmcnO1xuXG4vKipcbiAqIENvbmZpZ3VyYXRpb24gc2VydmljZSBmb3IgdGhlIE5nYlBvcG92ZXIgZGlyZWN0aXZlLlxuICogWW91IGNhbiBpbmplY3QgdGhpcyBzZXJ2aWNlLCB0eXBpY2FsbHkgaW4geW91ciByb290IGNvbXBvbmVudCwgYW5kIGN1c3RvbWl6ZSB0aGUgdmFsdWVzIG9mIGl0cyBwcm9wZXJ0aWVzIGluXG4gKiBvcmRlciB0byBwcm92aWRlIGRlZmF1bHQgdmFsdWVzIGZvciBhbGwgdGhlIHBvcG92ZXJzIHVzZWQgaW4gdGhlIGFwcGxpY2F0aW9uLlxuICovXG5ASW5qZWN0YWJsZSh7cHJvdmlkZWRJbjogJ3Jvb3QnfSlcbmV4cG9ydCBjbGFzcyBOZ2JQb3BvdmVyQ29uZmlnIHtcbiAgYXV0b0Nsb3NlOiBib29sZWFuIHwgJ2luc2lkZScgfCAnb3V0c2lkZScgPSB0cnVlO1xuICBwbGFjZW1lbnQ6IFBsYWNlbWVudEFycmF5ID0gJ3RvcCc7XG4gIHRyaWdnZXJzID0gJ2NsaWNrJztcbiAgY29udGFpbmVyOiBzdHJpbmc7XG4gIGRpc2FibGVQb3BvdmVyID0gZmFsc2U7XG4gIHBvcG92ZXJDbGFzczogc3RyaW5nO1xufVxuIl19