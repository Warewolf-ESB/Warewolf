import { OnChanges, SimpleChanges } from '@angular/core';
/**
 * A component that can be used inside a custom result template in order to highlight the term inside the text of the
 * result
 */
export declare class NgbHighlight implements OnChanges {
    parts: string[];
    /**
     * The CSS class of the span elements wrapping the term inside the result
     */
    highlightClass: string;
    /**
     * The result text to display. If the term is found inside this text, it's highlighted
     */
    result: string;
    /**
     * The searched term
     */
    term: string;
    ngOnChanges(changes: SimpleChanges): void;
}
