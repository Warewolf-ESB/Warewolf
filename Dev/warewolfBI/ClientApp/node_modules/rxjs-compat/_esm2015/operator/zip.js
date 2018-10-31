import { zip as zipStatic } from 'rxjs';
export function zipProto(...observables) {
    return this.lift.call(zipStatic(this, ...observables));
}
//# sourceMappingURL=zip.js.map