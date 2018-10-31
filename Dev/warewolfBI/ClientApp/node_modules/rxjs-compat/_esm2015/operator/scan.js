import { scan as higherOrderScan } from 'rxjs/operators';
export function scan(accumulator, seed) {
    if (arguments.length >= 2) {
        return higherOrderScan(accumulator, seed)(this);
    }
    return higherOrderScan(accumulator)(this);
}
//# sourceMappingURL=scan.js.map