import { onErrorResumeNext as higherOrder } from 'rxjs/operators';
export function onErrorResumeNext(...nextSources) {
    return higherOrder(...nextSources)(this);
}
//# sourceMappingURL=onErrorResumeNext.js.map