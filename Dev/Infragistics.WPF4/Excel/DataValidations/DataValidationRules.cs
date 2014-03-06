using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// A collection of <see cref="DataValidationRule"/> instances in a worksheet.
	/// </summary>
	/// <seealso cref="Infragistics.Documents.Excel.Worksheet.DataValidationRules"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 class DataValidationRuleCollection : IDictionary<DataValidationRule, WorksheetReferenceCollection>
	{
		#region Member Variables

		private Dictionary<DataValidationRule, WorksheetReferenceCollection> rules;
		private Worksheet worksheet;

		#endregion  // Member Variables

		#region Constructor

		internal DataValidationRuleCollection(Worksheet worksheet)
		{
			this.rules = new Dictionary<DataValidationRule, WorksheetReferenceCollection>();
			this.worksheet = worksheet;
		}

		#endregion  // Constructor

		#region Interfaces

		#region IDictionary<DataValidationRule,WorksheetReferenceCollection> Members

		void IDictionary<DataValidationRule, WorksheetReferenceCollection>.Add(DataValidationRule key, WorksheetReferenceCollection value)
		{
			this.Add(key, value);
		}

		bool IDictionary<DataValidationRule, WorksheetReferenceCollection>.ContainsKey(DataValidationRule key)
		{
			return this.Contains(key);
		}

		bool IDictionary<DataValidationRule, WorksheetReferenceCollection>.TryGetValue(DataValidationRule key, out WorksheetReferenceCollection value)
		{
			return this.TryGetReferences(key, out value);
		}

		ICollection<DataValidationRule> IDictionary<DataValidationRule, WorksheetReferenceCollection>.Keys
		{
			get { return this.rules.Keys; }
		}

		ICollection<WorksheetReferenceCollection> IDictionary<DataValidationRule, WorksheetReferenceCollection>.Values
		{
			get { return this.rules.Values; }
		}

		#endregion

		#region ICollection<KeyValuePair<DataValidationRule,WorksheetReferenceCollection>> Members

		void ICollection<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>.Add(KeyValuePair<DataValidationRule, WorksheetReferenceCollection> item)
		{
			this.Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>.Contains(KeyValuePair<DataValidationRule, WorksheetReferenceCollection> item)
		{
			WorksheetReferenceCollection value;
			if (this.TryGetReferences(item.Key, out value))
				return EqualityComparer<WorksheetReferenceCollection>.Default.Equals(value, item.Value);

			return false;
		}

		void ICollection<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>.CopyTo(KeyValuePair<DataValidationRule, WorksheetReferenceCollection>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>)this.rules).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>.Remove(KeyValuePair<DataValidationRule, WorksheetReferenceCollection> item)
		{
			return ((ICollection<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>)this.rules).Remove(item);
		}

		#endregion

		#region IEnumerable<KeyValuePair<DataValidationRule,WorksheetReferenceCollection>> Members

		IEnumerator<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>> IEnumerable<KeyValuePair<DataValidationRule, WorksheetReferenceCollection>>.GetEnumerator()
		{
			return this.rules.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.rules.GetEnumerator();
		}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region Public Methods

		#region Add (DataValidationRule - base)

		/// <summary>
		/// Applies a data validation rule to the a set of references.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the set of references.</param>
		/// <param name="references">The reference to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="references"/> are from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="references"/> collection already have a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="references"/> collection are in the totals row of a table.
		/// </exception>
		public void Add(DataValidationRule rule, WorksheetReferenceCollection references)
		{
			this.Add(rule, references, true);
		}

		/// <summary>
		/// Applies a data validation rule to the a set of references.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the set of references.</param>
		/// <param name="references">The reference to which the data validation rule should be applied.</param>
		/// <param name="overwriteExistingRules">
		/// Indicates whether cells in the references collection which already have data validation rules should have the rules overwritten.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="references"/> are from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="overwriteExistingRules"/> is False and the one or more of the cells in the <paramref name="references"/> 
		/// collection already have a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="references"/> collection are in the totals row of a table.
		/// </exception>
		public void Add(DataValidationRule rule, WorksheetReferenceCollection references, bool overwriteExistingRules)
		{
			if (references == null)
				throw new ArgumentNullException("references");

			this.Validate(rule, references);

			if (overwriteExistingRules == false && this.Contains(references))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ReferencesAlreadyContainDVs"), "references");

			// MD 3/13/12 - 12.1 - Table Support
			for (int regionIndex = 0; regionIndex < references.Regions.Count; regionIndex++)
			{
				WorksheetRegionAddress regionAddress = references.Regions[regionIndex];

				
				for (int i = 0; i < this.worksheet.Tables.Count; i++)
				{
					WorksheetTable table = this.worksheet.Tables[i];
					WorksheetRegion totalsRowRegion = table.TotalsRowRegion;
					if (totalsRowRegion != null && totalsRowRegion.Address.IntersectsWith(regionAddress))
						throw new ArgumentException(SR.GetString("LE_ArgumentException_CannotApplyDVRuleToTotalCell"), "references");
				}
			}

			// If overwriteExistingRules was True, make sure to remove the rules from all references.
			this.Remove(references);

			this.AddInternal(rule, references);
		}

		#endregion  // Add (DataValidationRule - base)

		#region Add (AnyValueDataValidationRule)

		/// <summary>
		/// Applies an <see cref="AnyValueDataValidationRule"/> to a <see cref="WorksheetCell"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the cell.</param>
		/// <param name="cell">The cell to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> already has a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(AnyValueDataValidationRule rule, WorksheetCell cell)
		{
			this.AddHelper(rule, cell);
		}

		/// <summary>
		/// Applies an <see cref="AnyValueDataValidationRule"/> to the cells in a <see cref="WorksheetRegion"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the region.</param>
		/// <param name="region">The region to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> already have a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(AnyValueDataValidationRule rule, WorksheetRegion region)
		{
			this.AddHelper(rule, region);
		}

		#endregion  // Add (AnyValueDataValidationRule)

		#region Add (CustomDataValidationRule)

		/// <summary>
		/// Applies an <see cref="CustomDataValidationRule"/> to a <see cref="WorksheetCell"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the cell.</param>
		/// <param name="cell">The cell to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> already has a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(CustomDataValidationRule rule, WorksheetCell cell)
		{
			this.AddHelper(rule, cell);
		}

		/// <summary>
		/// Applies an <see cref="CustomDataValidationRule"/> to the cells in a <see cref="WorksheetRegion"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the region.</param>
		/// <param name="region">The region to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> already have a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(CustomDataValidationRule rule, WorksheetRegion region)
		{
			this.AddHelper(rule, region);
		}

		#endregion  // Add (CustomDataValidationRule)

		#region Add (ListDataValidationRule)

		/// <summary>
		/// Applies an <see cref="ListDataValidationRule"/> to a <see cref="WorksheetCell"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the cell.</param>
		/// <param name="cell">The cell to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> already has a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(ListDataValidationRule rule, WorksheetCell cell)
		{
			this.AddHelper(rule, cell);
		}

		/// <summary>
		/// Applies an <see cref="ListDataValidationRule"/> to the cells in a <see cref="WorksheetRegion"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the region.</param>
		/// <param name="region">The region to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> already have a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(ListDataValidationRule rule, WorksheetRegion region)
		{
			this.AddHelper(rule, region);
		}

		#endregion  // Add (ListDataValidationRule)

		#region Add (OneConstraintDataValidationRule)

		/// <summary>
		/// Applies an <see cref="OneConstraintDataValidationRule"/> to a <see cref="WorksheetCell"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the cell.</param>
		/// <param name="cell">The cell to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> already has a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(OneConstraintDataValidationRule rule, WorksheetCell cell)
		{
			this.AddHelper(rule, cell);
		}

		/// <summary>
		/// Applies an <see cref="OneConstraintDataValidationRule"/> to the cells in a <see cref="WorksheetRegion"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the region.</param>
		/// <param name="region">The region to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> already have a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(OneConstraintDataValidationRule rule, WorksheetRegion region)
		{
			this.AddHelper(rule, region);
		}

		#endregion  // Add (OneConstraintDataValidationRule)

		#region Add (TwoConstraintDataValidationRule)

		/// <summary>
		/// Applies an <see cref="TwoConstraintDataValidationRule"/> to a <see cref="WorksheetCell"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the cell.</param>
		/// <param name="cell">The cell to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> already has a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(TwoConstraintDataValidationRule rule, WorksheetCell cell)
		{
			this.AddHelper(rule, cell);
		}

		/// <summary>
		/// Applies an <see cref="TwoConstraintDataValidationRule"/> to the cells in a <see cref="WorksheetRegion"/>.
		/// </summary>
		/// <param name="rule">The data validation rule to apply to the region.</param>
		/// <param name="region">The region to which the data validation rule should be applied.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> already applied to another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> already have a data validation rule applied to it.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the one or more of the cells in the <paramref name="region"/> is a total cell of a table.
		/// </exception>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection)"/>
		/// <seealso cref="Add(DataValidationRule,WorksheetReferenceCollection,bool)"/>
		public void Add(TwoConstraintDataValidationRule rule, WorksheetRegion region)
		{
			this.AddHelper(rule, region);
		}

		#endregion  // Add (TwoConstraintDataValidationRule)

		#region Clear

		/// <summary>
		/// Removes all data validation rules from the <see cref="Worksheet"/>.
		/// </summary>
		public void Clear()
		{
			// MD 8/25/11 - TFS84908
			// This was just calling back into the same method, which causes a stack overflow.
			//this.Clear();
			List<DataValidationRule> rules = new List<DataValidationRule>(this.rules.Keys);
			for (int i = 0; i < rules.Count; i++)
				this.Remove(rules[i]);
		}

		#endregion  // Clear

		#region Contains (DataValidationRule)

		/// <summary>
		/// Determines whether the specified data validation rule exists on the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="rule">The rule to test.</param>
		/// <returns>True if the rule is applied to one or more cells on the Worksheet; False otherwise.</returns>
		public bool Contains(DataValidationRule rule)
		{
			return this.rules.ContainsKey(rule);
		}

		#endregion  // Contains  (DataValidationRule)

		#region Contains (WorksheetCell)

		/// <summary>
		/// Determines whether the specified cell has a data validation rule applied to it.
		/// </summary>
		/// <param name="cell">The cell to test.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <returns>
		/// True if the cell has a data validation rule applied; False otherwise.
		/// </returns>
		public bool Contains(WorksheetCell cell)
		{
			this.ValidateCell(cell);

			foreach (KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair in this.rules)
			{
				WorksheetReferenceCollection references = pair.Value;

				if (references.Contains(cell))
					return true;
			}

			return false;
		}

		#endregion  // Contains (WorksheetCell)

		#region Contains (WorksheetReferenceCollection)

		/// <summary>
		/// Determines whether the specified references collection has a data validation rule applied to any of it's cells.
		/// </summary>
		/// <param name="references">The references collection to test.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="references"/> are from a worksheet other than this collection.
		/// </exception>
		/// <returns>
		/// True if the any cell from the references collection has a data validation rule applied; False otherwise.
		/// </returns>
		public bool Contains(WorksheetReferenceCollection references)
		{
			this.ValidateReferences(references);

			for (int i = 0; i < references.Regions.Count; i++)
			{
				if (this.Contains(references.Regions[i]))
					return true;
			}

			return false;
		}

		#endregion  // Contains (WorksheetReferenceCollection)

		#region Contains (WorksheetRegion)

		/// <summary>
		/// Determines whether the specified region has a data validation rule applied to any of it's cells.
		/// </summary>
		/// <param name="region">The region to test.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a worksheet other than this collection.
		/// </exception>
		/// <returns>
		/// True if the any cell from the region has a data validation rule applied; False otherwise.
		/// </returns>
		public bool Contains(WorksheetRegion region)
		{
			this.ValidateRegion(region);

			// MD 3/13/12 - 12.1 - Table Support
			// Moved all code to the new overload.
			return this.Contains(region.Address);
		}

		// MD 3/13/12 - 12.1 - Table Support
		internal bool Contains(WorksheetRegionAddress region)
		{
			foreach (KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair in this.rules)
			{
				if (pair.Value.IntersectsWith(region))
					return true;
			}

			return false;
		}

		#endregion  // Contains (WorksheetRegion)

		#region FindRule (WorksheetCell)

		/// <summary>
		/// Find the data validation rule applied to the specified cell.
		/// </summary>
		/// <param name="cell">The cell to test.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <returns>A <see cref="DataValidationRule"/>-derived instance or null if the cell has no rule applied.</returns>
		public DataValidationRule FindRule(WorksheetCell cell)
		{
			this.ValidateCell(cell);

			foreach (KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair in this.rules)
			{
				if (pair.Value.Contains(cell))
					return pair.Key;
			}

			return null;
		}

		#endregion  // FindRule (WorksheetCell)

		#region Remove (DataValidationRule)

		/// <summary>
		/// Removes the specified rule from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="rule">The rule which should be removed.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="rule"/> is null.
		/// </exception>
		/// <returns>True if the rule was contained on the Worksheet before removal; False otherwise.</returns>
		public bool Remove(DataValidationRule rule)
		{
			if (rule == null)
				throw new ArgumentNullException("rule");

			if (rule.ParentCollection != this)
				return false;

			this.rules.Remove(rule);
			rule.OnRemovedFromCollection();
			return true;
		}

		#endregion  // Remove (DataValidationRule)

		#region Remove (WorksheetCell)

		/// <summary>
		/// Removes the data validation rule form the cell if one is applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the rule is only applied to the specified cell, it will be removed from the collection.
		/// </p>
		/// </remarks>
		/// <param name="cell">The cell which should have its data validation rule cleared.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a worksheet other than this collection.
		/// </exception>
		/// <returns>
		/// True if the data validation rule was found and removed from the cell; False if the cell had no data validation rule applied.
		/// </returns>
		public bool Remove(WorksheetCell cell)
		{
			this.ValidateCell(cell);

			foreach (KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair in this.rules)
			{
				WorksheetReferenceCollection references = pair.Value;

				if (references.Contains(cell))
				{
					references.Remove(cell);
					if (references.CellsCount == 0)
						this.Remove(pair.Key);

					return true;
				}
			}

			return false;
		}

		#endregion  // Remove (WorksheetCell)

		#region Remove (WorksheetReferenceCollection)

		/// <summary>
		/// Removes the data validation rule form all cells in the specified collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any rules in the collection are not applied to any cells after this operation, they will be removed from the collection.
		/// </p>
		/// </remarks>
		/// <param name="references">The collection of references which should have its data validation rules cleared.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="references"/> are from a worksheet other than this collection.
		/// </exception>
		/// <returns>
		/// True if the data validation rule was found and removed from any cells in the collection; False if the cells had no data validation rule applied.
		/// </returns>
		public bool Remove(WorksheetReferenceCollection references)
		{
			this.ValidateReferences(references);

			bool returnValue = false;

			for (int i = 0; i < references.Regions.Count; i++)
				returnValue |= this.Remove(references.Regions[i]);

			return returnValue;
		}

		#endregion  // Remove (WorksheetReferenceCollection)

		#region Remove (WorksheetRegion)

		/// <summary>
		/// Removes the data validation rule form all cells in the specified region.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any rules in the collection are not applied to any cells after this operation, they will be removed from the collection.
		/// </p>
		/// </remarks>
		/// <param name="region">The region which should have its data validation rules cleared.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a worksheet other than this collection.
		/// </exception>
		/// <returns>
		/// True if the data validation rule was found and removed from any cells in the region; False if the region had no data validation rule applied.
		/// </returns>
		public bool Remove(WorksheetRegion region)
		{
			this.ValidateRegion(region);

			// MD 3/13/12 - 12.1 - Table Support
			// Moved all code to the new overload.
			return this.Remove(region.Address);
		}

		// MD 3/13/12 - 12.1 - Table Support
		internal bool Remove(WorksheetRegionAddress region)
		{
			bool returnValue = false;
			List<DataValidationRule> rulesToRemove = new List<DataValidationRule>();

			foreach (KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair in this.rules)
			{
				WorksheetReferenceCollection references = pair.Value;

				if (references.IntersectsWith(region))
				{
					references.Remove(region);
					if (references.CellsCount == 0)
						rulesToRemove.Add(pair.Key);

					returnValue = true;
				}
			}

			for (int i = 0; i < rulesToRemove.Count; i++)
				this.Remove(rulesToRemove[i]);

			return returnValue;
		}

		#endregion  // Remove (WorksheetRegion)

		#region TryGetReferences

		/// <summary>
		/// Gets the references which have the specified rule applied to them or null if the rule is not used on the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="rule">The rule to test.</param>
		/// <param name="references">
		/// [Out] When this method returns, contains the references which have the rule applied to them or null if the rule isn't applied on 
		/// the Worksheet.
		/// </param>
		/// <returns>True if the rule has been applied on the Worksheet; False otherwise.</returns>
		public bool TryGetReferences(DataValidationRule rule, out WorksheetReferenceCollection references)
		{
			return this.rules.TryGetValue(rule, out references);
		}

		#endregion  // TryGetReferences

		#endregion  // Public Methods

		#region Internal Methods

		#region AddHelper

		internal void AddHelper(DataValidationRule rule, WorksheetCell cell)
		{
			this.ValidateCell(cell);
			this.Add(rule, new WorksheetReferenceCollection(cell));
		}

		internal void AddHelper(DataValidationRule rule, WorksheetRegion region)
		{
			this.ValidateRegion(region);
			this.Add(rule, new WorksheetReferenceCollection(region));
		}

		#endregion  // AddHelper

		#region AddInternal

		internal void AddInternal(DataValidationRule rule, WorksheetReferenceCollection references)
		{
			WorksheetReferenceCollection otherReferences;
			if (this.rules.TryGetValue(rule, out otherReferences))
			{
				otherReferences.Add(references);
				return;
			}
			
			this.rules.Add(rule, references);
			rule.OnAddedToCollection(this);
		}

		#endregion  // AddInternal

		#endregion  // Internal Methods

		#region Private Methods

		#region Validate

		private void Validate(DataValidationRule rule, WorksheetReferenceCollection references)
		{
			if (rule == null)
				throw new ArgumentNullException("rule");

			if (rule.ParentCollection != null && rule.ParentCollection != this)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_RuleAppliedOnOtherWorksheet"), "rule");

			rule.VerifyState(this, references);

			if (references != null && references.Worksheet != this.worksheet)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ReferencesFromOtherWorksheet"), "references");
		}

		#endregion  // Validate

		#region ValidateCell

		private void ValidateCell(WorksheetCell cell)
		{
			if (cell == null)
				throw new ArgumentNullException("cell");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			if (cell.Worksheet != this.worksheet)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_CellFromOtherWorksheet"), "cell");
		}

		#endregion  // ValidateCell

		#region ValidateReferences

		private void ValidateReferences(WorksheetReferenceCollection references)
		{
			if (references == null)
				throw new ArgumentNullException("references");

			if (references.Worksheet != this.worksheet)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ReferencesFromOtherWorksheet"), "references");
		}

		#endregion  // ValidateReferences

		#region ValidateRegion

		private void ValidateRegion(WorksheetRegion region)
		{
			if (region == null)
				throw new ArgumentNullException("region");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (region.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"), "region");

			if (region.Worksheet != this.worksheet)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_RegionFromOtherWorksheet"), "region");
		}

		#endregion  // ValidateRegion

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of data validation rules applied on the <see cref="Worksheet"/>.
		/// </summary>
		public int Count
		{
			get { return this.rules.Count; }
		}

		#endregion  // Count

		#region Indexer

		/// <summary>
		/// Gets or sets the references which have the data validation rule applied to it.
		/// </summary>
		/// <param name="rule">A data validation rule.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="rule"/> is null.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// Occurs when the property is retrieved and the <paramref name="rule"/> is not applied to the <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the property is set and the value is from a Worksheet other than this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the property is set and the <paramref name="rule"/> is invalid based on its rule type's requirements.
		/// </exception>
		public WorksheetReferenceCollection this[DataValidationRule rule]
		{
			get { return this.rules[rule]; }
			set
			{
				if (value == null)
				{
					this.Remove(rule);
					return;
				}

				this.Validate(rule, value);

				// Remove the rules from all references before storing the new rule.
				this.Remove(value);

				this.rules[rule] = value;
				rule.OnAddedToCollection(this);
			}
		}

		#endregion  // Indexer

		#endregion  // Public Properties

		#region Internal Properties

		#region Worksheet

		internal Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion  // Worksheet

		#endregion  // Internal Properties

		#endregion  // Properties
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