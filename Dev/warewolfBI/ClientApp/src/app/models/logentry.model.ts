import { Deserializable } from "./../models/deserializable";

export class LogEntry implements Deserializable {
  public StartDateTime: string;
  public Status: string;
  public Url: string;
  public Result: string;
  public User: string;
  public CompletedDateTime: string;
  public ExecutionTime: string;
  public Count: number;
 
  public ExecutionId: string;
    LogEntry: LogEntry;
 
  deserialize(input: any) {
    Object.assign(this, input);
    return this;
  }
}
