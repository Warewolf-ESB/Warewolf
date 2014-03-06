using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
using System.ComponentModel;

namespace Infragistics
{
	#region GetSetProxy
	internal class GetSetProxy<TOwner, TProperty> 
		: IEquatable<GetSetProxy<TOwner, TProperty>>
	{
		public readonly Func<TOwner, TProperty> Getter;
		public readonly Action<TOwner, TProperty> Setter;
		internal readonly string Name;
		internal readonly string DisplayName;

		public GetSetProxy(Func<TOwner, TProperty> getter, Action<TOwner, TProperty> setter, string name, string displayName)
		{
			this.Getter = getter;
			this.Setter = setter;
			this.Name = name;
			this.DisplayName = displayName;
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as GetSetProxy<TOwner, TProperty>);
		}

		public bool Equals(GetSetProxy<TOwner, TProperty> other)
		{
			return other != null &&
				other.Getter == this.Getter &&
				other.Setter == this.Setter &&
				other.Name == this.Name &&
				other.DisplayName == this.DisplayName;
		}

		public override int GetHashCode()
		{
			int code = 0;

			if (Getter != null)
				code ^= Getter.GetHashCode();

			if (Setter != null)
				code ^= Setter.GetHashCode();

			if (Name != null)
				code ^= Name.GetHashCode();

			if (DisplayName != null)
				code ^= DisplayName.GetHashCode();

			return code;
		}
	}
	#endregion //GetSetProxy

	internal static partial class GetSetHelper
	{
		#region Member Variables

		private static Dictionary<MemberInfo, object> CachedByMemberInfo = new Dictionary<MemberInfo, object>();
		private static Dictionary<MemberNameKey, object> CachedByMemberName = new Dictionary<MemberNameKey, object>();
		private static MethodInfo _displayAttributeGetNameMethod = null;
		private static bool _skipGetDisplayName;

		#endregion //Member Variables

		#region Public Methods

		#region Get
		public static GetSetProxy<TOwner, TProperty> Get<TOwner, TProperty>(Expression<Func<TProperty>> getter)
			where TOwner : class
		{
			var memberExpression = getter.Body as MemberExpression;

			if (memberExpression == null)
				throw new ArgumentException(SR.GetString("LE_InvalidMemberExpression"), "getter");

			var member = memberExpression.Member;

			if (!member.DeclaringType.IsAssignableFrom(typeof(TOwner)))
				throw new ArgumentException(SR.GetString("LE_InvalidMemberForTarget", member.Name, typeof(TOwner).FullName), "getter");

			return Get<TOwner, TProperty>(member);
		}

		public static GetSetProxy<TOwner, TProperty> Get<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getter)
			where TOwner : class
		{
			var memberExpression = getter.Body as MemberExpression;

			if (memberExpression == null)
				throw new ArgumentException(SR.GetString("LE_InvalidMemberExpression"), "getter");

			return Get<TOwner, TProperty>(memberExpression.Member);
		}

		public static GetSetProxy<TOwner, TProperty> Get<TOwner, TProperty>(string memberName)
			where TOwner : class
		{
			CoreUtilities.ValidateNotNull(memberName, "memberName");

			var key = new MemberNameKey { OwnerType = typeof(TOwner), MemberName = memberName };
			object helper;

			if (!CachedByMemberName.TryGetValue(key, out helper))
			{
				var member = GetMember<TOwner>(memberName);

				if (member == null)
					throw new ArgumentException(SR.GetString("LE_InvalidMemberForTarget", memberName, typeof(TOwner).FullName), "memberName");

				CachedByMemberName[key] = helper = Get<TOwner, TProperty>(member);
			}

			return (GetSetProxy<TOwner, TProperty>)helper;
		}

		public static GetSetProxy<TOwner, TProperty> Get<TOwner, TProperty>(MemberInfo member)
			where TOwner : class
		{
			object helper;

			if (!CachedByMemberInfo.TryGetValue(member, out helper))
			{
				CachedByMemberInfo[member] = helper = Create<TOwner, TProperty>(member);
			}

			return (GetSetProxy<TOwner, TProperty>)helper;
		}
		#endregion //Get

		#endregion //Public Methods

		#region Private Methods

		#region Create
		private static GetSetProxy<TOwner, TProperty> Create<TOwner, TProperty>(MemberInfo memberInfo)
		{
			var propertyInfo = memberInfo as PropertyInfo;
			var instance = Expression.Parameter(memberInfo.DeclaringType, "i");

			if (null != propertyInfo)
			{
				var expression = Expression.Property(instance, propertyInfo);
				return Create<TOwner, TProperty>(expression, propertyInfo.CanRead, propertyInfo.CanWrite);
			}

			var fieldInfo = memberInfo as FieldInfo;

			if (null != fieldInfo)
			{
				var expression = Expression.Field(instance, fieldInfo);
				return Create<TOwner, TProperty>(expression, true, !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral);
			}

			throw new ArgumentException(SR.GetString("LE_InvalidMemberForTarget", memberInfo.Name, typeof(TOwner).FullName));
		}

		private static GetSetProxy<TOwner, TProperty> Create<TOwner, TProperty>(MemberExpression memberExpression, bool includeGetter, bool includeSetter)
		{
			CoreUtilities.ValidateNotNull(memberExpression, "memberExpression");

			Debug.Assert(memberExpression.Expression is ParameterExpression, "Expected this to be the instance");

			var tPropertyType = typeof(TProperty);
			var value = Expression.Parameter(tPropertyType, "v");
			var instance = memberExpression.Expression as ParameterExpression;
			var assignExpression = value as Expression;
			var getExpression = memberExpression as Expression;

			// support simple casting
			if (memberExpression.Type != tPropertyType && tPropertyType.IsAssignableFrom(memberExpression.Type))
			{
				assignExpression = Expression.Convert(value, memberExpression.Type);
				getExpression = Expression.Convert(getExpression, tPropertyType);
			}

			var setter = !includeSetter ? null : Expression.Lambda<Action<TOwner, TProperty>>(
				Expression.Assign(memberExpression, assignExpression),
				instance,
				value).Compile();

			var getter = !includeGetter ? null : Expression.Lambda<Func<TOwner, TProperty>>(
				getExpression,
				instance).Compile();

			var member = memberExpression.Member;

			var helper = new GetSetProxy<TOwner, TProperty>(getter, setter, member.Name, GetDisplayName(member));
			return helper;
		}
		#endregion //Create

		#region GetDisplayName
		private static string GetDisplayName(MemberInfo member)
		{
			try
			{
				if (!_skipGetDisplayName)
				{
					if (_displayAttributeGetNameMethod != null)
					{
						var displayAttribs = member.GetCustomAttributes(_displayAttributeGetNameMethod.DeclaringType, true);

						if (null != displayAttribs && displayAttribs.Length > 0)
							return _displayAttributeGetNameMethod.Invoke(displayAttribs[0], Type.EmptyTypes) as string;
					}
					else
					{
						var allAttribs = member.GetCustomAttributes(true);

						if (null != allAttribs && allAttribs.Length > 0)
						{
							foreach (object attrib in allAttribs)
							{
								if (attrib.GetType().FullName != "System.ComponentModel.DataAnnotations.DisplayAttribute")
									continue;

								try
								{
									_displayAttributeGetNameMethod = attrib.GetType().GetMethod("GetName");
									return _displayAttributeGetNameMethod.Invoke(attrib, Type.EmptyTypes) as string;
								}
								catch
								{
									_skipGetDisplayName = true;
								}
							}
						}
					}
				}


				var displayNameAttribs = member.GetCustomAttributes(typeof(DisplayNameAttribute), true);

				if (null != displayNameAttribs && displayNameAttribs.Length > 0)
					return ((DisplayNameAttribute)displayNameAttribs[0]).DisplayName;

			}
			catch
			{
			}

			return null;
		} 
		#endregion //GetDisplayName

		#region GetMember
		private static MemberInfo GetMember<TOwner>(string memberName)
		{
			var ownerType = typeof(TOwner);
			const BindingFlags BaseFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.IgnoreCase;
			MemberInfo memberInfo = ownerType.GetProperty(memberName, BindingFlags.Public | BaseFlags);

			if (null == memberInfo)
			{
				memberInfo = ownerType.GetField(memberName, BindingFlags.Public | BaseFlags);

				if (null == memberInfo)
				{
					memberInfo = ownerType.GetProperty(memberName, BindingFlags.NonPublic | BaseFlags);

					if (null == memberInfo)
					{
						memberInfo = ownerType.GetField(memberName, BindingFlags.NonPublic | BaseFlags);
					}
				}
			}

			return memberInfo;
		}
		#endregion //GetMember

		#endregion //Private Methods

		#region MemberNameKey struct
		private struct MemberNameKey : IEquatable<MemberNameKey>
		{
			#region Member Variables

			public Type OwnerType;
			public string MemberName;

			#endregion //Member Variables

			#region Base class overrides
			public override bool Equals(object obj)
			{
				if (obj is MemberNameKey)
					return this.Equals((MemberNameKey)obj);

				return false;
			}

			public override int GetHashCode()
			{
				return this.OwnerType.GetHashCode() ^ this.MemberName.GetHashCode();
			}
			#endregion //Base class overrides

			#region IEquatable<MemberNameKey> Members
			public bool Equals(MemberNameKey other)
			{
				return other.OwnerType == this.OwnerType &&
					string.CompareOrdinal(other.MemberName, this.MemberName) == 0;
			}
			#endregion //IEquatable<MemberNameKey> Members
		}
		#endregion //MemberNameKey struct
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved