import { groupBy as higherOrder } from 'rxjs/operators';
export function groupBy(keySelector, elementSelector, durationSelector, subjectSelector) {
    return higherOrder(keySelector, elementSelector, durationSelector, subjectSelector)(this);
}
//# sourceMappingURL=groupBy.js.map