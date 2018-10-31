import { Observable } from 'rxjs';
import { mergeMapTo } from '../../operator/mergeMapTo';
Observable.prototype.flatMapTo = mergeMapTo;
Observable.prototype.mergeMapTo = mergeMapTo;
//# sourceMappingURL=mergeMapTo.js.map