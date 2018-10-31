"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/* tslint:disable:no-unused-variable */
// Subject imported before Observable to bypass circular dependency issue since
// Subject extends Observable and Observable references Subject in it's
// definition
var rxjs_1 = require("rxjs");
exports.Observable = rxjs_1.Observable;
exports.Subject = rxjs_1.Subject;
var internal_compatibility_1 = require("rxjs/internal-compatibility");
exports.AnonymousSubject = internal_compatibility_1.AnonymousSubject;
/* tslint:enable:no-unused-variable */
var internal_compatibility_2 = require("rxjs/internal-compatibility");
exports.config = internal_compatibility_2.config;
// statics
/* tslint:disable:no-use-before-declare */
require("./add/observable/bindCallback");
require("./add/observable/bindNodeCallback");
require("./add/observable/combineLatest");
require("./add/observable/concat");
require("./add/observable/defer");
require("./add/observable/empty");
require("./add/observable/forkJoin");
require("./add/observable/from");
require("./add/observable/fromEvent");
require("./add/observable/fromEventPattern");
require("./add/observable/fromPromise");
require("./add/observable/generate");
require("./add/observable/if");
require("./add/observable/interval");
require("./add/observable/merge");
require("./add/observable/race");
require("./add/observable/never");
require("./add/observable/of");
require("./add/observable/onErrorResumeNext");
require("./add/observable/pairs");
require("./add/observable/range");
require("./add/observable/using");
require("./add/observable/throw");
require("./add/observable/timer");
require("./add/observable/zip");
//dom
require("./add/observable/dom/ajax");
require("./add/observable/dom/webSocket");
//internal/operators
require("./add/operator/buffer");
require("./add/operator/bufferCount");
require("./add/operator/bufferTime");
require("./add/operator/bufferToggle");
require("./add/operator/bufferWhen");
require("./add/operator/catch");
require("./add/operator/combineAll");
require("./add/operator/combineLatest");
require("./add/operator/concat");
require("./add/operator/concatAll");
require("./add/operator/concatMap");
require("./add/operator/concatMapTo");
require("./add/operator/count");
require("./add/operator/dematerialize");
require("./add/operator/debounce");
require("./add/operator/debounceTime");
require("./add/operator/defaultIfEmpty");
require("./add/operator/delay");
require("./add/operator/delayWhen");
require("./add/operator/distinct");
require("./add/operator/distinctUntilChanged");
require("./add/operator/distinctUntilKeyChanged");
require("./add/operator/do");
require("./add/operator/exhaust");
require("./add/operator/exhaustMap");
require("./add/operator/expand");
require("./add/operator/elementAt");
require("./add/operator/filter");
require("./add/operator/finally");
require("./add/operator/find");
require("./add/operator/findIndex");
require("./add/operator/first");
require("./add/operator/groupBy");
require("./add/operator/ignoreElements");
require("./add/operator/isEmpty");
require("./add/operator/audit");
require("./add/operator/auditTime");
require("./add/operator/last");
require("./add/operator/let");
require("./add/operator/every");
require("./add/operator/map");
require("./add/operator/mapTo");
require("./add/operator/materialize");
require("./add/operator/max");
require("./add/operator/merge");
require("./add/operator/mergeAll");
require("./add/operator/mergeMap");
require("./add/operator/mergeMapTo");
require("./add/operator/mergeScan");
require("./add/operator/min");
require("./add/operator/multicast");
require("./add/operator/observeOn");
require("./add/operator/onErrorResumeNext");
require("./add/operator/pairwise");
require("./add/operator/partition");
require("./add/operator/pluck");
require("./add/operator/publish");
require("./add/operator/publishBehavior");
require("./add/operator/publishReplay");
require("./add/operator/publishLast");
require("./add/operator/race");
require("./add/operator/reduce");
require("./add/operator/repeat");
require("./add/operator/repeatWhen");
require("./add/operator/retry");
require("./add/operator/retryWhen");
require("./add/operator/sample");
require("./add/operator/sampleTime");
require("./add/operator/scan");
require("./add/operator/sequenceEqual");
require("./add/operator/share");
require("./add/operator/shareReplay");
require("./add/operator/single");
require("./add/operator/skip");
require("./add/operator/skipLast");
require("./add/operator/skipUntil");
require("./add/operator/skipWhile");
require("./add/operator/startWith");
require("./add/operator/subscribeOn");
require("./add/operator/switch");
require("./add/operator/switchMap");
require("./add/operator/switchMapTo");
require("./add/operator/take");
require("./add/operator/takeLast");
require("./add/operator/takeUntil");
require("./add/operator/takeWhile");
require("./add/operator/throttle");
require("./add/operator/throttleTime");
require("./add/operator/timeInterval");
require("./add/operator/timeout");
require("./add/operator/timeoutWith");
require("./add/operator/timestamp");
require("./add/operator/toArray");
require("./add/operator/toPromise");
require("./add/operator/window");
require("./add/operator/windowCount");
require("./add/operator/windowTime");
require("./add/operator/windowToggle");
require("./add/operator/windowWhen");
require("./add/operator/withLatestFrom");
require("./add/operator/zip");
require("./add/operator/zipAll");
/* tslint:disable:no-unused-variable */
var rxjs_2 = require("rxjs");
exports.Subscription = rxjs_2.Subscription;
exports.ReplaySubject = rxjs_2.ReplaySubject;
exports.BehaviorSubject = rxjs_2.BehaviorSubject;
exports.Notification = rxjs_2.Notification;
exports.EmptyError = rxjs_2.EmptyError;
exports.ArgumentOutOfRangeError = rxjs_2.ArgumentOutOfRangeError;
exports.ObjectUnsubscribedError = rxjs_2.ObjectUnsubscribedError;
exports.UnsubscriptionError = rxjs_2.UnsubscriptionError;
exports.pipe = rxjs_2.pipe;
var testing_1 = require("rxjs/testing");
exports.TestScheduler = testing_1.TestScheduler;
var rxjs_3 = require("rxjs");
exports.Subscriber = rxjs_3.Subscriber;
exports.AsyncSubject = rxjs_3.AsyncSubject;
exports.ConnectableObservable = rxjs_3.ConnectableObservable;
exports.TimeoutError = rxjs_3.TimeoutError;
exports.VirtualTimeScheduler = rxjs_3.VirtualTimeScheduler;
var ajax_1 = require("rxjs/ajax");
exports.AjaxResponse = ajax_1.AjaxResponse;
exports.AjaxError = ajax_1.AjaxError;
exports.AjaxTimeoutError = ajax_1.AjaxTimeoutError;
var rxjs_4 = require("rxjs");
var internal_compatibility_3 = require("rxjs/internal-compatibility");
var internal_compatibility_4 = require("rxjs/internal-compatibility");
exports.TimeInterval = internal_compatibility_4.TimeInterval;
exports.Timestamp = internal_compatibility_4.Timestamp;
var _operators = require("rxjs/operators");
exports.operators = _operators;
/* tslint:enable:no-unused-variable */
/**
 * @typedef {Object} Rx.Scheduler
 * @property {Scheduler} queue Schedules on a queue in the current event frame
 * (trampoline scheduler). Use this for iteration operations.
 * @property {Scheduler} asap Schedules on the micro task queue, which is the same
 * queue used for promises. Basically after the current job, but before the next
 * job. Use this for asynchronous conversions.
 * @property {Scheduler} async Schedules work with `setInterval`. Use this for
 * time-based operations.
 * @property {Scheduler} animationFrame Schedules work with `requestAnimationFrame`.
 * Use this for synchronizing with the platform's painting
 */
var Scheduler = {
    asap: rxjs_4.asapScheduler,
    queue: rxjs_4.queueScheduler,
    animationFrame: rxjs_4.animationFrameScheduler,
    async: rxjs_4.asyncScheduler
};
exports.Scheduler = Scheduler;
/**
 * @typedef {Object} Rx.Symbol
 * @property {Symbol|string} rxSubscriber A symbol to use as a property name to
 * retrieve an "Rx safe" Observer from an object. "Rx safety" can be defined as
 * an object that has all of the traits of an Rx Subscriber, including the
 * ability to add and remove subscriptions to the subscription chain and
 * guarantees involving event triggering (can't "next" after unsubscription,
 * etc).
 * @property {Symbol|string} observable A symbol to use as a property name to
 * retrieve an Observable as defined by the [ECMAScript "Observable" spec](https://github.com/zenparsing/es-observable).
 * @property {Symbol|string} iterator The ES6 symbol to use as a property name
 * to retrieve an iterator from an object.
 */
var Symbol = {
    rxSubscriber: internal_compatibility_3.rxSubscriber,
    observable: internal_compatibility_3.observable,
    iterator: internal_compatibility_3.iterator
};
exports.Symbol = Symbol;
//# sourceMappingURL=Rx.js.map