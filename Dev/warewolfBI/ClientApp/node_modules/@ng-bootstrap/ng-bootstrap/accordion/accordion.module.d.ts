import { ModuleWithProviders } from '@angular/core';
export { NgbAccordion, NgbPanel, NgbPanelTitle, NgbPanelContent, NgbPanelChangeEvent } from './accordion';
export { NgbAccordionConfig } from './accordion-config';
export declare class NgbAccordionModule {
    /**
     * Importing with '.forRoot()' is no longer necessary, you can simply import the module.
     * Will be removed in 4.0.0.
     *
     * @deprecated 3.0.0
     */
    static forRoot(): ModuleWithProviders;
}
