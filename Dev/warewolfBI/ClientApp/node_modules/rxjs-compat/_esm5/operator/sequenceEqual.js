import { sequenceEqual as higherOrder } from 'rxjs/operators';
export function sequenceEqual(compareTo, comparor) {
    return higherOrder(compareTo, comparor)(this);
}
//# sourceMappingURL=sequenceEqual.js.map