import { Observable } from 'rxjs';
import { _catch } from '../../operator/catch';
Observable.prototype.catch = _catch;
Observable.prototype._catch = _catch;
//# sourceMappingURL=catch.js.map