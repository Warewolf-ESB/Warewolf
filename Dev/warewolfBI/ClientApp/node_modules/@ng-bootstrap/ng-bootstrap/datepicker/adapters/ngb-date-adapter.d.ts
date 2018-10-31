import { NgbDateStruct } from '../ngb-date-struct';
export declare function NGB_DATEPICKER_DATE_ADAPTER_FACTORY(): NgbDateStructAdapter;
/**
 * Abstract type serving as a DI token for the service converting from your application Date model to internal
 * NgbDateStruct model.
 * A default implementation converting from and to NgbDateStruct is provided for retro-compatibility,
 * but you can provide another implementation to use an alternative format, ie for using with native Date Object.
 */
export declare abstract class NgbDateAdapter<D> {
    /**
     * Converts user-model date into an NgbDateStruct for internal use in the library
     */
    abstract fromModel(value: D): NgbDateStruct;
    /**
     * Converts internal date value NgbDateStruct to user-model date
     * The returned type is supposed to be of the same type as fromModel() input-value param
     */
    abstract toModel(date: NgbDateStruct): D;
}
export declare class NgbDateStructAdapter extends NgbDateAdapter<NgbDateStruct> {
    /**
     * Converts a NgbDateStruct value into NgbDateStruct value
     */
    fromModel(date: NgbDateStruct): NgbDateStruct;
    /**
     * Converts a NgbDateStruct value into NgbDateStruct value
     */
    toModel(date: NgbDateStruct): NgbDateStruct;
}
