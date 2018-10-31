import { ModuleWithProviders } from '@angular/core';
export { NgbAccordionModule, NgbPanelChangeEvent, NgbAccordionConfig, NgbAccordion, NgbPanel, NgbPanelTitle, NgbPanelContent } from './accordion/accordion.module';
export { NgbAlertModule, NgbAlertConfig, NgbAlert } from './alert/alert.module';
export { NgbButtonsModule, NgbCheckBox, NgbRadioGroup } from './buttons/buttons.module';
export { NgbCarouselModule, NgbCarouselConfig, NgbCarousel, NgbSlide } from './carousel/carousel.module';
export { NgbCollapseModule, NgbCollapse } from './collapse/collapse.module';
export { NgbCalendar, NgbPeriod, NgbCalendarIslamicCivil, NgbCalendarIslamicUmalqura, NgbCalendarHebrew, NgbCalendarPersian, NgbDatepickerModule, NgbDatepickerI18n, NgbDatepickerI18nHebrew, NgbDatepickerConfig, NgbDateStruct, NgbDate, NgbDateParserFormatter, NgbDateAdapter, NgbDateNativeAdapter, NgbDateNativeUTCAdapter, NgbDatepicker, NgbInputDatepicker } from './datepicker/datepicker.module';
export { NgbDropdownModule, NgbDropdownConfig, NgbDropdown } from './dropdown/dropdown.module';
export { NgbModalModule, NgbModal, NgbModalConfig, NgbModalOptions, NgbActiveModal, NgbModalRef, ModalDismissReasons } from './modal/modal.module';
export { NgbPaginationModule, NgbPaginationConfig, NgbPagination } from './pagination/pagination.module';
export { NgbPopoverModule, NgbPopoverConfig, NgbPopover } from './popover/popover.module';
export { NgbProgressbarModule, NgbProgressbarConfig, NgbProgressbar } from './progressbar/progressbar.module';
export { NgbRatingModule, NgbRatingConfig, NgbRating } from './rating/rating.module';
export { NgbTabsetModule, NgbTabChangeEvent, NgbTabsetConfig, NgbTabset, NgbTab, NgbTabContent, NgbTabTitle } from './tabset/tabset.module';
export { NgbTimepickerModule, NgbTimepickerConfig, NgbTimeStruct, NgbTimepicker, NgbTimeAdapter } from './timepicker/timepicker.module';
export { NgbTooltipModule, NgbTooltipConfig, NgbTooltip } from './tooltip/tooltip.module';
export { NgbHighlight, NgbTypeaheadModule, NgbTypeaheadConfig, NgbTypeaheadSelectItemEvent, NgbTypeahead } from './typeahead/typeahead.module';
export { Placement } from './util/positioning';
export declare class NgbModule {
    /**
     * Importing with '.forRoot()' is no longer necessary, you can simply import the module.
     * Will be removed in 4.0.0.
     *
     * @deprecated 3.0.0
     */
    static forRoot(): ModuleWithProviders;
}
