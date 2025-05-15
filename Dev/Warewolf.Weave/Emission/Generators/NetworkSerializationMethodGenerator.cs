// Decompiled with JetBrains decompiler
// Type: System.Emission.Generators.NetworkSerializationMethodGenerator
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Emission.Emitters;
using System.Emission.Meta;
using System.Network;
using System.Reflection;

namespace System.Emission.Generators
{
  internal class NetworkSerializationMethodGenerator : MethodGenerator
  {
    private static readonly ConstructorInfo _createPacket = typeof (Packet).GetConstructor(new Type[1]
    {
      typeof (PacketTemplate)
    });
    private static readonly MethodInfo _sendSimplexPacket = typeof (__BaseNetworkTransparentProxy).GetMethod("SendSimplexPacket", new Type[1]
    {
      typeof (Packet)
    });
    private static readonly MethodInfo _sendDuplexPacket = typeof (__BaseNetworkTransparentProxy).GetMethod("SendDuplexPacket", new Type[1]
    {
      typeof (Packet)
    });
    private static readonly MethodInfo[] _allByteReaderBaseMethods = typeof (IByteReaderBase).GetMethods();
    private static readonly MethodInfo[] _allBaseNetworkTransparentProxyMethods = typeof (__BaseNetworkTransparentProxy).GetMethods();
    private static readonly MethodInfo _genericWrite = NetworkSerializationMethodGenerator.FindGenericWrite();
    private static readonly MethodInfo _writeUnhandled = typeof (__BaseNetworkTransparentProxy).GetMethod("WriteUnhandled");
    private static readonly MethodInfo _readUnhandled = typeof (__BaseNetworkTransparentProxy).GetMethod("ReadUnhandled");
    private static readonly MethodInfo _constructUnhandled = typeof (__BaseNetworkTransparentProxy).GetMethod("ConstructUnhandled");

    private static MethodInfo FindGenericWrite()
    {
      MethodInfo[] methods = typeof (__BaseNetworkTransparentProxy).GetMethods();
      for (int index = 0; index < methods.Length; ++index)
      {
        if (methods[index].ReturnType == typeof (void) && methods[index].IsGenericMethodDefinition && methods[index].GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof (IList<>))
          return methods[index];
      }
      return (MethodInfo) null;
    }

    public NetworkSerializationMethodGenerator(
      MetaMethod method,
      OverrideMethodDelegate overrideMethod)
      : base(method, overrideMethod)
    {
    }

