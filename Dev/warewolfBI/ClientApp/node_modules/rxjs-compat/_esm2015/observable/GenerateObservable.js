import { Observable, generate } from 'rxjs';
export class GenerateObservable extends Observable {
    static create(initialStateOrOptions, condition, iterate, resultSelectorOrObservable, scheduler) {
        return generate(initialStateOrOptions, condition, iterate, resultSelectorOrObservable, scheduler);
    }
}
//# sourceMappingURL=GenerateObservable.js.map