import { Deserializable } from "./../models/deserializable";

export class LogEntry implements Deserializable {
  public StartDateTime: string;
  public Status: boolean;
  public Url: string;
  public Result: string;
  public User: string;
  public CompletedDateTime: boolean;
   public ExecutionTime: string;
 
  public ExecutionId: string;
 
  deserialize(input: any) {
    Object.assign(this, input);
    return this;
  }
}
