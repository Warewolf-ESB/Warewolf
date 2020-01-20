/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Data.ServiceModel;
using Dev2.Providers.Validation.Rules;
using System;
using System.Collections.Generic;

namespace Dev2.Activities.Designers2.RedisValidator
{
    public static class RedisValidatorDesignerViewModel
    {
        public static IEnumerable<IActionableErrorInfo> Validate(RedisDesignerDTO redisDesignerDTO, Action<bool> RedisSourceFocused, Action<bool> KeyFocused)
        {
            foreach (var error in GetRuleSet(RuleSets.RedisSource, redisDesignerDTO).ValidateRules("'Redis Source'", () => RedisSourceFocused(true)))
            {
                yield return error;
            }
            foreach (var error in GetRuleSet(RuleSets.Key, redisDesignerDTO).ValidateRules("'Key'", () => KeyFocused(true)))
            {
                yield return error;
            }
        }

        private static IRuleSet GetRuleSet(RuleSets propertyName, RedisDesignerDTO redisDesignerDTO)
        {
            var ruleSet = new RuleSet();

            switch (propertyName)
            {
                case RuleSets.RedisSource:
                    ruleSet.Add(new IsNullRule(() => redisDesignerDTO.RedisSource));
                    break;

                case RuleSets.Key:
                    ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(() => redisDesignerDTO.Key));
                    break;

                default:
                    break;
            }
            return ruleSet;
        }

        private enum RuleSets
        {
            RedisSource,
            Key
        } 
    }

    public class RedisDesignerDTO
    {
        public RedisSource RedisSource { get; set; }
        public string Key { get; set; }
        public RedisDesignerDTO(RedisSource redisSource, string key)
        {
            RedisSource = redisSource;
            Key = key;
        }
    }
}
