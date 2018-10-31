import { finalize } from 'rxjs/operators';
export function _finally(callback) {
    return finalize(callback)(this);
}
//# sourceMappingURL=finally.js.map