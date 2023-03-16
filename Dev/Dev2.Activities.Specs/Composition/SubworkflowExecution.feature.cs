#line hidden
                TechTalk.SpecFlow.Table table692 = new TechTalk.SpecFlow.Table(new string[] {
                            "Input to Service",
                            "From Variable",
                            "Output from Service",
                            "To Variable"});
                table692.AddRow(new string[] {
                            "",
                            "",
                            "Result",
                            "[[Result]]"});
                table692.AddRow(new string[] {
                            "",
                            "",
                            "Error",
                            "[[Error]]"});
#line 366
 testRunner.And("\"Wolf-1212_Test\" contains \"ErrorHandled\" from server \"localhost\" with mapping as", ((string)(null)), table692, "And ");
#line hidden
#line 370
 testRunner.When("\"Wolf-1212_Test\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 371
 testRunner.Then("the workflow execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table693 = new TechTalk.SpecFlow.Table(new string[] {
                            ""});
                table693.AddRow(new string[] {
                            "[[Result]] = Fail"});
                table693.AddRow(new string[] {
                            "[[Error]] = Could not parse input datetime with given input format (if you left t" +
                                "he input format blank then even after trying default datetime formats from other" +
                                " cultures)"});
#line 372
 testRunner.And("the \"ErrorHandled\" in Workflow \"Wolf-1212_Test\" debug outputs as", ((string)(null)), table693, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Error not bubbling up error message")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SubworkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SubworkflowExecution")]
        public virtual void ErrorNotBubblingUpErrorMessage()
        {
            string[] tagsOfScenario = new string[] {
                    "SubworkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Error not bubbling up error message", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 378
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
#line 379
 testRunner.Given("I have a workflow \"Wolf-1212_2\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table694 = new TechTalk.SpecFlow.Table(new string[] {
                            "Input to Service",
                            "From Variable",
                            "Output from Service",
                            "To Variable"});
                table694.AddRow(new string[] {
                            "",
                            "",
                            "Result",
                            "[[Result]]"});
#line 380
 testRunner.And("\"Wolf-1212_2\" contains \"ErrorBubbleUp\" from server \"localhost\" with mapping as", ((string)(null)), table694, "And ");
#line hidden
#line 383
 testRunner.When("\"Wolf-1212_2\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 384
 testRunner.Then("the workflow execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table695 = new TechTalk.SpecFlow.Table(new string[] {
                            ""});
                table695.AddRow(new string[] {
                            "[[Result]] = Pass"});
#line 385
 testRunner.And("the \"ErrorBubbleUp\" in Workflow \"Wolf-1212_2\" debug outputs as", ((string)(null)), table695, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Rabbit MQ Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SubworkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SubworkflowExecution")]
        public virtual void RabbitMQTest()
        {
            string[] tagsOfScenario = new string[] {
                    "SubworkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Rabbit MQ Test", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 390
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
#line 391
 testRunner.Given("I depend on a valid RabbitMQ server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 392
 testRunner.And("I have a workflow \"RabbitMQ Tester WF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table696 = new TechTalk.SpecFlow.Table(new string[] {
                            "Input to Service",
                            "From Variable",
                            "Output from Service",
                            "To Variable"});
                table696.AddRow(new string[] {
                            "",
                            "",
                            "result",
                            "[[result]]"});
#line 393
 testRunner.And("\"RabbitMQ Tester WF\" contains \"RabbitMQTest\" from server \"localhost\" with mapping" +
                        " as", ((string)(null)), table696, "And ");
#line hidden
#line 396
 testRunner.When("\"RabbitMQ Tester WF\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 397
 testRunner.Then("the workflow execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table697 = new TechTalk.SpecFlow.Table(new string[] {
                            ""});
                table697.AddRow(new string[] {
                            "[[result]] = Pass"});
#line 398
 testRunner.And("the \"RabbitMQTest\" in Workflow \"RabbitMQ Tester WF\" debug outputs as", ((string)(null)), table697, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Executing WebGet Returning False")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "SubworkflowExecution")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("SubworkflowExecution")]
        public virtual void ExecutingWebGetReturningFalse()
        {
            string[] tagsOfScenario = new string[] {
                    "SubworkflowExecution"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Executing WebGet Returning False", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 403
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
this.FeatureBackground();
#line hidden
#line 404
 testRunner.Given("I depend on a valid HTTP verbs server", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 405
 testRunner.And("I have a workflow \"Testing - WebGet\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table698 = new TechTalk.SpecFlow.Table(new string[] {
                            "Input to Service",
                            "From Variable",
                            "Output from Service",
                            "To Variable"});
#line 406
 testRunner.And("\"Testing - WebGet\" contains \"GetWebResult\" from server \"localhost\" with mapping a" +
                        "s", ((string)(null)), table698, "And ");
#line hidden
#line 408
 testRunner.When("\"Testing - WebGet\" is executed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 409
 testRunner.Then("the workflow execution has \"NO\" error", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
                TechTalk.SpecFlow.Table table699 = new TechTalk.SpecFlow.Table(new string[] {
                            ""});
                table699.AddRow(new string[] {
                            "[[Result(1).Category]] = Electronic"});
                table699.AddRow(new string[] {
                            "[[Result(2).Category]] = Electronic"});
                table699.AddRow(new string[] {
                            "[[Result(3).Category]] = Electronic"});
                table699.AddRow(new string[] {
                            "[[Result(4).Category]] = Electronic"});
                table699.AddRow(new string[] {
                            "[[Result(5).Category]] = Electronic"});
                table699.AddRow(new string[] {
                            "[[Result(6).Category]] = Gift Items"});
                table699.AddRow(new string[] {
                            "[[Result(1).Id]] = 1"});
                table699.AddRow(new string[] {
                            "[[Result(2).Id]] = 2"});
                table699.AddRow(new string[] {
                            "[[Result(3).Id]] = 3"});
                table699.AddRow(new string[] {
                            "[[Result(4).Id]] = 4"});
                table699.AddRow(new string[] {
                            "[[Result(5).Id]] = 5"});
                table699.AddRow(new string[] {
                            "[[Result(6).Id]] = 6"});
                table699.AddRow(new string[] {
                            "[[Result(1).Name]] = Television"});
                table699.AddRow(new string[] {
                            "[[Result(2).Name]] = Refrigerator"});
                table699.AddRow(new string[] {
                            "[[Result(3).Name]] = Mobiles"});
                table699.AddRow(new string[] {
                            "[[Result(4).Name]] = Laptops"});
                table699.AddRow(new string[] {
                            "[[Result(5).Name]] = iPads"});
                table699.AddRow(new string[] {
                            "[[Result(6).Name]] = Toys"});
                table699.AddRow(new string[] {
                            "[[Result(1).Price]] = 82000"});
                table699.AddRow(new string[] {
                            "[[Result(2).Price]] = 23000"});
                table699.AddRow(new string[] {
                            "[[Result(3).Price]] = 20000"});
                table699.AddRow(new string[] {
                            "[[Result(4).Price]] = 45000"});
                table699.AddRow(new string[] {
                            "[[Result(5).Price]] = 67000"});
                table699.AddRow(new string[] {
                            "[[Result(6).Price]] = 15000"});
#line 410
 testRunner.And("the \"GetWebResult\" in Workflow \"GetWebResult\" debug outputs as", ((string)(null)), table699, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
