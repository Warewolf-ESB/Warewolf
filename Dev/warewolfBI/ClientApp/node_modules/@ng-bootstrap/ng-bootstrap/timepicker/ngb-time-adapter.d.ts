import { NgbTimeStruct } from './ngb-time-struct';
export declare function NGB_DATEPICKER_TIME_ADAPTER_FACTORY(): NgbTimeStructAdapter;
/**
 * Abstract type serving as a DI token for the service converting from your application Time model to internal
 * NgbTimeStruct model.
 * A default implementation converting from and to NgbTimeStruct is provided for retro-compatibility,
 * but you can provide another implementation to use an alternative format, ie for using with native Date Object.
 *
 * @since 2.2.0
 */
export declare abstract class NgbTimeAdapter<T> {
    /**
     * Converts user-model date into an NgbTimeStruct for internal use in the library
     */
    abstract fromModel(value: T): NgbTimeStruct;
    /**
     * Converts internal time value NgbTimeStruct to user-model date
     * The returned type is supposed to be of the same type as fromModel() input-value param
     */
    abstract toModel(time: NgbTimeStruct): T;
}
export declare class NgbTimeStructAdapter extends NgbTimeAdapter<NgbTimeStruct> {
    /**
     * Converts a NgbTimeStruct value into NgbTimeStruct value
     */
    fromModel(time: NgbTimeStruct): NgbTimeStruct;
    /**
     * Converts a NgbTimeStruct value into NgbTimeStruct value
     */
    toModel(time: NgbTimeStruct): NgbTimeStruct;
}
