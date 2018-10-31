import { delayWhen as higherOrder } from 'rxjs/operators';
export function delayWhen(delayDurationSelector, subscriptionDelay) {
    return higherOrder(delayDurationSelector, subscriptionDelay)(this);
}
//# sourceMappingURL=delayWhen.js.map