/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
using Dev2.Threading;

namespace Dev2.Activities.Designers2.PostgreSql
{
    public partial class PostgreSqlDatabaseDesigner
    {
        public PostgreSqlDatabaseDesigner()
        {
            InitializeComponent();
        }

        protected override PostgreSqlDatabaseDesignerViewModel CreateViewModel()
        {
            return new PostgreSqlDatabaseDesignerViewModel(ModelItem, new AsyncWorker(), new ViewPropertyBuilder());
        }
    }
}
    
