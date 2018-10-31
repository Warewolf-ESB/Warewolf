import { multicast as higherOrder } from 'rxjs/operators';
export function multicast(subjectOrSubjectFactory, selector) {
    return higherOrder(subjectOrSubjectFactory, selector)(this);
}
//# sourceMappingURL=multicast.js.map