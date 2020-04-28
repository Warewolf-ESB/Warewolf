using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Util;
using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json;
using Warewolf.Storage.Interfaces;

namespace Warewolf.GraphQL
{
  public class Query : ObjectGraphType
  {
    private IExecutionEnvironment _env;

    public Query(IExecutionEnvironment environment)
    {
      _env = environment;
      Field<ListGraphType<ScalarType>>("scalars",
                                       resolve: context => GetScalars());
      Field<ScalarType>("scalarName",
                        arguments: new QueryArguments(new QueryArgument<StringGraphType> {Name = "name"}),
                        resolve: context =>
                                 {
                                   var name = context.GetArgument<string>("name");
                                   return GetScalar(name);
                                 });

      Field<ListGraphType<RecordsetType>>("recordsets",
                                          resolve: context => GetRecordsets());
      Field<RecordsetType>("recordsetName",
                           arguments: new QueryArguments(new QueryArgument<StringGraphType> {Name = "name"}),
                           resolve: context =>
                                    {
                                      var name = context.GetArgument<string>("name");
                                      return GetRecordset(name);
                                    });
      Field<RecordsetType>("recordsetColumnName",
                           arguments: new QueryArguments(new QueryArgument<StringGraphType> {Name = "name"}),
                           resolve: context =>
                                    {
                                      var name = context.GetArgument<string>("name");
                                      return GetRecordsetColumn(name);
                                    });

      Field<ListGraphType<ObjectType>>("objects",
                                       resolve: context => GetObjects());

      Field<ObjectType>("objectData",
                        arguments: new QueryArguments(new QueryArgument<StringGraphType> {Name = "name"}),
                        resolve: context =>
                                 {
                                   var name = context.GetArgument<string>("name");
                                   return GetObjectData(name);
                                 });
    }

    private List<Scalar> GetScalars()
    {
      return _env.EvalAllScalars().Select(s => new Scalar {Name = s.scalarName, Value = s.scalar.ToString()}).ToList();
    }

    private Scalar GetScalar(string name)
    {
      var value = DataStorage.WarewolfAtom.Nothing;
      if (_env.Eval(DataListUtil.AddBracketsToValueIfNotExist(name), 0) is
            CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfEvalResult)
      {
        value = warewolfEvalResult.Item;
      }

      return new Scalar {Name = name, Value = value.ToString()};
    }



    private List<RecordSet> GetRecordsets()
    {
      var recordSets = new List<RecordSet>();
      var evalAllRecordsets = _env.EvalAllRecordsets();
      foreach (var (recSetName, warewolfRecordset) in evalAllRecordsets)
      {
        var recSet = new RecordSet
                     {
                       Name = recSetName
                     };
        foreach (var data in warewolfRecordset.Data)
        {
          if (data.Key != "WarewolfPositionColumn")
          {
            var scalarList = new ScalarList {Name = data.Key};
            var warewolfAtomList = data.Value;
            foreach (var warewolfAtom in warewolfAtomList)
            {
              scalarList.Value.Add(warewolfAtom.ToString());
            }

            recSet.Columns.Add(scalarList);

          }
        }

        recordSets.Add(recSet);
      }

      return recordSets;
    }

    private RecordSet GetRecordset(string name)
    {
      var recordSet = new RecordSet {Name = name};
      if (_env.Eval(DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(name, true)),
                    0) is
            CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult warewolfEvalResult)
      {
        var recsetData = warewolfEvalResult.Item.Data;
        foreach (var data in recsetData)
        {
          if (data.Key != "WarewolfPositionColumn")
          {
            var scalarList = new ScalarList {Name = data.Key};
            var warewolfAtomList = data.Value;
            foreach (var warewolfAtom in warewolfAtomList)
            {
              scalarList.Value.Add(warewolfAtom.ToString());
            }

            recordSet.Columns.Add(scalarList);

          }
        }
      }

      return recordSet;
    }

    private RecordSet GetRecordsetColumn(string name)
    {
      var recSetParts = name.Split('.');
      var recordSet = new RecordSet {Name = recSetParts[0]};
      if (
        _env.Eval(DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(recSetParts[0], recSetParts[1], "*")),
                  0) is
          CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfEvalResult)
      {
        var recsetData = warewolfEvalResult.Item;
        var scalarList = new ScalarList {Name = recSetParts[1]};
        foreach (var data in recsetData)
        {
          scalarList.Value.Add(data.ToString());
        }

        recordSet.Columns.Add(scalarList);
      }

      return recordSet;
    }

    private List<Object> GetObjects()
    {
      var objects = new List<Object>();
      var allObjects = _env.EvalAllObjects();
      foreach (var (name, jContainer) in allObjects)
      {
        var recSet = new Object
                     {
                       Name = name,
                       Value = jContainer.ToString(Formatting.Indented)
                     };
        objects.Add(recSet);
      }

      return objects;
    }

    private Object GetObjectData(string name)
    {
      var objectData = new Object
                       {
                         Name = name
                       };
      var evalResult = _env.EvalForJson(DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.ObjectStartMarker+name));
      if (evalResult != null)
      {
        objectData.Value = _env.EvalResultToString(evalResult);
      }

      return objectData;
    }
  }
}