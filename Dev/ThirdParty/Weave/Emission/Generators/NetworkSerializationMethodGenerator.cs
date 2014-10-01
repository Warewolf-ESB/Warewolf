
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Emission.Emitters;
using System.Emission.Meta;
using System.Linq;
using System.Network;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Generators
{
    internal class NetworkSerializationMethodGenerator : MethodGenerator
    {
        private static readonly ConstructorInfo _createPacket = typeof(Packet).GetConstructor(new Type[] { typeof(PacketTemplate) });
        private static readonly MethodInfo _sendSimplexPacket = typeof(System.Network.__BaseNetworkTransparentProxy).GetMethod("SendSimplexPacket", new Type[] { typeof(Packet) });
        private static readonly MethodInfo _sendDuplexPacket = typeof(System.Network.__BaseNetworkTransparentProxy).GetMethod("SendDuplexPacket", new Type[] { typeof(Packet) });
        private static readonly MethodInfo[] _allByteReaderBaseMethods = typeof(IByteReaderBase).GetMethods();
        private static readonly MethodInfo[] _allBaseNetworkTransparentProxyMethods = typeof(__BaseNetworkTransparentProxy).GetMethods();
        private static readonly MethodInfo _genericWrite = FindGenericWrite();
        private static readonly MethodInfo _writeUnhandled = typeof(__BaseNetworkTransparentProxy).GetMethod("WriteUnhandled");
        private static readonly MethodInfo _readUnhandled = typeof(__BaseNetworkTransparentProxy).GetMethod("ReadUnhandled");
        private static readonly MethodInfo _constructUnhandled = typeof(__BaseNetworkTransparentProxy).GetMethod("ConstructUnhandled");

        private static MethodInfo FindGenericWrite()
        {
            MethodInfo[] allMethods = typeof(__BaseNetworkTransparentProxy).GetMethods();

            for (int i = 0; i < allMethods.Length; i++)
                if (allMethods[i].ReturnType == typeof(void))
                    if (allMethods[i].IsGenericMethodDefinition)
                        if (allMethods[i].GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(IList<>))
                            return allMethods[i];

            return null;
        }


        public NetworkSerializationMethodGenerator(MetaMethod method, OverrideMethodDelegate overrideMethod)
            : base(method, overrideMethod)
        {
        }

        protected override MethodEmitter BuildProxiedMethodBody(MethodEmitter emitter, ClassEmitter @class, EmissionProxyOptions options, IDesignatingScope designatingScope, string dynamicAssemblyName)
        {
            ParameterInfo[] parameters = MethodToOverride.GetParameters();
            InitOutParameters(emitter, parameters);
            bool emitReturn = true;

            if (Method.Source == MetaMethodSource.Method)
            {
                EmitSerialization(emitter, @class, options, parameters, out emitReturn);
            }
            else if ((Method.Source & MetaMethodSource.Property) == MetaMethodSource.Property)
            {

            }
            else if ((Method.Source & MetaMethodSource.Event) == MetaMethodSource.Event)
            {

            }

            if (emitReturn)
            {
                if (emitter.ReturnType == typeof(void))
                    emitter.CodeBuilder.AddStatement(new ReturnStatement());
                else
                    emitter.CodeBuilder.AddStatement(new ReturnStatement(new DefaultValueExpression(emitter.ReturnType)));
            }

            return emitter;
        }

        

        private void EmitSerialization(MethodEmitter emitter, ClassEmitter @class, EmissionProxyOptions options, ParameterInfo[] parameters, out bool emitReturn)
        {
            LocalReference writer = emitter.CodeBuilder.DeclareLocal(typeof(System.Network.Packet));
            emitter.CodeBuilder.AddStatement(new AssignStatement(writer, new NewInstanceExpression(_createPacket, @class.GetField("__template").ToExpression())));
            bool hasOut = false;

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];

                if (!parameter.IsOut)
                {
                    Type pType = parameter.ParameterType;
                    MethodInfo info = typeof(System.Network.Packet).GetMethod("Write", new Type[] { pType });

                    if (info == null)
                    {
                        if (typeof(INetworkSerializable).IsAssignableFrom(pType))
                        {
                            info = typeof(INetworkSerializable).GetMethod("Serialize", new Type[] { typeof(IByteWriterBase) });
                            emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(new ArgumentReference(parameter.ParameterType, index + 1), info, writer.ToExpression())));
                        }
                        else
                        {
                            info = typeof(System.Network.__BaseNetworkTransparentProxy).GetMethod("Write", new Type[] { typeof(IByteWriterBase), pType });
                            if (info != null) emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(@class.GetField("__target"), info, writer.ToExpression(), new ArgumentReference(parameter.ParameterType, index + 1).ToExpression())));
                            else
                            {
                                if (pType.IsEnum)
                                {
                                    info = typeof(System.Network.Packet).GetMethod("Write", new Type[] { typeof(int) });
                                    emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(writer, info, new ConvertExpression(typeof(int), new ArgumentReference(parameter.ParameterType, index + 1).ToExpression()))));
                                }
                                else
                                {
                                    bool success = false;

                                    if (pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(IList<>))
                                    {
                                        Type type = pType.GetGenericArguments()[0];

                                        if (typeof(INetworkSerializable).IsAssignableFrom(type))
                                        {
                                            MethodInfo generic = _genericWrite.MakeGenericMethod(type);
                                            emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(@class.GetField("__target"), generic, writer.ToExpression(), new ArgumentReference(parameter.ParameterType, index + 1).ToExpression())));
                                            success = true;
                                        }
                                        
                                    }

                                    if (!success)
                                    {
                                        emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(@class.GetField("__target"), _writeUnhandled, writer.ToExpression(), new ConvertExpression(typeof(object), new ArgumentReference(parameter.ParameterType, index + 1).ToExpression()))));
                                        
                                    }
                                }
                            }
                        }
                    }
                    else emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(writer, info, new ArgumentReference(parameter.ParameterType, index + 1).ToExpression())));
                }
                else hasOut = true;
            }

            bool hasReturn = emitter.ReturnType != typeof(void);

            if (hasOut || hasReturn)
            {
                emitReturn = true;
                LocalReference reader = emitter.CodeBuilder.DeclareLocal(typeof(System.IByteReaderBase));
                emitter.CodeBuilder.AddStatement(new AssignStatement(reader, new MethodInvocationExpression(@class.GetField("__target"), _sendDuplexPacket, writer.ToExpression())));

                if (hasOut)
                {
                    for (int index = 0; index < parameters.Length; index++)
                    {
                        ParameterInfo parameter = parameters[index];

                        if (parameter.IsOut)
                        {
                            Type pType = parameter.ParameterType.GetElementType();
                            MethodInfo info = null;
                            
                            for (int i = 0; i < _allByteReaderBaseMethods.Length; i++)
                                if (_allByteReaderBaseMethods[i].ReturnType == pType)
                                {
                                    ParameterInfo[] baseParams = _allByteReaderBaseMethods[i].GetParameters();

                                    if (baseParams.Length == 0)
                                    {
                                        info = _allByteReaderBaseMethods[i];
                                        break;
                                    }
                                }

                            if (info != null)
                            {
                                emitter.CodeBuilder.AddStatement(new AssignStatement(new ArgumentReference(parameter.ParameterType, index + 1), new MethodInvocationExpression(reader, info)));
                            }
                            else
                            {
                                if (typeof(INetworkSerializable).IsAssignableFrom(pType))
                                {
                                    info = typeof(INetworkSerializable).GetMethod("Deserialize", new Type[] { typeof(IByteReaderBase) });

                                    ConstructorInfo ctor = pType.GetConstructor(Type.EmptyTypes);

                                    if (ctor == null) emitter.CodeBuilder.AddStatement(new AssignStatement(new ArgumentReference(parameter.ParameterType, index + 1), new MethodInvocationExpression(@class.GetField("__target"), _constructUnhandled, new TypeTokenExpression(pType))));
                                    else emitter.CodeBuilder.AddStatement(new AssignStatement(new ArgumentReference(parameter.ParameterType, index + 1), new NewInstanceExpression(pType.GetConstructor(Type.EmptyTypes))));
                                    
                                    emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(new ArgumentReference(parameter.ParameterType, index + 1), info, reader.ToExpression())));
                                }
                                else
                                {
                                    MethodInfo generic = null;

                                    for (int i = 0; i < _allBaseNetworkTransparentProxyMethods.Length; i++)
                                        if (_allBaseNetworkTransparentProxyMethods[i].ReturnType == pType)
                                        {
                                            ParameterInfo[] baseParams = _allBaseNetworkTransparentProxyMethods[i].GetParameters();

                                            if (baseParams.Length == 1 && baseParams[0].ParameterType == typeof(IByteReaderBase))
                                            {
                                                info = _allBaseNetworkTransparentProxyMethods[i];
                                                break;
                                            }
                                        }
                                        else if (generic == null && _allBaseNetworkTransparentProxyMethods[i].ReturnType != typeof(void) && _allBaseNetworkTransparentProxyMethods[i].IsGenericMethodDefinition)
                                            generic = _allBaseNetworkTransparentProxyMethods[i];


                                    if (info != null)
                                    {
                                        emitter.CodeBuilder.AddStatement(new AssignStatement(new ArgumentReference(parameter.ParameterType, index + 1), new MethodInvocationExpression(@class.GetField("__target"), info, reader.ToExpression())));
                                    }
                                    else
                                    {
                                        if (pType.IsEnum)
                                        {
                                            info = typeof(IByteReaderBase).GetMethod("ReadInt32");
                                            emitter.CodeBuilder.AddStatement(new AssignStatement(new ArgumentReference(parameter.ParameterType, index + 1), new ConvertExpression(pType, new MethodInvocationExpression(reader, info))));
                                        }
                                        else
                                        {
                                            bool success = false;

                                            if (generic != null && pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(IList<>))
                                            {
                                                Type type = pType.GetGenericArguments()[0];

                                                if (typeof(INetworkSerializable).IsAssignableFrom(type))
                                                {
                                                    generic = generic.MakeGenericMethod(type);
                                                    emitter.CodeBuilder.AddStatement(new AssignStatement(new ArgumentReference(parameter.ParameterType, index + 1), new MethodInvocationExpression(@class.GetField("__target"), generic, reader.ToExpression())));
                                                    success = true;
                                                }
                                                
                                            }

                                            if (!success)
                                            {
                                                emitter.CodeBuilder.AddStatement(new AssignStatement(new ArgumentReference(parameter.ParameterType, index + 1), new ConvertExpression(pType, new MethodInvocationExpression(@class.GetField("__target"), _readUnhandled, reader.ToExpression(), new TypeTokenExpression(pType)))));
                                                
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (hasReturn)
                {
                    Type pType = emitter.ReturnType;
                    MethodInfo info = null;

                    for (int i = 0; i < _allByteReaderBaseMethods.Length; i++)
                        if (_allByteReaderBaseMethods[i].ReturnType == pType)
                        {
                            ParameterInfo[] baseParams = _allByteReaderBaseMethods[i].GetParameters();

                            if (baseParams.Length == 0)
                            {
                                info = _allByteReaderBaseMethods[i];
                                break;
                            }
                        }

                    if (info != null)
                    {
                        emitReturn = false;
                        emitter.CodeBuilder.AddStatement(new ReturnStatement(new MethodInvocationExpression(reader, info)));
                    }
                    else
                    {
                        if (typeof(INetworkSerializable).IsAssignableFrom(pType))
                        {
                            emitReturn = false;

                            info = typeof(INetworkSerializable).GetMethod("Deserialize", new Type[] { typeof(IByteReaderBase) });

                            LocalReference result = emitter.CodeBuilder.DeclareLocal(pType);

                            ConstructorInfo ctor = pType.GetConstructor(Type.EmptyTypes);

                            if (ctor == null) emitter.CodeBuilder.AddStatement(new AssignStatement(result, new MethodInvocationExpression(@class.GetField("__target"), _constructUnhandled, new TypeTokenExpression(pType))));
                            else emitter.CodeBuilder.AddStatement(new AssignStatement(result, new NewInstanceExpression(pType.GetConstructor(Type.EmptyTypes))));

                            emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(result, info, reader.ToExpression())));
                            emitter.CodeBuilder.AddStatement(new ReturnStatement(result.ToExpression()));
                        }
                        else
                        {
                            MethodInfo generic = null;

                            for (int i = 0; i < _allBaseNetworkTransparentProxyMethods.Length; i++)
                                if (_allBaseNetworkTransparentProxyMethods[i].ReturnType == pType)
                                {
                                    ParameterInfo[] baseParams = _allBaseNetworkTransparentProxyMethods[i].GetParameters();

                                    if (baseParams.Length == 1 && baseParams[0].ParameterType == typeof(IByteReaderBase))
                                    {
                                        info = _allBaseNetworkTransparentProxyMethods[i];
                                        break;
                                    }
                                }
                                else if (generic == null && _allBaseNetworkTransparentProxyMethods[i].ReturnType != typeof(void) && _allBaseNetworkTransparentProxyMethods[i].IsGenericMethodDefinition)
                                    generic = _allBaseNetworkTransparentProxyMethods[i];

                            if (info != null)
                            {
                                emitReturn = false;
                                emitter.CodeBuilder.AddStatement(new ReturnStatement(new MethodInvocationExpression(@class.GetField("__target"), info, reader.ToExpression())));
                            }
                            else
                            {
                                if (pType.IsEnum)
                                {
                                    info = typeof(IByteReaderBase).GetMethod("ReadInt32");
                                    emitReturn = false;
                                    emitter.CodeBuilder.AddStatement(new ReturnStatement(new ConvertExpression(pType, new MethodInvocationExpression(reader, info))));
                                }
                                else
                                {
                                    bool success = false;

                                    if (generic != null && pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(IList<>))
                                    {
                                        Type type = pType.GetGenericArguments()[0];

                                        if (typeof(INetworkSerializable).IsAssignableFrom(type))
                                        {
                                            generic = generic.MakeGenericMethod(type);
                                            emitter.CodeBuilder.AddStatement(new ReturnStatement(new MethodInvocationExpression(@class.GetField("__target"), generic, reader.ToExpression())));
                                            success = true;
                                        }
                                        
                                    }

                                    if (!success)
                                    {
                                        emitter.CodeBuilder.AddStatement(new ReturnStatement(new ConvertExpression(pType, new MethodInvocationExpression(@class.GetField("__target"), _readUnhandled, reader.ToExpression(), new TypeTokenExpression(pType)))));
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                emitReturn = true;
                emitter.CodeBuilder.AddStatement(new ExpressionStatement(new MethodInvocationExpression(@class.GetField("__target"), _sendSimplexPacket, writer.ToExpression())));
            }
        }

        private void EmitWriteArray(LocalReference writer, MethodEmitter emitter, Type type)
        {
            Type elementType = type.GetElementType();

            if (elementType.IsArray)
            {
            }
            else
            {

            }
        }

        private void InitOutParameters(MethodEmitter emitter, ParameterInfo[] parameters)
        {
            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];

                if (parameter.IsOut)
                {
                    emitter.CodeBuilder.AddStatement(
                        new AssignArgumentStatement(new ArgumentReference(parameter.ParameterType, index + 1),
                                                    new DefaultValueExpression(parameter.ParameterType)));
                }
            }
        }
    }
}
