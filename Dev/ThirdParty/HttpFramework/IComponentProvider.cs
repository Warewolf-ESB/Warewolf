
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

namespace HttpFramework
{
	/// <summary>
	/// Inversion of control interface.
	/// </summary>
	public interface IComponentProvider
	{
		/// <summary>
		/// Add a component instance
		/// </summary>
		/// <typeparam name="T">Interface type</typeparam>
		/// <param name="instance">Instance to add</param>
		void AddInstance<T>(object instance);

		/// <summary>
		/// Get a component.
		/// </summary>
		/// <typeparam name="T">Interface type</typeparam>
		/// <returns>Component if registered, otherwise null.</returns>
		/// <remarks>
		/// Component will get created if needed.
		/// </remarks>
		T Get<T>() where T : class;

		/// <summary>
		/// Checks if the specified component interface have been added.
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns>true if found; otherwise false.</returns>
		bool Contains(Type interfaceType);

		/// <summary>
		/// Add a component.
		/// </summary>
		/// <typeparam name="InterfaceType">Type being requested.</typeparam>
		/// <typeparam name="InstanceType">Type being created.</typeparam>
		void Add<InterfaceType, InstanceType>();
	}
}
