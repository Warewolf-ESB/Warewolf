using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using Dev2.Common;
using Microsoft.JScript;
using System.Diagnostics.CodeAnalysis;
using Dev2;
using System.Activities;

namespace Dev2 {
    static public class JScriptEvaluator {
        private const string _jscriptSource =
            @"package Evaluator
        {
           class Evaluator
           {
              public function Eval(expr : String) : String 
              { 
                 return eval(expr); 
              }
           }
        }";

        static private object _evaluator;
        static private Type _evaluatorType;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "Can't be done inline - too complex")]
        static JScriptEvaluator() {
            InstantiateInternalEvaluator();
        }

        static private void InstantiateInternalEvaluator() {
            JScriptCodeProvider compiler = new JScriptCodeProvider();


            CompilerParameters parameters;
            parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;

            CompilerResults results;
            results = compiler.CompileAssemblyFromSource(parameters, _jscriptSource);

            Assembly assembly = results.CompiledAssembly;
            _evaluatorType = assembly.GetType("Evaluator.Evaluator");

            _evaluator = Activator.CreateInstance(_evaluatorType);
        }

        static public int EvaluateToInteger(string statement) {
            string s = EvaluateToString(statement);
            return int.Parse(s);
        }

        static public double EvaluateToDouble(string statement) {
            string s = EvaluateToString(statement);
            return double.Parse(s);
        }

        static public decimal ForceEvaluateToDecimal(string statement) {
            decimal result;
            bool s = Decimal.TryParse(statement, out result);
            return result;
        }

        static public decimal EvaluateToDecimal(string statement) {
            string s = EvaluateToString(statement);
            return decimal.Parse(s);
        }

        static public string EvaluateToString(string statement) {
            object o = EvaluateToObject(statement);
            return o.ToString();
        }

        static public bool EvaluateToBool(string statement) {
            object o = EvaluateToObject(statement);
            return (bool)o;
        }

        static public object EvaluateToObject(string statement) {
            try {
                return _evaluatorType.InvokeMember(
                    "Eval",
                    BindingFlags.InvokeMethod,
                    null,
                    _evaluator,
                    new object[] { statement }
                    );
            } catch (Exception ex) {
                InstantiateInternalEvaluator();
                return ex.Message;
            }
        }


        static public void VBExpression() {
            var t = new System.Activities.Statements.Sequence {
                Variables = {
                  new Variable<string>{Name = "LastResult"},
             }




            };



        }
    }

}