    protected override MethodEmitter BuildProxiedMethodBody(
      MethodEmitter emitter,
      ClassEmitter @class,
      EmissionProxyOptions options,
      IDesignatingScope designatingScope,
      string dynamicAssemblyName)
    {
      ParameterInfo[] parameters = this.MethodToOverride.GetParameters();
      this.InitOutParameters(emitter, parameters);
      bool emitReturn = true;
      if (this.Method.Source == MetaMethodSource.Method)
        this.EmitSerialization(emitter, @class, options, parameters, out emitReturn);
      else if ((this.Method.Source & MetaMethodSource.Property) != MetaMethodSource.Property)
      {
        int num = (int) (this.Method.Source & MetaMethodSource.Event);
      }
      if (emitReturn)
      {
        if (emitter.ReturnType == typeof (void))
          emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement());
        else
          emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement((Expression) new DefaultValueExpression(emitter.ReturnType)));
      }
      return emitter;
    }

    private void EmitSerialization(
      MethodEmitter emitter,
      ClassEmitter @class,
      EmissionProxyOptions options,
      ParameterInfo[] parameters,
      out bool emitReturn)
    {
      LocalReference localReference1 = emitter.CodeBuilder.DeclareLocal(typeof (Packet));
      emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) localReference1, (Expression) new NewInstanceExpression(NetworkSerializationMethodGenerator._createPacket, new Expression[1]
      {
        @class.GetField("__template").ToExpression()
      })));
      bool flag1 = false;
      for (int index = 0; index < parameters.Length; ++index)
      {
        ParameterInfo parameter = parameters[index];
        if (!parameter.IsOut)
        {
          Type parameterType = parameter.ParameterType;
          MethodInfo method1 = typeof (Packet).GetMethod("Write", new Type[1]
          {
            parameterType
          });
          if (method1 == (MethodInfo) null)
          {
            if (typeof (INetworkSerializable).IsAssignableFrom(parameterType))
            {
              MethodInfo method2 = typeof (INetworkSerializable).GetMethod("Serialize", new Type[1]
              {
                typeof (IByteWriterBase)
              });
              emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) new ArgumentReference(parameter.ParameterType, index + 1), method2, new Expression[1]
              {
                localReference1.ToExpression()
              })));
            }
            else
            {
              MethodInfo method3 = typeof (__BaseNetworkTransparentProxy).GetMethod("Write", new Type[2]
              {
                typeof (IByteWriterBase),
                parameterType
              });
              if (method3 != (MethodInfo) null)
                emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), method3, new Expression[2]
                {
                  localReference1.ToExpression(),
                  new ArgumentReference(parameter.ParameterType, index + 1).ToExpression()
                })));
              else if (parameterType.IsEnum)
              {
                MethodInfo method4 = typeof (Packet).GetMethod("Write", new Type[1]
                {
                  typeof (int)
                });
                emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) localReference1, method4, new Expression[1]
                {
                  (Expression) new ConvertExpression(typeof (int), new ArgumentReference(parameter.ParameterType, index + 1).ToExpression())
                })));
              }
              else
              {
                bool flag2 = false;
                if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof (IList<>))
                {
                  Type genericArgument = parameterType.GetGenericArguments()[0];
                  if (typeof (INetworkSerializable).IsAssignableFrom(genericArgument))
                  {
                    MethodInfo method5 = NetworkSerializationMethodGenerator._genericWrite.MakeGenericMethod(genericArgument);
                    emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), method5, new Expression[2]
                    {
                      localReference1.ToExpression(),
                      new ArgumentReference(parameter.ParameterType, index + 1).ToExpression()
                    })));
                    flag2 = true;
                  }
                }
                if (!flag2)
                  emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), NetworkSerializationMethodGenerator._writeUnhandled, new Expression[2]
                  {
                    localReference1.ToExpression(),
                    (Expression) new ConvertExpression(typeof (object), new ArgumentReference(parameter.ParameterType, index + 1).ToExpression())
                  })));
              }
            }
          }
          else
            emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) localReference1, method1, new Expression[1]
            {
              new ArgumentReference(parameter.ParameterType, index + 1).ToExpression()
            })));
        }
        else
          flag1 = true;
      }
      bool flag3 = emitter.ReturnType != typeof (void);
      if (flag1 | flag3)
      {
        emitReturn = true;
        LocalReference localReference2 = emitter.CodeBuilder.DeclareLocal(typeof (IByteReaderBase));
        emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) localReference2, (Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), NetworkSerializationMethodGenerator._sendDuplexPacket, new Expression[1]
        {
          localReference1.ToExpression()
        })));
        if (flag1)
        {
          for (int index1 = 0; index1 < parameters.Length; ++index1)
          {
            ParameterInfo parameter = parameters[index1];
            if (parameter.IsOut)
            {
              Type elementType = parameter.ParameterType.GetElementType();
              MethodInfo method6 = (MethodInfo) null;
              for (int index2 = 0; index2 < NetworkSerializationMethodGenerator._allByteReaderBaseMethods.Length; ++index2)
              {
                if (NetworkSerializationMethodGenerator._allByteReaderBaseMethods[index2].ReturnType == elementType && NetworkSerializationMethodGenerator._allByteReaderBaseMethods[index2].GetParameters().Length == 0)
                {
                  method6 = NetworkSerializationMethodGenerator._allByteReaderBaseMethods[index2];
                  break;
                }
              }
              if (method6 != (MethodInfo) null)
                emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), (Expression) new MethodInvocationExpression((Reference) localReference2, method6, Array.Empty<Expression>())));
              else if (typeof (INetworkSerializable).IsAssignableFrom(elementType))
              {
                MethodInfo method7 = typeof (INetworkSerializable).GetMethod("Deserialize", new Type[1]
                {
                  typeof (IByteReaderBase)
                });
                if (elementType.GetConstructor(Type.EmptyTypes) == (ConstructorInfo) null)
                  emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), (Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), NetworkSerializationMethodGenerator._constructUnhandled, new Expression[1]
                  {
                    (Expression) new TypeTokenExpression(elementType)
                  })));
                else
                  emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), (Expression) new NewInstanceExpression(elementType.GetConstructor(Type.EmptyTypes), Array.Empty<Expression>())));
                emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), method7, new Expression[1]
                {
                  localReference2.ToExpression()
                })));
              }
              else
              {
                MethodInfo methodInfo = (MethodInfo) null;
                for (int index3 = 0; index3 < NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods.Length; ++index3)
                {
                  if (NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index3].ReturnType == elementType)
                  {
                    ParameterInfo[] parameters1 = NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index3].GetParameters();
                    if (parameters1.Length == 1 && parameters1[0].ParameterType == typeof (IByteReaderBase))
                    {
                      method6 = NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index3];
                      break;
                    }
                  }
                  else if (methodInfo == (MethodInfo) null && NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index3].ReturnType != typeof (void) && NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index3].IsGenericMethodDefinition)
                    methodInfo = NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index3];
                }
                if (method6 != (MethodInfo) null)
                  emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), (Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), method6, new Expression[1]
                  {
                    localReference2.ToExpression()
                  })));
                else if (elementType.IsEnum)
                {
                  MethodInfo method8 = typeof (IByteReaderBase).GetMethod("ReadInt32");
                  emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), (Expression) new ConvertExpression(elementType, (Expression) new MethodInvocationExpression((Reference) localReference2, method8, Array.Empty<Expression>()))));
                }
                else
                {
                  bool flag4 = false;
                  if (methodInfo != (MethodInfo) null && elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof (IList<>))
                  {
                    Type genericArgument = elementType.GetGenericArguments()[0];
                    if (typeof (INetworkSerializable).IsAssignableFrom(genericArgument))
                    {
                      MethodInfo method9 = methodInfo.MakeGenericMethod(genericArgument);
                      emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), (Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), method9, new Expression[1]
                      {
                        localReference2.ToExpression()
                      })));
                      flag4 = true;
                    }
                  }
                  if (!flag4)
                    emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) new ArgumentReference(parameter.ParameterType, index1 + 1), (Expression) new ConvertExpression(elementType, (Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), NetworkSerializationMethodGenerator._readUnhandled, new Expression[2]
                    {
                      localReference2.ToExpression(),
                      (Expression) new TypeTokenExpression(elementType)
                    }))));
                }
              }
            }
          }
        }
        if (!flag3)
          return;
        Type returnType = emitter.ReturnType;
        MethodInfo method10 = (MethodInfo) null;
        for (int index = 0; index < NetworkSerializationMethodGenerator._allByteReaderBaseMethods.Length; ++index)
        {
          if (NetworkSerializationMethodGenerator._allByteReaderBaseMethods[index].ReturnType == returnType && NetworkSerializationMethodGenerator._allByteReaderBaseMethods[index].GetParameters().Length == 0)
          {
            method10 = NetworkSerializationMethodGenerator._allByteReaderBaseMethods[index];
            break;
          }
        }
        if (method10 != (MethodInfo) null)
        {
          emitReturn = false;
          emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement((Expression) new MethodInvocationExpression((Reference) localReference2, method10, Array.Empty<Expression>())));
        }
        else if (typeof (INetworkSerializable).IsAssignableFrom(returnType))
        {
          emitReturn = false;
          MethodInfo method11 = typeof (INetworkSerializable).GetMethod("Deserialize", new Type[1]
          {
            typeof (IByteReaderBase)
          });
          LocalReference localReference3 = emitter.CodeBuilder.DeclareLocal(returnType);
          if (returnType.GetConstructor(Type.EmptyTypes) == (ConstructorInfo) null)
            emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) localReference3, (Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), NetworkSerializationMethodGenerator._constructUnhandled, new Expression[1]
            {
              (Expression) new TypeTokenExpression(returnType)
            })));
          else
            emitter.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) localReference3, (Expression) new NewInstanceExpression(returnType.GetConstructor(Type.EmptyTypes), Array.Empty<Expression>())));
          emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) localReference3, method11, new Expression[1]
          {
            localReference2.ToExpression()
          })));
          emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement(localReference3.ToExpression()));
        }
        else
        {
          MethodInfo methodInfo = (MethodInfo) null;
          for (int index = 0; index < NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods.Length; ++index)
          {
            if (NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index].ReturnType == returnType)
            {
              ParameterInfo[] parameters2 = NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index].GetParameters();
              if (parameters2.Length == 1 && parameters2[0].ParameterType == typeof (IByteReaderBase))
              {
                method10 = NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index];
                break;
              }
            }
            else if (methodInfo == (MethodInfo) null && NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index].ReturnType != typeof (void) && NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index].IsGenericMethodDefinition)
              methodInfo = NetworkSerializationMethodGenerator._allBaseNetworkTransparentProxyMethods[index];
          }
          if (method10 != (MethodInfo) null)
          {
            emitReturn = false;
            emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement((Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), method10, new Expression[1]
            {
              localReference2.ToExpression()
            })));
          }
          else if (returnType.IsEnum)
          {
            MethodInfo method12 = typeof (IByteReaderBase).GetMethod("ReadInt32");
            emitReturn = false;
            emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement((Expression) new ConvertExpression(returnType, (Expression) new MethodInvocationExpression((Reference) localReference2, method12, Array.Empty<Expression>()))));
          }
          else
          {
            bool flag5 = false;
            if (methodInfo != (MethodInfo) null && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof (IList<>))
            {
              Type genericArgument = returnType.GetGenericArguments()[0];
              if (typeof (INetworkSerializable).IsAssignableFrom(genericArgument))
              {
                MethodInfo method13 = methodInfo.MakeGenericMethod(genericArgument);
                emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement((Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), method13, new Expression[1]
                {
                  localReference2.ToExpression()
                })));
                flag5 = true;
              }
            }
            if (flag5)
              return;
            emitter.CodeBuilder.AddStatement((Statement) new ReturnStatement((Expression) new ConvertExpression(returnType, (Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), NetworkSerializationMethodGenerator._readUnhandled, new Expression[2]
            {
              localReference2.ToExpression(),
              (Expression) new TypeTokenExpression(returnType)
            }))));
          }
        }
      }
      else
      {
        emitReturn = true;
        emitter.CodeBuilder.AddStatement((Statement) new ExpressionStatement((Expression) new MethodInvocationExpression((Reference) @class.GetField("__target"), NetworkSerializationMethodGenerator._sendSimplexPacket, new Expression[1]
        {
          localReference1.ToExpression()
        })));
      }
    }

    private void EmitWriteArray(LocalReference writer, MethodEmitter emitter, Type type)
    {
      int num = type.GetElementType().IsArray ? 1 : 0;
    }

    private void InitOutParameters(MethodEmitter emitter, ParameterInfo[] parameters)
    {
      for (int index = 0; index < parameters.Length; ++index)
      {
        ParameterInfo parameter = parameters[index];
        if (parameter.IsOut)
          emitter.CodeBuilder.AddStatement((Statement) new AssignArgumentStatement(new ArgumentReference(parameter.ParameterType, index + 1), (Expression) new DefaultValueExpression(parameter.ParameterType)));
      }
    }
  }
}
