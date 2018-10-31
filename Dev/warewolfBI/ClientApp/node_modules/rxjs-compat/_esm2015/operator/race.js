import { race as higherOrder } from 'rxjs/operators';
export function race(...observables) {
    return higherOrder(...observables)(this);
}
//# sourceMappingURL=race.js.map