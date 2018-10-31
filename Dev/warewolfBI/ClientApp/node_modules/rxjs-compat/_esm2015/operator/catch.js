import { catchError as higherOrder } from 'rxjs/operators';
export function _catch(selector) {
    return higherOrder(selector)(this);
}
//# sourceMappingURL=catch.js.map