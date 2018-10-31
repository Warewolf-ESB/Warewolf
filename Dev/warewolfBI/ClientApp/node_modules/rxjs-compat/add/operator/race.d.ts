import { race } from '../../operator/race';
declare module 'rxjs/internal/Observable' {
    interface Observable<T> {
        race: typeof race;
    }
}
