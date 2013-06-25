using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class TemplateFragmentInjectionInstructionBuilder {

        // Internal class for the builder ;)
        internal class InjectionInstruction {
            public enDev2WebSiteTags TemplateTag { get; private set; }
            public string InjectionFragment { get; private set; }

            internal InjectionInstruction(enDev2WebSiteTags tag, string fragment) {
                TemplateTag = tag;
                InjectionFragment = fragment;
            }
        }

        // start outer class def
        private readonly IList<InjectionInstruction> _inst = new List<InjectionInstruction>();

        public void AddInstruction(enDev2WebSiteTags tag, string fragment) {

            if(fragment == null){
                fragment = string.Empty;
            }

            _inst.Add(new InjectionInstruction(tag, fragment));
        }

        public string ExecuteInstructions(string template) {
            string result = template;
            // <dev2html type=""pagetitle"" />
            string tmp = template.ToLower();

            _inst
                .ToList()
                .ForEach(i => {
                    string opt = @"<dev2html type="""+i.TemplateTag.ToString().ToLower()+" />";
                    int start = tmp.IndexOf(opt);
                    int len = opt.Length;
                    if (start < 0) {
                        opt = @"<dev2html type=""" + i.TemplateTag.ToString().ToLower() + "/>";
                        start = tmp.IndexOf(opt);
                        len = opt.Length;
                    }

                    // we are ok to continue
                    if (start > 0) {
                        result = result.Remove(start, len).Insert(start, i.InjectionFragment);
                        // keep tmp in-sync with result ;)
                        tmp = tmp.Remove(start, len).Insert(start, i.InjectionFragment);
                    }
                });

            return result;
        }
    }
}
