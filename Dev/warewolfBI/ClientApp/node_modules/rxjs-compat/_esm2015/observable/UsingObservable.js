import { Observable, using } from 'rxjs';
export class UsingObservable extends Observable {
    static create(resourceFactory, observableFactory) {
        return using(resourceFactory, observableFactory);
    }
}
//# sourceMappingURL=UsingObservable.js.map