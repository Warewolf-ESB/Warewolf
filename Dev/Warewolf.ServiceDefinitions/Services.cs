/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Service
{
    public static class FindSourcesByType
    {
        public const string WorkspaceId = nameof(WorkspaceId);
    }
    public static class GetResourceById
    {
        public const string WorkspaceId = nameof(WorkspaceId);
        public const string ResourceId = nameof(ResourceId);
    }

    public static class SaveRedisSource
    {
        public const string RedisSource = nameof(RedisSource);
    }
    public static class TestRedisSource
    {
        public const string RedisSource = nameof(RedisSource);
    }
    public static class OptionsService
    {
        public const string ParameterName = nameof(ParameterName);

        public const string GateResume = nameof(GateResume);
    }

}
