/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbTabset, NgbTab, NgbTabContent, NgbTabTitle } from './tabset';
export { NgbTabset, NgbTab, NgbTabContent, NgbTabTitle } from './tabset';
export { NgbTabsetConfig } from './tabset-config';
/** @type {?} */
const NGB_TABSET_DIRECTIVES = [NgbTabset, NgbTab, NgbTabContent, NgbTabTitle];
export class NgbTabsetModule {
    /**
     * Importing with '.forRoot()' is no longer necessary, you can simply import the module.
     * Will be removed in 4.0.0.
     *
     * @deprecated 3.0.0
     * @return {?}
     */
    static forRoot() { return { ngModule: NgbTabsetModule }; }
}
NgbTabsetModule.decorators = [
    { type: NgModule, args: [{ declarations: NGB_TABSET_DIRECTIVES, exports: NGB_TABSET_DIRECTIVES, imports: [CommonModule] },] },
];

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidGFic2V0Lm1vZHVsZS5qcyIsInNvdXJjZVJvb3QiOiJuZzovL0BuZy1ib290c3RyYXAvbmctYm9vdHN0cmFwLyIsInNvdXJjZXMiOlsidGFic2V0L3RhYnNldC5tb2R1bGUudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7OztBQUFBLE9BQU8sRUFBQyxRQUFRLEVBQXNCLE1BQU0sZUFBZSxDQUFDO0FBQzVELE9BQU8sRUFBQyxZQUFZLEVBQUMsTUFBTSxpQkFBaUIsQ0FBQztBQUU3QyxPQUFPLEVBQUMsU0FBUyxFQUFFLE1BQU0sRUFBRSxhQUFhLEVBQUUsV0FBVyxFQUFDLE1BQU0sVUFBVSxDQUFDO0FBRXZFLE9BQU8sRUFBQyxTQUFTLEVBQUUsTUFBTSxFQUFFLGFBQWEsRUFBRSxXQUFXLEVBQW9CLE1BQU0sVUFBVSxDQUFDO0FBQzFGLE9BQU8sRUFBQyxlQUFlLEVBQUMsTUFBTSxpQkFBaUIsQ0FBQzs7QUFFaEQsTUFBTSxxQkFBcUIsR0FBRyxDQUFDLFNBQVMsRUFBRSxNQUFNLEVBQUUsYUFBYSxFQUFFLFdBQVcsQ0FBQyxDQUFDO0FBRzlFLE1BQU07Ozs7Ozs7O0lBT0osTUFBTSxDQUFDLE9BQU8sS0FBMEIsTUFBTSxDQUFDLEVBQUMsUUFBUSxFQUFFLGVBQWUsRUFBQyxDQUFDLEVBQUU7OztZQVI5RSxRQUFRLFNBQUMsRUFBQyxZQUFZLEVBQUUscUJBQXFCLEVBQUUsT0FBTyxFQUFFLHFCQUFxQixFQUFFLE9BQU8sRUFBRSxDQUFDLFlBQVksQ0FBQyxFQUFDIiwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0IHtOZ01vZHVsZSwgTW9kdWxlV2l0aFByb3ZpZGVyc30gZnJvbSAnQGFuZ3VsYXIvY29yZSc7XG5pbXBvcnQge0NvbW1vbk1vZHVsZX0gZnJvbSAnQGFuZ3VsYXIvY29tbW9uJztcblxuaW1wb3J0IHtOZ2JUYWJzZXQsIE5nYlRhYiwgTmdiVGFiQ29udGVudCwgTmdiVGFiVGl0bGV9IGZyb20gJy4vdGFic2V0JztcblxuZXhwb3J0IHtOZ2JUYWJzZXQsIE5nYlRhYiwgTmdiVGFiQ29udGVudCwgTmdiVGFiVGl0bGUsIE5nYlRhYkNoYW5nZUV2ZW50fSBmcm9tICcuL3RhYnNldCc7XG5leHBvcnQge05nYlRhYnNldENvbmZpZ30gZnJvbSAnLi90YWJzZXQtY29uZmlnJztcblxuY29uc3QgTkdCX1RBQlNFVF9ESVJFQ1RJVkVTID0gW05nYlRhYnNldCwgTmdiVGFiLCBOZ2JUYWJDb250ZW50LCBOZ2JUYWJUaXRsZV07XG5cbkBOZ01vZHVsZSh7ZGVjbGFyYXRpb25zOiBOR0JfVEFCU0VUX0RJUkVDVElWRVMsIGV4cG9ydHM6IE5HQl9UQUJTRVRfRElSRUNUSVZFUywgaW1wb3J0czogW0NvbW1vbk1vZHVsZV19KVxuZXhwb3J0IGNsYXNzIE5nYlRhYnNldE1vZHVsZSB7XG4gIC8qKlxuICAgKiBJbXBvcnRpbmcgd2l0aCAnLmZvclJvb3QoKScgaXMgbm8gbG9uZ2VyIG5lY2Vzc2FyeSwgeW91IGNhbiBzaW1wbHkgaW1wb3J0IHRoZSBtb2R1bGUuXG4gICAqIFdpbGwgYmUgcmVtb3ZlZCBpbiA0LjAuMC5cbiAgICpcbiAgICogQGRlcHJlY2F0ZWQgMy4wLjBcbiAgICovXG4gIHN0YXRpYyBmb3JSb290KCk6IE1vZHVsZVdpdGhQcm92aWRlcnMgeyByZXR1cm4ge25nTW9kdWxlOiBOZ2JUYWJzZXRNb2R1bGV9OyB9XG59XG4iXX0=