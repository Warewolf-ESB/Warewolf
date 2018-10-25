import { LogEntry } from './logentry.model';

describe('LogEntry', () => {

  it('should create an instance of LogEntry', () => {
    expect(new LogEntry()).toBeTruthy();
  });

  it('should accept values', () => {
    let log = new LogEntry();
    log = {
      StartDateTime: "StartDateTime",
      Status: "Status",
      Url: "Url",
      Result: "Result",
      User: "User",
      CompletedDateTime: "CompletedDateTime",
      ExecutionTime: "ExecutionTime",
      Count: 10,
      ExecutionId: "111"
    }
    expect(log.ExecutionId).toEqual("111");
    expect(log.Result).toEqual("Result");
    expect(log.User).toEqual('User');
  });
})
