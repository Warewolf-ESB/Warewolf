﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Service
{
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

    public static class SaveElasticsearchSource
    {
        public const string ElasticsearchSource = nameof(ElasticsearchSource);
    }

    public static class TestElasticsearchSource
    {
        public const string ElasticsearchSource = nameof(ElasticsearchSource);
    }

    public static class SaveServerSettings
    {
        public const string ServerSettings = nameof(ServerSettings);
    }

    public static class SavePersistenceSettings
    {
        public const string PersistenceSettings = nameof(PersistenceSettings);
    }
    public static class GetPersistenceSettings
    {
        public const string PersistenceSettings = nameof(PersistenceSettings);
    }

    public static class SaveAuditingSettings
    {
        public const string LegacySettings = nameof(LegacySettings);
        public const string AuditingSettings = nameof(AuditingSettings);
        public const string SinkType = nameof(SinkType);
    }

    public static class GetAuditingSettings
    {
        public const string LegacySettings = nameof(LegacySettings);
        public const string AuditingSettings = nameof(AuditingSettings);
        public const string SinkType = nameof(SinkType);
    }

    public static class GetDependanciesOnList
    {
        public const string Dependencies = nameof(Dependencies);
    }
    public static class SaveSubscriptionData
    {
        public const string SubscriptionData = nameof(SubscriptionData);
    }
    public static class GetSubscriptionData
    {
        public const string SubscriptionData = nameof(SubscriptionData);
    }
}