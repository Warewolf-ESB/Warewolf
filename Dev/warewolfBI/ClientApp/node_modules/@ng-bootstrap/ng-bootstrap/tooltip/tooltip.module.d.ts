import { ModuleWithProviders } from '@angular/core';
export { NgbTooltipConfig } from './tooltip-config';
export { NgbTooltip } from './tooltip';
export { Placement } from '../util/positioning';
export declare class NgbTooltipModule {
    /**
     * No need in forRoot anymore with tree-shakeable services
     *
     * @deprecated 3.0.0
     */
    static forRoot(): ModuleWithProviders;
}
