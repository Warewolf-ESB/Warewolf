
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission
{
    internal static class TypeUtil
    {
        public static bool IsGetType(this MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("GetType", methodInfo.Name, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsFinalizer(this MethodInfo methodInfo)
        {
            return string.Equals("Finalize", methodInfo.Name) && methodInfo.GetBaseDefinition().DeclaringType == typeof(object);
        }

        public static bool IsMemberwiseClone(this MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(object) && string.Equals("MemberwiseClone", methodInfo.Name, StringComparison.OrdinalIgnoreCase);
        }

        public static ICollection<Type> GetAllInterfaces(params Type[] types)
        {
            if (types == null) return Type.EmptyTypes;
            object dummy = new object();
            IDictionary<Type, object> interfaces = new Dictionary<Type, object>();

            foreach (Type type in types)
            {
                if (type == null) continue;

                if (type.IsInterface) interfaces[type] = dummy;
                foreach (Type @interface in type.GetInterfaces()) interfaces[@interface] = dummy;
            }

            return Sort(interfaces.Keys);
        }

        public static ICollection<Type> GetAllInterfaces(this Type type)
        {
            return GetAllInterfaces(new[] { type });
        }

        public static void SetStaticField(this Type type, string fieldName, BindingFlags additionalFlags, object value)
        {
            BindingFlags flags = additionalFlags | BindingFlags.Static | BindingFlags.SetField;

            try
            {
                type.InvokeMember(fieldName, flags, null, null, new[] { value });
            }
            catch (MissingFieldException e)
            {
                throw new InvalidOperationException(string.Format("Could not find field named '{0}' on type {1}. This is likely a bug in Weave. Please report it.",fieldName,type), e);
            }
            catch (TargetException e)
            {
                throw new InvalidOperationException(string.Format("There was an error trying to set field named '{0}' on type {1}. This is likely a bug in Weave. Please report it.", fieldName, type), e);
            }
            catch (TargetInvocationException e)
            {
                if ((e.InnerException is TypeInitializationException) == false) throw;
                throw new InvalidOperationException(string.Format("There was an error in static constructor on type {0}. This is likely a bug in Weave. Please report it.", type), e);
            }
        }

        public static MemberInfo[] Sort(MemberInfo[] members)
        {
            MemberInfo[] sortedMembers = new MemberInfo[members.Length];
            Array.Copy(members, sortedMembers, members.Length);
            Array.Sort(sortedMembers, (l, r) => string.Compare(l.Name, r.Name, StringComparison.OrdinalIgnoreCase));
            return sortedMembers;
        }

        private static Type[] Sort(IEnumerable<Type> types)
        {
            Type[] array = types.ToArray();
            Array.Sort(array, (l, r) => string.Compare(l.AssemblyQualifiedName, r.AssemblyQualifiedName, StringComparison.OrdinalIgnoreCase));
            return array;
        }

    }

    internal abstract class ArgumentsUtil
    {
        public static Expression[] ConvertArgumentReferenceToExpression(ArgumentReference[] args)
        {
            var expressions = new Expression[args.Length];

            for (var i = 0; i < args.Length; ++i)
            {
                expressions[i] = args[i].ToExpression();
            }

            return expressions;
        }

        public static ArgumentReference[] ConvertToArgumentReference(Type[] args)
        {
            var arguments = new ArgumentReference[args.Length];

            for (var i = 0; i < args.Length; ++i)
            {
                arguments[i] = new ArgumentReference(args[i]);
            }

            return arguments;
        }

        public static ArgumentReference[] ConvertToArgumentReference(ParameterInfo[] args)
        {
            var arguments = new ArgumentReference[args.Length];

            for (var i = 0; i < args.Length; ++i)
            {
                arguments[i] = new ArgumentReference(args[i].ParameterType);
            }

            return arguments;
        }

        public static ReferenceExpression[] ConvertToArgumentReferenceExpression(ParameterInfo[] args)
        {
            var arguments = new ReferenceExpression[args.Length];

            for (var i = 0; i < args.Length; ++i)
            {
                arguments[i] = new ReferenceExpression(new ArgumentReference(args[i].ParameterType, i + 1));
            }

            return arguments;
        }

        public static void EmitLoadOwnerAndReference(Reference reference, ILGenerator il)
        {
            if (reference == null)
            {
                return;
            }

            EmitLoadOwnerAndReference(reference.OwnerReference, il);

            reference.LoadReference(il);
        }

        public static Type[] GetTypes(ParameterInfo[] parameters)
        {
            var types = new Type[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].ParameterType;
            }
            return types;
        }

        public static Type[] InitializeAndConvert(ArgumentReference[] args)
        {
            var types = new Type[args.Length];

            for (var i = 0; i < args.Length; ++i)
            {
                args[i].Position = i + 1;
                types[i] = args[i].Type;
            }

            return types;
        }

        public static void InitializeArgumentsByPosition(ArgumentReference[] args, bool isStatic)
        {
            var offset = isStatic ? 0 : 1;
            for (var i = 0; i < args.Length; ++i)
            {
                args[i].Position = i + offset;
            }
        }

        public static bool IsAnyByRef(ParameterInfo[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    return true;
                }
            }
            return false;
        }
    }

    internal sealed class StindOpCodesDictionary : Dictionary<Type, OpCode>
    {
        private static readonly StindOpCodesDictionary dict = new StindOpCodesDictionary();
        private static readonly OpCode emptyOpCode = new OpCode();

        private StindOpCodesDictionary()
        {
            Add(typeof(bool), OpCodes.Stind_I1);
            Add(typeof(char), OpCodes.Stind_I2);
            Add(typeof(SByte), OpCodes.Stind_I1);
            Add(typeof(Int16), OpCodes.Stind_I2);
            Add(typeof(Int32), OpCodes.Stind_I4);
            Add(typeof(Int64), OpCodes.Stind_I8);
            Add(typeof(float), OpCodes.Stind_R4);
            Add(typeof(double), OpCodes.Stind_R8);
            Add(typeof(byte), OpCodes.Stind_I1);
            Add(typeof(UInt16), OpCodes.Stind_I2);
            Add(typeof(UInt32), OpCodes.Stind_I4);
            Add(typeof(UInt64), OpCodes.Stind_I8);
        }

        public new OpCode this[Type type]
        {
            get
            {
                if (ContainsKey(type))
                {
                    return base[type];
                }
                return EmptyOpCode;
            }
        }

        public static OpCode EmptyOpCode
        {
            get { return emptyOpCode; }
        }

        public static StindOpCodesDictionary Instance
        {
            get { return dict; }
        }
    }

    internal sealed class LdindOpCodesDictionary : Dictionary<Type, OpCode>
    {
        private static readonly LdindOpCodesDictionary dict = new LdindOpCodesDictionary();

        // has to be assigned explicitly to suppress compiler warning
        private static readonly OpCode emptyOpCode = new OpCode();

        private LdindOpCodesDictionary()
        {
            Add(typeof(bool), OpCodes.Ldind_I1);
            Add(typeof(char), OpCodes.Ldind_I2);
            Add(typeof(SByte), OpCodes.Ldind_I1);
            Add(typeof(Int16), OpCodes.Ldind_I2);
            Add(typeof(Int32), OpCodes.Ldind_I4);
            Add(typeof(Int64), OpCodes.Ldind_I8);
            Add(typeof(float), OpCodes.Ldind_R4);
            Add(typeof(double), OpCodes.Ldind_R8);
            Add(typeof(byte), OpCodes.Ldind_U1);
            Add(typeof(UInt16), OpCodes.Ldind_U2);
            Add(typeof(UInt32), OpCodes.Ldind_U4);
            Add(typeof(UInt64), OpCodes.Ldind_I8);
        }

        public new OpCode this[Type type]
        {
            get
            {
                if (ContainsKey(type))
                {
                    return base[type];
                }
                return EmptyOpCode;
            }
        }

        public static OpCode EmptyOpCode
        {
            get { return emptyOpCode; }
        }

        public static LdindOpCodesDictionary Instance
        {
            get { return dict; }
        }
    }

    sealed class LdcOpCodesDictionary : Dictionary<Type, OpCode>
    {
        private static readonly LdcOpCodesDictionary dict = new LdcOpCodesDictionary();

        // has to be assigned explicitly to suppress compiler warning
        private static readonly OpCode emptyOpCode = new OpCode();

        private LdcOpCodesDictionary()
        {
            Add(typeof(bool), OpCodes.Ldc_I4);
            Add(typeof(char), OpCodes.Ldc_I4);
            Add(typeof(SByte), OpCodes.Ldc_I4);
            Add(typeof(Int16), OpCodes.Ldc_I4);
            Add(typeof(Int32), OpCodes.Ldc_I4);
            Add(typeof(Int64), OpCodes.Ldc_I8);
            Add(typeof(float), OpCodes.Ldc_R4);
            Add(typeof(double), OpCodes.Ldc_R8);
            Add(typeof(byte), OpCodes.Ldc_I4_0);
            Add(typeof(UInt16), OpCodes.Ldc_I4_0);
            Add(typeof(UInt32), OpCodes.Ldc_I4_0);
            Add(typeof(UInt64), OpCodes.Ldc_I4_0);
        }

        public new OpCode this[Type type]
        {
            get
            {
                if (ContainsKey(type))
                {
                    return base[type];
                }
                return EmptyOpCode;
            }
        }

        public static OpCode EmptyOpCode
        {
            get { return emptyOpCode; }
        }

        public static LdcOpCodesDictionary Instance
        {
            get { return dict; }
        }
    }

    internal abstract class OpCodeUtil
    {
        /// <summary>
        ///   Emits a load indirect opcode of the appropriate type for a value or object reference.
        ///   Pops a pointer off the evaluation stack, dereferences it and loads
        ///   a value of the specified type.
        /// </summary>
        /// <param name = "gen"></param>
        /// <param name = "type"></param>
        public static void EmitLoadIndirectOpCodeForType(ILGenerator gen, Type type)
        {
            if (type.IsEnum)
            {
                EmitLoadIndirectOpCodeForType(gen, GetUnderlyingTypeOfEnum(type));
                return;
            }

            if (type.IsByRef)
            {
                throw new NotSupportedException("Cannot load ByRef values");
            }
            else if (type.IsPrimitive && type != typeof(IntPtr))
            {
                var opCode = LdindOpCodesDictionary.Instance[type];

                if (opCode == LdindOpCodesDictionary.EmptyOpCode)
                {
                    throw new ArgumentException("Type " + type + " could not be converted to a OpCode");
                }

                gen.Emit(opCode);
            }
            else if (type.IsValueType)
            {
                gen.Emit(OpCodes.Ldobj, type);
            }
            else if (type.IsGenericParameter)
            {
                gen.Emit(OpCodes.Ldobj, type);
            }
            else
            {
                gen.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        ///   Emits a load opcode of the appropriate kind for a constant string or
        ///   primitive value.
        /// </summary>
        /// <param name = "gen"></param>
        /// <param name = "value"></param>
        public static void EmitLoadOpCodeForConstantValue(ILGenerator gen, object value)
        {
            if (value is String)
            {
                gen.Emit(OpCodes.Ldstr, value.ToString());
            }
            else if (value is Int32)
            {
                var code = LdcOpCodesDictionary.Instance[value.GetType()];
                gen.Emit(code, (int)value);
            }
            else if (value is bool)
            {
                var code = LdcOpCodesDictionary.Instance[value.GetType()];
                gen.Emit(code, Convert.ToInt32(value));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        ///   Emits a load opcode of the appropriate kind for the constant default value of a
        ///   type, such as 0 for value types and null for reference types.
        /// </summary>
        public static void EmitLoadOpCodeForDefaultValueOfType(ILGenerator gen, Type type)
        {
            if (type.IsPrimitive)
            {
                var opCode = LdcOpCodesDictionary.Instance[type];
                switch (opCode.StackBehaviourPush)
                {
                    case StackBehaviour.Pushi:
                        gen.Emit(opCode, 0);
                        if (Is64BitTypeLoadedAsInt32(type))
                        {
                            // we load Int32, and have to convert it to 64bit type
                            gen.Emit(OpCodes.Conv_I8);
                        }
                        break;
                    case StackBehaviour.Pushr8:
                        gen.Emit(opCode, 0D);
                        break;
                    case StackBehaviour.Pushi8:
                        gen.Emit(opCode, 0L);
                        break;
                    case StackBehaviour.Pushr4:
                        gen.Emit(opCode, 0F);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                gen.Emit(OpCodes.Ldnull);
            }
        }

        /// <summary>
        ///   Emits a store indirectopcode of the appropriate type for a value or object reference.
        ///   Pops a value of the specified type and a pointer off the evaluation stack, and
        ///   stores the value.
        /// </summary>
        /// <param name = "gen"></param>
        /// <param name = "type"></param>
        public static void EmitStoreIndirectOpCodeForType(ILGenerator gen, Type type)
        {
            if (type.IsEnum)
            {
                EmitStoreIndirectOpCodeForType(gen, GetUnderlyingTypeOfEnum(type));
                return;
            }

            if (type.IsByRef)
            {
                throw new NotSupportedException("Cannot store ByRef values");
            }
            else if (type.IsPrimitive && type != typeof(IntPtr))
            {
                var opCode = StindOpCodesDictionary.Instance[type];

                if (Equals(opCode, StindOpCodesDictionary.EmptyOpCode))
                {
                    throw new ArgumentException("Type " + type + " could not be converted to a OpCode");
                }

                gen.Emit(opCode);
            }
            else if (type.IsValueType)
            {
                gen.Emit(OpCodes.Stobj, type);
            }
            else if (type.IsGenericParameter)
            {
                gen.Emit(OpCodes.Stobj, type);
            }
            else
            {
                gen.Emit(OpCodes.Stind_Ref);
            }
        }

        private static Type GetUnderlyingTypeOfEnum(Type enumType)
        {
            var baseType = (Enum)Activator.CreateInstance(enumType);
            var code = baseType.GetTypeCode();

            switch (code)
            {
                case TypeCode.SByte:
                    return typeof(SByte);
                case TypeCode.Byte:
                    return typeof(Byte);
                case TypeCode.Int16:
                    return typeof(Int16);
                case TypeCode.Int32:
                    return typeof(Int32);
                case TypeCode.Int64:
                    return typeof(Int64);
                case TypeCode.UInt16:
                    return typeof(UInt16);
                case TypeCode.UInt32:
                    return typeof(UInt32);
                case TypeCode.UInt64:
                    return typeof(UInt64);
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool Is64BitTypeLoadedAsInt32(Type type)
        {
            return type == typeof(long) || type == typeof(ulong);
        }
    }

    internal delegate GenericTypeParameterBuilder[] ApplyGenArgs(String[] argumentNames);

    internal class GenericUtil
    {
        public static GenericTypeParameterBuilder[] CopyGenericArguments(
            MethodInfo methodToCopyGenericsFrom,
            TypeBuilder builder,
            Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
        {
            return
                CopyGenericArguments(methodToCopyGenericsFrom, name2GenericType,
                                     builder.DefineGenericParameters);
        }

        public static GenericTypeParameterBuilder[] CopyGenericArguments(
            MethodInfo methodToCopyGenericsFrom,
            MethodBuilder builder,
            Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
        {
            return
                CopyGenericArguments(methodToCopyGenericsFrom, name2GenericType,
                                     builder.DefineGenericParameters);
        }

        public static Type ExtractCorrectType(Type paramType, Dictionary<string, GenericTypeParameterBuilder> name2GenericType)
        {
            if (paramType.IsArray)
            {
                var rank = paramType.GetArrayRank();

                var underlyingType = paramType.GetElementType();

                if (underlyingType.IsGenericParameter)
                {
                    GenericTypeParameterBuilder genericType;
                    if (name2GenericType.TryGetValue(underlyingType.Name, out genericType) == false)
                    {
                        return paramType;
                    }

                    if (rank == 1)
                    {
                        return genericType.MakeArrayType();
                    }
                    return genericType.MakeArrayType(rank);
                }
                if (rank == 1)
                {
                    return underlyingType.MakeArrayType();
                }
                return underlyingType.MakeArrayType(rank);
            }

            if (paramType.IsGenericParameter)
            {
                GenericTypeParameterBuilder value;
                if (name2GenericType.TryGetValue(paramType.Name, out value))
                {
                    return value;
                }
            }

            return paramType;
        }

        public static Type[] ExtractParametersTypes(ParameterInfo[] baseMethodParameters, Dictionary<String, GenericTypeParameterBuilder> name2GenericType)
        {
            Type[] newParameters = new Type[baseMethodParameters.Length];

            for (int i = 0; i < baseMethodParameters.Length; i++)
            {
                ParameterInfo param = baseMethodParameters[i];
                Type paramType = param.ParameterType;
                newParameters[i] = ExtractCorrectType(paramType, name2GenericType);
            }

            return newParameters;
        }

        public static Dictionary<string, GenericTypeParameterBuilder> GetGenericArgumentsMap(AbstractTypeEmitter parentEmitter)
        {
            if (parentEmitter.GenericTypeParams == null || parentEmitter.GenericTypeParams.Length == 0) return new Dictionary<string, GenericTypeParameterBuilder>(0);
            Dictionary<string, GenericTypeParameterBuilder> name2GenericType = new Dictionary<string, GenericTypeParameterBuilder>(parentEmitter.GenericTypeParams.Length);

            foreach (GenericTypeParameterBuilder genType in parentEmitter.GenericTypeParams)
                name2GenericType.Add(genType.Name, genType);

            return name2GenericType;
        }

        private static Type AdjustConstraintToNewGenericParameters(Type constraint, MethodInfo methodToCopyGenericsFrom, Type[] originalGenericParameters, GenericTypeParameterBuilder[] newGenericParameters)
        {
            if (constraint.IsGenericType)
            {
                Type[] genericArgumentsOfConstraint = constraint.GetGenericArguments();

                for (int i = 0; i < genericArgumentsOfConstraint.Length; ++i)
                    genericArgumentsOfConstraint[i] = AdjustConstraintToNewGenericParameters(genericArgumentsOfConstraint[i], methodToCopyGenericsFrom, originalGenericParameters, newGenericParameters);

                return constraint.GetGenericTypeDefinition().MakeGenericType(genericArgumentsOfConstraint);
            }
            else if (constraint.IsGenericParameter)
            {
                if (constraint.DeclaringMethod != null)
                {
                    int index = Array.IndexOf(originalGenericParameters, constraint);
                    return newGenericParameters[index];
                }
                else
                {
                    int index = Array.IndexOf(constraint.DeclaringType.GetGenericArguments(), constraint);
                    return methodToCopyGenericsFrom.DeclaringType.GetGenericArguments()[index];
                }
            }
            else
            {
                return constraint;
            }
        }

        private static Type[] AdjustGenericConstraints(MethodInfo methodToCopyGenericsFrom, GenericTypeParameterBuilder[] newGenericParameters, Type[] originalGenericArguments, Type[] constraints)
        {
            for (int i = 0; i < constraints.Length; i++)
                constraints[i] = AdjustConstraintToNewGenericParameters(constraints[i], methodToCopyGenericsFrom, originalGenericArguments, newGenericParameters);

            return constraints;
        }

        private static GenericTypeParameterBuilder[] CopyGenericArguments(MethodInfo methodToCopyGenericsFrom, Dictionary<String, GenericTypeParameterBuilder> name2GenericType, ApplyGenArgs genericParameterGenerator)
        {
            Type[] originalGenericArguments = methodToCopyGenericsFrom.GetGenericArguments();
            if (originalGenericArguments.Length == 0) return null;

            string[] argumentNames = GetArgumentNames(originalGenericArguments);
            GenericTypeParameterBuilder[] newGenericParameters = genericParameterGenerator(argumentNames);

            for (int i = 0; i < newGenericParameters.Length; i++)
            {
                try
                {
                    var attributes = originalGenericArguments[i].GenericParameterAttributes;
                    newGenericParameters[i].SetGenericParameterAttributes(attributes);
                    var constraints = AdjustGenericConstraints(methodToCopyGenericsFrom, newGenericParameters, originalGenericArguments, originalGenericArguments[i].GetGenericParameterConstraints());

                    newGenericParameters[i].SetInterfaceConstraints(constraints);
                    CopyNonInheritableAttributes(newGenericParameters[i], originalGenericArguments[i]);
                }
                catch (NotSupportedException)
                {
                    // Doesnt matter
                    newGenericParameters[i].SetGenericParameterAttributes(GenericParameterAttributes.None);
                }

                name2GenericType[argumentNames[i]] = newGenericParameters[i];
            }

            return newGenericParameters;
        }

        private static void CopyNonInheritableAttributes(GenericTypeParameterBuilder newGenericParameter, Type originalGenericArgument)
        {
            foreach (CustomAttributeBuilder attribute in originalGenericArgument.GetNonInheritableAttributes())
                newGenericParameter.SetCustomAttribute(attribute);
        }

        private static string[] GetArgumentNames(Type[] originalGenericArguments)
        {
            string[] argumentNames = new string[originalGenericArguments.Length];
            for (int i = 0; i < argumentNames.Length; i++) argumentNames[i] = originalGenericArguments[i].Name;
            return argumentNames;
        }
    }
}
