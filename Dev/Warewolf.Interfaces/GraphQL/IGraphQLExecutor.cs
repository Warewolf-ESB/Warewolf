using System;
using System.Collections.Generic;
using System.Text;

namespace Warewolf.GraphQL
{
  public interface IGraphQLExecutor
  {
    string Execute(string query);
  }
}
