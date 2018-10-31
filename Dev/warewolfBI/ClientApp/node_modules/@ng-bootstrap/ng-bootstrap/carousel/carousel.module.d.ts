import { ModuleWithProviders } from '@angular/core';
export { NgbCarousel, NgbSlide, NgbSlideEvent } from './carousel';
export { NgbCarouselConfig } from './carousel-config';
export declare class NgbCarouselModule {
    /**
     * Importing with '.forRoot()' is no longer necessary, you can simply import the module.
     * Will be removed in 4.0.0.
     *
     * @deprecated 3.0.0
     */
    static forRoot(): ModuleWithProviders;
}
