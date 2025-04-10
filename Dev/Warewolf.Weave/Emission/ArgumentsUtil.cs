// Decompiled with JetBrains decompiler
// Type: System.Emission.ArgumentsUtil
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  internal abstract class ArgumentsUtil
  {
    public static Expression[] ConvertArgumentReferenceToExpression(ArgumentReference[] args)
    {
      Expression[] expression = new Expression[args.Length];
      for (int index = 0; index < args.Length; ++index)
        expression[index] = args[index].ToExpression();
      return expression;
    }

    public static ArgumentReference[] ConvertToArgumentReference(Type[] args)
    {
      ArgumentReference[] argumentReference = new ArgumentReference[args.Length];
      for (int index = 0; index < args.Length; ++index)
        argumentReference[index] = new ArgumentReference(args[index]);
      return argumentReference;
    }

    public static ArgumentReference[] ConvertToArgumentReference(ParameterInfo[] args)
    {
      ArgumentReference[] argumentReference = new ArgumentReference[args.Length];
      for (int index = 0; index < args.Length; ++index)
        argumentReference[index] = new ArgumentReference(args[index].ParameterType);
      return argumentReference;
    }

    public static ReferenceExpression[] ConvertToArgumentReferenceExpression(ParameterInfo[] args)
    {
      ReferenceExpression[] referenceExpression = new ReferenceExpression[args.Length];
      for (int index = 0; index < args.Length; ++index)
        referenceExpression[index] = new ReferenceExpression((Reference) new ArgumentReference(args[index].ParameterType, index + 1));
      return referenceExpression;
    }

    public static void EmitLoadOwnerAndReference(Reference reference, ILGenerator il)
    {
      if (reference == null)
        return;
      ArgumentsUtil.EmitLoadOwnerAndReference(reference.OwnerReference, il);
      reference.LoadReference(il);
    }

    public static Type[] GetTypes(ParameterInfo[] parameters)
    {
      Type[] types = new Type[parameters.Length];
      for (int index = 0; index < parameters.Length; ++index)
        types[index] = parameters[index].ParameterType;
      return types;
    }

    public static Type[] InitializeAndConvert(ArgumentReference[] args)
    {
      Type[] typeArray = new Type[args.Length];
      for (int index = 0; index < args.Length; ++index)
      {
        args[index].Position = index + 1;
        typeArray[index] = args[index].Type;
      }
      return typeArray;
    }

    public static void InitializeArgumentsByPosition(ArgumentReference[] args, bool isStatic)
    {
      int num = !isStatic ? 1 : 0;
      for (int index = 0; index < args.Length; ++index)
        args[index].Position = index + num;
    }

    public static bool IsAnyByRef(ParameterInfo[] parameters)
    {
      for (int index = 0; index < parameters.Length; ++index)
      {
        if (parameters[index].ParameterType.IsByRef)
          return true;
      }
      return false;
    }
  }
}
