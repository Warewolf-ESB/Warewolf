import { ModuleWithProviders } from '@angular/core';
export { NgbTabset, NgbTab, NgbTabContent, NgbTabTitle, NgbTabChangeEvent } from './tabset';
export { NgbTabsetConfig } from './tabset-config';
export declare class NgbTabsetModule {
    /**
     * Importing with '.forRoot()' is no longer necessary, you can simply import the module.
     * Will be removed in 4.0.0.
     *
     * @deprecated 3.0.0
     */
    static forRoot(): ModuleWithProviders;
}
